/*using EFT;
using HarmonyLib;
using SPTushonka.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.Audio
{
    internal class BetterAudio_GetSource2_Transpiler : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BetterAudio).GetMethod(nameof(BetterAudio.GetSource),
                [typeof(BetterAudio.AudioSourceGroupType), typeof(bool)]);
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }
}
*/