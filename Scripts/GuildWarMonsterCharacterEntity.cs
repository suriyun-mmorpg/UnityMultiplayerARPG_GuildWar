namespace MultiplayerARPG.MMO.GuildWar
{
    public class GuildWarMonsterCharacterEntity : MonsterCharacterEntity
    {
        public override EntityInfo GetInfo()
        {
            return new EntityInfo()
            {
                type = EntityTypes.GuildWarMonster,
                id = Id,
                dataId = DataId,
                isInSafeArea = IsInSafeArea,
                summonerInfo = Summoner != null ? Summoner.GetInfo() : default,
            };
        }

        public int GetGuildId(EntityInfo entityInfo)
        {
            if (!string.IsNullOrEmpty(entityInfo.id))
            {
                if (entityInfo.type == EntityTypes.Player)
                {
                    return entityInfo.guildId;
                }
                else if (entityInfo.type == EntityTypes.Monster &&
                    entityInfo.summonerInfo != null &&
                    entityInfo.summonerInfo.type == EntityTypes.Player)
                {
                    return entityInfo.summonerInfo.guildId;
                }
            }
            return 0;
        }

        public override bool CanReceiveDamageFrom(EntityInfo instigator)
        {
            if (!base.CanReceiveDamageFrom(instigator))
                return false;
            return CurrentGameManager.GuildWarRunning && GetGuildId(instigator) > 0 && GetGuildId(instigator) != CurrentGameManager.DefenderGuildId;
        }
    }
}
