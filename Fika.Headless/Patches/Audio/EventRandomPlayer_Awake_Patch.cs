using Audio.AmbientSubsystem;
using Fika.Core.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Headless.Patches.Audio
{
    internal class EventRandomPlayer_Awake_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(EventRandomPlayer).GetMethod(nameof(EventRandomPlayer.Awake));
        }

        [PatchPrefix]
        public static bool Prefix(EventRandomPlayer __instance)
        {
            GameObject.Destroy(__instance);
            return false;
        }
    }
}
