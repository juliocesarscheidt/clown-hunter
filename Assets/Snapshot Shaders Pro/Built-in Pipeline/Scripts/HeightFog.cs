namespace SnapshotShaders.BuiltIn
{
    using System;
    using UnityEngine;
    using UnityEngine.Rendering.PostProcessing;

    [Serializable]
    [PostProcess(typeof(HeightFogRenderer), PostProcessEvent.AfterStack, "Snapshot Pro/Height Fog")]
    public sealed class HeightFog : PostProcessEffectSettings
    {
        [Range(0.0f, 1.0f), Tooltip("How strongly to mix fog colors and original scene colors.")]
        public FloatParameter strength = new FloatParameter { value = 0.0f };

        [Tooltip("Distance away from the camera at which distance fog starts appearing.")]
        public FloatParameter startDistance = new FloatParameter { value = 20.0f };

        [ColorUsage(false, true)]
        [Tooltip("Color of the distance fog at the start distance.")]
        public ColorParameter distanceStartColor = new ColorParameter { value = new Color(0.514151f, 0.6437106f, 1.0f) };

        [Tooltip("Distance away from the camera at which distance fog is at full strength.")]
        public FloatParameter endDistance = new FloatParameter { value = 50.0f };

        [ColorUsage(false, true)]
        [Tooltip("Color of the distance fog at the end distance and beyond.")]
        public ColorParameter distanceEndColor = new ColorParameter { value = new Color(0.1745283f, 0.3942103f, 1.0f) };

        [Range(0.01f, 10.0f), Tooltip("Controls the color curve at distances between start and end.")]
        public FloatParameter distanceFalloff = new FloatParameter { value = 1.0f };

        [Tooltip("Height in world space at which height fog starts appearing.\n\nNote: this value should be higher than End Height.")]
        public FloatParameter startHeight = new FloatParameter { value = 5.0f };

        [ColorUsage(false, true)]
        [Tooltip("Color of the height fog at the start height.")]
        public ColorParameter heightStartColor = new ColorParameter { value = new Color(0.8313726f, 0.3137255f, 0.2235294f) };

        [Tooltip("Height in world space at which height fog is at full strength.\n\nNote: this value should be lower than Start Height.")]
        public FloatParameter endHeight = new FloatParameter { value = -5.0f };

        [ColorUsage(false, true)]
        [Tooltip("Color of the height fog at the end height and below.")]
        public ColorParameter heightEndColor = new ColorParameter { value = new Color(0.3294118f, 0.1254902f, 0.1254902f) };

        [Range(0.01f, 10.0f), Tooltip("Controls the color curve at height values between start and end.")]
        public FloatParameter heightFalloff = new FloatParameter { value = 1.0f };
    }

    public sealed class HeightFogRenderer : PostProcessEffectRenderer<HeightFog>
    {
        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(Shader.Find("Hidden/SnapshotPro/HeightFog"));

            var cam = context.camera;
            cam.depthTextureMode = DepthTextureMode.DepthNormals;

            var p = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false);
            p[2, 3] = p[3, 2] = 0.0f;
            p[3, 3] = 1.0f;
            var clipToWorld = Matrix4x4.Inverse(p * cam.worldToCameraMatrix) * Matrix4x4.TRS(new Vector3(0, 0, -p[2, 2]), Quaternion.identity, Vector3.one);
            sheet.properties.SetMatrix("clipToWorld", clipToWorld);

            sheet.properties.SetFloat("_FogStrength", settings.strength.value);
            sheet.properties.SetFloat("_StartDistance", settings.startDistance.value);
            sheet.properties.SetColor("_DistanceStartColor", settings.distanceStartColor.value);
            sheet.properties.SetFloat("_EndDistance", settings.endDistance.value);
            sheet.properties.SetColor("_DistanceEndColor", settings.distanceEndColor.value);
            sheet.properties.SetFloat("_DistanceFalloff", settings.distanceFalloff.value);
            sheet.properties.SetFloat("_StartHeight", settings.startHeight.value);
            sheet.properties.SetColor("_HeightStartColor", settings.heightStartColor.value);
            sheet.properties.SetFloat("_EndHeight", settings.endHeight.value);
            sheet.properties.SetColor("_HeightEndColor", settings.heightEndColor.value);
            sheet.properties.SetFloat("_HeightFalloff", settings.heightFalloff.value);

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}
