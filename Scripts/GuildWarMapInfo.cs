using System;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO.GuildWar
{
    [CreateAssetMenu(fileName = "Guild War Map Info", menuName = "Create GameData/Guild War Map Info", order = -4799)]
    public class GuildWarMapInfo : BaseMapInfo
    {
        [Serializable]
        public struct EventTime
        {
            public bool isOn;
            [Range(0, 23)]
            public byte startTime;
            [Range(1, 24)]
            public byte endTime;
        }

        [Header("Event time settings")]
        public EventTime sunday;
        public EventTime monday;
        public EventTime tuesday;
        public EventTime wednesday;
        public EventTime thursday;
        public EventTime friday;
        public EventTime saturday;
        [Tooltip("Battle duration (minutes), if defender can defend castle within this duration, defender will win that round.")]
        [Min(1)]
        public int battleDuration = 15;

        [Header("Announce messages")]
        public string eventStartedMessage = "Guild war started !!";
        public string eventEndedMessage = "Guild war ended !!";
        public string roundEndedMessage = "Current guild war round ended !!";
        [Tooltip("{0} is guild name")]
        public string defenderWinMessage = "{0} can defend the castle and win this round.";
        [Tooltip("{0} is guild name")]
        public string attackerWinMessage = "{0} can occupy the castle and win this round.";

        public override bool AutoRespawnWhenDead { get { return true; } }
        public override bool SaveCurrentMapPosition { get { return false; } }

        public bool IsOn
        {
            get
            {
                EventTime eventTime;
                switch (DateTime.Now.DayOfWeek)
                {
                    case DayOfWeek.Sunday:
                        eventTime = sunday;
                        break;
                    case DayOfWeek.Monday:
                        eventTime = monday;
                        break;
                    case DayOfWeek.Tuesday:
                        eventTime = tuesday;
                        break;
                    case DayOfWeek.Wednesday:
                        eventTime = wednesday;
                        break;
                    case DayOfWeek.Thursday:
                        eventTime = thursday;
                        break;
                    case DayOfWeek.Friday:
                        eventTime = friday;
                        break;
                    case DayOfWeek.Saturday:
                        eventTime = saturday;
                        break;
                    default:
                        eventTime = sunday;
                        break;
                }
                return eventTime.isOn && DateTime.Now.Hour >= eventTime.startTime && DateTime.Now.Hour < eventTime.endTime;
            }
        }

        protected override bool IsPlayerAlly(BasePlayerCharacterEntity playerCharacter, EntityInfo targetEntity)
        {
            if (string.IsNullOrEmpty(targetEntity.id))
                return false;

            if (targetEntity.type == EntityTypes.Player)
            {
                return targetEntity.guildId != 0 && targetEntity.guildId == playerCharacter.GuildId;
            }

            if (targetEntity.type == EntityTypes.GuildWarMonster)
            {
                return BaseGameNetworkManager.Singleton.DefenderGuildId != 0 && BaseGameNetworkManager.Singleton.DefenderGuildId == playerCharacter.GuildId;
            }

            if (targetEntity.type == EntityTypes.Monster)
            {
                // If this character is summoner so it is ally
                if (targetEntity.summonerInfo != null)
                {
                    // If summoned by someone, will have same allies with summoner
                    return playerCharacter.IsAlly(targetEntity.summonerInfo);
                }
                else
                {
                    // Monster always not player's ally
                    return false;
                }
            }

            return false;
        }

        protected override bool IsMonsterAlly(BaseMonsterCharacterEntity monsterCharacter, EntityInfo targetEntity)
        {
            if (string.IsNullOrEmpty(targetEntity.id))
                return false;

            if (monsterCharacter is GuildWarMonsterCharacterEntity)
            {
                if (targetEntity.type == EntityTypes.Player)
                {
                    return BaseGameNetworkManager.Singleton.DefenderGuildId != 0 && BaseGameNetworkManager.Singleton.DefenderGuildId == targetEntity.guildId;
                }

                if (targetEntity.type == EntityTypes.Monster)
                {
                    // If this character is summoner so it is ally
                    if (targetEntity.summonerInfo != null)
                    {
                        // If summoned by someone, will have same allies with summoner
                        return monsterCharacter.IsAlly(targetEntity.summonerInfo);
                    }
                    else
                    {
                        // Monster always not player's ally
                        return false;
                    }
                }

                return true;
            }

            if (monsterCharacter.IsSummoned)
            {
                // If summoned by someone, will have same allies with summoner
                return targetEntity.id.Equals(monsterCharacter.Summoner.Id) || monsterCharacter.Summoner.IsAlly(targetEntity);
            }

            if (targetEntity.type == EntityTypes.GuildWarMonster)
            {
                // Monsters won't attack castle heart
                return true;
            }

            if (targetEntity.type == EntityTypes.Monster)
            {
                // If another monster has same allyId so it is ally
                if (targetEntity.summonerInfo != null)
                    return monsterCharacter.IsAlly(targetEntity.summonerInfo);
                return GameInstance.MonsterCharacters[targetEntity.dataId].AllyId == monsterCharacter.CharacterDatabase.AllyId;
            }

            return false;
        }

        protected override bool IsPlayerEnemy(BasePlayerCharacterEntity playerCharacter, EntityInfo targetEntity)
        {
            if (string.IsNullOrEmpty(targetEntity.id))
                return false;

            if (targetEntity.type == EntityTypes.Player)
            {
                return targetEntity.guildId == 0 || targetEntity.guildId != playerCharacter.GuildId;
            }

            if (targetEntity.type == EntityTypes.GuildWarMonster)
            {
                return BaseGameNetworkManager.Singleton.DefenderGuildId == 0 || BaseGameNetworkManager.Singleton.DefenderGuildId != playerCharacter.GuildId;
            }

            if (targetEntity.type == EntityTypes.Monster)
            {
                // If this character is not summoner so it is enemy
                if (targetEntity.summonerInfo != null)
                {
                    // If summoned by someone, will have same enemies with summoner
                    return playerCharacter.IsEnemy(targetEntity.summonerInfo);
                }
                else
                {
                    // Monster always be player's enemy
                    return true;
                }
            }

            return false;
        }

        protected override bool IsMonsterEnemy(BaseMonsterCharacterEntity monsterCharacter, EntityInfo targetEntity)
        {
            if (string.IsNullOrEmpty(targetEntity.id))
                return false;

            if (monsterCharacter is GuildWarMonsterCharacterEntity)
            {
                if (targetEntity.type == EntityTypes.Player)
                {
                    return BaseGameNetworkManager.Singleton.DefenderGuildId == 0 || BaseGameNetworkManager.Singleton.DefenderGuildId != targetEntity.guildId;
                }

                if (targetEntity.type == EntityTypes.Monster)
                {
                    // If this character is not summoner so it is enemy
                    if (targetEntity.summonerInfo != null)
                    {
                        // If summoned by someone, will have same enemies with summoner
                        return monsterCharacter.IsEnemy(targetEntity.summonerInfo);
                    }
                    else
                    {
                        // Monster always be player's enemy
                        return true;
                    }
                }

                return false;
            }

            if (monsterCharacter.IsSummoned)
            {
                // If summoned by someone, will have same enemies with summoner
                return targetEntity.id.Equals(monsterCharacter.Summoner.Id) && monsterCharacter.Summoner.IsEnemy(targetEntity);
            }

            // Attack only player by default
            return targetEntity.type == EntityTypes.Player;
        }
    }
}
