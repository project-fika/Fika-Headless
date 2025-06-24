using Fika.Core.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.Audio.Tripwire
{
    internal class GClass2457_PlayPlantSound_Transpiler : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GClass2457).GetMethod(nameof(GClass2457.PlayPlantSound));
        }

        [PatchTranspiler]
        public static IEnumerable<CodeInstruction> Transpile()
        {
            yield return new CodeInstruction(OpCodes.Ret);
        }
    }
}
