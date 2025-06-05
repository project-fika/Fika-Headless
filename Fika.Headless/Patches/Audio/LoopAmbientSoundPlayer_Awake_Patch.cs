using Audio.AmbientSubsystem;
using Fika.Core.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Headless.Patches.Audio
{
    internal class LoopAmbientSoundPlayer_Awake_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(LoopAmbientSoundPlayer).GetMethod(nameof(LoopAmbientSoundPlayer.Awake));
        }

        [PatchPrefix]
        public static bool Prefix(LoopAmbientSoundPlayer __instance)
        {
            GameObject.Destroy(__instance);
            return false;
        }
    }
}
