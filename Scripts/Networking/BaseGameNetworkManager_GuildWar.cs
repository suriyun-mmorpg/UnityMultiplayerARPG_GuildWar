using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using LiteNetLib;
using LiteNetLib.Utils;
using MultiplayerARPG.GuildWar;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityRestClient;
using UnityEngine.Serialization;

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

        [System.Serializable]
        public struct GuildWarConfig
        {
            public bool? recoverMonstersWhenGuildWarRoundEnd;
            public string guildWarMailSenderId;
            public string guildWarMailSenderName;
            public string guildWarServiceUrl;
            public string guildWarServiceUrlForClient;
            public string guildWarSecretKey;
        }

        [Header("Guild War")]
        public GuildWarMessageTypes guildWarMessageTypes = new GuildWarMessageTypes()
        {
            statusMsgType = 1001,
            getClientConfigRequestType = 1400,
        };
        [FormerlySerializedAs("recoverMonstersWhenRoundEnd")]
        public bool recoverMonstersWhenGuildWarRoundEnd = true;
        public string guildWarMailSenderId = "GUILDWAR";
        public string guildWarMailSenderName = "Guild War Manager";
        public string guildWarServiceUrl = "http://localhost:9801";
        public string guildWarServiceUrlForClient = "http://localhost:9801";
        public string guildWarSecretKey = "secret";

        private GuildWarRestClient _guildWarRestClientForClient;
        public GuildWarRestClient GuildWarRestClientForClient
        {
            get
            {
                if (_guildWarRestClientForClient == null)
                    _guildWarRestClientForClient = gameObject.AddComponent<GuildWarRestClient>();
                return _guildWarRestClientForClient;
            }
        }
        private GuildWarRestClient _guildWarRestClientForServer;
        public GuildWarRestClient GuildWarRestClientForServer
        {
            get
            {
                if (_guildWarRestClientForServer == null)
                    _guildWarRestClientForServer = gameObject.AddComponent<GuildWarRestClient>();
                return _guildWarRestClientForServer;
            }
        }

        public bool GuildWarRunning { get; private set; }
        public System.DateTime LastOccupyTime { get; private set; }
        public int DefenderGuildId { get; private set; }
        public string DefenderGuildName { get; private set; }
        public string DefenderGuildOptions { get; private set; }

        public void ReadGuildWarConfig()
        {
            // Json file read
            bool configFileFound = false;
            const string configFolder = "./Config";
            const string configFilePath = configFolder + "/guildWarConfig.json";
            GuildWarConfig config = new GuildWarConfig()
            {
                recoverMonstersWhenGuildWarRoundEnd = recoverMonstersWhenGuildWarRoundEnd,
                guildWarMailSenderId = guildWarMailSenderId,
                guildWarMailSenderName = guildWarMailSenderName,
                guildWarServiceUrl = guildWarServiceUrl,
                guildWarServiceUrlForClient = guildWarServiceUrlForClient,
                guildWarSecretKey = guildWarSecretKey,
            };
            Logging.Log(LogTag, "Reading guild war config file from " + configFilePath);
            if (File.Exists(configFilePath))
            {
                Logging.Log(LogTag, "Found guild war config file");
                string dataAsJson = File.ReadAllText(configFilePath);
                GuildWarConfig replacingConfig = JsonConvert.DeserializeObject<GuildWarConfig>(dataAsJson);
                if (replacingConfig.recoverMonstersWhenGuildWarRoundEnd.HasValue)
                    config.recoverMonstersWhenGuildWarRoundEnd = replacingConfig.recoverMonstersWhenGuildWarRoundEnd.Value;
                if (replacingConfig.guildWarMailSenderId != null)
                    config.guildWarMailSenderId = replacingConfig.guildWarMailSenderId;
                if (replacingConfig.guildWarMailSenderName != null)
                    config.guildWarMailSenderName = replacingConfig.guildWarMailSenderName;
                if (replacingConfig.guildWarServiceUrl != null)
                    config.guildWarServiceUrl = replacingConfig.guildWarServiceUrl;
                if (replacingConfig.guildWarServiceUrlForClient != null)
                    config.guildWarServiceUrlForClient = replacingConfig.guildWarServiceUrlForClient;
                if (replacingConfig.guildWarSecretKey != null)
                    config.guildWarSecretKey = replacingConfig.guildWarSecretKey;
                configFileFound = true;
            }

            recoverMonstersWhenGuildWarRoundEnd = config.recoverMonstersWhenGuildWarRoundEnd.Value;
            guildWarMailSenderId = config.guildWarMailSenderId;
            guildWarMailSenderName = config.guildWarMailSenderName;
            guildWarServiceUrl = config.guildWarServiceUrl;
            guildWarServiceUrlForClient = config.guildWarServiceUrlForClient;
            guildWarSecretKey = config.guildWarSecretKey;

            // Read configs from ENV
            string envVal;
            envVal = Environment.GetEnvironmentVariable("recoverMonstersWhenGuildWarRoundEnd");
            if (!string.IsNullOrEmpty(envVal) && bool.TryParse(envVal, out bool envRecoverMonstersWhenGuildWarRoundEnd))
                recoverMonstersWhenGuildWarRoundEnd = envRecoverMonstersWhenGuildWarRoundEnd;
            envVal = Environment.GetEnvironmentVariable("guildWarMailSenderId");
            if (!string.IsNullOrEmpty(envVal))
                guildWarMailSenderId = envVal;
            envVal = Environment.GetEnvironmentVariable("guildWarMailSenderName");
            if (!string.IsNullOrEmpty(envVal))
                guildWarMailSenderName = envVal;
            envVal = Environment.GetEnvironmentVariable("guildWarServiceUrl");
            if (!string.IsNullOrEmpty(envVal))
                guildWarServiceUrl = envVal;
            envVal = Environment.GetEnvironmentVariable("guildWarServiceUrlForClient");
            if (!string.IsNullOrEmpty(envVal))
                guildWarServiceUrlForClient = envVal;
            envVal = Environment.GetEnvironmentVariable("guildWarSecretKey");
            if (!string.IsNullOrEmpty(envVal))
                guildWarSecretKey = envVal;

            if (!configFileFound)
            {
                // Write config file
                Logging.Log(LogTag, "Not found guild war config file, creating a new one");
                if (!Directory.Exists(configFolder))
                    Directory.CreateDirectory(configFolder);
                File.WriteAllText(configFilePath, JsonConvert.SerializeObject(config, Formatting.Indented));
            }
        }

        [DevExtMethods("RegisterMessages")]
        protected void RegisterMessages_GuildWar()
        {
            RegisterClientMessage(guildWarMessageTypes.statusMsgType, HandleGuildWarStatusAtClient);
            RegisterRequestToServer<EmptyMessage, ResponseClientConfigMessage>(guildWarMessageTypes.getClientConfigRequestType, HandleGetGuildWarClientConfigAtServer);
        }

        [DevExtMethods("OnStartServer")]
        protected void OnStartServer_GuildWar()
        {
            ReadGuildWarConfig();
            GuildWarRestClientForServer.apiUrl = guildWarServiceUrl;
            GuildWarRestClientForServer.secretKey = guildWarSecretKey;
            CancelInvoke(nameof(Update_GuildWar));
            InvokeRepeating(nameof(Update_GuildWar), 1, 1);
        }

        [DevExtMethods("OnPeerConnected")]
        protected void OnPeerConnected_GuildWar(long connectionId)
        {
            SendGuildWarStatus(connectionId);
        }

        [DevExtMethods("OnServerOnlineSceneLoaded")]
        protected void OnServerOnlineSceneLoaded_GuildWar()
        {
            SendGuildWarStatus();
        }

        [DevExtMethods("Clean")]
        protected void Clean_GuildWar()
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

        public async void Update_GuildWar()
        {
            if (!IsServer || CurrentMapInfo == null || !(CurrentMapInfo is GuildWarMapInfo mapInfo))
                return;

            if (!GuildWarRunning && mapInfo.IsOn)
            {
                ServerSendSystemAnnounce(mapInfo.eventStartedMessage);
                RestClient.Result<OccupyData> occupy = await GuildWarRestClientForServer.GetOccupy(mapInfo.Id);
                if (!occupy.IsError())
                {
                    DefenderGuildId = occupy.Content.guildId;
                    DefenderGuildName = occupy.Content.guildName;
                    DefenderGuildOptions = occupy.Content.guildOptions;
                }
                else
                {
                    DefenderGuildId = 0;
                    DefenderGuildName = string.Empty;
                    DefenderGuildOptions = string.Empty;
                }
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
                    await GuildWarRestClientForServer.CreateOccupy(mapInfo.Id, DefenderGuildId, DefenderGuildName, DefenderGuildOptions, false);
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
                        await GuildWarRestClientForServer.CreateOccupy(mapInfo.Id, DefenderGuildId, DefenderGuildName, DefenderGuildOptions, false);
                    }
                    SendGuildWarStatus();
                }
            }
        }

        public async void CastleOccupied(int attackerGuildId)
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
                await GuildWarRestClientForServer.CreateOccupy(mapInfo.Id, DefenderGuildId, DefenderGuildName, DefenderGuildOptions, true);
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
            if (!recoverMonstersWhenGuildWarRoundEnd)
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

        public async UniTask<AsyncResponseData<ResponseClientConfigMessage>> GetGuildWarClientConfig()
        {
            AsyncResponseData<ResponseClientConfigMessage> result = await ClientSendRequestAsync<EmptyMessage, ResponseClientConfigMessage>(guildWarMessageTypes.getClientConfigRequestType, EmptyMessage.Value);
            if (result.ResponseCode == AckResponseCode.Success)
            {
                GuildWarRestClientForClient.apiUrl = result.Response.serviceUrl;
            }
            return result;
        }

        private UniTaskVoid HandleGetGuildWarClientConfigAtServer(RequestHandlerData requestHandler, EmptyMessage request,
            RequestProceedResultDelegate<ResponseClientConfigMessage> result)
        {
            if (!ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                // Do nothing, player character is not enter the game yet.
                result.InvokeError(new ResponseClientConfigMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return default;
            }
            result.InvokeSuccess(new ResponseClientConfigMessage()
            {
                serviceUrl = guildWarServiceUrlForClient,
            });
            return default;
        }
    }
}
