using Cysharp.Text;

namespace MultiplayerARPG.GuildWar
{
    public class GuildWarMonsterCharacterEntity : MonsterCharacterEntity
    {
        public override string GetId()
        {
            using (Utf16ValueStringBuilder strBuilder = ZString.CreateStringBuilder(true))
            {
                strBuilder.Append(EntityTypes.GuildWarMonster);
                strBuilder.Append('_');
                strBuilder.Append(ObjectId);
                return strBuilder.ToString();
            }
        }

        public override EntityInfo GetInfo()
        {
            return _info.SetEntityInfo(
                EntityTypes.GuildWarMonster,
                ObjectId,
                Id,
                SubChannelId,
                DataId,
                FactionId,
                0 /* Party ID */,
                0 /* Guild ID */,
                IsInSafeArea,
                this,
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
                    entityInfo.Summoner.Type == EntityTypes.Player)
                {
                    return entityInfo.Summoner.GuildId;
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
