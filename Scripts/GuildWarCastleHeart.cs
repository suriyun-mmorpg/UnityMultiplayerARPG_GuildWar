namespace MultiplayerARPG.MMO.GuildWar
{
    public class GuildWarCastleHeart : GuildWarMonsterCharacterEntity
    {
        public override void Killed(IGameEntity lastAttacker)
        {
            base.Killed(lastAttacker);

            // Get winner guild id
            int attackerGuildId = GetGuildId(lastAttacker.GetInfo());
            BaseGameNetworkManager.Singleton.CastleOccupied(attackerGuildId);
        }
    }
}