using EFT;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Locales;

public class LocalizationManager_LocalizedValue_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(LocalizationManager).GetMethod(nameof(LocalizationManager.LocalizedValue), [typeof(string)]);
    }

    [PatchPrefix]
    public static bool Prefix(ref string __result)
    {
        __result = string.Empty;
        return false;
    }
}
