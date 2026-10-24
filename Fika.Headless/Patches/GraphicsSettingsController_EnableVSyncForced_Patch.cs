using EFT.Settings.Graphics;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches;

public class GraphicsSettingsController_EnableVSyncForced_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(GraphicsSettingsController).GetMethod(nameof(GraphicsSettingsController.EnableVSyncForced));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
