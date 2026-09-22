using EFT.Settings.Graphics;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches;

/// <summary>
/// Keeps the game from overriding the headless frame limit
/// </summary>
public class GraphicsSettingsController_ChangeFramerate_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(GraphicsSettingsController).GetMethod(nameof(GraphicsSettingsController.ChangeFramerate));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
