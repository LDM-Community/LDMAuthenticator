using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace papershredder432.LDMAuthenticator.Modules
{
    [Name("Gists")]
    [RequireContext(ContextType.Guild)]
    public class GistsModule : ModuleBase<SocketCommandContext>
    {
        [Command("Gist")]
        [Summary("Sends a request.")]
        private async Task Gist(string gistId)
        {
            if (gistId.StartsWith("https://gist.github.com/"))
            {
                gistId.Replace("https://gist.github.com/", "");
            }
            
            var webRequest = WebRequest.Create($"https://api.github.com/gists/{gistId}") as HttpWebRequest;
            if (webRequest == null) return;

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
                    embed.WithDescription($"Requester: {outPut.Owner.Login}");
                    embed.WithUrl(outPut.HtmlUrl.ToString());
                    embed.WithThumbnailUrl(outPut.Owner.AvatarUrl.ToString());
                    embed.AddField("Content", outPut.Files.LdmAuth.Content);

                    var chnl = Context.Client.GetChannel(750528045290618901) as ISocketMessageChannel;
                    chnl.SendMessageAsync(embed: embed.Build());
                    
                    await ReplyAsync($"Successfully sent a request under the GitHub name of {outPut.Owner.Login}.");
                }
            }
        }

        
    }
}