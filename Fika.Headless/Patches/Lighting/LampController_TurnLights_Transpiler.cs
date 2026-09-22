using EFT.Interactive;
using SPTushonka.Reflection.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.Lighting;

internal class LampController_TurnLights_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(LampLights).GetMethod(nameof(LampLights.TurnLights));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
