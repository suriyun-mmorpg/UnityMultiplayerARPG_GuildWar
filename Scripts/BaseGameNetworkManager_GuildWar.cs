using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MultiplayerARPG.MMO.GuildWar;

namespace MultiplayerARPG
{
    public abstract partial class BaseGameNetworkManager
    {
        public bool GuildWarStarted { get; protected set; }
        internal int DefenderGuildId;

        [DevExtMethods("OnStartServer")]
        public void OnStartServer_GuildWar()
        {
            CancelInvoke("Update_GuildWar");
            InvokeRepeating("Update_GuildWar", 1, 1);
        }

        [DevExtMethods("Clean")]
        public void Clean_GuildWar()
        {
            CancelInvoke("Update_GuildWar");
        }

        public void Update_GuildWar()
        {
            if (!IsServer || !(CurrentMapInfo is GWMapInfo))
                return;

            GWMapInfo mapInfo = CurrentMapInfo as GWMapInfo;
            if (!GuildWarStarted && mapInfo.IsOn())
            {
                // Announce to players that the guild war started
                GuildWarStarted = true;
            }

            if (GuildWarStarted && !mapInfo.IsOn())
            {
                // Announce to players that the guild war ended
                GuildWarStarted = true;
            }
        }
    }
}
