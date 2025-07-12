using EFT.Settings.Graphics;
using Fika.Core.Patching;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace Fika.Headless.Patches
{
    public class SettingsPatch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GClass2135)
                .GetMethod(nameof(GClass2135.smethod_2));
        }

        [PatchPrefix]
        public static bool Prefix(ref Task __result)
        {
            __result = FikaHeadlessSettingsManager.Initalize();
            return false;
        }
    }

    public static class FikaHeadlessSettingsManager
    {
        private static bool _hasSet;

        internal static async Task Initalize()
        {
            await Class1726.smethod_5("LoadUserSettings", false);
            await SetSettings(await SharedGameSettingsClass.InstantiateSingleton());
            await GClass897.InstantiateSingleton();
            await Task.Yield();
        }

        internal static async Task SetSettings(SharedGameSettingsClass gameSettings)
        {
            if (_hasSet)
            {
                return;
            }

            if (gameSettings == null)
            {
                return;
            }

            FikaHeadlessPlugin.FikaHeadlessLogger.LogInfo("Setting graphics and volume");

            await gameSettings.Sound.Settings.OverallVolume.SetValue(0);
            await gameSettings.Sound.Settings.BinauralSound.SetValue(false);
            await gameSettings.Sound.Settings.VoipEnabled.SetValue(false);

            await gameSettings.Graphics.Settings.VSync.SetValue(false);
            await gameSettings.Graphics.Settings.ShadowsQuality.SetValue(0);
            await gameSettings.Graphics.Settings.TextureQuality.SetValue(0);
            await gameSettings.Graphics.Settings.SuperSampling.SetValue(ESamplingMode.DownX05);
            await gameSettings.Graphics.Settings.AnisotropicFiltering.SetValue(AnisotropicFiltering.Disable);
            await gameSettings.Graphics.Settings.OverallVisibility.SetValue(4000);
            await gameSettings.Graphics.Settings.LodBias.SetValue(2);
            await gameSettings.Graphics.Settings.Ssao.SetValue(ESSAOMode.Off);
            await gameSettings.Graphics.Settings.SSR.SetValue(ESSRMode.Off);
            await gameSettings.Graphics.Settings.AntiAliasing.SetValue(EAntialiasingMode.None);
            await gameSettings.Graphics.Settings.NVidiaReflex.SetValue(ENvidiaReflexMode.Off);
            await gameSettings.Graphics.Settings.GrassShadow.SetValue(false);
            await gameSettings.Graphics.Settings.ChromaticAberrations.SetValue(false);
            await gameSettings.Graphics.Settings.Noise.SetValue(false);
            await gameSettings.Graphics.Settings.ZBlur.SetValue(false);
            await gameSettings.Graphics.Settings.HighQualityColor.SetValue(false);
            await gameSettings.Graphics.Settings.MipStreaming.SetValue(false);
            await gameSettings.Graphics.Settings.SdTarkovStreets.SetValue(true);
            await gameSettings.Graphics.Settings.DLSSMode.SetValue(EDLSSMode.Off);
            await gameSettings.Graphics.Settings.DLSSPreset.SetValue(EDLSSPreset.Default);
            await gameSettings.Graphics.Settings.FSR2Mode.SetValue(EFSR2Mode.Off);
            await gameSettings.Graphics.Settings.FSR3Mode.SetValue(EFSR3Mode.Off);
            await gameSettings.Graphics.Settings.CloudsQuality.SetValue(CloudsMode.Low);
            await gameSettings.Graphics.Settings.VolumetricLight.SetValue(false);

            await gameSettings.Graphics.Settings.LobbyFramerate.SetValue(30);
            await gameSettings.Graphics.Settings.GameFramerate.SetValue(FikaHeadlessPlugin.UpdateRate.Value);

            await gameSettings.Game.Settings.EnableHideoutPreload.SetValue(false);
            await gameSettings.Game.Settings.Language.SetValue("en");

            await gameSettings.Graphics.Settings.DisplaySettings.SetValue(new()
            {
                AspectRatio = Class1724.smethod_0(new(1024, 768)),
                Display = 0,
                FullScreenMode = FullScreenMode.Windowed,
                Resolution = new(1024, 768)
            });

            await gameSettings.Sound.Save();
            await gameSettings.Graphics.Save();
            await gameSettings.Game.Save();

            EFTHardSettings hardSettings = EFTHardSettings.Instance;
            hardSettings.CULL_GROUNDER = 2000f;
            hardSettings.AnimatorCullDistance = 2000f;
            hardSettings.DRAW_DEFERRED_DECALS = false;

            hardSettings.PLAYER_HIT_DECALS_ENEBLED = false;
            hardSettings.HIT_EFFECTS_ENABLED = false;
            hardSettings.HEAT_EMITTER_ENABLED = false;
            hardSettings.SHOT_EFFECTS_ENABLED = false;
            hardSettings.DEFERRED_DECALS_ENABLED = false;
            hardSettings.STATIC_DEFERRED_DECALS_ENABLED = false;

            hardSettings.TriggersCastLayerMask = hardSettings.ServerTriggersCastLayerMask;
            hardSettings.WEAPON_OCCLUSION_LAYERS = hardSettings.WEAPON_OCCLUSION_SERVER_LAYERS;
            hardSettings.LootVolumeForHighQuallityPhysicsClient = hardSettings.LootVolumeForHighQuallityPhysicsServer;

            _hasSet = true;
        }
    }
}
