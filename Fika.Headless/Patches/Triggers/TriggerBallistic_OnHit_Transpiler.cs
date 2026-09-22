using EFT.GameTriggers;
using SPTushonka.Reflection.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.Triggers;

internal class TriggerBallistic_OnHit_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(TriggerBallistic).GetMethod(nameof(TriggerBallistic.OnBallisticHit));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
