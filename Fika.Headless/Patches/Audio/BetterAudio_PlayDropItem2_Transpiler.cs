using UnityEngine.Audio;
using System;
using EFT.Ballistics;
using EFT;
using SPTushonka.Reflection.Patching;
using HarmonyLib;
using JsonType;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.Audio;

internal class BetterAudio_PlayDropItem2_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(BetterAudio).GetMethod(nameof(BetterAudio.PlayDropItem), [typeof(BaseBallistic.ESurfaceSound), typeof(string), typeof(Vector3), typeof(float)]);
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
