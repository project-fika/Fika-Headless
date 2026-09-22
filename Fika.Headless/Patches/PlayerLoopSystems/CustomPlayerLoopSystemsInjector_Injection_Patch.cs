using CustomPlayerLoopSystem;
using SPTushonka.Reflection.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.PlayerLoopSystems;

/// <summary>
/// This transpilers skips the usage of the <see cref="CustomPlayerLoopSystemsInjector.Injection"/>
/// </summary>
internal class CustomPlayerLoopSystemsInjector_Injection_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(CustomPlayerLoopSystemsInjector).GetMethod(nameof(CustomPlayerLoopSystemsInjector.Injection));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
