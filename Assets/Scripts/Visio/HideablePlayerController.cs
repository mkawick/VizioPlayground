using System;
using UnityEngine;

public class HideablePlayerController : IHideableObject
{
    public override void Hide() { }
    public override bool IsLocalPlayer() { return true; }

    public override bool HasVisibilityEffect()
    {
        throw new NotImplementedException();
    }

    public override int ApplyVisibilityEffect(int lengthInMs, bool isInvisible)
    {
        throw new NotImplementedException();
    }

    public override void CancelEffect(int id)
    {
        throw new NotImplementedException();
    }

    public override void ClearAllEffects()
    {
        throw new NotImplementedException();
    }
}
