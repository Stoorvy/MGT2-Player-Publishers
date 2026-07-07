using Mirror;
using static MGMod.SendPublisherOffer;

namespace MGMod
{
    public static class MirrorSerializer
    {
        private static bool initialized;

        public static void Register()
        {
            if (initialized)
                return;

            initialized = true;

            RegisterPublishOffer();
            RegisterPublishGame();
            RegisterGameUpdate();
        }

        #region PublishOfferMessage

        static void RegisterPublishOffer()
        {
            Writer<PublishOfferMessage>.write = WritePublishOfferMessage;
            Reader<PublishOfferMessage>.read = ReadPublishOfferMessage;
        }

        static void WritePublishOfferMessage(NetworkWriter writer, PublishOfferMessage value)
        {
            writer.WriteInt(value.gameID);
            writer.WriteInt(value.developerID);
            writer.WriteInt(value.publisherID);
            writer.WriteInt(value.garanti);
            writer.WriteFloat(value.kar);
            writer.WriteBool(value.isSenderDev);
        }

        static PublishOfferMessage ReadPublishOfferMessage(NetworkReader reader)
        {
            return new PublishOfferMessage
            {
                gameID = reader.ReadInt(),
                developerID = reader.ReadInt(),
                publisherID = reader.ReadInt(),
                garanti = reader.ReadInt(),
                kar = reader.ReadFloat(),
                isSenderDev = reader.ReadBool(),
            };
        }

        #endregion

        #region PublishGameMessage

        static void RegisterPublishGame()
        {
            Writer<PublishGameMessage>.write = WritePublishGameMessage;
            Reader<PublishGameMessage>.read = ReadPublishGameMessage;
        }

        static void WritePublishGameMessage(NetworkWriter writer, PublishGameMessage value)
        {
            writer.WriteInt(value.gameID);
            writer.WriteInt(value.devID);
            writer.WriteFloat(value.profitShare);
        }

        static PublishGameMessage ReadPublishGameMessage(NetworkReader reader)
        {
            return new PublishGameMessage
            {
                gameID = reader.ReadInt(),
                devID = reader.ReadInt(),
                profitShare = reader.ReadFloat()
            };
        }

        #endregion

        #region GameUpdateMessage

        static void RegisterGameUpdate()
        {
            Writer<GameUpdateSender.GameUpdateMessage>.write = WriteGameUpdateMessage;
            Reader<GameUpdateSender.GameUpdateMessage>.read = ReadGameUpdateMessage;
        }

        static void WriteGameUpdateMessage(NetworkWriter writer, GameUpdateSender.GameUpdateMessage value)
        {
            writer.WriteInt(value.gameID);

            writer.WriteFloat(value.hype);

            writer.WriteInt(value.amountUpdates);
            writer.WriteFloat(value.bonusSellsUpdates);

            writer.WriteFloat(value.pointsGameplay);
            writer.WriteFloat(value.pointsGrafik);
            writer.WriteFloat(value.pointsSound);
            writer.WriteFloat(value.pointsTechnik);
            writer.WriteFloat(value.pointsBugs);

            writer.WriteLong(value.costsUpdates);

            writer.WriteArray(value.gameLanguage);
        }

        static GameUpdateSender.GameUpdateMessage ReadGameUpdateMessage(NetworkReader reader)
        {
            return new GameUpdateSender.GameUpdateMessage
            {
                gameID = reader.ReadInt(),

                hype = reader.ReadFloat(),

                amountUpdates = reader.ReadInt(),
                bonusSellsUpdates = reader.ReadFloat(),

                pointsGameplay = reader.ReadFloat(),
                pointsGrafik = reader.ReadFloat(),
                pointsSound = reader.ReadFloat(),
                pointsTechnik = reader.ReadFloat(),
                pointsBugs = reader.ReadFloat(),

                costsUpdates = reader.ReadLong(),

                gameLanguage = reader.ReadArray<bool>()
            };
        }

        #endregion
    }
}