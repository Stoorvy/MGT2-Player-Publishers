using HarmonyLib;
using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MGMod
{
    [HarmonyPatch(typeof(mpMain), "INPUTFIELD_Chat")]
    public static class PatchLobbyChatCommands
    {
        static bool Prefix(mpMain __instance)
        {
            var guiMain = Traverse.Create(__instance)
                .Field("guiMain_")
                .GetValue<GUI_Main>();

            if (!guiMain)
                return false;

            var input = __instance.uiObjects[70].GetComponent<InputField>();
            string text = input.text;

            if (string.IsNullOrWhiteSpace(text))
                return false;

            if (!text.StartsWith("/"))
                return true; // normal chat

            input.text = "";

            var mS = Traverse.Create(__instance).Field("mS_").GetValue<mainScript>();
            var mp = Traverse.Create(__instance).Field("mpCalls_").GetValue<mpCalls>();

            PatchChatCommands.ExecuteCommand(text, guiMain, mS, mp);

            return false;
        }
    }

    [HarmonyPatch(typeof(GUI_Main), "INPUTFIELD_Chat")]
    public static class PatchChatCommands
    {
        static bool Prefix(GUI_Main __instance)
        {
            Debug.Log("PatchChatCommands Prefix");

            var input = __instance.uiObjects[209].GetComponent<InputField>();
            string text = input.text;

            if (string.IsNullOrWhiteSpace(text))
                return false;

            if (!text.StartsWith("/"))
                return true; // Normal chat devam etsin

            input.text = "";

            var mS = Traverse.Create(__instance).Field("mS_").GetValue<mainScript>();
            var mp = Traverse.Create(__instance).Field("mpCalls_").GetValue<mpCalls>();

            return ExecuteCommand(text.ToLower(), __instance, mS, mp);
        }

        public static bool ExecuteCommand(string command, GUI_Main __instance, mainScript mS, mpCalls mp)
        {
            string[] args = command.Split(' ');

            switch (args[0].ToLower())
            {
                case "/help":
                    __instance.AddChat(mS.myID,
                        "[Player Publishers]\n" +
                        "/help: shows this list\n" +
                        "/debug: activates/deactivates debug mode\n");
                    return false;

                case "/debug":
                    MGMod.DebugMode.Value = !MGMod.DebugMode.Value;
                    __instance.AddChat(mS.myID,
                        "[Player Publishers] Debug: " + (MGMod.DebugMode.Value ? "Active" : "Deactive"));
                    return false;

                default:
                    __instance.AddChat(mS.myID, "[Player Publishers] Unknown command. /help for all commands");
                    return true;
            }
        }
    }
}