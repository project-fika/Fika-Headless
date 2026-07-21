using EFT;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches;

/// <summary>
/// This patch ensures that the raid settings are skipped if you are playing as a scav
/// </summary>
internal class MainMenuShowOperation_CG_method_78_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(MainMenuShowOperation).GetMethod(nameof(MainMenuShowOperation.CG_method_78));
    }

    [PatchPostfix]
    public static void Postfix(MainMenuShowOperation __instance, RaidSettings ___raidSettings_0)
    {
        if (___raidSettings_0.IsScav)
        {
            __instance.method_51();
        }
    }
}
