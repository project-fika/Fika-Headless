using Fika.Core.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.Audio.Tripwire;

internal class GClass2581_PlayPinSound_Transpiler : FikaPatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(GClass2581)
            .GetMethod(nameof(GClass2581.PlayPinSound));
    }

    [PatchTranspiler]
    public static IEnumerable<CodeInstruction> Transpile()
    {
        yield return new CodeInstruction(OpCodes.Ret);
    }
}
