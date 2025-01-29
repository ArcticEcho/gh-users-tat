using System;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace GhUsersTat.Helpers
{
    public static class HttpHelpers
    {
        public static async Task<T> ReadAsJsonAsync<T>(this HttpResponseMessage response)
        {
            try
            {
                var json = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}