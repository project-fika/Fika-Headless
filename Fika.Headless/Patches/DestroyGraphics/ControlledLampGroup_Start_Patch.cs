using Fika.Core.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

public class ControlledLampGroup_Start_Patch : FikaPatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ControlledLampGroup).GetMethod(nameof(ControlledLampGroup.Start));
    }

    [PatchPrefix]
    public static bool Prefix(ControlledLampGroup __instance)
    {
        GameObject.Destroy(__instance);
        return false;
    }
}
