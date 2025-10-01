using SPT.Reflection.Patching;
using System.Reflection;
using System.Threading.Tasks;

namespace Fika.Headless.Patches.Locales;

public class LocaleClass_LoadMainMenuLocale_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(LocaleClass)
            .GetMethod(nameof(LocaleClass.LoadMainMenuLocale));
    }

    [PatchPrefix]
    public static bool Prefix(ref Task __result)
    {
        LocaleManagerClass.LocaleManagerClass.UpdateApplicationLanguage();
        __result = Task.CompletedTask;
        return false;
    }
}
