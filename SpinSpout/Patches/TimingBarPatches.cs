using HarmonyLib;
using Unity.XR.CoreUtils; // what is SetLayerRecursively doing in here? lol
using UnityEngine;

namespace SpinSpout.Patches;

[HarmonyPatch]
internal static class TimingBarPatches
{
    private static GameObject _timingBar;
    
    [HarmonyPatch(typeof(HudTimingAccuracyBar), nameof(HudTimingAccuracyBar.Init))]
    [HarmonyPatch(typeof(HudTimingAccuracyBar), nameof(HudTimingAccuracyBar.Layout))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    internal static void Postfix(HudTimingAccuracyBar __instance)
    {
        if (_timingBar == null)
        {
            _timingBar = __instance.transform.parent.gameObject;
        }

        UpdateLayerCulling();
    }
    
    internal static void UpdateLayerCulling() =>
        _timingBar.SetLayerRecursively(LayerMask.NameToLayer(Plugin.ForceShowAccuracyBar.Value ? "UITop" : "Hud"));
}