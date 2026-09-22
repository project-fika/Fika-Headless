using EFT.Vehicle;
using HarmonyLib;
using SPTushonka.Reflection.Patching;
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

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
