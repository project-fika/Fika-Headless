using EFT;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches;

internal class LocalizationManager_Culture_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(LocalizationManager).GetProperty(nameof(LocalizationManager.Culture)).GetSetMethod();
    }

    [PatchPrefix]
    public static bool Prefix(ref string ____culture)
    {
        Logger.LogInfo("Forcing 'en' language");
        ____culture = "en";
        return false;
    }
}
