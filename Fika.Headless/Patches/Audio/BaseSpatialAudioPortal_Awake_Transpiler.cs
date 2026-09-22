using Audio.SpatialSystem;
using SPTushonka.Reflection.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.Audio;

internal class BaseSpatialAudioPortal_Awake_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(BaseSpatialAudioPortal).GetMethod(nameof(BaseSpatialAudioPortal.Awake));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
