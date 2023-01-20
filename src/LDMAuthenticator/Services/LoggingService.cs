using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LDMAuthenticator.Enum;
using Microsoft.Extensions.Configuration;

namespace LDMAuthenticator.Services;

public class LoggingService
{
    private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _configuration;

        private string _logDirectory { get; }
        private string _logFile => Path.Combine(_logDirectory, $"{DateTime.UtcNow:yyyy-M-d}.txt");
        private string _verboseLogFile => Path.Combine(_logDirectory, $"verbose_{DateTime.UtcNow:yyyy-M-d}.txt");

        public LoggingService(DiscordSocketClient discord, CommandService commands, IConfigurationRoot configuration)
        {
            _logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
            
            _discord = discord;
            _commands = commands;
            _configuration = configuration;
            
            _discord.Log += OnLogAsync;
            _commands.Log += OnLogAsync;
        }
        
        private Task OnLogAsync(LogMessage msg)
        {
            if (!Directory.Exists(_logDirectory))
                Directory.CreateDirectory(_logDirectory);
            if (!File.Exists(_logFile))
                File.Create(_logFile).Dispose();

            string logText = $"[{DateTime.UtcNow:u}] [{msg.Severity}/{msg.Source}]: {msg.Exception?.ToString() ?? msg.Message}";
            File.AppendAllText(_logFile, logText + "\n");

            return Console.Out.WriteLineAsync(logText);
        }

        public Task LogVerbose(ELogType logType , string? msg)
        {
            if (!Directory.Exists(_logDirectory))
                Directory.CreateDirectory(_logDirectory);
            if (!File.Exists(_verboseLogFile))
                File.Create(_verboseLogFile).Dispose();
            
            if (msg == null && msg!.Length < 1) msg = "ERR";

            string logText = $"[{DateTime.UtcNow:u}] [{logType}/Verbose]: {msg}";
            File.AppendAllText(_verboseLogFile, logText + "\n");
            
            if (logType == ELogType.Debug && !_configuration.GetSection("debug").Get<bool>())
                return Task.CompletedTask;

            return Console.Out.WriteLineAsync(logText);
        }
}