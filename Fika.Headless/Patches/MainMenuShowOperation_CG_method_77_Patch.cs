using EFT;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches;

/// <summary>
/// This patch skips a bunch of unneccesary methods
/// </summary>
internal class MainMenuShowOperation_CG_method_77_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(MainMenuShowOperation)
            .GetMethod(nameof(MainMenuShowOperation.CG_method_77));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
