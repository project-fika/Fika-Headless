using SPTushonka.Reflection.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches;

public class LevelSettings_ApplyTreeWindSettings_Transpiler : ModulePatch
{
    /// <summary>
    /// Prevents unneccesary code from running
    /// </summary>
    protected override MethodBase GetTargetMethod()
    {
        return typeof(LevelSettings)
            .GetMethod(nameof(LevelSettings.ApplyTreeWindSettings));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
