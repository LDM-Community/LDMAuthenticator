using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Interactions;
using Discord.WebSocket;
using LDMAuthenticator.Enum;

namespace LDMAuthenticator.Services;

public class InteractionHandler
{
    private readonly DiscordSocketClient _discord;
    private readonly InteractionService _interaction;
    private readonly LoggingService _loggingService;
    private readonly IServiceProvider _serviceProvider;

    public InteractionHandler(DiscordSocketClient discord, InteractionService interaction, LoggingService loggingService, 
        IServiceProvider serviceProvider)
    {
        _discord = discord;
        _interaction = interaction;
        _loggingService = loggingService;
        _serviceProvider = serviceProvider;
    }
    
    public async Task InitializeAsync()
    {
        await _interaction.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);

        _discord.InteractionCreated += InteractionCreated;
    }

    private async Task InteractionCreated(SocketInteraction arg)
    {
        try
        {
            var ctx = new SocketInteractionContext(_discord, arg);

            await _interaction.ExecuteCommandAsync(ctx, _serviceProvider);
        }
        catch (Exception ex)
        {
            await _loggingService.LogVerbose(ELogType.Error, ex.Message);
            await _loggingService.LogVerbose(ELogType.Debug, ex.StackTrace);
        }
    }
}