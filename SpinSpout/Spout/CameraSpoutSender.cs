// ReeCamera - Spout2 camera system for Beat Saber
// https://github.com/Reezonate/ReeCamera

using UnityEngine;

namespace SpinSpout.Spout;

[AddComponentMenu("Spout/CameraSpoutSender")]
public class CameraSpoutSender : AbstractSpoutSender {
    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        SendCameraMode(source, destination);
    }
}