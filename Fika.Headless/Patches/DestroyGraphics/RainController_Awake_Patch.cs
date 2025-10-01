using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

public class RainController_Awake_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(RainController).GetMethod(nameof(RainController.Awake));
    }

    [PatchPrefix]
    public static bool Prefix(RainController __instance)
    {
        Object.Destroy(__instance);
        return false;
    }
}
