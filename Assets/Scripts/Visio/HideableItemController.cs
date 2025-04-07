using System;
using UnityEngine;

public class HideableItemController : IHideableObject
{
    [SerializeField] protected ParticleSystem[] _particles;

    protected override void FindRenderersInChildren(bool invalidatePrevious, Transform search)
    {
        base.FindRenderersInChildren(invalidatePrevious, search);
        if (invalidatePrevious
            || _particles == null
            || _particles.Length == 0)
        {
            _particles = search.GetComponentsInChildren<ParticleSystem>();
        }
    }

    public override bool Observant { get { return false; } }
    public override bool Moveable
    {
        get;
        set;
    }


    bool invisibilityActive = false;

    public override bool HasVisibilityEffect()
    {
        return invisibilityActive;
    }

    public override void ApplyVisibilityEffect(int lengthInMs, bool isInvisible)
    {
        invisibilityActive = isInvisible;
    }

    public override void CancelEffect(int id)
    {
        invisibilityActive = false;
    }

    public override void ClearAllEffects()
    {
        invisibilityActive = false;
    }

    public override void MakeMeshVisible(bool visible)
    {
        if (_meshRenderers != null && _meshRenderers.Length > 0)
        {
            // Debug.LogWarning($"MakeMeshVisible: _meshRenderer {this.name}, {Environment.StackTrace}");
            for (int i = 0; i < _meshRenderers.Length; i++)
            {
                _meshRenderers[i].enabled = visible;
            }
        }

        var layer = PhysicsLayer.particles.LayerInt;
        if (!visible)
        {
            layer = PhysicsLayer.invisibleObjects.LayerInt;
        }
        if (_particles != null && _particles.Length > 0)
        {
            for (int i = 0; i < _particles.Length; i++)
            {
                _particles[i].gameObject.layer = layer;

            }
        }
    }
    public override void ApplyZoneSettings(VisibilityZoneDefinition definition)
    {
        //do nothing
    }
#if __TINYWIZARD__
#endif
}
