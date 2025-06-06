using EFT;
using EFT.HealthSystem;
using Fika.Core.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Headless.Patches.PlayerPatches
{
    public class Player_Init_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player).GetMethod(nameof(Player.Init));
        }

        [PatchPostfix]
        public static void Postfix(Player __instance)
        {
            if (__instance.IsYourPlayer)
            {
                ActiveHealthController healthController = __instance.ActiveHealthController;
                if (healthController != null)
                {
                    healthController.SetDamageCoeff(0f);
                    healthController.DisableMetabolism();
                }

                Vector3 currentPosition = __instance.Position;
                __instance.Teleport(new(currentPosition.x, currentPosition.y - 100f, currentPosition.z));
            }
        }
    }
}
