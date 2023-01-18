using System.Net;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using LDMAuthenticator.Enum;
using LDMAuthenticator.Models;
using LDMAuthenticator.Services;
using Newtonsoft.Json;

namespace LDMAuthenticator.Modules;

[RequireContext(ContextType.Guild)]
public class GistModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly LoggingService _loggingService;

    public GistModule(LoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    [SlashCommand("gist", "Sends a request.")]
    public async Task Gist(string gistId)
    {
        if (gistId.Length < 1)
        {
            await RespondAsync("Please specify a valid Gist. " +
                               "\nYou can either do: `/gist 41a94dde65a5c6f6aafd56e4c74c577` or `/gist https://gist.github.com/papershredder432/41a94dde65a5c6f6aafd56e4c74c577e`", ephemeral: true);
            return;
        }
        
        if (!gistId.StartsWith("https://gist.github.com/"))
        {
            gistId.Replace("https://gist.github.com/", "");
        }

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

                    var embed = new EmbedBuilder();
                    embed.WithTitle("New Auth Request");
                    embed.WithUrl(outPut.HtmlUrl.ToString());
                    embed.WithThumbnailUrl(outPut.Owner.AvatarUrl.ToString());
                    embed.AddField("Content", outPut.Files.LdmAuth.Content);

                    var chnl = Context.Client.GetChannel(750528045290618901) as ISocketMessageChannel;
                    await chnl.SendMessageAsync(embed: embed.Build());
                    
                    await ReplyAsync($"Successfully sent a request under the GitHub name of {outPut.Owner.Login}.");
                }
            }
        }
        catch (Exception ex)
        {
            await _loggingService.LogVerbose(ELogType.Error, ex.Message);
            await _loggingService.LogVerbose(ELogType.Debug, ex.StackTrace);
        }
    }
}