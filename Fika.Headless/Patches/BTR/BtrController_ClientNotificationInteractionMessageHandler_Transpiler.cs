using EFT.Vehicle;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.BTR;

/// <summary>
/// Prevents a nullref on headless due to having no player
/// </summary>
public class BtrController_ClientNotificationInteractionMessageHandler_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(BtrController)
            .GetMethod(nameof(BtrController.ClientNotificationInteractionMessageHandler));
    }

    [PatchTranspiler]
    public static IEnumerable<CodeInstruction> Transpile()
    {
        yield return new(OpCodes.Ret);
    }
}
