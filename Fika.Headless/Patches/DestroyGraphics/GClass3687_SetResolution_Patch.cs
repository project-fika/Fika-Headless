using Fika.Core.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

public class GClass3687_SetResolution_Patch : FikaPatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(GClass3687)
            .GetMethod(nameof(GClass3687.SetResolution));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
