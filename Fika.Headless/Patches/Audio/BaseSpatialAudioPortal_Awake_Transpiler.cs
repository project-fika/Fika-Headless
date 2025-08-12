using Audio.SpatialSystem;
using Fika.Core.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.Audio;

internal class BaseSpatialAudioPortal_Awake_Transpiler : FikaPatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(BaseSpatialAudioPortal).GetMethod(nameof(BaseSpatialAudioPortal.Awake));
    }

    [PatchTranspiler]
    public static IEnumerable<CodeInstruction> Transpile()
    {
        yield return new(OpCodes.Ret);
    }
}
