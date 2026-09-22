using UnityEngine.Audio;
using System;
using EFT.Ballistics;
using EFT;
using SPTushonka.Reflection.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.Audio;

internal class BetterAudio_PlayNonspatial_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(BetterAudio).GetMethod(nameof(BetterAudio.PlayNonspatial), [typeof(AudioClip), typeof(BetterAudio.AudioSourceGroupType), typeof(float), typeof(float), typeof(AudioMixerGroup), typeof(bool), typeof(bool)]);
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
