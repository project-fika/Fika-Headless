using EFT;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.TarkovAppPatches;

public class HideoutController_StartLoadHideoutBundles_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(TarkovApplication.HideoutController.HideoutGameLoader)
            .GetMethod(nameof(TarkovApplication.HideoutController.HideoutGameLoader.LoadBundles));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
