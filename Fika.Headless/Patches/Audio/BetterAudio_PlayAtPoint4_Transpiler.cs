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

internal class BetterAudio_PlayAtPoint4_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(BetterAudio).GetMethod(nameof(BetterAudio.PlayAtPointForPlayer), [typeof(IPlayer), typeof(Vector3), typeof(SoundBank), typeof(PlayOptions).MakeByRefType()]);
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
