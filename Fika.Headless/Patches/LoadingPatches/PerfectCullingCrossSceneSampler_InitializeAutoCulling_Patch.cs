using Koenigz.PerfectCulling.EFT;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.LoadingPatches;

public class PerfectCullingCrossSceneSampler_InitializeAutoCulling_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(PerfectCullingCrossSceneSampler)
            .GetMethod(nameof(PerfectCullingCrossSceneSampler.InitializeAutoCulling));
    }

    [PatchPrefix]
    public static bool Prefix(ref Il2CppSystem.Threading.Tasks.Task __result)
    {
        __result = Il2CppSystem.Threading.Tasks.Task.CompletedTask;
        return false;
    }
}
