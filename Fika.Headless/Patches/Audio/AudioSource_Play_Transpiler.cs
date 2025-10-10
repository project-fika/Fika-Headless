using SPT.Reflection.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.Audio;

public class AudioSource_Play_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(AudioSource).GetMethods().Where(x => x.Name == "Play" && x.GetParameters().Length == 0).SingleOrDefault();
    }

    [PatchTranspiler]
    public static IEnumerable<CodeInstruction> Transpile()
    {
        yield return new(OpCodes.Ret);
    }
}
