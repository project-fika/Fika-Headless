using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

public class DistantShadow_Awake_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(DistantShadow).GetMethod(nameof(DistantShadow.Awake));
    }

    [PatchPrefix]
    public static bool Prefix(DistantShadow __instance, ref RenderTexture[] ___renderTexture_10)
    {
        ___renderTexture_10 = [];
        Object.Destroy(__instance);
        return false;
    }
}

public class DistantShadow_Update_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(DistantShadow).GetMethod(nameof(DistantShadow.Update));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
