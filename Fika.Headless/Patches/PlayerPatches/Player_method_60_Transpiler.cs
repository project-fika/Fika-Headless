using EFT;
using SPT.Reflection.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.PlayerPatches;

internal class Player_method_60_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(Player).GetMethod(nameof(Player.method_60));
    }

    [PatchTranspiler]
    public static IEnumerable<CodeInstruction> Transpile()
    {
        yield return new(OpCodes.Ret);
    }
}
