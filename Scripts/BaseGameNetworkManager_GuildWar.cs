using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MultiplayerARPG.MMO.GuildWar;

namespace MultiplayerARPG
{
    public abstract partial class BaseGameNetworkManager
    {
        public bool GuildWarStarted { get; private set; }
        public System.DateTime LastOccupyTime { get; private set; }
        public int DefenderGuildId { get; private set; }

        [DevExtMethods("OnStartServer")]
        public void OnStartServer_GuildWar()
        {
            CancelInvoke(nameof(Update_GuildWar));
            InvokeRepeating(nameof(Update_GuildWar), 1, 1);
        }

        [DevExtMethods("Clean")]
        public void Clean_GuildWar()
        {
            CancelInvoke(nameof(Update_GuildWar));
        }

        public void Update_GuildWar()
        {
            if (!IsServer || CurrentMapInfo == null || !(CurrentMapInfo is GuildWarMapInfo))
                return;

            GuildWarMapInfo mapInfo = CurrentMapInfo as GuildWarMapInfo;
            if (!GuildWarStarted && mapInfo.IsOn)
            {
                // TODO: Announce to players that the guild war started
                GuildWarStarted = true;
            }

            if (GuildWarStarted && !mapInfo.IsOn)
            {
                // TODO: Announce to players that the guild war ended
                GuildWarStarted = false;
                GiveGuildBattleRewardTo(DefenderGuildId);
                ExpelLoserGuilds(DefenderGuildId);
            }

            if (GuildWarStarted)
            {
                if ((System.DateTime.Now - LastOccupyTime).TotalMinutes >= mapInfo.battleDuration)
                {
                    // TODO: Announce to players that defender guild win
                    LastOccupyTime = System.DateTime.Now;
                    GiveGuildBattleRewardTo(DefenderGuildId);
                    ExpelLoserGuilds(DefenderGuildId);
                }
            }
        }

        public void CastleOccupied(int attackerGuildId)
        {
            // TODO: Announce to players that attacker guild win
            LastOccupyTime = System.DateTime.Now;
            DefenderGuildId = attackerGuildId;
            GiveGuildBattleRewardTo(DefenderGuildId);
            ExpelLoserGuilds(DefenderGuildId);
        }

        private void ExpelLoserGuilds(int winnerGuildId)
        {
            // Teleport other guild characters to other map (for now, teleport to respawn position)
            List<BasePlayerCharacterEntity> otherGuildCharacters = new List<BasePlayerCharacterEntity>(GetPlayerCharacters());
            for (int i = 0; i < otherGuildCharacters.Count; ++i)
            {
                if (otherGuildCharacters[i].GuildId <= 0 ||
                    otherGuildCharacters[i].GuildId != winnerGuildId)
                {
                    WarpCharacter(WarpPortalType.Default,
                        otherGuildCharacters[i],
                        otherGuildCharacters[i].RespawnMapName,
                        otherGuildCharacters[i].RespawnPosition,
                        false, Vector3.zero);
                }
            }
        }

        private void GiveGuildBattleRewardTo(int guildId)
        {

        }
    }
}
