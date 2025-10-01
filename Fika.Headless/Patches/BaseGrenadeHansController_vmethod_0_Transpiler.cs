using EFT;
using SPT.Reflection.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches;

internal class BaseGrenadeHansController_vmethod_0_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(Player.BaseGrenadeHandsController)
            .GetMethod(nameof(Player.BaseGrenadeHandsController.vmethod_0));
    }

    [PatchTranspiler]
    public static IEnumerable<CodeInstruction> Transpile()
    {
        yield return new(OpCodes.Ret);
    }
}
