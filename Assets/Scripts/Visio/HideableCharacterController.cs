using System;
using UnityEngine;

public class HideableCharacterController : IHideableObject
{
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
