using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SpinSpout.Patches;
using SpinSpout.Spout;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SpinSpout;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public partial class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger;
    private static readonly Harmony HarmonyPatcher = new(MyPluginInfo.PLUGIN_GUID);
    
    private static RenderTexture _mainCameraRenderTexture;
    private static RenderTexture _secondaryCameraRenderTexture;
    private static RenderTexture _spectatorCameraRenderTexture;

    private static Shader _blitShader;

    private void Awake()
    {
        Logger = base.Logger;
        
        SpoutLoader.LoadPlugin();

        RegisterConfigEntries();
        CreateModPage();
        
        Logger.LogInfo("Plugin loaded");
    }

    private void OnEnable()
    {
        HarmonyPatcher.PatchAll(typeof(CameraPatches));
        HarmonyPatcher.PatchAll(typeof(TimingBarPatches));
        
        StartCoroutine(Utils.AssetBundleUtils.LoadShaderAsset($"{nameof(SpinSpout)}.KlakSpoutBlitShader.assetbundle", "Assets/Blit.shader",
            shader =>
            {
                _blitShader = shader;
            }));
        
        UpdateRenderTexture();
        
        MainCamera.OnCurrentCameraChanged += MainCameraOnOnCurrentCameraChanged;
    }

    private void OnDestroy()
    {
        _mainCameraRenderTexture.Release();
        _secondaryCameraRenderTexture.Release();
        _spectatorCameraRenderTexture.Release();
        
        foreach (TextureSpoutSender textureSpoutSender in FindObjectsByType<TextureSpoutSender>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            textureSpoutSender.sourceTexture = null;
                
            if (textureSpoutSender.gameObject.TryGetComponent(out Camera camera))
            {
                camera.targetTexture = null;
            }
        }
        
        HarmonyPatcher.UnpatchSelf();
    }

    private static void UpdateRenderTexture()
    {
        Logger.LogInfo("Updating _mainCameraRenderTexture...");
        
        foreach (TextureSpoutSender textureSpoutSender in FindObjectsByType<TextureSpoutSender>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            textureSpoutSender.sourceTexture = null;
                
            if (textureSpoutSender.gameObject.TryGetComponent(out Camera camera))
            {
                camera.targetTexture = null;
            }
        }
        
        _mainCameraRenderTexture?.Release();
        _secondaryCameraRenderTexture?.Release();
        _spectatorCameraRenderTexture?.Release();
        
        _mainCameraRenderTexture = new RenderTexture(Width.Value, Height.Value, 16, RenderTextureFormat.ARGB32)
        {
            filterMode = FilterMode.Point,
            antiAliasing = 1
        };
        _mainCameraRenderTexture.Create();
        
        _secondaryCameraRenderTexture = new RenderTexture(SecondaryWidth.Value, SecondaryHeight.Value, 16, RenderTextureFormat.ARGB32)
        {
            filterMode = FilterMode.Point,
            antiAliasing = 1
        };
        _secondaryCameraRenderTexture.Create();
        
        _spectatorCameraRenderTexture = new RenderTexture(VRSpectatorWidth.Value, VRSpectatorHeight.Value, 16, RenderTextureFormat.ARGB32)
        {
            filterMode = FilterMode.Point,
            antiAliasing = 1
        };
        _spectatorCameraRenderTexture.Create();
        
        foreach (TextureSpoutSender textureSpoutSender in FindObjectsByType<TextureSpoutSender>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (textureSpoutSender.gameObject.name == "Spectator Cam")
            {
                textureSpoutSender.sourceTexture = _spectatorCameraRenderTexture;
                if (textureSpoutSender.gameObject.TryGetComponent(out Camera vrCamera))
                {
                    vrCamera.targetTexture = _spectatorCameraRenderTexture;
                }
                continue;
            }
            
            bool isMainCamera = textureSpoutSender.gameObject.name == "MainCameraSpoutObject(Clone)";
            textureSpoutSender.sourceTexture = isMainCamera ? _mainCameraRenderTexture : _secondaryCameraRenderTexture;
                
            if (textureSpoutSender.gameObject.TryGetComponent(out Camera camera))
            {
                camera.targetTexture = isMainCamera ? _mainCameraRenderTexture : _secondaryCameraRenderTexture;
            }
        }
    }

    private static void UpdateCameraTransforms()
    {
        if (PreviouslyActiveSpoutCameraTransform != null)
        {
            PreviouslyActiveSpoutCameraTransform.localPosition = Offset.Value;
            PreviouslyActiveSpoutCameraTransform.localRotation = Quaternion.Euler(Rotation.Value);
        }
        
        if (PreviouslyActiveSecondarySpoutCameraTransform != null)
        {
            PreviouslyActiveSecondarySpoutCameraTransform.localPosition = SecondaryOffset.Value;
            PreviouslyActiveSecondarySpoutCameraTransform.localRotation = Quaternion.Euler(SecondaryRotation.Value);
        }
    }

    private static void UpdateCameraFieldOfViews()
    {
        if (PreviouslyActiveSpoutCamera == null ||
            PreviouslyActiveSecondarySpoutCamera == null ||
            ActiveCamera == null)
        {
            return;
        }

        PreviouslyActiveSpoutCamera.fieldOfView = FieldOfViewIsStatic.Value
            ? FieldOfView.Value
            : ActiveCamera.fieldOfView;
        
        PreviouslyActiveSecondarySpoutCamera.fieldOfView = SecondaryFieldOfViewIsStatic.Value
            ? SecondaryFieldOfView.Value
            : ActiveCamera.fieldOfView;
    }
    
    private static void UpdateCameraHudCulling()
    {
        if (PreviouslyActiveSpoutCamera == null ||
            PreviouslyActiveSecondarySpoutCamera == null ||
            ActiveCamera == null)
        {
            return;
        }

        List<string> primaryLayerNames = [];
        if (!ShowHud.Value) { primaryLayerNames.Add("Hud"); }
        if (!ShowUi.Value) { primaryLayerNames.Add("UI"); }
        
        List<string> secondaryLayerNames = [];
        if (!SecondaryShowHud.Value) { secondaryLayerNames.Add("Hud"); }
        if (!SecondaryShowUi.Value) { secondaryLayerNames.Add("UI"); }
        
        int primaryHudLayerMask = ~LayerMask.GetMask(primaryLayerNames.ToArray());
        int secondaryHudLayerMask = ~LayerMask.GetMask(secondaryLayerNames.ToArray());
        
        PreviouslyActiveSpoutCamera.cullingMask = primaryHudLayerMask;
        PreviouslyActiveSecondarySpoutCamera.cullingMask = secondaryHudLayerMask;
    }

    internal static void UpdateVRSpectatorCamera()
    {
        XROrigin xrOrigin = FindAnyObjectByType<XROrigin>();
        Camera camera = xrOrigin?.CameraFloorOffsetObject.transform.Find("Spectator Cam Stable/Spectator Cam")?.GetComponent<Camera>();
        
        if (camera == null)
        {
            Logger.LogInfo("UpdateVRSpectatorCamera -- targeted camera is null");
            return;
        }

        if (camera.TryGetComponent(out TextureSpoutSender textureSpoutSender))
        {
            Logger.LogInfo("UpdateVRSpectatorCamera -- TextureSpoutSender found");
            textureSpoutSender.enabled = TakeOverVRSpectatorCamera.Value;
            camera.targetTexture = TakeOverVRSpectatorCamera.Value ? _spectatorCameraRenderTexture : null;
            return;
        }
        
        TextureSpoutSender spectatorCameraSpoutSender = camera.gameObject.AddComponent<TextureSpoutSender>();
        spectatorCameraSpoutSender.sourceTexture = _spectatorCameraRenderTexture;
        spectatorCameraSpoutSender.blitShader = _blitShader;
        spectatorCameraSpoutSender.channelName = "SpinSpout_VRSpectatorCamera";
        spectatorCameraSpoutSender.AlphaSupport = false;
        spectatorCameraSpoutSender.enabled = TakeOverVRSpectatorCamera.Value;
    }

    internal static Transform PreviouslyActiveSpoutCameraTransform;
    internal static Camera PreviouslyActiveSpoutCamera;
    internal static Transform PreviouslyActiveSecondarySpoutCameraTransform;
    internal static Camera PreviouslyActiveSecondarySpoutCamera;
    internal static Camera ActiveCamera;
    private static void MainCameraOnOnCurrentCameraChanged(Camera originalCamera)
    {
        Logger.LogInfo($"MainCameraOnOnCurrentCameraChanged triggered on {originalCamera.name}");
        
        if (ActiveCamera == originalCamera && ActiveCamera != null)
        {
            Logger.LogInfo("Camera is the same as the previous camera, not updating");
            return;
        }
        ActiveCamera = originalCamera;
        
        #region (primary camera)
        Transform currentlyActiveSpoutCameraTransform = originalCamera.transform.Find("MainCameraSpoutObject(Clone)");
        if (currentlyActiveSpoutCameraTransform == null)
        {
            Logger.LogInfo($"Creating Spout2 camera on object {originalCamera.name}...");
            
            currentlyActiveSpoutCameraTransform = Instantiate(new GameObject("MainCameraSpoutObject"), originalCamera.gameObject.transform).transform;
            GameObject currentlyActiveSpoutCameraObject = currentlyActiveSpoutCameraTransform.gameObject;
            currentlyActiveSpoutCameraObject.tag = "MainCamera";
            
            currentlyActiveSpoutCameraObject.AddComponent<Skybox>();
            
            Camera mainCamera = currentlyActiveSpoutCameraObject.AddComponent<Camera>();
            mainCamera.CopyFrom(originalCamera);
            mainCamera.targetTexture = _mainCameraRenderTexture;
            
            UniversalAdditionalCameraData originalCameraData = originalCamera.GetComponent<UniversalAdditionalCameraData>();
            UniversalAdditionalCameraData addtlCameraData = mainCamera.GetUniversalAdditionalCameraData();
            addtlCameraData.volumeStack = originalCameraData.volumeStack;
            addtlCameraData.volumeLayerMask = originalCameraData.volumeLayerMask;
            addtlCameraData.stopNaN = originalCameraData.stopNaN;
            addtlCameraData.renderPostProcessing = true;
            addtlCameraData.allowHDROutput = true;
            
            TextureSpoutSender mainCameraSpoutSender = mainCamera.gameObject.AddComponent<TextureSpoutSender>();
            mainCameraSpoutSender.sourceTexture = _mainCameraRenderTexture;
            mainCameraSpoutSender.blitShader = _blitShader;
            mainCameraSpoutSender.channelName = "SpinSpout_MainCamera";
            mainCameraSpoutSender.AlphaSupport = false;
            mainCameraSpoutSender.enabled = Enabled.Value;
            PreviouslyActiveSpoutCamera = mainCamera;
            
            Logger.LogInfo($"Created Spout2 camera on object {originalCamera.name}");
        }
        else
        {
            Logger.LogInfo($"Spout2 camera on object {originalCamera.name} already exists");
            
            currentlyActiveSpoutCameraTransform.gameObject.SetActive(true);
            PreviouslyActiveSpoutCamera = currentlyActiveSpoutCameraTransform.gameObject.GetComponent<Camera>();
        }
        
        if (PreviouslyActiveSpoutCameraTransform != null)
        {
            PreviouslyActiveSpoutCameraTransform.gameObject.SetActive(false);
            Logger.LogInfo("Disabled inactive Spout2 camera");
        }
        PreviouslyActiveSpoutCameraTransform = currentlyActiveSpoutCameraTransform;
        #endregion (primary camera)
        
        #region (secondary camera)
        currentlyActiveSpoutCameraTransform = originalCamera.transform.Find("SecondaryCameraSpoutObject(Clone)");
        if (currentlyActiveSpoutCameraTransform == null)
        {
            Logger.LogInfo($"Creating secondary Spout2 camera on object {originalCamera.name}...");
            
            currentlyActiveSpoutCameraTransform = Instantiate(new GameObject("SecondaryCameraSpoutObject"), originalCamera.gameObject.transform).transform;
            GameObject currentlyActiveSpoutCameraObject = currentlyActiveSpoutCameraTransform.gameObject;
            currentlyActiveSpoutCameraObject.tag = "MainCamera";
            
            currentlyActiveSpoutCameraObject.AddComponent<Skybox>();
            
            Camera mainCamera = currentlyActiveSpoutCameraObject.AddComponent<Camera>();
            mainCamera.CopyFrom(originalCamera);
            mainCamera.targetTexture = _secondaryCameraRenderTexture;
            
            UniversalAdditionalCameraData originalCameraData = originalCamera.GetComponent<UniversalAdditionalCameraData>();
            UniversalAdditionalCameraData addtlCameraData = currentlyActiveSpoutCameraObject.AddComponent<UniversalAdditionalCameraData>();
            addtlCameraData.volumeStack = originalCameraData.volumeStack;
            addtlCameraData.volumeLayerMask = originalCameraData.volumeLayerMask;
            addtlCameraData.stopNaN = originalCameraData.stopNaN;
            addtlCameraData.renderPostProcessing = true;
            addtlCameraData.allowHDROutput = true;
            
            TextureSpoutSender mainCameraSpoutSender = mainCamera.gameObject.AddComponent<TextureSpoutSender>();
            mainCameraSpoutSender.sourceTexture = _secondaryCameraRenderTexture;
            mainCameraSpoutSender.blitShader = _blitShader;
            mainCameraSpoutSender.channelName = "SpinSpout_SecondaryCamera";
            mainCameraSpoutSender.AlphaSupport = false;
            mainCameraSpoutSender.enabled = Enabled.Value;
            PreviouslyActiveSecondarySpoutCamera = mainCamera;
            
            Logger.LogInfo($"Created secondary Spout2 camera on object {originalCamera.name}");
        }
        else
        {
            Logger.LogInfo($"Secondary Spout2 camera on object {originalCamera.name} already exists");
            
            currentlyActiveSpoutCameraTransform.gameObject.SetActive(true);
            PreviouslyActiveSecondarySpoutCamera = currentlyActiveSpoutCameraTransform.gameObject.GetComponent<Camera>();
        }
        
        if (PreviouslyActiveSecondarySpoutCameraTransform != null)
        {
            PreviouslyActiveSecondarySpoutCameraTransform.gameObject.SetActive(false);
            Logger.LogInfo("Disabled inactive secondary Spout2 camera");
        }
        PreviouslyActiveSecondarySpoutCameraTransform = currentlyActiveSpoutCameraTransform;
        #endregion (secondary camera)

        UpdateCameraTransforms();
        UpdateCameraFieldOfViews();
        UpdateCameraHudCulling();
    }
}