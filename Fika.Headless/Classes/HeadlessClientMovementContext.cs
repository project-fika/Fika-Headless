using EFT;
using System;

namespace Fika.Headless.Classes;

public class HeadlessClientMovementContext : MovementContext
{
    public override void ApplyGravity(ref Vector3 motion, float deltaTime, bool stickToGround)
    {
        // Do nothing
    }

    public new static HeadlessClientMovementContext Create(Player player, Func<IAnimator> animatorGetter, Func<ICharacterController> characterControllerGetter, LayerMask groundMask)
    {
        HeadlessClientMovementContext movementContext = Create<HeadlessClientMovementContext>(player, animatorGetter, characterControllerGetter, groundMask);
        return movementContext;
    }

    public override void DirectApplyMotion(Vector3 motion, float deltaTime)
    {
        // Do nothing
    }
}
