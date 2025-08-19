using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SpinSpout.Spout;
using UnityEngine;

namespace SpinSpout;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public partial class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger;
    private static readonly Harmony HarmonyPatcher = new(MyPluginInfo.PLUGIN_GUID);
    
    private static RenderTexture _mainCameraRenderTexture;
    private static RenderTexture _secondaryCameraRenderTexture;

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
        HarmonyPatcher.PatchAll(typeof(PatchWrapper));
        
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
        
        _mainCameraRenderTexture = new RenderTexture(Width.Value, Height.Value, 32, RenderTextureFormat.ARGB32)
        {
            filterMode = FilterMode.Bilinear,
            antiAliasing = 2
        };
        _mainCameraRenderTexture.Create();
        
        _secondaryCameraRenderTexture = new RenderTexture(SecondaryWidth.Value, SecondaryHeight.Value, 32, RenderTextureFormat.ARGB32)
        {
            filterMode = FilterMode.Bilinear,
            antiAliasing = 2
        };
        _secondaryCameraRenderTexture.Create();
        
        foreach (TextureSpoutSender textureSpoutSender in FindObjectsByType<TextureSpoutSender>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
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
        if (_previouslyActiveSpoutCameraTransform != null)
        {
            _previouslyActiveSpoutCameraTransform.localPosition = Offset.Value;
            _previouslyActiveSpoutCameraTransform.localRotation = Quaternion.Euler(Rotation.Value);
        }
        
        if (_previouslyActiveSecondarySpoutCameraTransform != null)
        {
            _previouslyActiveSecondarySpoutCameraTransform.localPosition = SecondaryOffset.Value;
            _previouslyActiveSecondarySpoutCameraTransform.localRotation = Quaternion.Euler(SecondaryRotation.Value);
        }
    }

    private static void UpdateCameraFieldOfViews()
    {
        if (_previouslyActiveSpoutCamera == null ||
            _previouslyActiveSecondarySpoutCamera == null ||
            _activeCamera == null)
        {
            return;
        }

        _previouslyActiveSpoutCamera.fieldOfView = FieldOfViewIsStatic.Value
            ? FieldOfView.Value
            : _activeCamera.fieldOfView;
        
        _previouslyActiveSecondarySpoutCamera.fieldOfView = SecondaryFieldOfViewIsStatic.Value
            ? SecondaryFieldOfView.Value
            : _activeCamera.fieldOfView;
    }

    private static readonly int HudLayerMask = ~LayerMask.GetMask("Hud", "UI");
    private static void UpdateCameraHudCulling()
    {
        if (_previouslyActiveSpoutCamera == null ||
            _previouslyActiveSecondarySpoutCamera == null ||
            _activeCamera == null)
        {
            return;
        }
        
        _previouslyActiveSpoutCamera.cullingMask = ShowHud.Value ? _activeCamera.cullingMask : HudLayerMask;
        _previouslyActiveSecondarySpoutCamera.cullingMask = SecondaryShowHud.Value ? _activeCamera.cullingMask : HudLayerMask;
    }

    private static Transform _previouslyActiveSpoutCameraTransform;
    private static Camera _previouslyActiveSpoutCamera;
    private static Transform _previouslyActiveSecondarySpoutCameraTransform;
    private static Camera _previouslyActiveSecondarySpoutCamera;
    private static Camera _activeCamera;
    private static void MainCameraOnOnCurrentCameraChanged(Camera originalCamera)
    {
        Logger.LogInfo($"MainCameraOnOnCurrentCameraChanged triggered on {originalCamera.name}");
        
        if (_activeCamera == originalCamera && _activeCamera != null)
        {
            Logger.LogInfo("Camera is the same as the previous camera, not updating");
            return;
        }
        _activeCamera = originalCamera;
        
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
            
            TextureSpoutSender mainCameraSpoutSender = mainCamera.gameObject.AddComponent<TextureSpoutSender>();
            mainCameraSpoutSender.sourceTexture = _mainCameraRenderTexture;
            mainCameraSpoutSender.blitShader = _blitShader;
            mainCameraSpoutSender.channelName = "SpinSpout_MainCamera";
            mainCameraSpoutSender.AlphaSupport = false;
            mainCameraSpoutSender.enabled = Enabled.Value;
            _previouslyActiveSpoutCamera = mainCamera;
            
            Logger.LogInfo($"Created Spout2 camera on object {originalCamera.name}");
        }
        else
        {
            Logger.LogInfo($"Spout2 camera on object {originalCamera.name} already exists");
            
            currentlyActiveSpoutCameraTransform.gameObject.SetActive(true);
            _previouslyActiveSpoutCamera = currentlyActiveSpoutCameraTransform.gameObject.GetComponent<Camera>();
        }
        
        if (_previouslyActiveSpoutCameraTransform != null)
        {
            _previouslyActiveSpoutCameraTransform.gameObject.SetActive(false);
            Logger.LogInfo("Disabled inactive Spout2 camera");
        }
        _previouslyActiveSpoutCameraTransform = currentlyActiveSpoutCameraTransform;
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
            
            TextureSpoutSender mainCameraSpoutSender = mainCamera.gameObject.AddComponent<TextureSpoutSender>();
            mainCameraSpoutSender.sourceTexture = _secondaryCameraRenderTexture;
            mainCameraSpoutSender.blitShader = _blitShader;
            mainCameraSpoutSender.channelName = "SpinSpout_SecondaryCamera";
            mainCameraSpoutSender.AlphaSupport = false;
            mainCameraSpoutSender.enabled = Enabled.Value;
            _previouslyActiveSecondarySpoutCamera = mainCamera;
            
            Logger.LogInfo($"Created secondary Spout2 camera on object {originalCamera.name}");
        }
        else
        {
            Logger.LogInfo($"Secondary Spout2 camera on object {originalCamera.name} already exists");
            
            currentlyActiveSpoutCameraTransform.gameObject.SetActive(true);
            _previouslyActiveSecondarySpoutCamera = currentlyActiveSpoutCameraTransform.gameObject.GetComponent<Camera>();
        }
        
        if (_previouslyActiveSecondarySpoutCameraTransform != null)
        {
            _previouslyActiveSecondarySpoutCameraTransform.gameObject.SetActive(false);
            Logger.LogInfo("Disabled inactive secondary Spout2 camera");
        }
        _previouslyActiveSecondarySpoutCameraTransform = currentlyActiveSpoutCameraTransform;
        #endregion (secondary camera)

        UpdateCameraTransforms();
        UpdateCameraFieldOfViews();
        UpdateCameraHudCulling();
    }

    [HarmonyPatch]
    internal class PatchWrapper
    {
        [HarmonyPatch(typeof(Camera), nameof(Camera.fieldOfView), MethodType.Setter)]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        private static void FixFieldOfView(Camera __instance, ref float value)
        {
            if (__instance != _activeCamera)
            {
                return;
            }
        
            if (_previouslyActiveSpoutCamera != null && !FieldOfViewIsStatic.Value)
            {
                _previouslyActiveSpoutCamera.fieldOfView = value;
            }
            if (_previouslyActiveSecondarySpoutCamera != null && !SecondaryFieldOfViewIsStatic.Value)
            {
                _previouslyActiveSecondarySpoutCamera.fieldOfView = value;
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

            _previouslyActiveSpoutCameraTransform.gameObject.TryGetComponent(out Skybox skybox);
            _previouslyActiveSecondarySpoutCameraTransform.gameObject.TryGetComponent(out Skybox secondarySkybox);

            if (skybox == __instance)
            {
                return;
            }
            
            skybox.material = value;
            secondarySkybox.material = value;
        }
    }
}