using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

public class ControlledLampGroup_Start_Patch : ModulePatch
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
