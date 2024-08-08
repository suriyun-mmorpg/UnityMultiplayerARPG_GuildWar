using System.Collections.Generic;
using System.Threading.Tasks;
using UnityRestClient;

namespace MultiplayerARPG.GuildWar
{
    public class GuildWarRestClient : RestClient
    {
        public string apiUrl;
        public string secretKey;

        public Task<Result<OccupyListResponse>> GetOccupyHistoryList(int guildId, int limit = 20, int page = 1)
        {
            Dictionary<string, object> queries = new Dictionary<string, object>();
            queries[nameof(limit)] = limit;
            queries[nameof(page)] = page;
            return Get<OccupyListResponse>(GetUrl(apiUrl, $"/occupy-history/{guildId}"), queries);
        }

        public Task<Result<OccupyListResponse>> GetOccupyHistoryList(int guildId, string mapName, int limit = 20, int page = 1)
        {
            Dictionary<string, object> queries = new Dictionary<string, object>();
            queries[nameof(limit)] = limit;
            queries[nameof(page)] = page;
            return Get<OccupyListResponse>(GetUrl(apiUrl, $"/occupy-history/{guildId}/{mapName}"), queries);
        }

        public Task<Result<OccupyListResponse>> GetOccupyHistoryList(string mapName, int limit = 20, int page = 1)
        {
            Dictionary<string, object> queries = new Dictionary<string, object>();
            queries[nameof(limit)] = limit;
            queries[nameof(page)] = page;
            return Get<OccupyListResponse>(GetUrl(apiUrl, $"/occupy-history-by-map/{mapName}"), queries);
        }

        public Task<Result<OccupyData>> GetOccupy(string mapName)
        {
            return Get<OccupyData>(GetUrl(apiUrl, $"/{mapName}"));
        }

        public Task<Result> CreateOccupy(string mapName, int guildId, string guildName, string guildOptions, bool attackerWin)
        {
            Dictionary<string, object> form = new Dictionary<string, object>
            {
                { nameof(mapName), mapName },
                { nameof(guildId), guildId },
                { nameof(guildName), guildName },
                { nameof(guildOptions), guildOptions },
                { nameof(attackerWin), attackerWin }
            };
            return Post(GetUrl(apiUrl, "/internal/occupy"), form, secretKey, ApiKeyAuthHeaderSettings);
        }
    }
}
