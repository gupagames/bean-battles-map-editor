using System;
using UnityEngine;
using UnityEngine.Rendering;

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
        // skybox / sun
        [HideInInspector] public Material Skybox;
        [HideInInspector] public Light Sun;

        // ambient
        [HideInInspector] public AmbientMode AmbientMode;
        [HideInInspector] public Color AmbientLight;
        [HideInInspector] public Color AmbientSkyColor;
        [HideInInspector] public Color AmbientEquatorColor;
        [HideInInspector] public Color AmbientGroundColor;
        [HideInInspector] public float AmbientIntensity;

        // reflections
        [HideInInspector] public DefaultReflectionMode ReflectionMode;
        [HideInInspector] public int ReflectionResolution;
        [HideInInspector] public float ReflectionIntensity;
        [HideInInspector] public int ReflectionBounces;
        [HideInInspector] public Cubemap CustomReflection;

        // fog
        [HideInInspector] public bool FogEnabled;
        [HideInInspector] public Color FogColor;
        [HideInInspector] public FogMode FogMode;
        [HideInInspector] public float FogDensity;
        [HideInInspector] public float FogStartDistance;
        [HideInInspector] public float FogEndDistance;

        // subtractive lighting
        [HideInInspector] public Color SubtractiveShadowColor;

        // flare / halo
        [HideInInspector] public float HaloStrength;
        [HideInInspector] public float FlareStrength;
        [HideInInspector] public float FlareFadeSpeed;

        // lightmaps
        [HideInInspector] public LightmapsMode LightmapsMode;
        [HideInInspector] public EditorMapLightmapData[] Lightmaps;

        public void ExportLighting()
        {
#if UNITY_EDITOR
            // skybox / sun
            Skybox = RenderSettings.skybox;
            Sun = RenderSettings.sun;

            // ambient
            AmbientMode = RenderSettings.ambientMode;
            AmbientLight = RenderSettings.ambientLight;
            AmbientSkyColor = RenderSettings.ambientSkyColor;
            AmbientEquatorColor = RenderSettings.ambientEquatorColor;
            AmbientGroundColor = RenderSettings.ambientGroundColor;
            AmbientIntensity = RenderSettings.ambientIntensity;

            // reflections
            ReflectionMode = RenderSettings.defaultReflectionMode;
            ReflectionResolution = RenderSettings.defaultReflectionResolution;
            ReflectionIntensity = RenderSettings.reflectionIntensity;
            ReflectionBounces = RenderSettings.reflectionBounces;
            CustomReflection = RenderSettings.customReflection;

            // fog
            FogEnabled = RenderSettings.fog;
            FogColor = RenderSettings.fogColor;
            FogMode = RenderSettings.fogMode;
            FogDensity = RenderSettings.fogDensity;
            FogStartDistance = RenderSettings.fogStartDistance;
            FogEndDistance = RenderSettings.fogEndDistance;

            // subtractive
            SubtractiveShadowColor = RenderSettings.subtractiveShadowColor;

            // flare / halo
            HaloStrength = RenderSettings.haloStrength;
            FlareStrength = RenderSettings.flareStrength;
            FlareFadeSpeed = RenderSettings.flareFadeSpeed;

            // lightmaps
            LightmapsMode = LightmapSettings.lightmapsMode;

            Lightmaps = new EditorMapLightmapData[LightmapSettings.lightmaps.Length];

            for (int i = 0; i < Lightmaps.Length; i++)
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
            // skybox / sun
            if (Skybox != null)  RenderSettings.skybox = Skybox;
            if (Sun != null) RenderSettings.sun = Sun;

            // ambient
            RenderSettings.ambientMode = AmbientMode;
            RenderSettings.ambientLight = AmbientLight;
            RenderSettings.ambientSkyColor = AmbientSkyColor;
            RenderSettings.ambientEquatorColor = AmbientEquatorColor;
            RenderSettings.ambientGroundColor = AmbientGroundColor;
            RenderSettings.ambientIntensity = AmbientIntensity;

            // reflections
            RenderSettings.defaultReflectionMode = ReflectionMode;
            RenderSettings.defaultReflectionResolution = ReflectionResolution;
            RenderSettings.reflectionIntensity = ReflectionIntensity;
            RenderSettings.reflectionBounces = ReflectionBounces;
            RenderSettings.customReflection = CustomReflection;

            // fog
            RenderSettings.fog = FogEnabled;
            RenderSettings.fogColor = FogColor;
            RenderSettings.fogMode = FogMode;
            RenderSettings.fogDensity = FogDensity;
            RenderSettings.fogStartDistance = FogStartDistance;
            RenderSettings.fogEndDistance = FogEndDistance;

            // subtractive
            RenderSettings.subtractiveShadowColor = SubtractiveShadowColor;

            // flare / halo
            RenderSettings.haloStrength = HaloStrength;
            RenderSettings.flareStrength = FlareStrength;
            RenderSettings.flareFadeSpeed = FlareFadeSpeed;

            // lightmaps
            if (Lightmaps != null && Lightmaps.Length > 0)
            {
                LightmapSettings.lightmapsMode = LightmapsMode;

                LightmapData[] lightmaps = new LightmapData[Lightmaps.Length];

                for (int i = 0; i < Lightmaps.Length; i++)
                {
                    lightmaps[i] = new LightmapData();

                    lightmaps[i].lightmapColor = Lightmaps[i].LightmapColor;
                    lightmaps[i].lightmapDir = Lightmaps[i].LightmapDir;
                    lightmaps[i].shadowMask = Lightmaps[i].ShadowMask;
                }

                LightmapSettings.lightmaps = lightmaps;
            }

            DynamicGI.UpdateEnvironment();
        }
    }
}