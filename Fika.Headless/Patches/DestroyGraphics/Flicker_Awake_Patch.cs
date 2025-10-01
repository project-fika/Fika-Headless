using EFT.Visual;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

public class Flicker_Awake_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(Flicker).GetMethod(nameof(Flicker.Awake));
    }

    [PatchPrefix]
    public static bool Prefix(Flicker __instance)
    {
        Object.Destroy(__instance);
        return false;
    }
}
