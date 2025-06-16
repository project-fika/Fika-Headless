using Audio.AmbientSubsystem;
using Fika.Core.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.Audio
{
    internal class BaseAmbientSoundPlayer_Stop_Transpiler : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BaseAmbientSoundPlayer).GetMethod(nameof(BaseAmbientSoundPlayer.Stop));
        }

        [PatchTranspiler]
        public static IEnumerable<CodeInstruction> Transpile()
        {
            yield return new CodeInstruction(OpCodes.Ret);
        }
    }
}
