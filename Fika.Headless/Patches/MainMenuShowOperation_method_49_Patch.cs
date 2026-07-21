using EFT;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches;

/// <summary>
/// This patch simulates clicking "Next" by calling the method bound to the event of the button <br/>
/// <see cref="EFT.UI.Matchmaker.MatchmakerOfflineRaidScreen.CreateRaidSettingsForProfileClass"/>
/// </summary>
internal class MainMenuShowOperation_method_49_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(MainMenuShowOperation)
            .GetMethod(nameof(MainMenuShowOperation.method_49));
    }

    [PatchPrefix]
    public static bool Prefix(MainMenuShowOperation __instance)
    {
        __instance.CG_method_81();
        return false;
    }
}
