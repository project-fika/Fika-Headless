using EFT;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Locales;

public class LocalizationManager_UpdateLocales_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(LocalizationManager).GetMethod(nameof(LocalizationManager.UpdateLocales));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
