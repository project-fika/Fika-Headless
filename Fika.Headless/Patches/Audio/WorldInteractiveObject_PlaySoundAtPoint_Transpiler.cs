using EFT.Interactive;
using Fika.Core.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.Audio;

internal class WorldInteractiveObject_PlaySoundAtPoint_Transpiler : FikaPatch
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
