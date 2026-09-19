namespace SnapshotShaders.BuiltIn
{
    using System;
    using UnityEngine;
    using UnityEngine.Rendering.PostProcessing;

    [Serializable]
    [PostProcess(typeof(LightStreaksRenderer), PostProcessEvent.AfterStack, "Snapshot Pro/LightStreaks")]
    public class LightStreaks : PostProcessEffectSettings
    {
        [Range(3, 1000), Tooltip("LightStreaks Strength")]
        public IntParameter strength = new IntParameter { value = 250 };

        [Range(0.0f, 25.0f), Tooltip("Luminance Threshold - pixels above this luminance will glow.")]
        public FloatParameter luminanceThreshold = new FloatParameter { value = 20.0f };

        [Range(1, 128), Tooltip("Divisor to apply to the screen resolution in the x-direction for the blur pass.")]
        public IntParameter downsamples = new IntParameter { value = 24 };
    }

    public sealed class LightStreaksRenderer : PostProcessEffectRenderer<LightStreaks>
    {
        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(Shader.Find("Hidden/SnapshotPro/LightStreaks"));
            sheet.properties.SetInt("_KernelSize", settings.strength);
            sheet.properties.SetFloat("_Spread", settings.strength / 7.5f);
            sheet.properties.SetFloat("_LuminanceThreshold", settings.luminanceThreshold);

            var tmp = RenderTexture.GetTemporary(Screen.width / settings.downsamples, Screen.height, 0);

            context.command.BlitFullscreenTriangle(context.source, tmp, sheet, 0);
            sheet.properties.SetTexture("_BlurTex", tmp);
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 1);

            RenderTexture.ReleaseTemporary(tmp);
        }
    }
}
