namespace LDMAuthenticator.Models;

public class PendingGist
{
    public string Title;
    public string Description;
    public Gist Gist;
    public ulong Requester;
    public string Type;
}