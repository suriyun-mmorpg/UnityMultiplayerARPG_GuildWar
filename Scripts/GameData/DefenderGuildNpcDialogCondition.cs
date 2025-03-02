using Cysharp.Threading.Tasks;
using Insthync.UnityRestClient;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.GuildWar
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.DEFENDER_GUILD_NPC_DIALOG_CONDITION_FILE, menuName = GameDataMenuConsts.DEFENDER_GUILD_NPC_DIALOG_CONDITION_MENU, order = GameDataMenuConsts.DEFENDER_GUILD_NPC_DIALOG_CONDITION_ORDER)]
    public class DefenderGuildNpcDialogCondition : BaseCustomNpcDialogCondition
    {
        [SerializeField]
        private GuildWarMapInfo mapInfo;

        [SerializeField]
        [Tooltip("Leave it empty to allow all guild member to access menu")]
        private List<byte> availableRoles = new List<byte>();

        public GuildWarRestClient RestClient
        {
            get
            {
                if (BaseGameNetworkManager.Singleton.IsServer)
                    return BaseGameNetworkManager.Singleton.GuildWarRestClientForServer;
                else
                    return BaseGameNetworkManager.Singleton.GuildWarRestClientForClient;
            }
        }

        public override async UniTask<bool> IsPass(IPlayerCharacterData playerCharacterEntity)
        {
            RestClient.Result<OccupyData> getOccupyResult = await RestClient.GetOccupy(mapInfo.Id);
            if (getOccupyResult.IsError())
            {
                return false;
            }
            if (getOccupyResult.Content.guildId != playerCharacterEntity.GuildId)
            {
                return false;
            }
            if (availableRoles != null && availableRoles.Count > 0 && !availableRoles.Contains(playerCharacterEntity.GuildRole))
            {
                return false;
            }
            return true;
        }
    }
}