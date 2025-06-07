using GPUInstancer;
using HarmonyLib;
using Fika.Core.Patching;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.DestroyGraphics
{
    public class GPUInstancerManager_Update_Transpiler : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GPUInstancerManager).GetMethod(nameof(GPUInstancerManager.Update));
        }

        [PatchTranspiler]
        public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
        {
            yield return new(OpCodes.Ret);
        }
    }
}
