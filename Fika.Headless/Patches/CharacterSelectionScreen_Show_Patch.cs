using EFT;
using EFT.UI;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches;

public class CharacterSelectionScreen_Show_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(CharacterSelectionScreen).GetMethod(nameof(CharacterSelectionScreen.Show),
            [typeof(CharacterSelectionScreen.CharacterSelectionScreenController)]);
    }

    [PatchPostfix]
    public static void Postfix(CharacterSelectionScreen.CharacterSelectionScreenController controller)
    {
        if (!controller.Data.TryGetValue(EGameMode.Pve, out var profileData))
        {
            Logger.LogError("No PVE profile to select");
            return;
        }

        Logger.LogInfo("Selecting the PVE profile");
        // Submitting closes the screen, which must wait until Show has returned
        MainThread.Post(() => controller.Submit(EGameMode.Pve, profileData));
    }
}
