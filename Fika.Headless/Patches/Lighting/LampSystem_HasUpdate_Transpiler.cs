using EFT.Interactive;
using SPT.Reflection.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.Lighting;

internal class LampSystem_HasUpdate_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(LampSystem)
            .GetProperty(nameof(LampSystem.HasUpdate)).GetGetMethod();
    }

    [PatchTranspiler]
    public static IEnumerable<CodeInstruction> Transpile()
    {
        yield return new(OpCodes.Ldc_I4_0);
        yield return new(OpCodes.Ret);
    }
}
