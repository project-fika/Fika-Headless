using Fika.Core.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

public class TubeLight_Start_Patch : FikaPatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(TubeLight).GetMethod(nameof(TubeLight.Start));
    }

    [PatchPrefix]
    public static bool Prefix(TubeLight __instance)
    {
        GameObject.Destroy(__instance);
        return false;
    }
}
