using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace MGMod
{
    [BepInPlugin(pluginGuid,pluginName,pluginVersion)]
    public class MGMod : BaseUnityPlugin
    {
        public const string pluginGuid = "com.stoorvy.mgt2.playerPublishers";
        public const string pluginName = "Player Publishers";
        public const string pluginVersion = "1.0.0.0";

        public static ConfigEntry<bool> DebugMode;

        public void Awake()
        {
            MirrorSerializer.Register();

            Logger.LogInfo($"{pluginName} - {pluginVersion} is loaded!");

            var harmony = new Harmony(pluginGuid);
            harmony.PatchAll();

            Logger.LogInfo("Harmony patched!");

            DebugMode = Config.Bind(
                "General",
                "Debug Mode",
                false,
                "Enable debug mode."
                );
        }
    }
}
