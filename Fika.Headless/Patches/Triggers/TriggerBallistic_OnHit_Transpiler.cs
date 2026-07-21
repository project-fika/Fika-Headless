using EFT.GameTriggers;
using SPT.Reflection.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.Triggers;

internal class TriggerBallistic_OnHit_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(TriggerBallistic).GetMethod(nameof(TriggerBallistic.OnHit));
    }

    [PatchTranspiler]
    public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
    {
        yield return new(OpCodes.Ret);
    }
}
