using SPT.Reflection.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.Audio;

internal class BetterAudio_LimitedPlay_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(BetterAudio)
            .GetMethods()
            .Where(x => x.Name == "LimitedPlay" && x.ReturnType == typeof(bool))
            .First();
    }

    [PatchTranspiler]
    public static IEnumerable<CodeInstruction> Transpile()
    {
        yield return new CodeInstruction(OpCodes.Ldc_I4_0);
        yield return new CodeInstruction(OpCodes.Ret);
    }
}
