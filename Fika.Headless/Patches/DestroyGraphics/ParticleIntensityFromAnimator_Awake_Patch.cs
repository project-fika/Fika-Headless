using EFT;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

public class ParticleIntensityFromAnimator_Awake_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ParticleIntensityFromAnimator).GetMethod(nameof(ParticleIntensityFromAnimator.Awake));
    }

    [PatchPrefix]
    public static bool Prefix(ParticleIntensityFromAnimator __instance)
    {
        GameObject.Destroy(__instance);
        return false;
    }
}
