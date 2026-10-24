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

internal class BetterAudio_PlayAtPoint1_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(BetterAudio).GetMethod(nameof(BetterAudio.TryPlayAtPointBankCore));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
