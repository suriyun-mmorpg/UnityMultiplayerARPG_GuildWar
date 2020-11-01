using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO.GuildWar
{
    public class GuildWarCastleHeart : GuildWarMonsterCharacterEntity
    {

        public override void Killed(IGameEntity lastAttacker)
        {
            base.Killed(lastAttacker);

            // Get winner guild id
            int winnerGuildId = GetGuildId(lastAttacker);

            // Teleport other guild characters to other map (for now, teleport to respawn position)
            List<BasePlayerCharacterEntity> otherGuildCharacters = new List<BasePlayerCharacterEntity>(CurrentGameManager.GetPlayerCharacters());
            for (int i = 0; i < otherGuildCharacters.Count; ++i)
            {
                if (otherGuildCharacters[i].GuildId <= 0 ||
                    otherGuildCharacters[i].GuildId != winnerGuildId)
                {
                    CurrentGameManager.WarpCharacter(WarpPortalType.Default,
                        otherGuildCharacters[i],
                        otherGuildCharacters[i].RespawnMapName,
                        otherGuildCharacters[i].RespawnPosition,
                        false, Vector3.zero);
                }
            }

            BaseGameNetworkManager.Singleton.DefenderGuildId = winnerGuildId;
        }
    }
}