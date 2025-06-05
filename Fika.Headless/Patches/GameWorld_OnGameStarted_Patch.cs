using Comfort.Common;
using EFT;
using Fika.Core.Coop.Players;
using Fika.Headless.Classes;
using Fika.Core.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Headless.Patches
{
    public class GameWorld_OnGameStarted_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        public static void Postfix(GameWorld __instance)
        {
            HeadlessRaidController raidController = FikaHeadlessPlugin.raidController;
            if (raidController == null)
            {
                FikaHeadlessPlugin.raidController = Singleton<GameWorld>.Instance.gameObject.AddComponent<HeadlessRaidController>();
                raidController = FikaHeadlessPlugin.raidController;

                raidController.MainPlayer = (CoopPlayer)__instance.MainPlayer;
                raidController.MainPlayer.MovementContext.PitchLimit = new Vector2(-90, -90);
            }

            CameraClass.Instance.SetOcclusionCullingEnabled(false);
        }
    }
}
