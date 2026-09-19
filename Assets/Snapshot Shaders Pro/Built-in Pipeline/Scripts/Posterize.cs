namespace SnapshotShaders.BuiltIn
{
    using System;
    using UnityEngine;
    using UnityEngine.Rendering.PostProcessing;

    [Serializable]
    [PostProcess(typeof(PosterizeRenderer), PostProcessEvent.AfterStack, "Snapshot Pro/Posterize")]
    public sealed class Posterize : PostProcessEffectSettings
    {
        [Tooltip("How many red levels are supported.")]
        public IntParameter redLevels = new IntParameter { value = 2 };

        [Tooltip("How many green levels are supported.")]
        public IntParameter greenLevels = new IntParameter { value = 2 };

        [Tooltip("How many blue levels are supported.")]
        public IntParameter blueLevels = new IntParameter { value = 2 };

        [Tooltip("Modify the input colors via a power ramp. 1 = original mapping, " +
            "higher = favors darker output, lower = favors lighter output.")]
        public FloatParameter powerRamp = new FloatParameter { value = 1.0f };
    }

    public sealed class PosterizeRenderer : PostProcessEffectRenderer<Posterize>
    {
        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(Shader.Find("Hidden/SnapshotPro/Posterize"));
            sheet.properties.SetInt("_RedLevels", settings.redLevels);
            sheet.properties.SetInt("_GreenLevels", settings.greenLevels);
            sheet.properties.SetInt("_BlueLevels", settings.blueLevels);
            sheet.properties.SetFloat("_PowerRamp", settings.powerRamp);
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}
