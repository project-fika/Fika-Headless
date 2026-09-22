using EFT;
using SPTushonka.Reflection.Patching;
using System.Reflection;
using System.Threading.Tasks;

namespace Fika.Headless.Patches.Locales;

public class DataPrepareOperation_LoadMainMenuLocale_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(LocalizationLoader)
            .GetMethod(nameof(LocalizationLoader.LoadMainMenuLocale));
    }

    [PatchPrefix]
    public static bool Prefix(ref Il2CppSystem.Threading.Tasks.Task __result)
    {
        LocalizationManager.Instance.UpdateApplicationLanguage();
        __result = Il2CppSystem.Threading.Tasks.Task.CompletedTask;
        return false;
    }
}
