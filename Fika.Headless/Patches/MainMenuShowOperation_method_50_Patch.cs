using EFT;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches;

/// <summary>
/// This patch simulates clicking "Next" by calling the method bound to the event of the button
/// </summary>
internal class MainMenuShowOperation_method_50_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(MainMenuShowOperation)
            .GetMethod(nameof(MainMenuShowOperation.method_50));
    }

    [PatchPrefix]
    public static bool Prefix(MainMenuShowOperation __instance)
    {
        __instance.method_51();
        return false;
    }
}
