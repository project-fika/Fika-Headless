using EFT;
using SPT.Reflection.Patching;
using System.Reflection;
using System.Threading.Tasks;

namespace Fika.Headless.Patches.Locales;

public class TarkovApplication_method_6_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(TarkovApplication).GetMethod(nameof(TarkovApplication.method_6));
    }

    [PatchPrefix]
    public static bool Prefix(ref Task __result)
    {
        __result = Task.CompletedTask;
        return false;
    }
}
