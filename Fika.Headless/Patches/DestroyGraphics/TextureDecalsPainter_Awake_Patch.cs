﻿using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Headless.Patches.DestroyGraphics
{
    public class TextureDecalsPainter_Awake_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(TextureDecalsPainter).GetMethod(nameof(TextureDecalsPainter.Awake));
        }

        [PatchPrefix]
        public static bool Prefix(TextureDecalsPainter __instance, ref GClass814<RenderTexture> ___gclass814_0)
        {
            ___gclass814_0 = new(0, FakeClassFunc);
            Object.Destroy(__instance);
            return false;
        }

        private static RenderTexture FakeClassFunc()
        {
            return new();
        }
    }
}
