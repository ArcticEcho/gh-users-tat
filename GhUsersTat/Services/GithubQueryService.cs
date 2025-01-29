using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using GhUsersTat.Helpers;
using GhUsersTat.Models;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace GhUsersTat.Services
{
    public interface IGithubQueryService
    {
        Task<GithubUser> GetUserAsync(string username);
    }

    public class GithubQueryService : IGithubQueryService
    {
        private const string _githubApiBaseUrlConfig = "githubApiBaseUrl";
        private const string _githubApiKeyConfig = "githubApiKey";
        private readonly TimeSpan _userCacheLifetime = TimeSpan.FromHours(1);
        private readonly int _maxReposToReturn;
        private readonly HttpClient _http;
        private readonly ILogger<GithubQueryService> _logger;
        private readonly IMemoryCache _cache;

        public GithubQueryService(
            HttpClient httpClient,
            ILogger<GithubQueryService> logger,
            IMemoryCache cache)
        {
            _http = httpClient;
            _logger = logger;
            _cache = cache;

            var githubApiBaseUrl = ConfigurationManager.AppSettings[_githubApiBaseUrlConfig];
            var githubApiKey = ConfigurationManager.AppSettings[_githubApiKeyConfig];

            if (string.IsNullOrEmpty(githubApiBaseUrl) || string.IsNullOrEmpty(githubApiKey))
            {
                throw new Exception(
                    $"Failed to initialise {nameof(GithubQueryService)}. " +
                    $"Please ensure app settings '{_githubApiBaseUrlConfig}' " +
                    $"and '{_githubApiKeyConfig}' are set.");
            }

            var maxRepos = ConfigurationManager.AppSettings["maxUserReposToReturn"];

            if (!int.TryParse(maxRepos, out _maxReposToReturn))
            {
                _maxReposToReturn = 5;
            }

            _http.BaseAddress = new Uri(githubApiBaseUrl);
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", githubApiKey);
            _http.DefaultRequestHeaders.Add("User-Agent", "Github-Users-Tat-App");
        }

        public async Task<GithubUser> GetUserAsync(string username)
        {
            var cacheKey = $"GitHub User {username}";

            if (!_cache.TryGetValue<GithubUser>(cacheKey, out var user))
            {
                user = await FetchUserAsync(username);

                if (user == null)
                {
                    return null;
                }

                _cache.Set(cacheKey, user, _userCacheLifetime);
            }
            else
            {
                _logger.LogInformation("Returning cached github user data for user {username}", username);
            }

            return user;
        }

        private async Task<GithubUser> FetchUserAsync(string username)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogInformation("Fetching github user data for user {username}", username);

            var response = await _http.GetAsync($"users/{username}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Failed to fetch github user data for user {username}. " +
                    "Error code {code}.",
                    username,
                    response.StatusCode);

                return null;
            }

            var user = await response.ReadAsJsonAsync<GithubUser>();

            if (user == null)
            {
                _logger.LogError(
                    "Failed to parse github user data for user {username}.",
                    username);

                return null;
            }

            user.TopRepos = await FetchTopUserReposAsync(user.ReposUrl);

            if (user.TopRepos == null)
            {
                return null;
            }

            _logger.LogInformation(
                "Fetched github user data for user {username} in {ms}ms.",
                username,
                sw.ElapsedMilliseconds);

            return user;
        }

        private async Task<List<GithubRepo>> FetchTopUserReposAsync(string reposUrl)
        {
            var path = new Uri(reposUrl).AbsolutePath;
            var response = await _http.GetAsync(path);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Failed to fetch github repo data for given path {path}. " +
                    "Error code {code}.",
                    path,
                    response.StatusCode);

                return null;
            }

            var repos = await response.ReadAsJsonAsync<GithubRepo[]>();

            if (repos == null)
            {
                _logger.LogError(
                    "Failed to parse github repo data for given path {path}.",
                    path);

                return null;
            }

            return repos
                .OrderByDescending(x => x.StarGazers)
                .Take(_maxReposToReturn)
                .ToList();
        }
    }
}