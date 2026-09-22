using HarmonyLib;
using SPTushonka.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches;

/// <summary>
/// The purpose of this patch is to disable bot sleeping on the headless host
/// </summary>
[IgnoreAutoPatch]
public class BotStandBy_Update_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(BotStandBy)
            .GetMethod(nameof(BotStandBy.Update));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
