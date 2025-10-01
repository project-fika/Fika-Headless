using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

public class AmbientLight_Start_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(AmbientLight).GetMethod(nameof(AmbientLight.Start));
    }

    [PatchPrefix]
    public static bool Prefix(AmbientLight __instance)
    {
        Object.Destroy(__instance);
        return false;
    }
}
