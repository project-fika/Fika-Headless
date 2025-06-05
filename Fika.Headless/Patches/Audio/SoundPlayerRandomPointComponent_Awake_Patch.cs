using Audio.AmbientSubsystem;
using Fika.Core.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Headless.Patches.Audio
{
    internal class SoundPlayerRandomPointComponent_Awake_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(SoundPlayerRandomPointComponent).GetMethod(nameof(SoundPlayerRandomPointComponent.Awake));
        }

        [PatchPrefix]
        public static bool Prefix(AmbientSoundPlayer __instance)
        {
            GameObject.Destroy(__instance);
            return false;
        }
    }
}
