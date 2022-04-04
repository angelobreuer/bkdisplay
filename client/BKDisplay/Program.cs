using BKDisplay;
using BKDisplay.Controlling;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var hostBuilder = Host.CreateDefaultBuilder();

hostBuilder.ConfigureAppConfiguration(options => options
    .AddJsonFile("appsettings.json", false));

hostBuilder.ConfigureServices((host, services) => services
    .AddSingleton<IDisplayClient, BKClient>()
    .AddSingleton<IDisplayController, DisplayController>()
    .AddHostedService<HostedDisplayService>()
    .Configure<BKClientOptions>(host.Configuration.GetSection("Client")));

await hostBuilder.RunConsoleAsync();