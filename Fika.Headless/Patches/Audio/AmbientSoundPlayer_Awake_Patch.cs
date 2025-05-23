﻿using Audio.AmbientSubsystem;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Headless.Patches.Audio
{
    internal class AmbientSoundPlayer_Awake_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(AmbientSoundPlayer).GetMethod(nameof(AmbientSoundPlayer.Awake));
        }

        [PatchPrefix]
        public static bool Prefix(AmbientSoundPlayer __instance)
        {
            GameObject.Destroy(__instance);
            return false;
        }
    }
}
