using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Haxbot;
using Haxbot.Api;
using Haxbot.Settings;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PuppeteerSharp;
using Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddBlazorise(options => options.Immediate = true)
    .AddBootstrapProviders()
    .AddFontAwesomeIcons();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var configuration = new ConfigurationBuilder()
        .AddJsonFile("haxbotconfig.json")
        .Build()
        .Get<Configuration>();


builder.Services.Add(new ServiceDescriptor(typeof(Configuration), configuration));
builder.Services.Add(new ServiceDescriptor(typeof(GamesService), _ => new GamesService(new HaxbotContext(configuration)), ServiceLifetime.Transient));
builder.Services.Add(new ServiceDescriptor(typeof(RoomsService), _ => new RoomsService(), ServiceLifetime.Singleton));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
