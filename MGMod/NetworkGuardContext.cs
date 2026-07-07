using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace MGMod
{
    public static class NetworkGuardContext
    {
        public static bool InGameSyncGuard = false;
    }

    // 1) SERVER_Get_Game çalışırken bir "bayrak" aç
    [HarmonyPatch(typeof(mpCalls), "SERVER_Get_Game")]
    public static class Patch_SERVER_Get_Game_Guard
    {
        static void Prefix()
        {
            NetworkGuardContext.InGameSyncGuard = true;
        }

        static void Postfix()
        {
            NetworkGuardContext.InGameSyncGuard = false;
        }
    }

    // 2) Sadece o bayrak açıkken VE tam olarak bizim senaryomuzda IsMyGame()'i false'a çevir
    [HarmonyPatch(typeof(gameScript), nameof(gameScript.IsMyGame))]
    public static class Patch_IsMyGame_BypassForHumanPublisher
    {
        static void Postfix(gameScript __instance, ref bool __result)
        {
            if (!NetworkGuardContext.InGameSyncGuard) return;
            if (!__result) return; // zaten false, dokunma

            var mainObj = GameObject.FindWithTag("Main");
            if (!mainObj) return;
            var mS_ = mainObj.GetComponent<mainScript>();
            if (!mS_) return;

            // Ben geliştiriciyim, ama yayıncı BEN değilim ve yayıncı bir insan oyuncu (ID >= 100000)
            if (__instance.developerID == mS_.myID
                && __instance.publisherID != mS_.myID
                && __instance.publisherID >= 100000)
            {
                __result = false; // guard'ı atla, güncellemenin işlenmesine izin ver
            }
        }
    }

    [HarmonyPatch(typeof(mpCalls), nameof(mpCalls.CLIENT_Send_GameSell))]
    public static class Patch_CLIENT_Send_GameSell
    {
        static bool Prefix(mpCalls __instance, gameScript script_)
        {
            if (!__instance.isClient)
                return false;

            var mS_ = __instance.main_.GetComponent<mainScript>();

            if (mS_ == null)
                return true;

            // İnsan developer + insan publisher + farklı kişiler
            bool humanPublisherDeveloperCase =
                script_.developerID >= 100000 &&
                script_.publisherID >= 100000 &&
                script_.developerID != script_.publisherID;

            // Bizim senaryomuz değilse oyunun kendi kodu çalışsın.
            if (!humanPublisherDeveloperCase)
                return true;

            // Sadece publisher satış paketini göndersin.
            return mS_.myID == script_.publisherID;
        }
    }

    [HarmonyPatch(typeof(mpCalls), nameof(mpCalls.CLIENT_Send_Game))]
    public static class Patch_CLIENT_Send_Game
    {
        static bool Prefix(mpCalls __instance, gameScript script_)
        {
            if (!__instance.isClient)
                return false;

            var mS_ = __instance.main_.GetComponent<mainScript>();

            if (mS_ == null)
                return true;

            bool humanPublisherDeveloperCase =
                script_.developerID >= 100000 &&
                script_.publisherID >= 100000 &&
                script_.developerID != script_.publisherID;

            if (!humanPublisherDeveloperCase)
                return true;

            // Sadece publisher CLIENT_Send_Game göndersin.
            return mS_.myID == script_.publisherID;
        }
    }

    [HarmonyPatch(typeof(mpCalls), nameof(mpCalls.SERVER_Get_GameSell))]
    public static class Patch_SERVER_Get_GameSell
    {
        static bool Prefix(mpCalls __instance, mpCalls.s_GameSell msg)
        {
            if (!__instance.isClient)
                return false;

            var mS_ = __instance.main_.GetComponent<mainScript>();

            if (mS_ == null)
                return true;

            MethodInfo getGameMethod = AccessTools.Method(typeof(mpCalls), "GetGame");

            GameObject game = (GameObject)getGameMethod.Invoke(__instance, new object[]
            {
                msg.gameID
            });

            if (game == null) return true;

            gameScript component = game.GetComponent<gameScript>();

            if (component == null) return true;

            bool humanPublisherDeveloperCase =
            component.developerID >= 100000 &&
            component.publisherID >= 100000 &&
            component.developerID != component.publisherID &&
            component.developerID == mS_.myID;

            if (!humanPublisherDeveloperCase)
                return true;

            bool isOnMarket = component.isOnMarket;

            component.isOnMarket = msg.isOnMarket;
            component.weeksOnMarket = msg.weeksOnMarket;
            component.sellsTotal = msg.sellsTotal;
            component.sellsTotalStandard = msg.sellsTotalStandard;
            component.sellsTotalDeluxe = msg.sellsTotalDeluxe;
            component.sellsTotalCollectors = msg.sellsTotalCollectors;
            component.sellsTotalOnline = msg.sellsTotalOnline;
            component.abonnements = msg.abonnements;
            component.abonnementsWoche = msg.abonnementsWoche;
            component.bestAbonnements = msg.bestAbonnements;
            component.exklusivKonsolenSells = msg.exklusivKonsolenSells;
            component.userPositiv = msg.userPositiv;
            component.userNegativ = msg.userNegativ;
            component.costs_entwicklung = msg.costs_entwicklung;
            component.costs_mitarbeiter = msg.costs_mitarbeiter;
            component.costs_marketing = msg.costs_marketing;
            component.costs_enginegebuehren = msg.costs_enginegebuehren;
            component.costs_server = msg.costs_server;
            component.costs_production = msg.costs_production;
            component.costs_updates = msg.costs_updates;
            component.points_gameplay = msg.points_gameplay;
            component.points_grafik = msg.points_grafik;
            component.points_sound = msg.points_sound;
            component.points_technik = msg.points_technik;
            component.points_bugs = msg.points_bugs;
            component.points_bugsInvis = msg.points_bugsInvis;
            component.umsatzTotal = msg.umsatzTotal;
            component.umsatzInApp = msg.umsatzInApp;
            component.umsatzAbos = msg.umsatzAbos;
            component.bestChartPosition = msg.bestChartPosition;
            component.lastChartPosition = msg.lastChartPosition;
            component.f2pInteresse = msg.f2pInteresse;
            component.mmoInteresse = msg.mmoInteresse;
            component.vorbestellungen = msg.vorbestellungen;
            component.realsticPower = msg.realsticPower;
            component.hype = msg.hype;
            component.stornierungen = msg.stornierungen;
            component.commercialFlop = msg.commercialFlop;
            component.commercialHit = msg.commercialHit;
            component.freigabeBudget = msg.freigabeBudget;
            component.releaseDate = msg.releaseDate;
            component.inAppPurchaseWeek = msg.inAppPurchaseWeek;
            component.sellsPerWeek = (int[])msg.sellsPerWeek.Clone();
            component.verkaufspreis = (int[])msg.verkaufspreis.Clone();

            var games = Traverse.Create(__instance).Field("games_").GetValue<games>();

            var guiMain = Traverse.Create(__instance).Field("guiMain_").GetValue<GUI_Main>();

            games.UpdateChartsWeek();
            guiMain.UpdateCharts();

            return false;
        }
    }

    [HarmonyPatch(typeof(gameScript), nameof(gameScript.InitUI))]
    class Patch_InitUI
    {
        static bool Prefix(gameScript __instance)
        {
            var main = GameObject.FindWithTag("Main");
            var mS = main.GetComponent<mainScript>();

            bool humanPub =
                __instance.developerID == mS.myID &&
                __instance.publisherID != mS.myID &&
                __instance.publisherID >= 100000;

            if(MGMod.DebugMode.Value)
                humanPub = true;

            if (!humanPub)
                return true; // bizim senaryomuz değil, orijinal InitUI çalışsın

            if (__instance.gameTab_)
                return false; // zaten var, tekrar oluşturma

            if (!__instance.isOnMarket && !__instance.schublade)
                return false; // koşul sağlanmıyor, hiçbir şey yapma

            // gameScript'in KENDİ private FindScripts()'ini çağır
            MethodInfo findScriptsMethod = AccessTools.Method(typeof(gameScript), "FindScripts");
            findScriptsMethod.Invoke(__instance, null);

            GameObject obj = Object.Instantiate(
                __instance.guiMain_.uiPrefabs[11],
                __instance.guiMain_.uiObjects[81].transform);

            __instance.gameTab_ = obj.GetComponent<gameTab>();

            __instance.gameTab_.gS_ = __instance;
            __instance.gameTab_.mS_ = __instance.mS_;
            __instance.gameTab_.main_ = __instance.main_;
            __instance.gameTab_.guiMain_ = __instance.guiMain_;
            __instance.gameTab_.sfx_ = __instance.sfx_;
            __instance.gameTab_.tS_ = __instance.tS_;
            __instance.gameTab_.themes_ = __instance.themes_;
            __instance.gameTab_.genres_ = __instance.genres_;

            __instance.gameTab_.Init(__instance.myID);

            return false;
        }
    }
}