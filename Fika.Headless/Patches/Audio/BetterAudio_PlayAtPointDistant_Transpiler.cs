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

internal class BetterAudio_PlayAtPointDistant_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(BetterAudio).GetMethod(nameof(BetterAudio.PlayAtPointForGameObject), [typeof(GameObject), typeof(Vector3), typeof(AudioClip), typeof(BetterAudio.AudioSourceGroupType), typeof(int), typeof(PlayOptions).MakeByRefType()]);
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
