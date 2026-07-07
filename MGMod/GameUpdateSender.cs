using HarmonyLib;
using Mirror;
using UnityEngine;

namespace MGMod
{
    public static class GameUpdateSender
    {
        public struct GameUpdateMessage : NetworkMessage
        {
            public int gameID;

            public float hype;

            public int amountUpdates;
            public float bonusSellsUpdates;

            public float pointsGameplay;
            public float pointsGrafik;
            public float pointsSound;
            public float pointsTechnik;
            public float pointsBugs;

            public long costsUpdates;

            public bool[] gameLanguage;
        }

        public static void SendGameUpdate(gameScript game, bool isServer)
        {
            GameUpdateMessage msg = new GameUpdateMessage
            {
                gameID = game.myID,

                hype = game.hype,

                amountUpdates = game.amountUpdates,
                bonusSellsUpdates = game.bonusSellsUpdates,

                pointsGameplay = game.points_gameplay,
                pointsGrafik = game.points_grafik,
                pointsSound = game.points_sound,
                pointsTechnik = game.points_technik,
                pointsBugs = game.points_bugs,

                costsUpdates = game.costs_updates,

                gameLanguage = (bool[])game.gameLanguage.Clone()
            };

            if (isServer)
                NetworkServer.SendToAll(msg);
            else
                NetworkClient.Send(msg);
        }

        public static void ServerOnGameUpdate(NetworkConnectionToClient conn, GameUpdateSender.GameUpdateMessage msg)
        {
            NetworkServer.SendToAll(msg);
        }

        public static void ClientOnGameUpdate(GameUpdateSender.GameUpdateMessage msg)
        {
            var mainObj = GameObject.FindWithTag("Main");
            if (!mainObj)
                return;

            var mS_ = mainObj.GetComponent<mainScript>();

            foreach (var game in mS_.games_.arrayGamesScripts)
            {
                if (game == null)
                    continue;

                if (game.myID != msg.gameID)
                    continue;

                // Sadece publisher uygulasın.
                if (game.publisherID != mS_.myID)
                    return;

                game.hype = msg.hype;

                game.amountUpdates = msg.amountUpdates;
                game.bonusSellsUpdates = msg.bonusSellsUpdates;

                game.points_gameplay = msg.pointsGameplay;
                game.points_grafik = msg.pointsGrafik;
                game.points_sound = msg.pointsSound;
                game.points_technik = msg.pointsTechnik;
                game.points_bugs = msg.pointsBugs;

                game.costs_updates = msg.costsUpdates;

                if (msg.gameLanguage != null)
                    game.gameLanguage = (bool[])msg.gameLanguage.Clone();

                if (MGMod.DebugMode.Value)
                    Debug.Log($"GameUpdate applied: {game.GetNameSimple()}");

                break;
            }
        }

        [HarmonyPatch(typeof(taskUpdate), "Complete")]
        public static class Patch_TaskUpdate_Complete
        {
            static void Postfix(taskUpdate __instance)
            {
                if (__instance.gS_ == null)
                    return;

                var gS_ = __instance.gS_;

                if (gS_.developerID < 100000)
                    return;

                if (gS_.publisherID < 100000)
                    return;

                if (gS_.developerID == gS_.publisherID)
                    return;

                var mS_ = Traverse.Create(__instance).Field("mS_").GetValue<mainScript>();

                if (mS_ == null)
                    return;

                // Sadece developer göndersin.
                if (mS_.myID != gS_.developerID)
                    return;

                GameUpdateSender.SendGameUpdate(gS_, mS_.mpCalls_.isServer);
            }
        }

        [HarmonyPatch(typeof(gameScript), nameof(gameScript.AddHype))]
        public static class Patch_GameScript_AddHype
        {
            static void Postfix(gameScript __instance)
            {
                var mS_ = Traverse.Create(__instance).Field("mS_").GetValue<mainScript>();

                if (mS_ == null)
                    return;

                // Sadece multiplayer
                if (!mS_.multiplayer)
                    return;

                // İnsan developer + insan publisher
                if (__instance.developerID < 100000)
                    return;

                if (__instance.publisherID < 100000)
                    return;

                if (__instance.developerID == __instance.publisherID)
                    return;

                // Sadece developer göndersin
                if (mS_.myID != __instance.developerID)
                    return;

                GameUpdateSender.SendGameUpdate(__instance, mS_.mpCalls_.isServer);
            }
        }
    }
}