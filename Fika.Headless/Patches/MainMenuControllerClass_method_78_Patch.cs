using EFT;
using Fika.Core.Patching;
using System.Reflection;

namespace Fika.Headless.Patches;

/// <summary>
/// This patch ensures that the raid settings are skipped if you are playing as a scav
/// </summary>
internal class MainMenuControllerClass_method_78_Patch : FikaPatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(MainMenuControllerClass).GetMethod(nameof(MainMenuControllerClass.method_78));
    }

    [PatchPostfix]
    public static void Postfix(MainMenuControllerClass __instance, RaidSettings ___RaidSettings_0)
    {
        if (___RaidSettings_0.IsScav)
        {
            __instance.method_51();
        }
    }
}
