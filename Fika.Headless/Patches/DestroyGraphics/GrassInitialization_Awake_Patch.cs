using GPUInstancer;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

public class GrassInitialization_Awake_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(GrassInitialization)
            .GetMethod(nameof(GrassInitialization.Awake));
    }

    [PatchPrefix]
    public static bool Prefix(GrassInitialization __instance)
    {
        Object.Destroy(__instance);
        return false;
    }
}
