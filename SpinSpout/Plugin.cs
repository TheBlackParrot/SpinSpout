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
        HarmonyPatcher.UnpatchSelf();
        _mainCameraRenderTexture.Release();
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
        
        _mainCameraRenderTexture = new RenderTexture(Width.Value, Height.Value, 32, RenderTextureFormat.ARGB32)
        {
            filterMode = FilterMode.Bilinear,
            antiAliasing = 2
        };
        _mainCameraRenderTexture.Create();
        
        foreach (TextureSpoutSender textureSpoutSender in FindObjectsByType<TextureSpoutSender>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            textureSpoutSender.sourceTexture = _mainCameraRenderTexture;
                
            if (textureSpoutSender.gameObject.TryGetComponent(out Camera camera))
            {
                camera.targetTexture = _mainCameraRenderTexture;
            }
        }
    }

    private static Transform _previouslyActiveSpoutCameraTransform;
    private static Camera _previouslyActiveSpoutCamera;
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
        
            if (_previouslyActiveSpoutCamera != null)
            {
                _previouslyActiveSpoutCamera.fieldOfView = value;
            }
        }

        [HarmonyPatch(typeof(Skybox), nameof(Skybox.material), MethodType.Setter)]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        private static void FixSkyboxMaterial(Skybox __instance, ref Material value)
        {
            if (_previouslyActiveSpoutCameraTransform == null)
            {
                return;
            }

            if (!_previouslyActiveSpoutCameraTransform.gameObject.TryGetComponent(out Skybox skybox))
            {
                return;
            }
            
            if (skybox != __instance)
            {
                skybox.material = value;
            }
        }
    }
}