using Fika.Core.Patching;
using System.Reflection;

namespace Fika.Headless.Patches;

/// <summary>
/// This patch simulates clicking "Next" by calling the method bound to the event of the button
/// </summary>
internal class MainMenuControllerClass_method_50_Patch : FikaPatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(MainMenuControllerClass)
            .GetMethod(nameof(MainMenuControllerClass.method_50));
    }

    [PatchPrefix]
    public static bool Prefix(MainMenuControllerClass __instance)
    {
        __instance.method_51();
        return false;
    }
}
