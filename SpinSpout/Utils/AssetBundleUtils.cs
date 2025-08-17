using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace SpinSpout.Utils;

// https://stackoverflow.com/a/47033537
public abstract class AssetBundleUtils
{
    public static IEnumerator LoadShaderAsset(string resourcePath, string filepath, Action<Shader> callback = null)
    {
        AssetBundle assetBundleCreateRequest = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath));
        yield return assetBundleCreateRequest;
        
        AssetBundleRequest asset = assetBundleCreateRequest.LoadAssetAsync<Shader>(filepath);
        Shader shader = asset.asset as Shader;
        yield return shader;
        
        callback?.Invoke(shader);
    }
}