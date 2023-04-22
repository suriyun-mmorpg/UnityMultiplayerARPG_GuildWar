using LiteNetLib.Utils;

namespace MultiplayerARPG.GuildWar
{
    public struct ResponseClientConfigMessage : INetSerializable
    {
        public UITextKeys message;
        public string serviceUrl;

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
            serviceUrl = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
            writer.Put(serviceUrl);
        }
    }
}
