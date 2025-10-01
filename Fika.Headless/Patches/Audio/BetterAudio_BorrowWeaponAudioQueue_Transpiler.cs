using SPT.Reflection.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.Audio;

internal class BetterAudio_BorrowWeaponAudioQueue_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(BetterAudio).GetMethod(nameof(BetterAudio.BorrowWeaponAudioQueue));
    }

    [PatchTranspiler]
    public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
    {
        yield return new(OpCodes.Ldnull);
        yield return new(OpCodes.Ret);
    }
}
