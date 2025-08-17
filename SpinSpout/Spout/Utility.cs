// KlakSpout - Spout video frame sharing plugin for Unity
// https://github.com/keijiro/KlakSpout

// ReeCamera - Spout2 camera system for Beat Saber
// https://github.com/Reezonate/ReeCamera

using UnityEngine;
using UnityEngine.Rendering;

namespace SpinSpout.Spout;

// Internal utilities
internal static class Util {
    internal static void Destroy(Object obj) {
        if (obj == null) return;

        if (Application.isPlaying) {
            Object.Destroy(obj);
        } else {
            Object.DestroyImmediate(obj);
        }
    }

    private static CommandBuffer _commandBuffer;

    internal static void IssuePluginEvent(PluginEntry.Event pluginEvent, System.IntPtr ptr) {
        _commandBuffer ??= new CommandBuffer();

        _commandBuffer.IssuePluginEventAndData(
            PluginEntry.GetRenderEventFunc(), (int)pluginEvent, ptr
        );

        Graphics.ExecuteCommandBuffer(_commandBuffer);

        _commandBuffer.Clear();
    }
}