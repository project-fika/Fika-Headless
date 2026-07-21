using EFT;
using SPT.Reflection.Patching;
using System.Reflection;
using System.Threading.Tasks;

namespace Fika.Headless.Patches.Locales;

public class TarkovApplication_LoadAvailableLanguages_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(TarkovApplication).GetMethod(nameof(TarkovApplication.LoadAvailableLanguages));
    }

    [PatchPrefix]
    public static bool Prefix(ref Task __result)
    {
        __result = Task.CompletedTask;
        return false;
    }
}
