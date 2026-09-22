using EFT;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Locales;

public class TarkovApplication_LoadAvailableLanguages_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(TarkovApplication).GetMethod(nameof(TarkovApplication.LoadAvailableLanguages));
    }

    [PatchPrefix]
    public static bool Prefix(ref Il2CppSystem.Threading.Tasks.Task __result)
    {
        __result = Il2CppSystem.Threading.Tasks.Task.CompletedTask;
        return false;
    }
}
