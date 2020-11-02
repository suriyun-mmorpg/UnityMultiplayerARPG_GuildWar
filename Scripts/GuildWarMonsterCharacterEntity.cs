using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO.GuildWar
{
    public class GuildWarMonsterCharacterEntity : MonsterCharacterEntity
    {
        public int GetGuildId(IGameEntity entity)
        {
            int guildId = 0;
            if (entity != null)
            {
                BasePlayerCharacterEntity playerCharacter = entity.Entity as BasePlayerCharacterEntity;
                BaseMonsterCharacterEntity monsterCharacter = entity.Entity as BaseMonsterCharacterEntity;
                if (monsterCharacter != null && monsterCharacter.IsSummoned &&
                    monsterCharacter.Summoner is BasePlayerCharacterEntity)
                    playerCharacter = monsterCharacter.Summoner as BasePlayerCharacterEntity;
                if (playerCharacter != null)
                    guildId = (entity.Entity as BasePlayerCharacterEntity).GuildId;
            }
            return guildId;
        }

        public override bool CanReceiveDamageFrom(IGameEntity attacker)
        {
            if (!base.CanReceiveDamageFrom(attacker))
                return false;
            return CurrentGameManager.GuildWarRunning && GetGuildId(attacker) > 0 && GetGuildId(attacker) != CurrentGameManager.DefenderGuildId;
        }
    }
}
