using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

internal class RainFallDrops_Awake_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(RainFallDrops).GetMethod(nameof(RainFallDrops.smethod_0));
    }

    [PatchPrefix]
    public static bool Prefix(int count, ref Mesh __result)
    {
        __result = new();
        return false;
    }
}
