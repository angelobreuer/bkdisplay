namespace BKDisplay.Emulator;

using BKDisplay.Controlling;
using Microsoft.Extensions.DependencyInjection;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        using var serviceProvider = new ServiceCollection()
            .AddSingleton<IDisplayClient>(serviceProvider => serviceProvider.GetRequiredService<MainForm>())
            .AddSingleton<IDisplayController, DisplayController>()
            .AddSingleton<HostedDisplayService>()
            .AddSingleton<MainForm>()
            .AddLogging()
            .BuildServiceProvider();

        var form = serviceProvider.GetRequiredService<MainForm>();
        var displayService = serviceProvider.GetRequiredService<HostedDisplayService>();

        displayService.StartAsync(default).GetAwaiter().GetResult();
        Application.Run(form);
        displayService.StopAsync(default).GetAwaiter().GetResult();
    }
}