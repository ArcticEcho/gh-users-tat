using System.Collections.Generic;

using Newtonsoft.Json;

namespace GhUsersTat.Models
{
    public class GithubUser
    {
        [JsonProperty("login")]
        public string Username { get; set; }
        public string Location { get; set; }
        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }
        [JsonProperty("repos_url")]
        public string ReposUrl { get; set; }
        public List<GithubRepo> TopRepos { get; set; }
    }
}