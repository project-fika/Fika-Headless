using Audio.AmbientSubsystem;
using Fika.Core.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Headless.Patches.Audio
{
    internal class BaseAmbientSoundPlayer_Awake_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BaseAmbientSoundPlayer).GetMethod(nameof(BaseAmbientSoundPlayer.Awake));
        }

        [PatchPrefix]
        public static bool Prefix(BaseAmbientSoundPlayer __instance)
        {
            GameObject.Destroy(__instance);
            return false;
        }
    }
}
