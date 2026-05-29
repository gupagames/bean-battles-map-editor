using System;
using UnityEngine;

namespace GG.BeanBattles.MapEditor
{
    [Serializable]
    public class EditorMapLightmapData
    {
        public Texture2D LightmapColor;
        public Texture2D LightmapDir;
        public Texture2D ShadowMask;
    }

    [Serializable]
    public class EditorMapLighting
    {
        [HideInInspector] public Material Skybox;
        [HideInInspector] public Light Sun;
        // environment
        [HideInInspector] public Color AmbientLight;
        [HideInInspector] public float AmbientIntensity;

        // fog
        [HideInInspector] public bool FogEnabled;
        [HideInInspector] public Color FogColor;
        [HideInInspector] public FogMode FogMode;
        [HideInInspector] public float FogDensity;
        [HideInInspector] public float FogStartDistance;
        [HideInInspector] public float FogEndDistance;

        // lightmaps
        [HideInInspector] public LightmapsMode LightmapsMode;
        [HideInInspector] public EditorMapLightmapData[] Lightmaps;

        public void ExportLighting()
        {
#if UNITY_EDITOR
            Skybox = RenderSettings.skybox;
            Sun = RenderSettings.sun;

            AmbientLight = RenderSettings.ambientLight;
            AmbientIntensity = RenderSettings.ambientIntensity;

            FogEnabled = RenderSettings.fog;
            FogColor = RenderSettings.fogColor;
            FogMode = RenderSettings.fogMode;
            FogDensity = RenderSettings.fogDensity;
            FogStartDistance = RenderSettings.fogStartDistance;
            FogEndDistance = RenderSettings.fogEndDistance;

            LightmapsMode = LightmapSettings.lightmapsMode;
            Lightmaps = new EditorMapLightmapData[LightmapSettings.lightmaps.Length];
            for (int i = 0; i < LightmapSettings.lightmaps.Length; i++)
            {
                Lightmaps[i] = new EditorMapLightmapData
                {
                    LightmapColor = LightmapSettings.lightmaps[i].lightmapColor,
                    LightmapDir = LightmapSettings.lightmaps[i].lightmapDir,
                    ShadowMask = LightmapSettings.lightmaps[i].shadowMask
                };
            }
#endif
        }

        public void ApplyLighting()
        {
            if (Skybox != null) RenderSettings.skybox = Skybox;
            if (Sun != null) RenderSettings.sun = Sun;

            RenderSettings.ambientLight = AmbientLight;
            RenderSettings.ambientIntensity = AmbientIntensity;

            RenderSettings.fog = FogEnabled;
            RenderSettings.fogColor = FogColor;
            RenderSettings.fogMode = FogMode;
            RenderSettings.fogDensity = FogDensity;
            RenderSettings.fogStartDistance = FogStartDistance;
            RenderSettings.fogEndDistance = FogEndDistance;

            if (Lightmaps == null || Lightmaps.Length == 0) return;

            LightmapSettings.lightmapsMode = LightmapsMode;
            LightmapData[] lightmaps = new LightmapData[Lightmaps.Length];

            for (int i = 0; i < lightmaps.Length; i++)
            {
                lightmaps[i] = new LightmapData
                {
                    lightmapColor = Lightmaps[i].LightmapColor,
                    lightmapDir = Lightmaps[i].LightmapDir,
                    shadowMask = Lightmaps[i].ShadowMask
                };
            }

            LightmapSettings.lightmaps = lightmaps;
        }
    }
}