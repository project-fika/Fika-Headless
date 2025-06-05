using Audio.AmbientSubsystem;
using Audio.AutoPanner;
using Fika.Core.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Headless.Patches.Audio
{
    internal class AmbientPlayerAutoPanner_Awake_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(AmbientPlayerAutoPanner).GetMethod(nameof(AmbientPlayerAutoPanner.Awake));
        }

        [PatchPrefix]
        public static bool Prefix(AmbientSoundPlayer __instance)
        {
            GameObject.Destroy(__instance);
            return false;
        }
    }
}
