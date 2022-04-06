using Haxbot.Api;
using Haxbot.Settings;
using Moq;
using NUnit.Framework;
using PuppeteerSharp;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Tests.Haxbot.Api;

[Parallelizable(ParallelScope.All)]
public class ApiTests
{
    public Browser Browser { get; set; } = default!;
    public Configuration Configuration { get; set; } = default!;

    private const string RoomUrl = "https://this-site-does-not-exist.com/";

    [OneTimeSetUp]
    public async Task SetUp()
    {
        var options = new LaunchOptions { Args = new[] { "--disable-web-security" }, Headless = true };
        await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
        Browser = await Puppeteer.LaunchAsync(options);

        Configuration = new Configuration
        {
            HaxballHeadlessUrl = Path.GetFullPath("testpage.html")
        };
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        await Browser.DisposeAsync();
    }

    private async Task<Page> SetUpPage(string roomObjectJsFn = "roomConfiguration => roomConfiguration")
    {
        var page = await Browser.NewPageAsync();
        await page.EvaluateExpressionOnNewDocumentAsync(
$@"const getRoomResult = {roomObjectJsFn};
HBInit = roomConfiguration => {{
  const roomlink = document.querySelector('iframe').contentWindow.document.getElementById('roomlink');
  const link = document.createElement('a');
  link.href = '{RoomUrl}';
  roomlink.appendChild(link);
  const room = getRoomResult(roomConfiguration);
  room.setTimeLimit = () => {{}};
  room.startRecording = () => {{}};
  return room;
}};");
        return page;
    }

    [Test]
    public async Task CreateRoom_ReturnsLinkAfterInit()
    {
        var page = await SetUpPage();
        var api = new HaxballApi(Mock.Of<IHaxballApiFunctions>(), Configuration, page, string.Empty);

        var result = await api.CreateRoomAsync();

        Assert.AreEqual(RoomUrl, result);
    }

    [Test]
    public async Task CreateRoom_PassesToken()
    {
        var expected = "token";
        var page = await SetUpPage();
        var api = new HaxballApi(Mock.Of<IHaxballApiFunctions>(), Configuration, page, expected);

        await api.CreateRoomAsync();
        var result = await page.EvaluateExpressionAsync<string>("window.room.token");

        Assert.AreEqual(expected, result);
    }

    [Test]
    public async Task PlayerJoinedRoom_AuthInAdmins_SetsAdmin()
    {
        // arrange
        var auth = "admin";
        var page = await SetUpPage("_ => { return { setPlayerAdmin: (id, value) => window.admin = value }; }");
        var configuration = Configuration with { RoomAdmins = new [] { auth } };
        var api = new HaxballApi(Mock.Of<IHaxballApiFunctions>(), configuration, page, string.Empty);

        // act
        await api.CreateRoomAsync();
        await page.EvaluateExpressionAsync($"room.onPlayerJoin({{ auth: '{auth}' }})");
        var result = await page.EvaluateExpressionAsync<bool>($"window.admin");

        // assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task PlayerJoinedRoom_CallsOnPlayerJoin()
    {
        // arrange
        var page = await SetUpPage();
        var functions = new Mock<IHaxballApiFunctions>();
        var api = new HaxballApi(functions.Object, Configuration, page, string.Empty);

        // act
        await api.CreateRoomAsync();
        await page.EvaluateExpressionAsync("room.onPlayerJoin({})");

        // assert
        functions.Verify(f => f.OnPlayerJoin(It.IsAny<HaxballPlayer>()));
    }

    [Test]
    public async Task StartGame_EnrichedPlayerWithAuth()
    {
        // arrange
        var expected = new HaxballPlayer { Id = 1, Auth = "player" };
        var page = await SetUpPage($"_ => {{ return {{ getPlayerList: _ => [ {{ id: {expected.Id} }} ] }}; }}");
        var functions = new Mock<IHaxballApiFunctions>();
        functions.Setup(f => f.StartGame(It.IsAny<HaxballPlayer[]>())).Returns(true);
        var api = new HaxballApi(functions.Object, Configuration, page, string.Empty);

        // act
        await api.CreateRoomAsync();
        await page.EvaluateExpressionAsync($"room.onPlayerJoin({{ id: {expected.Id}, auth: '{expected.Auth}' }}); room.onGameStart();");

        // assert
        functions.Verify(f => f.StartGame(It.Is<HaxballPlayer[]>(players => players.Single() == expected)));
    }

    [Test]
    public async Task StartGame_ReturnsFalse_SendsChatMessage()
    {
        // arrange
        var page = await SetUpPage("_ => { return { getPlayerList: _ => [], sendChat: message => window.message = message }; }");
        var functions = new Mock<IHaxballApiFunctions>();
        functions.Setup(f => f.StartGame(It.IsAny<HaxballPlayer[]>())).Returns(false);
        var api = new HaxballApi(functions.Object, Configuration, page, string.Empty);

        // act
        await api.CreateRoomAsync();
        await page.EvaluateExpressionAsync("room.onGameStart()");
        var result = await page.EvaluateExpressionAsync<string>("window.message");

        // assert
        Assert.AreEqual("Failed to save game to database!", result);
    }

    [Test]
    public async Task FinishGame_ReturnsFalse_SendsChatMessage()
    {
        // arrange
        var page = await SetUpPage("_ => { return { sendChat: message => window.message = message }; }");
        var functions = new Mock<IHaxballApiFunctions>();
        functions.Setup(f => f.FinishGame(It.IsAny<HaxballScores>())).Returns(false);
        var api = new HaxballApi(functions.Object, Configuration, page, string.Empty);

        // act
        await api.CreateRoomAsync();
        await page.EvaluateExpressionAsync("room.onTeamVictory()");
        var result = await page.EvaluateExpressionAsync<string>("window.message");

        // assert
        Assert.AreEqual("Failed to save results to database!", result);
    }

    [Test]
    public async Task OnPlayerLeave_PlayerStillInRoom_NotCallingCloseRoom()
    {
        // arrange
        var page = await SetUpPage("_ => { return { getPlayerList: () => [ { id: 1 } ] }; }");
        var functions = new Mock<IHaxballApiFunctions>();
        var api = new HaxballApi(functions.Object, Configuration, page, string.Empty);

        // act
        await api.CreateRoomAsync();
        await page.EvaluateExpressionAsync("room.onPlayerLeave()");

        // assert
        functions.Verify(f => f.CloseRoom(), Times.Never);
    }

    [Test]
    public async Task OnPlayerLeave_NoPlayersLeft_CallingCloseRoom()
    {
        // arrange
        var page = await SetUpPage("_ => { return { getPlayerList: () => [] }; }");
        var functions = new Mock<IHaxballApiFunctions>();
        var api = new HaxballApi(functions.Object, Configuration, page, string.Empty);

        // act
        await api.CreateRoomAsync();
        await page.EvaluateExpressionAsync("room.onPlayerLeave()");

        // assert
        functions.Verify(f => f.CloseRoom());
    }

    [Test]
    public async Task OnStadiumChange_CallsSetStadium()
    {
        // arrange
        var page = await SetUpPage();
        var functions = new Mock<IHaxballApiFunctions>();
        var api = new HaxballApi(functions.Object, Configuration, page, string.Empty);

        // act
        await api.CreateRoomAsync();
        await page.EvaluateExpressionAsync("room.onStadiumChange('teeeheee', {})");

        // assert
        functions.Verify(f => f.SetStadium("teeeheee", It.IsAny<HaxballPlayer>()));
    }

    [Test]
    public async Task HandleCommand_SendsAnswerInChat()
    {
        // arrange
        var expected = "command";
        var page = await SetUpPage("_ => { return { sendChat: message => window.message = message }; }");
        var functions = new Mock<IHaxballApiFunctions>();
        functions.Setup(f => f.HandleCommand(It.IsAny<HaxballPlayer>(), It.IsAny<string>())).Returns(expected);
        var api = new HaxballApi(functions.Object, Configuration, page, string.Empty);

        // act
        await api.CreateRoomAsync();
        await page.EvaluateExpressionAsync($"room.onPlayerChat({{}}, '{expected}')");
        var result = await page.EvaluateExpressionAsync<string>("window.message");

        // assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public async Task SaveReplay_EncodesToBase64()
    {
        // arrange
        var expected = "QQ==";
        var page = await SetUpPage("_ => { return { stopRecording: () => [65] }; }");
        var functions = new Mock<IHaxballApiFunctions>();
        var api = new HaxballApi(functions.Object, Configuration, page, string.Empty);

        // act
        await api.CreateRoomAsync();
        await page.EvaluateExpressionAsync("room.onGameStop()");

        // assert
        functions.Verify(f => f.SaveReplay(expected));
    }
}
