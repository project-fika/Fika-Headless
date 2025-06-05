using Audio.AmbientSubsystem;
using Fika.Core.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Headless.Patches.Audio
{
    internal class EventLoopPlayer_Awake_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(EventLoopPlayer).GetMethod(nameof(EventLoopPlayer.Awake));
        }

        [PatchPrefix]
        public static bool Prefix(EventLoopPlayer __instance)
        {
            GameObject.Destroy(__instance);
            return false;
        }
    }
}
