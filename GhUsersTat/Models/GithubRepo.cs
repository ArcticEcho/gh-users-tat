using Newtonsoft.Json;

namespace GhUsersTat.Models
{
    public class GithubRepo
    {
        public string Name { get; set; }
        [JsonProperty("html_url")]
        public string Url { get; set; }
        public string Description { get; set; }
        [JsonProperty("stargazers_count")]
        public int StarGazers { get; set; }
    }
}