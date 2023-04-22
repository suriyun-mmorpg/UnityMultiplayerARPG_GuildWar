using System.Collections.Generic;

namespace MultiplayerARPG.GuildWar
{
    [System.Serializable]
    public class OccupyListResponse
    {
        public List<OccupyData> list = new List<OccupyData>();
        public int limit = 20;
        public int page = 1;
        public int totalPage = 1;
    }
}
