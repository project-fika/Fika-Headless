using Fika.Core.Patching;
using GPUInstancer;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

public class GPUInstancerDetailManager_Awake_Patch : FikaPatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(GPUInstancerDetailManager).GetMethod(nameof(GPUInstancerDetailManager.Awake));
    }

    [PatchPrefix]
    public static bool Prefix(GPUInstancerDetailManager __instance)
    {
        Object.Destroy(__instance);
        return false;
    }
}
