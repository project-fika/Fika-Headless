using EFT;
using HarmonyLib;
using System;
using UnityEngine;

namespace Fika.Headless.Classes
{
    public class HeadlessMovementContext : MovementContext
    {
        private Action<CollisionFlags> _onMotion;

        public new static HeadlessMovementContext Create(Player player, Func<IAnimator> animatorGetter, Func<ICharacterController> characterControllerGetter, LayerMask groundMask)
        {
            HeadlessMovementContext movementContext = Create<HeadlessMovementContext>(player, animatorGetter, characterControllerGetter, groundMask);
            movementContext._onMotion = Traverse.Create(movementContext).Field<Action<CollisionFlags>>("OnMotionApplied").Value;
            if (movementContext._onMotion == null)
            {
                throw new NullReferenceException("Could not find OnMotionApplied event");
            }
            return movementContext;
        }

        public override void DirectApplyMotion(Vector3 motion, float deltaTime)
        {
            CollisionFlags collisionFlags = CharacterController.Move(motion + PlatformMotion, deltaTime);
            method_1(motion);
            _onMotion?.Invoke(collisionFlags);
        }
    }
}
