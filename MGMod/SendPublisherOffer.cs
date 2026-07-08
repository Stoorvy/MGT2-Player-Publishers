using HarmonyLib;
using Mirror;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

namespace MGMod
{
    public static class SendPublisherOffer
    {
        public static Dictionary<OfferKey, OfferData> Offers = new Dictionary<OfferKey, OfferData>();

        public class OfferData
        {
            public int gameID;
            public int developerID;
            public int publisherID;
            public int garanti;
            public bool gelenMi;
            public float kar;
            public taskGame task;
            public bool isOnMarket;
        }

        public struct OfferKey
        {
            public int gameID;
            public int publisherID;

            public OfferKey(int gameID, int publisherID)
            {
                this.gameID = gameID;
                this.publisherID = publisherID;
            }
        }

        public struct PublishOfferMessage : NetworkMessage
        {
            public int gameID;
            public int developerID;
            public int publisherID;
            public int garanti;
            public float kar;
            public bool isSenderDev;
        }

        public struct PublishGameMessage : NetworkMessage
        {
            public int gameID;
            public int devID;
            public float profitShare;
        }

        public static void SetOfferData(OfferData offerData, OfferKey offerKey)
        {
            if (Offers.TryGetValue(offerKey, out var offer))
            {
                offer.gameID = offerData.gameID;
                offer.developerID = offerData.developerID;
                offer.publisherID = offerData.publisherID;
                if (offerData.garanti != 0)
                    offer.garanti = offerData.garanti;
                offer.gelenMi = offerData.gelenMi;
                if (offerData.kar != 0)
                    offer.kar = offerData.kar;
                if (offer.task == null)
                    offer.task = offerData.task;
                if(!offer.isOnMarket)
                    offer.isOnMarket = offerData.isOnMarket;
                Debug.Log($"Offer güncellendi: GameID={offer.gameID}, DevID={offer.developerID}, PubID={offer.publisherID}, Garanti={offer.garanti}, Kar={offer.kar}, GelenMi={offer.gelenMi}, Task={offer.task != null}, Market={offer.isOnMarket}");
            }
            else
            {
                Offers[offerKey] = offerData;
            }
        }

        public static void SendPublishOfferMessage(gameScript game, bool IsServer, int pubID, float kar, int garanti)
        {
            var mainObj = GameObject.FindWithTag("Main");
            if (!mainObj) return;
            var mS_ = mainObj.GetComponent<mainScript>();

            if (!mS_.multiplayer) return;

            bool isSenderDev = false;
            if(game.developerID == mS_.myID)
                isSenderDev = true;

            if (IsServer)
            {
                NetworkServer.SendToAll(new PublishOfferMessage
                {
                    gameID = game.myID,
                    developerID = game.developerID,
                    publisherID = pubID,
                    kar = kar,
                    garanti = garanti,
                    isSenderDev = isSenderDev
                });
            }
            else
            {
                NetworkClient.Send(new PublishOfferMessage
                {
                    gameID = game.myID,
                    developerID = game.developerID,
                    publisherID = pubID,
                    kar = kar,
                    garanti = garanti,
                    isSenderDev = isSenderDev
                });
            }

            mS_.publishingOfferMain_.amountPublishingOffers--;
            mS_.publishingOfferMain_.UpdateGUI();
        }

        public static void SendPublishGameMessage(gameScript game, bool IsServer)
        {
            if (!IsServer)
            {
                NetworkClient.Send(new PublishGameMessage
                {
                    gameID = game.myID,
                    devID = game.developerID,
                    profitShare = game.pubAngebot_Gewinnbeteiligung
                });
            }
            else
            {
                NetworkServer.SendToAll<PublishGameMessage>(new PublishGameMessage
                {
                    gameID = game.myID,
                    devID = game.developerID,
                    profitShare = game.pubAngebot_Gewinnbeteiligung
                });
            }
        }

        public static void ClientOnPublishOffer(PublishOfferMessage msg)
        {
            UnityEngine.Debug.Log($"PublishOfferMessage geldi. GameID={msg.gameID} {msg.garanti} {msg.kar} {msg.isSenderDev}");

            var mainObj = GameObject.FindWithTag("Main");
            if (!mainObj) return;
            var mS_ = mainObj.GetComponent<mainScript>();

            if (mS_.myID != msg.publisherID && mS_.myID != msg.developerID)
            {
                Debug.Log("Offer bana değil.");
                return;
            }

            bool gelenMi = false;

            bool senderIsMe = (msg.isSenderDev && msg.developerID == mS_.myID) || (!msg.isSenderDev && msg.publisherID == mS_.myID);

            if (!senderIsMe)
            {
                gelenMi = true;
            }

            if (!gelenMi) return;

            var offerData = new OfferData
            {
                gameID = msg.gameID,
                developerID = msg.developerID,
                publisherID = msg.publisherID,
                garanti = msg.garanti,
                kar = msg.kar,
                gelenMi = gelenMi
            };

            SetOfferData(offerData, new OfferKey(msg.gameID, msg.publisherID));

            foreach (var game in mS_.games_.arrayGamesScripts)
            {
                if (game.myID == msg.gameID)
                {
                    game.SetPublisher(msg.publisherID);
                    if(game.publisherID == mS_.myID)
                    {
                        mS_.guiMain_.CreateTopNewsInfo($"<color=blue>{game.GetDeveloperName()}</color> sent you an offer!");
                    }
                    else if(game.developerID == mS_.myID)
                    {
                        mS_.guiMain_.CreateTopNewsInfo($"<color=blue>{game.GetPublisherName()}</color> sent you a counter-offer!");
                    }
                    break;
                }
            }

            mS_.publishingOfferMain_.amountPublishingOffers++;
            mS_.publishingOfferMain_.UpdateGUI();
        }

        public static void ClientOnPublishGame(PublishGameMessage msg)
        {
            UnityEngine.Debug.Log($"PublishGameMessage geldi. GameID={msg.gameID} DevID={msg.devID}");

            var mainObj = GameObject.FindWithTag("Main");
            if (!mainObj) return;
            var mS_ = mainObj.GetComponent<mainScript>();

            gameScript gS = null;

            foreach (var game in mS_.games_.arrayGamesScripts)
            {
                if (game.myID == msg.gameID)
                {
                    gS = game;
                    break;
                }
            }

            SetOfferData(new OfferData { isOnMarket = true }, new OfferKey(msg.gameID, gS.publisherID));

            if (msg.devID != mS_.myID) return;

            if(gS == null)
            {
                Debug.Log("Game bulunamadı.");
                return;
            }

            var guiMain_ = mS_.guiMain_;

            if (Offers.TryGetValue(new OfferKey(msg.gameID, gS.publisherID), out var offer))
            {
                Debug.Log("Offerı buldum.");
                if (offer.task != null)
                {
                    UnityEngine.Object.Destroy(offer.task.gameObject);
                    Debug.Log("Taskı buldum ve sildim.");
                }
            }

            Debug.Log($"{gS.GetNameSimple()} {gS.reviewTotal} {gS.isOnMarket}");

            guiMain_.ActivateMenu(guiMain_.uiObjects[71]);
            guiMain_.uiObjects[71].GetComponent<Menu_Dev_XP>().Init(gS);
            guiMain_.CreateTopNewsInfo($"<color=blue>{gS.GetNameSimple()}</color>, published by <color=blue>{gS.GetPublisherName()}</color> with {msg.profitShare.ToString()}% profit share!");

        }

        public static void ServerOnPublishOffer(NetworkConnectionToClient conn, PublishOfferMessage msg)
        {
            UnityEngine.Debug.Log($"SERVER PublishOffer aldı {msg.gameID}");

            NetworkServer.SendToAll(msg);
        }

        public static void ServerOnPublishGame(NetworkConnectionToClient conn, PublishGameMessage msg)
        {
            UnityEngine.Debug.Log($"SERVER PublishGame aldı {msg.gameID}");

            NetworkServer.SendToAll(msg);
        }

        [HarmonyPatch(typeof(gameScript), nameof(gameScript.SetOnMarket))]
        public static class PatchSetOnMarketTrace
        {
            static void Postfix(gameScript __instance)
            {
                var gS_ = __instance;
                var mS_ = Traverse.Create(__instance).Field("mS_").GetValue<mainScript>();

                if (gS_.developerID != mS_.myID && gS_.developerID >= 100000 && gS_.publisherID == mS_.myID && mS_.multiplayer)
                {
                    SendPublishGameMessage(gS_, mS_.mpCalls_.isServer);
                }
            }
        }

        [HarmonyPatch(typeof(Menu_PublishingOfferSelect), "SetData")]
        public class Patch_MenuPublishingOfferSelect
        {
            static void Postfix(Menu_PublishingOfferSelect __instance)
            {
                foreach (var offer in Offers.Values)
                {
                    gameScript game = null;

                    var mS_ = Traverse.Create(__instance).Field("mS_").GetValue<mainScript>();

                    foreach (var g in mS_.games_.arrayGamesScripts)
                    {
                        if (g && g.myID == offer.gameID)
                        {
                            game = g;
                            break;
                        }
                    }

                    if (!game)
                        continue;

                    if (Exists(__instance.uiObjects[0], game.myID))
                        continue;

                    if (!offer.gelenMi)
                        continue;

                    var obj = UnityEngine.Object.Instantiate(
                        __instance.uiPrefabs[0],
                        Vector3.zero,
                        Quaternion.identity,
                        __instance.uiObjects[0].transform);

                    var item = obj.GetComponent<Item_PubOfferSelect>();

                    item.game_ = game;
                    item.mS_ = mS_;
                    item.tS_ = Traverse.Create(__instance).Field("tS_").GetValue<textScript>();
                    item.sfx_ = Traverse.Create(__instance).Field("sfx_").GetValue<sfxScript>();
                    item.guiMain_ = Traverse.Create(__instance).Field("guiMain_").GetValue<GUI_Main>();
                    item.genres_ = Traverse.Create(__instance).Field("genres_").GetValue<genres>();
                    item.games_ = Traverse.Create(__instance).Field("games_").GetValue<games>();

                    item.UpdateReview();
                }

                __instance.DROPDOWN_Sort();
            }

            static bool Exists(GameObject parent, int id)
            {
                for (int i = 0; i < parent.transform.childCount; i++)
                {
                    var child = parent.transform.GetChild(i);

                    if (!child.gameObject.activeSelf)
                        continue;

                    var item = child.GetComponent<Item_PubOfferSelect>();

                    if (item && item.game_ && item.game_.myID == id)
                        return true;
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(Item_PubOfferSelect), nameof(Item_PubOfferSelect.BUTTON_Click))]
        public class Patch_ItemPubOfferSelect_ButtonClick
        {
            static bool Prefix(Item_PubOfferSelect __instance)
            {
                if (__instance.game_ == null)
                    return true;

                if (!Offers.TryGetValue(new OfferKey(__instance.game_.myID, __instance.game_.publisherID), out var offer))
                    return true; // NPC teklifi

                if(__instance.game_.isOnMarket || offer.isOnMarket)
                {
                    GameObject.Destroy(__instance.gameObject);
                    return false;
                }

                __instance.sfx_.PlaySound(3, true);

                new CustomMenu().CreatePubOfferMenu(
                    offer.gameID,
                    offer.developerID,
                    offer.publisherID,
                    offer.kar,
                    offer.garanti);

                return false; // Orijinal BUTTON_Click çalışmasın.
            }
        }

        [HarmonyPatch(typeof(Item_PubOfferSelect), "Update")]
        public class Patch_ItemPubOfferSelect_Update
        {
            static bool Prefix(Item_PubOfferSelect __instance)
            {
                if (__instance.game_ == null)
                    return true;

                if (!Offers.TryGetValue(new OfferKey(__instance.game_.myID, __instance.game_.publisherID), out var offer))
                    return true; // NPC teklifleri normal çalışsın

                if (__instance.game_.isOnMarket || offer.isOnMarket)
                {
                    GameObject.Destroy(__instance.gameObject);
                    return false;
                }

                var tS_ = Traverse.Create(__instance).Field("tS_").GetValue<textScript>();


                __instance.uiObjects[5].GetComponent<Text>().text =
                    tS_.GetText(1730) + "<color=#BE0000>" +
                    __instance.mS_.GetMoney(offer.garanti, true) +
                    "</color>";

                __instance.uiObjects[6].GetComponent<Text>().text =
                    tS_.GetText(1731) + "<color=#BE0000>" +
                    Mathf.RoundToInt(offer.kar) +
                    "%</color>";

                Sprite playerIcon = PatchAddHumanPublishers.humanIconSprite;

                if (playerIcon != null)
                    __instance.uiObjects[9].GetComponent<Image>().sprite = playerIcon;

                return false; // Orijinal Update çalışmasın.
            }
        }
    }
}
