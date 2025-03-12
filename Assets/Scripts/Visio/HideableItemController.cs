using System;
using UnityEngine;

public class HideableItemController : IHideableObject
{
    public override bool Observant { get { return false; } }


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
