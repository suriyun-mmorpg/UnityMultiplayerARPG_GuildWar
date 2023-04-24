using System;
using UnityEngine;

namespace MultiplayerARPG.GuildWar
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.GUILD_WAR_MAP_INFO_FILE, menuName = GameDataMenuConsts.GUILD_WAR_MAP_INFO_MENU, order = GameDataMenuConsts.GUILD_WAR_MAP_INFO_ORDER)]
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

        [Category("Guild War Settings")]
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

        [Header("Rewarding")]
        public string participantMailTitle;
        public string participantMailContent;
        public int participantRewardGold;
        public CurrencyAmount[] participantRewardCurrencies;
        public ItemAmount[] participantRewardItems;
        public string winMailTitle;
        public string winMailContent;
        public int winRewardGold;
        public CurrencyAmount[] winRewardCurrencies;
        public ItemAmount[] winRewardItems;

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

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddItems(participantRewardItems);
            GameInstance.AddItems(winRewardItems);
            GameInstance.AddCurrencies(participantRewardCurrencies);
            GameInstance.AddCurrencies(winRewardCurrencies);
        }

        protected override bool IsPlayerAlly(BasePlayerCharacterEntity playerCharacter, EntityInfo targetEntity)
        {
            if (string.IsNullOrEmpty(targetEntity.Id))
                return false;

            if (targetEntity.Type == EntityTypes.Player)
            {
                return targetEntity.GuildId != 0 && targetEntity.GuildId == playerCharacter.GuildId;
            }

            if (targetEntity.Type == EntityTypes.GuildWarMonster)
            {
                return BaseGameNetworkManager.Singleton.DefenderGuildId != 0 && BaseGameNetworkManager.Singleton.DefenderGuildId == playerCharacter.GuildId;
            }

            if (targetEntity.Type == EntityTypes.Monster)
            {
                // If this character is summoner so it is ally
                if (targetEntity.HasSummoner)
                {
                    // If summoned by someone, will have same allies with summoner
                    return playerCharacter.IsAlly(targetEntity.Summoner);
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
            if (string.IsNullOrEmpty(targetEntity.Id))
                return false;

            if (monsterCharacter is GuildWarMonsterCharacterEntity)
            {
                if (targetEntity.Type == EntityTypes.Player)
                {
                    return BaseGameNetworkManager.Singleton.DefenderGuildId != 0 && BaseGameNetworkManager.Singleton.DefenderGuildId == targetEntity.GuildId;
                }

                if (targetEntity.Type == EntityTypes.Monster)
                {
                    // If this character is summoner so it is ally
                    if (targetEntity.HasSummoner)
                    {
                        // If summoned by someone, will have same allies with summoner
                        return monsterCharacter.IsAlly(targetEntity.Summoner);
                    }
                    else
                    {
                        // Monster always not player's ally
                        return false;
                    }
                }

                return true;
            }

            if (monsterCharacter.IsSummonedAndSummonerExisted)
            {
                // If summoned by someone, will have same allies with summoner
                return targetEntity.Id.Equals(monsterCharacter.Summoner.Id) || monsterCharacter.Summoner.IsAlly(targetEntity);
            }

            if (targetEntity.Type == EntityTypes.GuildWarMonster)
            {
                // Monsters won't attack castle heart
                return true;
            }

            if (targetEntity.Type == EntityTypes.Monster)
            {
                // If another monster has same allyId so it is ally
                if (targetEntity.HasSummoner)
                    return monsterCharacter.IsAlly(targetEntity.Summoner);
                return GameInstance.MonsterCharacters[targetEntity.DataId].AllyId == monsterCharacter.CharacterDatabase.AllyId;
            }

            return false;
        }

        protected override bool IsPlayerEnemy(BasePlayerCharacterEntity playerCharacter, EntityInfo targetEntity)
        {
            if (string.IsNullOrEmpty(targetEntity.Id))
                return false;

            if (targetEntity.Type == EntityTypes.Player)
            {
                return targetEntity.GuildId == 0 || targetEntity.GuildId != playerCharacter.GuildId;
            }

            if (targetEntity.Type == EntityTypes.GuildWarMonster)
            {
                return BaseGameNetworkManager.Singleton.DefenderGuildId == 0 || BaseGameNetworkManager.Singleton.DefenderGuildId != playerCharacter.GuildId;
            }

            if (targetEntity.Type == EntityTypes.Monster)
            {
                // If this character is not summoner so it is enemy
                if (targetEntity.HasSummoner)
                {
                    // If summoned by someone, will have same enemies with summoner
                    return playerCharacter.IsEnemy(targetEntity.Summoner);
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
            if (string.IsNullOrEmpty(targetEntity.Id))
                return false;

            if (monsterCharacter is GuildWarMonsterCharacterEntity)
            {
                if (targetEntity.Type == EntityTypes.Player)
                {
                    return BaseGameNetworkManager.Singleton.DefenderGuildId == 0 || BaseGameNetworkManager.Singleton.DefenderGuildId != targetEntity.GuildId;
                }

                if (targetEntity.Type == EntityTypes.Monster)
                {
                    // If this character is not summoner so it is enemy
                    if (targetEntity.HasSummoner)
                    {
                        // If summoned by someone, will have same enemies with summoner
                        return monsterCharacter.IsEnemy(targetEntity.Summoner);
                    }
                    else
                    {
                        // Monster always be player's enemy
                        return true;
                    }
                }

                return false;
            }

            // If summoned by someone, will have same enemies with summoner
            if (monsterCharacter.IsSummonedAndSummonerExisted)
                return monsterCharacter.Summoner.IsEnemy(targetEntity);

            // Attack only player by default
            if (targetEntity.Type == EntityTypes.Player)
                return true;

            // Attack monster which its summoner is enemy
            if (targetEntity.Type == EntityTypes.Monster && targetEntity.TryGetEntity(out BaseMonsterCharacterEntity targetMonster) && targetMonster.IsSummonedAndSummonerExisted)
                return monsterCharacter.IsEnemy(targetEntity.Summoner);

            return false;
        }
    }
}
