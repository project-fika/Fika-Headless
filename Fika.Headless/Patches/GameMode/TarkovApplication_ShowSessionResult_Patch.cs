using EFT;
using SPT.Reflection.Patching;
using System.Reflection;
using System.Threading.Tasks;

namespace Fika.Headless.Patches.GameMode;

internal class TarkovApplication_ShowSessionResult_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(TarkovApplication)
            .GetMethod(nameof(TarkovApplication.ShowSessionResult));
    }

    [PatchPrefix]
    public static bool Prefix(ref Task __result, TarkovApplication __instance)
    {
        __result = __instance.ComebackToMainMenu();
        return false;
    }
}
