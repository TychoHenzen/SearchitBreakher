using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SearchitBreakher.Graphics;
using SearchitLibrary.Abstractions;
using SearchitLibrary.Graphics;
using SearchitLibrary.IO;

namespace SearchitBreakher;

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
                services.AddSingleton<IConstantProvider>(_ =>
                {
                    var jsonProvider = new JsonConstantProvider(constantsPath);
                    return new CachingConstantProvider(jsonProvider);
                });

                services.AddSingleton<ICamera>(sp =>
                {
                    var game = sp.GetRequiredService<BreakerGame>();
                    return new MonoGameCamera(game.GraphicsDevice,
                        sp.GetRequiredService<IConstantProvider>());
                });

                services.AddSingleton<IChunkManager>(_ =>
                    new VoxelChunkManager(Path.Combine("Content", "voxels")));

                services.AddSingleton<IChunkRenderer>(sp =>
                    new ChunkRendererManager(sp.GetRequiredService<BreakerGame>().GraphicsDevice,
                        sp.GetRequiredService<ICamera>()));
            })
            .Build();

        // Resolve and run the game
        var game = host.Services.GetRequiredService<BreakerGame>();
        game.Run();
    }
}