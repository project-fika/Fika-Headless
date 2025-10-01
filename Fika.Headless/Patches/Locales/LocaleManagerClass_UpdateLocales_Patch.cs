using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Locales;

public class LocaleManagerClass_UpdateLocales_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(LocaleManagerClass).GetMethod(nameof(LocaleManagerClass.UpdateLocales));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
