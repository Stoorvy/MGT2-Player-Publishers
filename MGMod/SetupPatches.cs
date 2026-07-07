using HarmonyLib;
using Mirror;

namespace MGMod
{
    public class SetupPatches
    {
        [HarmonyPatch(typeof(mpCalls), "SetupClient")]
        public class PatchSetupClient
        {
            static void Postfix()
            {
                NetworkClient.RegisterHandler<SendPublisherOffer.PublishOfferMessage>(SendPublisherOffer.ClientOnPublishOffer, true);
                NetworkClient.RegisterHandler<SendPublisherOffer.PublishGameMessage>(SendPublisherOffer.ClientOnPublishGame, true);
                NetworkClient.RegisterHandler<GameUpdateSender.GameUpdateMessage>(GameUpdateSender.ClientOnGameUpdate, true);
            }
        }

        [HarmonyPatch(typeof(mpCalls), "SetupServer")]
        public class PatchSetupServer
        {
            static void Postfix()
            {
                NetworkServer.RegisterHandler<SendPublisherOffer.PublishOfferMessage>(SendPublisherOffer.ServerOnPublishOffer, true);
                NetworkServer.RegisterHandler<SendPublisherOffer.PublishGameMessage>(SendPublisherOffer.ServerOnPublishGame, true);
                NetworkServer.RegisterHandler<GameUpdateSender.GameUpdateMessage>(GameUpdateSender.ServerOnGameUpdate, true);
            }
        }
    }
}
