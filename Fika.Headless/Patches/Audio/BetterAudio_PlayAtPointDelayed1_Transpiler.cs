using System;
using EFT.Ballistics;
using EFT;
using SPTushonka.Reflection.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine.Audio;

namespace Fika.Headless.Patches.Audio;

internal class BetterAudio_PlayAtPointDelayed1_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(BetterAudio).GetMethod(nameof(BetterAudio.PlayAtPointDelayed), [typeof(Vector3), typeof(AudioClip), typeof(BetterAudio.AudioSourceGroupType), typeof(int), typeof(float), typeof(PlayOptions).MakeByRefType()]);
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
