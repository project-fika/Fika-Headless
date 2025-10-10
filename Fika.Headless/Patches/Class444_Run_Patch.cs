using SPT.Reflection.Patching;
using System.Reflection;
using System.Threading.Tasks;

namespace Fika.Headless.Patches;

/// <summary>
/// This prevents the season controller from running due to no graphics being used
/// </summary>
public class Class444_Run_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(Class444).GetMethod(nameof(Class444.Run));
    }

    [PatchPrefix]
    public static bool Prefix(Class444 __instance, ref Task __result, ref Class444.Interface3 ___Interface3_0)
    {
        ___Interface3_0 = new Class444.Class447(__instance);
        __result = Task.CompletedTask;
        return false;
    }
}
