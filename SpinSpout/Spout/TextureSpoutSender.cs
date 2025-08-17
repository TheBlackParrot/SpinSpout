// ReeCamera - Spout2 camera system for Beat Saber
// https://github.com/Reezonate/ReeCamera

using UnityEngine;

namespace SpinSpout.Spout;

[AddComponentMenu("Spout/TextureSpoutSender")]
public class TextureSpoutSender : AbstractSpoutSender {
    public RenderTexture sourceTexture;

    protected override void Update() {
        base.Update();
        SendTextureMode(sourceTexture);
    }
}