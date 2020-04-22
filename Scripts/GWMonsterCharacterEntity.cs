using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO.GuildWar
{
    public class GWMonsterCharacterEntity : MonsterCharacterEntity
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

        public override void ReceiveDamage(IGameEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> damageAmounts, BaseSkill skill, short skillLevel)
        {
            if (!IsServer || IsDead() || GetGuildId(attacker) == 0 || GetGuildId(attacker) == CurrentGameManager.DefenderGuildId || !CanReceiveDamageFrom(attacker))
                return;

            base.ReceiveDamage(attacker, weapon, damageAmounts, skill, skillLevel);
        }
    }
}
