using HarmonyLib;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace SpinSpout.Patches;

[HarmonyPatch]
internal class CameraPatches
{
    [HarmonyPatch(typeof(Camera), nameof(Camera.fieldOfView), MethodType.Setter)]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void FixFieldOfView(Camera __instance, ref float value)
    {
        if (__instance != Plugin.ActiveCamera)
        {
            return;
        }
        
        if (Plugin.PreviouslyActiveSpoutCamera != null && !Plugin.FieldOfViewIsStatic.Value)
        {
            Plugin.PreviouslyActiveSpoutCamera.fieldOfView = value;
        }
        if (Plugin.PreviouslyActiveSecondarySpoutCamera != null && !Plugin.SecondaryFieldOfViewIsStatic.Value)
        {
            Plugin.PreviouslyActiveSecondarySpoutCamera.fieldOfView = value;
        }
    }

    [HarmonyPatch(typeof(Skybox), nameof(Skybox.material), MethodType.Setter)]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void FixSkyboxMaterial(Skybox __instance, ref Material value)
    {
        if (__instance.gameObject.name.Contains("Spout"))
        {
            return;
        }

        Plugin.PreviouslyActiveSpoutCameraTransform.gameObject.TryGetComponent(out Skybox skybox);
        Plugin.PreviouslyActiveSecondarySpoutCameraTransform.gameObject.TryGetComponent(out Skybox secondarySkybox);

        if (skybox == __instance)
        {
            return;
        }
            
        skybox.material = value;
        secondarySkybox.material = value;
    }

    [HarmonyPatch(typeof(XROrigin), nameof(XROrigin.TryInitializeCamera))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void PatchSpectatorCamera(XROrigin __instance)
    {
        if (!__instance.m_CameraInitialized)
        {
            return;
        }

        Plugin.UpdateVRSpectatorCamera();
    }
}