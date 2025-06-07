using HarmonyLib;
using Fika.Core.Patching;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.Audio
{
    internal class BetterAudio_LimitedPlayNonSpatial_Transpiler : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BetterAudio).GetMethod(nameof(BetterAudio.LimitedPlayNonSpatial));
        }

        [PatchTranspiler]
        public static IEnumerable<CodeInstruction> Transpile()
        {
            yield return new CodeInstruction(OpCodes.Ret);
        }
    }
}
