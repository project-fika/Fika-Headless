using Audio.AmbientSubsystem;
using Audio.SpatialSystem;
using Comfort.Common;
using EFT;
using Fika.Core.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Audio
{
    internal class TarkovApplication_method_43_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(TarkovApplication).GetMethod(nameof(TarkovApplication.method_43));
        }

        [PatchPrefix]
        public static void Prefix()
        {
            if (Singleton<BetterAudio>.Instantiated)
            {
                Singleton<BetterAudio>.Release(Singleton<BetterAudio>.Instance);
            }
            if (Singleton<SpatialAudioSystem>.Instantiated)
            {
                Singleton<SpatialAudioSystem>.Instance.Dispose();
                Singleton<SpatialAudioSystem>.Release(Singleton<SpatialAudioSystem>.Instance);
            }
            if (Singleton<AmbientAudioSystem>.Instantiated)
            {
                Singleton<AmbientAudioSystem>.Instance.Dispose();
                Singleton<AmbientAudioSystem>.Release(Singleton<AmbientAudioSystem>.Instance);
            }
        }
    }
}
