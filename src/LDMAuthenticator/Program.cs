using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using LDMAuthenticator.Enum;
using LDMAuthenticator.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Yaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LDMAuthenticator;

public class LDMAuthenticator
{
    public static Task Main() => new LDMAuthenticator().MainAsync();

    private async Task MainAsync()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddYamlFile("configuration.yaml")
            .Build();
        
        using IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) => services
                .AddSingleton(config)
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    GatewayIntents = GatewayIntents.All,
                    AlwaysDownloadUsers = true,
                    MessageCacheSize = 500
                }))
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<InteractionHandler>()
                .AddSingleton<LoggingService>()
                .AddSingleton(new CommandService()))
            .Build();

        await RunAsync(host);
    }
    
    private async Task RunAsync(IHost host)
    {
        using IServiceScope serviceScope = host.Services.CreateAsyncScope();
        IServiceProvider provider = serviceScope.ServiceProvider;

        var _client = provider.GetRequiredService<DiscordSocketClient>();
        var _commands = provider.GetRequiredService<InteractionService>();
        await provider.GetRequiredService<InteractionHandler>().InitializeAsync();
        var _config = provider.GetRequiredService<IConfigurationRoot>();
        var _loggingService = provider.GetRequiredService<LoggingService>();

        _client.Ready += async () =>
        {
            await _commands.RegisterCommandsGloballyAsync();
                
            await _loggingService.LogVerbose(ELogType.Info,
                $"Client logged in as {_client.CurrentUser.Username} #{_client.CurrentUser.Discriminator} ({_client.CurrentUser.Id})");
            await _loggingService.LogVerbose(ELogType.Info, $"Registered {_commands.SlashCommands.Count} Slash Commands(s)");
            await _client.SetActivityAsync(new Game("Over #gist-list", ActivityType.Watching));
        };

        await _client.LoginAsync(TokenType.Bot, _config["client:token"]);
        await _client.StartAsync();

        await Task.Delay(-1);
    }
}