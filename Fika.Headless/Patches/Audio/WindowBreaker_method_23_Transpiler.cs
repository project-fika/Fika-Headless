using HarmonyLib;
using Fika.Core.Patching;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using EFT.Interactive;

namespace Fika.Headless.Patches.Audio
{
    internal class WindowBreaker_method_23_Transpiler : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(WindowBreaker).GetMethod(nameof(WindowBreaker.method_23));
        }

        [PatchTranspiler]
        public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
        {
            yield return new(OpCodes.Ret);
        }
    }
}
