using SPT.Reflection.Patching;
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
    public static bool Prefix(SeasonsController __instance, ref Task __result, ref SeasonsController.IState ____state)
    {
        ____state = new SeasonsController.StateSpring(__instance);
        __result = Task.CompletedTask;
        return false;
    }
}
