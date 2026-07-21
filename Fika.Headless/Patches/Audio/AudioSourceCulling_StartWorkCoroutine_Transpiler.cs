using SPT.Reflection.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.Audio;

internal class AudioSourceCulling_StartWorkCoroutine_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(AudioSourceCulling).GetMethod(nameof(AudioSourceCulling.StartWorkCoroutine));
    }

    [PatchTranspiler]
    public static IEnumerable<CodeInstruction> Transpile()
    {
        yield return new(OpCodes.Ret);
    }
}
