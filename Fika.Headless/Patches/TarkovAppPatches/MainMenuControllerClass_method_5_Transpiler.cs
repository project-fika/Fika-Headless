using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.TarkovAppPatches;

/// <summary>
/// Stops the <see cref="MainMenuControllerClass"/> from trying to set the hideout inventory
/// </summary>
public class MainMenuControllerClass_method_5_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(MainMenuControllerClass.Struct438)
            .GetMethod(nameof(MainMenuControllerClass.Struct438.MoveNext));
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
