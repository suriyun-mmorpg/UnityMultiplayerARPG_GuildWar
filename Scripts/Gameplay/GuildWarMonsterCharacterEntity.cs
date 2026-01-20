using Cysharp.Text;

namespace MultiplayerARPG.GuildWar
{
    public class GuildWarMonsterCharacterEntity : MonsterCharacterEntity
    {
        public override EntityInfo GetInfo()
        {
            string id;
            using (Utf16ValueStringBuilder strBuilder = ZString.CreateStringBuilder(true))
            {
                strBuilder.Append(ObjectId);
                id = strBuilder.ToString();
            }
            return new EntityInfo(
                EntityTypes.GuildWarMonster,
                ObjectId,
                id,
                SubChannelId,
                DataId,
                FactionId,
                0 /* Party ID */,
                0 /* Guild ID */,
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
