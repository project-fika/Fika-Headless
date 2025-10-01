using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches;

/// <summary>
/// This patch skips a bunch of unneccesary methods
/// </summary>
internal class MainMenuControllerClass_method_77_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(MainMenuControllerClass)
            .GetMethod(nameof(MainMenuControllerClass.method_77));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
