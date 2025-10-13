using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.BTR;

/// <summary>
/// Prevents a nullref on headless due to having no player
/// </summary>
public class BTRControllerClass_method_18_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(BTRControllerClass)
            .GetMethod(nameof(BTRControllerClass.method_18));
    }

    [PatchTranspiler]
    public static IEnumerable<CodeInstruction> Transpile()
    {
        yield return new(OpCodes.Ret);
    }
}
