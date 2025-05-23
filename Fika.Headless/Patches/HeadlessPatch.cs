﻿using HarmonyLib;
using SPT.Core.Utils;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches
{
    public class HeadlessPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ValidationUtil), nameof(ValidationUtil.Validate));
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref bool __result, ref bool ____hasRun)
        {
            ____hasRun = true;
            __result = true;
            return false;
        }
    }
}
