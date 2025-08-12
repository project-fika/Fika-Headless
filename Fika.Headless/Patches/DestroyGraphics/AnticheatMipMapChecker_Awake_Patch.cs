using Fika.Core.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

public class AnticheatMipMapChecker_Awake_Patch : FikaPatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(AnticheatMipMapChecker).GetMethod(nameof(AnticheatMipMapChecker.Awake));
    }

    [PatchPrefix]
    public static bool Prefix(AnticheatMipMapChecker __instance)
    {
        Object.Destroy(__instance);
        return false;
    }
}

