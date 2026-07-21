using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.TarkovAppPatches;

/// <summary>
/// Stops the <see cref="MainMenuShowOperation"/> from trying to set the hideout inventory
/// </summary>
public class MainMenuShowOperation_method_5_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(MainMenuShowOperation.CG_Init)
            .GetMethod(nameof(MainMenuShowOperation.CG_Init.MoveNext));
    }

    [PatchTranspiler]
    public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
    {
        var instr = instructions
            .ToList();

        // remove the code the sets the hideout inventory
        instr[650].opcode = OpCodes.Nop;
        instr[650].operand = null;

        return instr;
    }
}
