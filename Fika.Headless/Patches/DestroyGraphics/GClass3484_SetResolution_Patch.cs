using Fika.Core.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

public class GClass3484_SetResolution_Patch : FikaPatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(GClass3484).GetMethod(nameof(GClass3484.SetResolution));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
