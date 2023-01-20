using System;
using System.Collections.Generic;
using Discord.WebSocket;
using LDMAuthenticator.Models;

namespace LDMAuthenticator.Services;

public class DataService
{
    private readonly DiscordSocketClient _discord;
    private readonly LoggingService _loggingService;
    private readonly IServiceProvider _serviceProvider;
    public List<PendingGist> PendingGists;

    public DataService(DiscordSocketClient discord, LoggingService loggingService, 
        IServiceProvider serviceProvider)
    {
        _discord = discord;
        _loggingService = loggingService;
        _serviceProvider = serviceProvider;

        PendingGists = new List<PendingGist>();
    }
}