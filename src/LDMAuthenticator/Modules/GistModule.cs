using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using LDMAuthenticator.Services;

namespace LDMAuthenticator.Modules;

[RequireContext(ContextType.Guild)]
public class GistModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly LoggingService _loggingService;
    private readonly DataService _dataService;

    public GistModule(LoggingService loggingService, DataService dataService)
    {
        _loggingService = loggingService;
        _dataService = dataService;
    }

    [SlashCommand("gist", "Sends a request.")]
    public async Task Gist()
    {
        var modal = new ModalBuilder()
            .WithTitle("New Gist")
            .WithCustomId("gist_create")
            .AddTextInput("Gist ID", "gist_id", placeholder: "41a94dde65a5c6f6aafd56e4c74c577e", style: TextInputStyle.Short)
            .AddTextInput("Input Type", "gist_type", placeholder: "Documentation | RAW", style: TextInputStyle.Short)
            .AddTextInput("Title", "gist_title", placeholder: "What It Is", style: TextInputStyle.Short)
            .AddTextInput("Description", "gist_description", placeholder: "What it is in length, add more detail here", style: TextInputStyle.Paragraph);

        await RespondWithModalAsync(modal.Build());
        await RespondAsync("Complete!");
    }
}