using EFT;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches;

/// <summary>
/// This patch skips checking for keys (e.g. Labs)
/// </summary>
public class MainMenuShowOperation_method_53_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(MainMenuShowOperation)
            .GetMethod(nameof(MainMenuShowOperation.method_53));
    }

    [PatchPrefix]
    public static bool Prefix(ref bool __result)
    {
        __result = true;
        return false;
    }
}
