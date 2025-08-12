using EFT;
using Fika.Core.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

public class ParticleIntensityFromAnimator_Awake_Patch : FikaPatch
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
