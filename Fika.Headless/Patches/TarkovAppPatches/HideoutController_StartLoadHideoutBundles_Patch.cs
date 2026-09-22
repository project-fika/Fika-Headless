using EFT;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.TarkovAppPatches;

public class HideoutController_StartLoadHideoutBundles_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(TarkovApplication.HideoutController)
            .GetMethod(nameof(TarkovApplication.HideoutController.StartLoadHideoutBundles));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
