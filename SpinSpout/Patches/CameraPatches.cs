using HarmonyLib;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace SpinSpout.Patches;

[HarmonyPatch]
internal class CameraPatches
{
    private static Camera ActiveCamera => Plugin.ActiveCamera;
    private static Camera PreviouslyActiveSpoutCamera => Plugin.PreviouslyActiveSpoutCamera;
    private static Camera PreviouslyActiveSecondarySpoutCamera => Plugin.PreviouslyActiveSecondarySpoutCamera;
    private static Transform PreviouslyActiveSpoutCameraTransform => Plugin.PreviouslyActiveSpoutCameraTransform;
    private static Transform PreviouslyActiveSecondarySpoutCameraTransform => Plugin.PreviouslyActiveSecondarySpoutCameraTransform;
    
    [HarmonyPatch(typeof(Camera), nameof(Camera.fieldOfView), MethodType.Setter)]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void FixFieldOfView(Camera __instance, ref float value)
    {
        if (__instance != ActiveCamera)
        {
            return;
        }
        
        if (PreviouslyActiveSpoutCamera != null && !Plugin.FieldOfViewIsStatic.Value)
        {
            PreviouslyActiveSpoutCamera.fieldOfView = value;
        }
        if (PreviouslyActiveSecondarySpoutCamera != null && !Plugin.SecondaryFieldOfViewIsStatic.Value)
        {
            PreviouslyActiveSecondarySpoutCamera.fieldOfView = value;
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

        PreviouslyActiveSpoutCameraTransform.gameObject.TryGetComponent(out Skybox skybox);
        PreviouslyActiveSecondarySpoutCameraTransform.gameObject.TryGetComponent(out Skybox secondarySkybox);

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