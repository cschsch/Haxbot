using Microsoft.Extensions.Configuration;

namespace Haxbot;

public static class HaxbotSettings
{
    private static Configuration Configuration { get; set; } = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddCommandLine(Environment.GetCommandLineArgs())
                .Build()
                .Get<Configuration>();

    public static string DatabasePath => Configuration.DatabasePath;
    public static string ConnectionString => string.Format(Configuration.ConnectionStringTemplate, new[] { Configuration.DatabasePath });
}
