using System;
using EFT;
using Il2CppInterop.Runtime.Injection;

namespace Fika.Headless.Classes;

public class HeadlessClientMovementContext : MovementContext
{
    public HeadlessClientMovementContext(IntPtr pointer) : base(pointer)
    {
    }

    public HeadlessClientMovementContext() : base(Il2CppInjection.Allocate<HeadlessClientMovementContext>())
    {
        ClassInjector.DerivedConstructorBody(this);
        ClassInjector.InvokeBaseConstructor<MovementContext>(this);
    }

    public override void ApplyGravity(ref Vector3 motion, float deltaTime, bool stickToGround)
    {
        // Do nothing
    }

    public new static HeadlessClientMovementContext Create(Player player, Il2CppSystem.Func<ICharacterController> characterControllerGetter, LayerMask groundMask)
    {
        return Create<HeadlessClientMovementContext>(player, characterControllerGetter, groundMask);
    }

    public override void DirectApplyMotion(Vector3 motion, float deltaTime)
    {
        // Do nothing
    }
}
