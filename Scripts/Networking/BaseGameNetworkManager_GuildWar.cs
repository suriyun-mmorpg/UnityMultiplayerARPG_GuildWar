using System.Collections.Generic;
using UnityEngine;
using MultiplayerARPG.MMO.GuildWar;
using LiteNetLibManager;
using LiteNetLib;
using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public partial class BaseGameNetworkManager
    {
        [System.Serializable]
        public struct GuildWarMessageTypes
        {
            public ushort statusMsgType;
            public ushort getClientConfigRequestType;
        }

        [Header("Guild War")]
        public GuildWarMessageTypes guildWarMessageTypes = new GuildWarMessageTypes()
        {
            statusMsgType = 1001,
            getClientConfigRequestType = 1400,
        };
        public bool recoverMonstersWhenRoundEnd = true;
        public string guildWarMailSenderId = "GUILDWAR";
        public string guildWarMailSenderName = "Guild War Manager";
        public string guildWarServiceUrl = "http://localhost:9801";
        public string guildWarServiceUrlForClient = "http://localhost:9801";
        public string guildWarSecretKey = "secret";

        public bool GuildWarRunning { get; private set; }
        public System.DateTime LastOccupyTime { get; private set; }
        public int DefenderGuildId { get; private set; }
        public string DefenderGuildName { get; private set; }
        public string DefenderGuildOptions { get; private set; }

        [DevExtMethods("RegisterMessages")]
        private void RegisterMessages_GuildWar()
        {
            RegisterClientMessage(guildWarMessageTypes.statusMsgType, HandleGuildWarStatusAtClient);
        }

        [DevExtMethods("OnStartServer")]
        private void OnStartServer_GuildWar()
        {
            CancelInvoke(nameof(Update_GuildWar));
            InvokeRepeating(nameof(Update_GuildWar), 1, 1);
        }

        [DevExtMethods("OnPeerConnected")]
        private void OnPeerConnected_GuildWar(long connectionId)
        {
            SendGuildWarStatus(connectionId);
        }

        [DevExtMethods("OnServerOnlineSceneLoaded")]
        private void OnServerOnlineSceneLoaded_GuildWar()
        {
            SendGuildWarStatus();
        }

        [DevExtMethods("Clean")]
        private void Clean_GuildWar()
        {
            CancelInvoke(nameof(Update_GuildWar));
        }

        public void SendGuildWarStatus()
        {
            if (!IsServer)
                return;
            foreach (long connectionId in Server.ConnectionIds)
            {
                SendGuildWarStatus(connectionId);
            }
        }

        public void SendGuildWarStatus(long connectionId)
        {
            if (!IsServer)
                return;
            ServerSendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, guildWarMessageTypes.statusMsgType, (writer) =>
            {
                writer.Put(GuildWarRunning);
                writer.PutPackedInt(DefenderGuildId);
                writer.Put(DefenderGuildName);
                writer.Put(DefenderGuildOptions);
            });
        }

        private void HandleGuildWarStatusAtClient(MessageHandlerData messageHandler)
        {
            if (IsServer)
                return;
            GuildWarRunning = messageHandler.Reader.GetBool();
            DefenderGuildId = messageHandler.Reader.GetPackedInt();
            DefenderGuildName = messageHandler.Reader.GetString();
            DefenderGuildOptions = messageHandler.Reader.GetString();
        }

        public void Update_GuildWar()
        {
            if (!IsServer || CurrentMapInfo == null || !(CurrentMapInfo is GuildWarMapInfo))
                return;

            GuildWarMapInfo mapInfo = CurrentMapInfo as GuildWarMapInfo;
            if (!GuildWarRunning && mapInfo.IsOn)
            {
                ServerSendSystemAnnounce(mapInfo.eventStartedMessage);
                DefenderGuildId = 0;
                DefenderGuildName = string.Empty;
                DefenderGuildOptions = string.Empty;
                ExpelLoserGuilds(DefenderGuildId);
                RegenerateMonsters();
                GuildWarRunning = true;
                SendGuildWarStatus();
            }

            if (GuildWarRunning && !mapInfo.IsOn)
            {
                ServerSendSystemAnnounce(mapInfo.eventEndedMessage);
                GuildWarRunning = false;
                if (DefenderGuildId > 0)
                {
                    ServerSendSystemAnnounce(string.Format(mapInfo.defenderWinMessage, DefenderGuildName));
                    GiveGuildBattleRewardTo(DefenderGuildId);
                    ExpelLoserGuilds(DefenderGuildId);
                    RegenerateMonsters();
                }
                SendGuildWarStatus();
            }

            if (GuildWarRunning)
            {
                if ((System.DateTime.Now - LastOccupyTime).TotalMinutes >= mapInfo.battleDuration)
                {
                    ServerSendSystemAnnounce(mapInfo.roundEndedMessage);
                    LastOccupyTime = System.DateTime.Now;
                    if (DefenderGuildId > 0)
                    {
                        ServerSendSystemAnnounce(string.Format(mapInfo.defenderWinMessage, DefenderGuildName));
                        GiveGuildBattleRewardTo(DefenderGuildId);
                        ExpelLoserGuilds(DefenderGuildId);
                        RegenerateMonsters();
                    }
                    SendGuildWarStatus();
                }
            }
        }

        public void CastleOccupied(int attackerGuildId)
        {
            GuildWarMapInfo mapInfo = CurrentMapInfo as GuildWarMapInfo;
            LastOccupyTime = System.DateTime.Now;
            if (attackerGuildId > 0 && ServerGuildHandlers.TryGetGuild(attackerGuildId, out GuildData guild))
            {
                DefenderGuildId = attackerGuildId;
                DefenderGuildName = guild.guildName;
                DefenderGuildOptions = guild.options;
                ServerSendSystemAnnounce(string.Format(mapInfo.attackerWinMessage, DefenderGuildName));
                GiveGuildBattleRewardTo(DefenderGuildId);
                ExpelLoserGuilds(DefenderGuildId);
                RegenerateMonsters();
            }
            SendGuildWarStatus();
        }

        private void ExpelLoserGuilds(int winnerGuildId)
        {
            // Teleport other guild characters to other map (for now, teleport to respawn position)
            List<IPlayerCharacterData> otherGuildCharacters = new List<IPlayerCharacterData>(ServerUserHandlers.GetPlayerCharacters());
            for (int i = 0; i < otherGuildCharacters.Count; ++i)
            {
                if (otherGuildCharacters[i].GuildId <= 0 ||
                    otherGuildCharacters[i].GuildId != winnerGuildId)
                {
                    WarpCharacter(WarpPortalType.Default,
                        otherGuildCharacters[i] as BasePlayerCharacterEntity,
                        otherGuildCharacters[i].RespawnMapName,
                        otherGuildCharacters[i].RespawnPosition,
                        false, Vector3.zero);
                }
            }
        }

        private void RegenerateMonsters()
        {
            if (!recoverMonstersWhenRoundEnd)
                return;
            foreach (LiteNetLibIdentity identity in Assets.GetSpawnedObjects())
            {
                GuildWarMonsterCharacterEntity monsterEntity = identity.GetComponent<GuildWarMonsterCharacterEntity>();
                if (monsterEntity == null)
                    continue;
                monsterEntity.CurrentHp = monsterEntity.MaxHp;
                monsterEntity.CurrentMp = monsterEntity.MaxMp;
                monsterEntity.CurrentFood = monsterEntity.MaxFood;
                monsterEntity.CurrentWater = monsterEntity.MaxWater;
                monsterEntity.CurrentStamina = monsterEntity.MaxStamina;
            }
        }

        private void GiveGuildBattleRewardTo(int winnerGuildId)
        {
            GuildWarMapInfo mapInfo = CurrentMapInfo as GuildWarMapInfo;
            string tempMailTitle;
            string tempMailContent;
            int tempRewardGold;
            CurrencyAmount[] tempRewardCurrencies;
            ItemAmount[] tempRewardItems;
            Mail tempMail;

            foreach (IPlayerCharacterData participant in ServerUserHandlers.GetPlayerCharacters())
            {
                if (participant.GuildId == winnerGuildId)
                {
                    tempMailTitle = mapInfo.winMailTitle;
                    tempMailContent = mapInfo.winMailContent;
                    tempRewardGold = mapInfo.winRewardGold;
                    tempRewardCurrencies = mapInfo.winRewardCurrencies;
                    tempRewardItems = mapInfo.winRewardItems;
                }
                else
                {
                    tempMailTitle = mapInfo.participantMailTitle;
                    tempMailContent = mapInfo.participantMailContent;
                    tempRewardGold = mapInfo.participantRewardGold;
                    tempRewardCurrencies = mapInfo.participantRewardCurrencies;
                    tempRewardItems = mapInfo.participantRewardItems;
                }
                tempMail = new Mail()
                {
                    SenderId = guildWarMailSenderId,
                    SenderName = guildWarMailSenderName,
                    ReceiverId = participant.UserId,
                    Title = tempMailTitle,
                    Content = tempMailContent,
                    Gold = tempRewardGold,
                };
                foreach (CurrencyAmount currencyAmount in tempRewardCurrencies)
                {
                    if (currencyAmount.currency == null) continue;
                    tempMail.Currencies.Add(CharacterCurrency.Create(currencyAmount.currency.DataId, currencyAmount.amount));
                }
                foreach (ItemAmount itemAmount in tempRewardItems)
                {
                    if (itemAmount.item == null) continue;
                    tempMail.Items.Add(CharacterItem.Create(itemAmount.item, 1, itemAmount.amount));
                }
                ServerMailHandlers.SendMail(tempMail);
            }
        }
    }
}
