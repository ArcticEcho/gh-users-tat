using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using GhUsersTat.Helpers;
using GhUsersTat.Models;

namespace GhUsersTat.Services
{
    public interface IGithubQueryService
    {
        Task<GithubUser> GetUser(string username);
    }

    public class GithubQueryService : IGithubQueryService
    {
        private const string _githubApiBaseUrlConfig = "githubApiBaseUrl";
        private const string _githubApiKeyConfig = "githubApiKey";
        private readonly HttpClient _http;

        public GithubQueryService(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient();

            var githubApiBaseUrl = ConfigurationManager.AppSettings[_githubApiBaseUrlConfig];
            var githubApiKey = ConfigurationManager.AppSettings[_githubApiKeyConfig];

            if (string.IsNullOrEmpty(githubApiBaseUrl) || string.IsNullOrEmpty(githubApiKey))
            {
                throw new Exception(
                    $"Failed to initialise {nameof(GithubQueryService)}. " +
                    $"Please ensure app settings '{_githubApiBaseUrlConfig}' " +
                    $"and '{_githubApiKeyConfig}' are set.");
            }

            _http.BaseAddress = new Uri(githubApiBaseUrl);
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", githubApiKey);
            _http.DefaultRequestHeaders.Add("User-Agent", "Github-Users-Tat-App");
        }

        public async Task<GithubUser> GetUser(string username)
        {
            var response = await _http.GetAsync($"users/{username}");

            if (!response.IsSuccessStatusCode)
            {
                // todo logging
                return null;
            }

            var user = await response.ReadAsJsonAsync<GithubUser>();

            if (user == null)
            {
                // todo logging
                return null;
            }

            user.TopRepos = await GetTopUserRepos(user.ReposUrl);

            if (user.TopRepos == null)
            {
                return null;
            }

            return user;
        }

        private async Task<List<GithubRepo>> GetTopUserRepos(string reposUrl)
        {
            var path = new Uri(reposUrl).AbsolutePath;
            var response = await _http.GetAsync(path);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var repos = await response.ReadAsJsonAsync<GithubRepo[]>();

            if (repos == null)
            {
                // todo logging
                return null;
            }

            return repos
                .OrderByDescending(x => x.StarGazers)
                .Take(5)
                .ToList();
        }
    }
}