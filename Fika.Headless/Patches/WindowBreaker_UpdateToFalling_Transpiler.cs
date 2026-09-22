using EFT.Interactive;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches;

/// <summary>
/// Attempts to fix a nullref due to renders being disabled
/// </summary>
public class WindowBreaker_UpdateToFalling_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(WindowBreaker)
            .GetMethod(nameof(WindowBreaker.UpdateToFalling));
    }
    
    [PatchPrefix]
    public static bool Prefix(WindowBreaker __instance, WindowBreaker.Piece piece, bool wasStuck, in Vector3 position, in Vector3 force, bool instantFall)
    {
        if (wasStuck)
        {
            var ballisticCollider = piece.Description.BallisticCollider;
            ballisticCollider.UnsubscribeHitAction();
            ballisticCollider.enabled = false;
        }

        if (instantFall)
        {
            WindowBreaker.DestroyPiece(piece);
            return false;
        }

        var destroyTime = Random.Range(__instance.TimeUntilPartDie * 0.5f, __instance.TimeUntilPartDie * 1.5f);
        __instance.PieceDestroyTask(piece, destroyTime, position, force.normalized).HandleExceptions();
        return false;
    }
}
