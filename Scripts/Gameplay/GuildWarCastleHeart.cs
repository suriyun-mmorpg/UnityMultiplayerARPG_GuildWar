namespace MultiplayerARPG.GuildWar
{
    public class GuildWarCastleHeart : GuildWarMonsterCharacterEntity
    {
        public override void Killed(EntityInfo lastAttacker)
        {
            base.Killed(lastAttacker);

            // Get winner guild id
            int attackerGuildId = GetGuildId(lastAttacker);
            BaseGameNetworkManager.Singleton.CastleOccupied(attackerGuildId);
        }
    }
}