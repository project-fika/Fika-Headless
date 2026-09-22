using EFT;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches;

internal class LocalizationManager_Culture_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(LocalizationManager).GetMethod(nameof(LocalizationManager.UpdateApplicationLanguage));
    }

    [PatchPrefix]
    public static void Prefix(LocalizationManager __instance)
    {
        if (__instance.Culture != "en")
        {
            Logger.LogInfo("Forcing 'en' language");
            __instance.Culture = "en";
        }
    }
}
