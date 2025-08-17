// KlakSpout - Spout video frame sharing plugin for Unity
// https://github.com/keijiro/KlakSpout

// ReeCamera - Spout2 camera system for Beat Saber
// https://github.com/Reezonate/ReeCamera

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SpinSpout.Spout;

internal static class PluginEntry {
    internal enum Event {
        Update,
        Dispose
    }

    internal static bool IsAvailable => SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D11;

    [DllImport("KlakSpout")]
    internal static extern IntPtr GetRenderEventFunc();

    [DllImport("KlakSpout")]
    internal static extern IntPtr CreateSender(string name, int width, int height);

    [DllImport("KlakSpout")]
    internal static extern IntPtr GetTexturePointer(IntPtr ptr);

    [DllImport("KlakSpout")]
    internal static extern int GetTextureWidth(IntPtr ptr);

    [DllImport("KlakSpout")]
    internal static extern int GetTextureHeight(IntPtr ptr);
}