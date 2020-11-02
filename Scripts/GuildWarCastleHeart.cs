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
            int attackerGuildId = GetGuildId(lastAttacker);
            BaseGameNetworkManager.Singleton.CastleOccupied(attackerGuildId);
        }
    }
}