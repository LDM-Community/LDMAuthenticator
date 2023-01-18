using Newtonsoft.Json;

namespace LDMAuthenticator.Models;

public class Gist
{
    [JsonProperty("url")]
    public Uri Url { get; set; }

    [JsonProperty("forks_url")]
    public Uri ForksUrl { get; set; }

    [JsonProperty("commits_url")]
    public Uri CommitsUrl { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("node_id")]
    public string NodeId { get; set; }

    [JsonProperty("git_pull_url")]
    public Uri GitPullUrl { get; set; }

    [JsonProperty("git_push_url")]
    public Uri GitPushUrl { get; set; }

    [JsonProperty("html_url")]
    public Uri HtmlUrl { get; set; }

    [JsonProperty("files")]
    public Files Files { get; set; }

    [JsonProperty("public")]
    public bool Public { get; set; }

    [JsonProperty("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonProperty("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("comments")]
    public long Comments { get; set; }

    [JsonProperty("user")]
    public object User { get; set; }
        
    [JsonProperty("comments_url")]
    public Uri CommentsUrl { get; set; }

    [JsonProperty("owner")]
    public Owner Owner { get; set; }
}

public class Files
{ 
    [JsonProperty("ldmAuth")]
    public LdmAuth LdmAuth { get; set; }
}

public class LdmAuth
{
    [JsonProperty("filename")]
    public string Filename { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("language")]
    public string Language { get; set; }

    [JsonProperty("raw_url")]
    public Uri RawUrl { get; set; }

    [JsonProperty("size")]
    public long Size { get; set; }

    [JsonProperty("truncated")]
    public bool Truncated { get; set; }

    [JsonProperty("content")]
    public string Content { get; set; }
}

public class Owner
{
    [JsonProperty("login")]
    public string Login { get; set; }
        
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("node_id")]
    public string NodeId { get; set; }

    [JsonProperty("avatar_url")]
    public Uri AvatarUrl { get; set; }
}