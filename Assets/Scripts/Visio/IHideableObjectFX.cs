using System.Collections.Generic;
using UnityEngine;

public abstract partial class IHideableObject: MonoBehaviour
{
    //------------------------------------------------------------------------

    public abstract bool HasVisibilityEffect();
    // effect ID is returned for canceling
    public abstract int ApplyVisibilityEffect(int lengthInMs, bool isInvisible);// can be forced to remain visible
    public abstract void CancelEffect(int id);
    public abstract void ClearAllEffects();
}