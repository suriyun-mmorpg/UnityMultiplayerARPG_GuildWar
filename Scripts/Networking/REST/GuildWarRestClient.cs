using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Serialization;
using UnityRestClient;

namespace MultiplayerARPG.GuildWar
{
    public class GuildWarRestClient : RestClient
    {
        [FormerlySerializedAs("url")]
        public string apiUrl;
        [FormerlySerializedAs("accessToken")]
        public string secretKey;

        public Task<Result<OccupyListResponse>> GetOccupyHistoryList(int guildId, int limit = 20, int page = 1)
        {
            Dictionary<string, object> queries = new Dictionary<string, object>();
            queries[nameof(limit)] = limit;
            queries[nameof(page)] = page;
            return Get<OccupyListResponse>(GetUrl(apiUrl, $"/occupy-history/{guildId}"), queries, secretKey);
        }

        public Task<Result<OccupyListResponse>> GetOccupyHistoryList(int guildId, string mapName, int limit = 20, int page = 1)
        {
            Dictionary<string, object> queries = new Dictionary<string, object>();
            queries[nameof(limit)] = limit;
            queries[nameof(page)] = page;
            return Get<OccupyListResponse>(GetUrl(apiUrl, $"/occupy-history/{guildId}/{mapName}"), queries, secretKey);
        }

        public Task<Result<OccupyListResponse>> GetOccupyHistoryList(string mapName, int limit = 20, int page = 1)
        {
            Dictionary<string, object> queries = new Dictionary<string, object>();
            queries[nameof(limit)] = limit;
            queries[nameof(page)] = page;
            return Get<OccupyListResponse>(GetUrl(apiUrl, $"/occupy-history-by-map/{mapName}"), queries, secretKey);
        }

        public Task<Result<OccupyData>> GetOccupy(string mapName)
        {
            Dictionary<string, object> queries = new Dictionary<string, object>();
            return Get<OccupyData>(GetUrl(apiUrl, $"/{mapName}"), queries, secretKey);
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
            return Post(GetUrl(apiUrl, "/internal/occupy"), form, secretKey);
        }
    }
}
