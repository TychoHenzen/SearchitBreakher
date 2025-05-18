using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SearchitBreakher;
using SearchitLibrary.Abstractions;
using SearchitLibrary.IO;

internal static class Program
{
    private static void Main()
    {
        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                // Register MonoGame services + your game
                services.AddSingleton<BreakerGame>();

                var constantsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "constants.json");
                IConstantProvider jsonProvider = new JsonConstantProvider(constantsPath);
                IConstantProvider constantProvider = new CachingConstantProvider(jsonProvider);
                services.AddSingleton(constantProvider);
                // Register any other app services
            })
            .Build();

        // Resolve and run the game
        var game = host.Services.GetRequiredService<BreakerGame>();
        game.Run();
    }
}