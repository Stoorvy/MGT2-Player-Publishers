using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace MGMod
{
    [HarmonyPatch(typeof(Menu_Dev_SelectPublisher), "Init")]
    public static class PatchAddHumanPublishers
    {
        private const float HUMAN_PUBLISHER_SHARE = 0f;
        public static Sprite humanIconSprite = null;

        static void Postfix(Menu_Dev_SelectPublisher __instance)
        {
            var mS_ = Traverse.Create(__instance).Field("mS_").GetValue<mainScript>();
            var tS_ = Traverse.Create(__instance).Field("tS_").GetValue<textScript>();
            var sfx_ = Traverse.Create(__instance).Field("sfx_").GetValue<sfxScript>();
            var guiMain_ = Traverse.Create(__instance).Field("guiMain_").GetValue<GUI_Main>();
            var genres_ = Traverse.Create(__instance).Field("genres_").GetValue<genres>();

            if (!MGMod.DebugMode.Value)
                if (!mS_.multiplayer) return;

            GameObject[] array = GameObject.FindGameObjectsWithTag("Publisher");
            foreach (var obj in array)
            {
                if (!obj) continue;

                var pub = obj.GetComponent<publisherScript>();

                bool listingSelf = true;
                if(pub.myID == mS_.myID)
                {
                    if (MGMod.DebugMode.Value)
                        listingSelf = true;
                    else
                        listingSelf = false;
                }

                if (pub.isPlayer &&
                    listingSelf &&
                    pub.publisher &&
                    !pub.Geschlossen())
                {
                    pub.share = HUMAN_PUBLISHER_SHARE;
                    pub.relation = 100f;

                    var item = Object.Instantiate(
                        __instance.uiPrefabs[0],
                        Vector3.zero,
                        Quaternion.identity,
                        __instance.uiObjects[0].transform)
                        .GetComponent<Item_SelectPublisher>();

                    item.pS_ = pub;
                    item.mS_ = mS_;
                    item.tS_ = tS_;
                    item.sfx_ = sfx_;
                    item.guiMain_ = guiMain_;
                    item.genres_ = genres_;

                    if(humanIconSprite == null) 
                    {
                        humanIconSprite = item.uiObjects[7].GetComponent<Image>().sprite;
                    }
                }
            }

            __instance.DROPDOWN_Sort();
        }
    }
}