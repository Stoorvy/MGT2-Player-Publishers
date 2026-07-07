using HarmonyLib;
using Mirror;
using System;
using UnityEngine;
using static MGMod.SendPublisherOffer;

namespace MGMod
{
    [HarmonyPatch(typeof(Menu_Dev_SelectPublisher), "SelectPublisher")]
    public static class PatchAssignPublisherOnly
    {
        static bool Prefix(Menu_Dev_SelectPublisher __instance, int id_)
        {
            var gS_ = Traverse.Create(__instance).Field("gS_").GetValue<gameScript>();
            var task_ = Traverse.Create(__instance).Field("task_").GetValue<taskGame>();
            var mS_ = Traverse.Create(__instance).Field("mS_").GetValue<mainScript>();
            var guiMain_ = Traverse.Create(__instance).Field("guiMain_").GetValue<GUI_Main>();

            gS_.SetPublisher(id_);

            // NPC ise VANILLA çalışsın
            if (gS_.PublisherIsNPC())
                return true;

            // -------- Multiplayer publisher --------

            var offerData = new OfferData
            {
                task = task_,
                gameID = gS_.myID,
                developerID = gS_.developerID,
                publisherID = gS_.publisherID,
                gelenMi = false,
            };

            SendPublisherOffer.SetOfferData(offerData, new OfferKey(gS_.myID, gS_.publisherID));

            __instance.gameObject.SetActive(false);
            guiMain_.CloseMenu();

            new CustomMenu().CreatePubOfferMenu(gS_.myID, mS_.myID, id_);
            return false;
        }
    }
}