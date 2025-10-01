using SPT.Reflection.Patching;
using MultiFlare;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

internal class FlareLight_OnEnable_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(FlareLight).GetMethod(nameof(FlareLight.OnEnable));
    }

    [PatchPrefix]
    public static bool Prefix(FlareLight __instance)
    {
        MonoBehaviour.Destroy(__instance);
        return false;
    }
}
