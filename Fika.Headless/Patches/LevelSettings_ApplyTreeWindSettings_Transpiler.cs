using Fika.Core.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches
{
    public class LevelSettings_ApplyTreeWindSettings_Transpiler : FikaPatch
    {
        /// <summary>
        /// Prevents unneccesary code from running
        /// </summary>
        protected override MethodBase GetTargetMethod()
        {
            return typeof(LevelSettings).GetMethod(nameof(LevelSettings.ApplyTreeWindSettings));
        }

        [PatchTranspiler]
        public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
        {
            yield return new(OpCodes.Ret);
        }
    }
}
