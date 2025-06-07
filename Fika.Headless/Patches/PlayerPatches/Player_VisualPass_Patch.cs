using EFT;
using Fika.Core.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Headless.Patches.PlayerPatches
{
    /// <summary>
    /// This patch aims to decrease the amount of CPU cycles spent updating data the headless does not see
    /// </summary>
    public class Player_VisualPass_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player).GetMethod(nameof(Player.VisualPass));
        }

        [PatchPrefix]
        public static bool Prefix(Player __instance, bool ____bodyupdated, bool ____armsupdated, float ____bodyTime, float ___ThirdPersonWeaponRootAuthority)
        {
            __instance.BodyAnimatorCommon.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            __instance.ArmsAnimatorCommon.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            if (____bodyupdated)
            {
                __instance.method_14();
                __instance.MouseLook(false);
            }

            if (____armsupdated || __instance.ArmsUpdateMode is Player.EUpdateMode.Auto)
            {
                float rootAuth = __instance.MovementContext.StationaryWeapon != null ? 0f : ___ThirdPersonWeaponRootAuthority;

                __instance.ProceduralWeaponAnimation.LateTransformations(Time.deltaTime);
                if (__instance.HandsController != null)
                {
                    __instance.HandsController.ManualLateUpdate(Time.deltaTime);
                }
                __instance.PlayerBones.ShiftWeaponRoot(____bodyTime, EPointOfView.ThirdPerson,
                    rootAuth, false, 0f, 0f, __instance.CurrentState.Name == EPlayerState.Sprint,
                    __instance.MovementContext.LeftStanceController.LastAnimValue, __instance.MovementContext.LeftStanceController.LeftStance,
                    __instance.ProceduralWeaponAnimation.IsAiming, __instance.MovementContext.PlayerAnimator.AnimatedInteractions.IsInteractionPlaying);
            }

            return false;
        }
    }
}
