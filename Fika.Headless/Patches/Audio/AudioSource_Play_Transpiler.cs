using HarmonyLib;
using Fika.Core.Patching;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Fika.Headless.Patches.Audio
{
    public class AudioSource_Play_Transpiler : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(AudioSource).GetMethods().Where(x => x.Name == "Play" && x.GetParameters().Length == 0).SingleOrDefault();
        }

        [PatchTranspiler]
        public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
        {
            yield return new(OpCodes.Ret);
        }
    }
}
