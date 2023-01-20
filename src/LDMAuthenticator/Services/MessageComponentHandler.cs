using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using LDMAuthenticator.Enum;
using LDMAuthenticator.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace LDMAuthenticator.Services;

public class MessageComponentHandler
{
    private readonly DiscordSocketClient _discord;
    private readonly LoggingService _loggingService;
    private readonly IServiceProvider _serviceProvider;
    private readonly DataService _dataService;
    private readonly IConfiguration _configuration;

    public MessageComponentHandler(DiscordSocketClient discord, LoggingService loggingService, 
        IServiceProvider serviceProvider, DataService dataService, IConfiguration configuration)
    {
        _discord = discord;
        _loggingService = loggingService;
        _serviceProvider = serviceProvider;
        _dataService = dataService;
        _configuration = configuration;

        _discord.ButtonExecuted += ButtonExecuted;
        _discord.ModalSubmitted += ModalSubmitted;
    }

    private async Task ModalSubmitted(SocketModal modal)
    {
        List<SocketMessageComponentData> components = modal.Data.Components.ToList();

        var gistId = components.FirstOrDefault(x => x.CustomId == "gist_id").Value;
        var gistType = components.FirstOrDefault(x => x.CustomId == "gist_type").Value;
        var gistTitle = components.FirstOrDefault(x => x.CustomId == "gist_title").Value;
        var gistDescription = components.FirstOrDefault(x => x.CustomId == "gist_description").Value;

        /*
        ISocketMessageChannel? chnl;
        if (_configuration.GetSection("debug").Get<bool>())
            chnl = _discord.GetChannelAsync(_configuration.GetSection("channels:debug:pending").Get<ulong>()).Result as ISocketMessageChannel;
        else
            chnl = _discord.GetChannelAsync(_configuration.GetSection("channels:release:accepted").Get<ulong>()).Result as ISocketMessageChannel;
        */

        ISocketMessageChannel? chnl = _discord.GetChannelAsync(750528045290618901).Result as ISocketMessageChannel;

        try
        {
            var webRequest = WebRequest.Create($"https://api.github.com/gists/{gistId}") as HttpWebRequest;

            webRequest.ContentType = "application/json";
            webRequest.UserAgent = "Nothing";

            using (var x = webRequest.GetResponse().GetResponseStream())
            {
                using (var sr = new StreamReader(x))
                {
                    var asJson = sr.ReadToEnd();
                    var outPut = JsonConvert.DeserializeObject<Gist>(asJson);
                    
                    var content = gistType.ToLower() switch
                    {
                        "documentation" => outPut.Files.LdmAuth.Content,
                        "raw" => $"```cs\n{outPut.Files.LdmAuth.Content}```",
                        _ => outPut.Files.LdmAuth.Content
                    };

                    var embed = new EmbedBuilder()
                        .WithTitle("New Auth Request")
                        .WithAuthor(modal.User)
                        .WithDescription($"{gistTitle} : {gistDescription}")
                        .WithUrl(outPut.HtmlUrl.ToString())
                        .WithThumbnailUrl(outPut.Owner.AvatarUrl.ToString())
                        .AddField("Content", content)
                        .WithTimestamp(DateTimeOffset.Now);

                    var buttons = new ComponentBuilder()
                        .WithButton("Accept", "gist-accept", ButtonStyle.Success)
                        .WithButton("Deny", "gist-deny", ButtonStyle.Danger)
                        .Build();

                    _dataService.PendingGists.Add(new PendingGist
                    {
                        Gist = outPut, 
                        Title = gistTitle, 
                        Description = gistDescription,
                        Requester = modal.User.Id,
                        Type = gistType
                    });

                    await chnl.SendMessageAsync(embed: embed.Build(), components: buttons, text: $"||<@{modal.User.Id}>||");

                    await modal.RespondAsync($"Successfully sent in your request!", ephemeral: true);
                }
            }
        }
        catch (Exception ex)
        {
            await _loggingService.LogVerbose(ELogType.Error, ex.Message);
            await _loggingService.LogVerbose(ELogType.Debug, ex.StackTrace);
        }
    }

    private async Task ButtonExecuted(SocketMessageComponent component)
    {
        /*
        ISocketMessageChannel? chnl;
        if (_configuration.GetSection("debug").Get<bool>())
            chnl = _discord.GetChannelAsync(_configuration.GetSection("channels:debug:accepted").Get<ulong>()).Result as ISocketMessageChannel;
        else
            chnl = _discord.GetChannelAsync(_configuration.GetSection("channels:release:accepted").Get<ulong>()).Result as ISocketMessageChannel;
        */
        
        ISocketMessageChannel? chnl = _discord.GetChannelAsync(743626157252935741).Result as ISocketMessageChannel;
        
        var request = _dataService.PendingGists.FirstOrDefault(x => x.Requester == component.Message.MentionedUsers.FirstOrDefault().Id);
        var usr = _discord.GetUserAsync(request.Requester).Result;
        
        var content = request.Type.ToLower() switch
        {
            "documentation" => request.Gist.Files.LdmAuth.Content,
            "raw" => $"```cs\n{request.Gist.Files.LdmAuth.Content}```",
            _ => request.Gist.Files.LdmAuth.Content
        };

        switch (component.Data.CustomId)
        {
            case "gist-accept":
                var embed = new EmbedBuilder()
                    .WithTitle(request.Title)
                    .WithAuthor(usr)
                    .WithDescription(request.Description)
                    .WithUrl(request.Gist.HtmlUrl.ToString())
                    .WithThumbnailUrl(request.Gist.Owner.AvatarUrl.ToString())
                    .AddField("Content", content)
                    .WithTimestamp(DateTimeOffset.Now);

                var msg = await chnl.SendMessageAsync(embed: embed.Build());
                await msg.AddReactionsAsync(new List<IEmote> { new Emoji("👍"), new Emoji("👎") });
                
                await component.RespondAsync($"Accepted <@{usr.Id}>'s Gist!");
                
                _dataService.PendingGists.Remove(request);
                break;
            
            case "gist-deny":
                await component.RespondAsync($"Denied <@{usr.Id}>'s Gist!");
                
                _dataService.PendingGists.Remove(request);
                break;
        }
    }
}