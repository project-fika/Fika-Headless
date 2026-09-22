using SPTushonka.Reflection.Patching;
using System.Reflection;
using System.Threading.Tasks;

namespace Fika.Headless.Patches;

/// <summary>
/// This prevents the season controller from running due to no graphics being used
/// </summary>
public class SeasonsController_Run_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(SeasonsController).GetMethod(nameof(SeasonsController.Run));
    }

    [PatchPrefix]
    public static bool Prefix(SeasonsController __instance, ref Il2CppSystem.Threading.Tasks.Task __result)
    {
        __instance._state = new SeasonsController.StateSpring(__instance);
        __result = Il2CppSystem.Threading.Tasks.Task.CompletedTask;
        return false;
    }
}
