using EFT;
using Fika.Core.Coop.ObservedClasses;
using Fika.Core.Coop.Players;
using Fika.Core.Patching;
using System;
using System.Reflection;
using UnityEngine;

namespace Fika.Headless.Patches
{
    public class CoopPlayer_CreateMovementContext_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(CoopPlayer).GetMethod(nameof(CoopPlayer.CreateMovementContext));
        }

        [PatchPrefix]
        public static bool Prefix(Player __instance)
        {
            if (__instance.IsYourPlayer)
            {
                LayerMask localMask = EFTHardSettings.Instance.MOVEMENT_MASK;
                __instance.MovementContext = HeadlessClientMovementContext.Create(__instance, new Func<IAnimator>(__instance.GetBodyAnimatorCommon),
                    new Func<ICharacterController>(__instance.GetCharacterControllerCommon), localMask);

                return false;
            }

            return true;
        }
    }
}
