using EFT;
using SPT.Reflection.Patching;
using System.Reflection;
using System.Threading.Tasks;

namespace Fika.Headless.Patches.Locales;

public class DataPrepareOperation_LoadMainMenuLocale_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(DataPrepareOperation)
            .GetMethod(nameof(DataPrepareOperation.LoadMainMenuLocale));
    }

    [PatchPrefix]
    public static bool Prefix(ref Task __result)
    {
        LocalizationManager.Instance.UpdateApplicationLanguage();
        __result = Task.CompletedTask;
        return false;
    }
}
