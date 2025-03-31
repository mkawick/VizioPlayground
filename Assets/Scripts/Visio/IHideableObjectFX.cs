using System.Collections.Generic;
using UnityEngine;

public class VisibilityZoneDefinition 
{
    public float speed;
    public float visibility;
    public float range;

}

public abstract partial class IHideableObject: MonoBehaviour
{
    [HideInInspector] public float restrictedVisionRange = float.MaxValue;
    [HideInInspector] public float normalVisionRange = float.MaxValue;

    //------------------------------------------------------------------------
    public virtual bool HasBlindEffect() => false;
    public abstract bool HasVisibilityEffect();
    // effect ID is returned for canceling
    public abstract void ApplyVisibilityEffect(int lengthInMs, bool isInvisible);// can be forced to remain visible
    public abstract void CancelEffect(int id);
    public abstract void ClearAllEffects();
    public abstract void ApplyZoneSettings(VisibilityZoneDefinition definition);
}