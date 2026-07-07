using HarmonyLib;
using UnityEngine;

namespace MGMod
{
    [HarmonyPatch(typeof(gameScript), nameof(gameScript.PayGewinnbeteiligung))]
    public static class PatchPayGewinnbeteiligung
    {
        static bool Prefix(gameScript __instance, long i)
        {
            var mS_ = Traverse.Create(__instance).Field("mS_").GetValue<mainScript>();

            if (__instance.developerID == mS_.myID) return false;

            //  Orjinal Kod

            if (__instance.PUBOFFER_GetGewinnbeteiligung() <= 0)
            {
                return false;
            }
            float num = (float)i / 100f;
            num *= (float)__instance.PUBOFFER_GetGewinnbeteiligung();
            __instance.mS_.Pay((long)Mathf.RoundToInt(num), 26);

            //  Eklenen kod
            //  belirlenen payı developera gönder
            if(__instance.developerID != mS_.myID && __instance.developerID >= 100000)
            {
                int payment = Mathf.RoundToInt(num);
                if (mS_.mpCalls_.isServer)
                {
                    mS_.mpCalls_.SERVER_Send_Help(mS_.myID, __instance.developerID, 0, payment, -1, -1);
                }
                else if (mS_.mpCalls_.isClient)
                {
                    mS_.mpCalls_.CLIENT_Send_Help(__instance.developerID, 0, payment, -1, -1);
                }
            }

            // Orijinal metodu çalıştırma
            return false;
        }
    }
}