using Audio.SpatialSystem;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.LoadingPatches;

public class SpatialAudioSystem_Initialize_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(SpatialAudioSystem)
            .GetMethod(nameof(SpatialAudioSystem.Initialize));
    }

    [PatchPrefix]
    public static bool Prefix(ref Il2CppSystem.Threading.Tasks.Task __result)
    {
        __result = Il2CppSystem.Threading.Tasks.Task.CompletedTask;
        return false;
    }
}
