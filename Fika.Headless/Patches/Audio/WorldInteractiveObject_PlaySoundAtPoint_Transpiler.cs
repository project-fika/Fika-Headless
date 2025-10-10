using EFT.Interactive;
using SPT.Reflection.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.Audio;

internal class WorldInteractiveObject_PlaySoundAtPoint_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(WorldInteractiveObject).GetMethod(nameof(WorldInteractiveObject.PlaySoundAtPoint));
    }

    [PatchTranspiler]
    public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
    {
        yield return new(OpCodes.Ret);
    }
}
