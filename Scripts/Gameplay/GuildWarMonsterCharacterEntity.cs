namespace MultiplayerARPG.GuildWar
{
    public class GuildWarMonsterCharacterEntity : MonsterCharacterEntity
    {
        public override EntityInfo GetInfo()
        {
            return new EntityInfo(
                EntityTypes.GuildWarMonster,
                ObjectId,
                ObjectId.ToString(),
                DataId,
                0, 0, 0,
                IsInSafeArea,
                Summoner);
        }

        public int GetGuildId(EntityInfo entityInfo)
        {
            if (!string.IsNullOrEmpty(entityInfo.Id))
            {
                if (entityInfo.Type == EntityTypes.Player)
                {
                    return entityInfo.GuildId;
                }
                else if (entityInfo.Type == EntityTypes.Monster &&
                    entityInfo.HasSummoner &&
                    entityInfo.SummonerType == EntityTypes.Player)
                {
                    return entityInfo.SummonerGuildId;
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
