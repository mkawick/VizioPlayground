using System;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class IHideableObject: MonoBehaviour
{
    Renderer[] _meshRenderer;
    static protected TinyWizHideableManager _tinyWizHideableManager;
    bool _isVisible;

    Vector3 _lastPosition;
    bool _visibilityDirty;
    public int HideableId { get; set; }

    int invisioStack = 0;
    int superVisionStack = 0;

    public bool VisibilityDirty { get { return _visibilityDirty; } }
    public void ClearDirty() { _visibilityDirty = false; }

    private void Start()
    {
        _visibilityDirty = false;
        _isVisible = true;
        _lastPosition = transform.position;

        if (_tinyWizHideableManager == null)
        {
            _tinyWizHideableManager = GameObject.FindAnyObjectByType<TinyWizHideableManager>();
        }
        _tinyWizHideableManager.Register(this);

        _meshRenderer = GetComponentsInChildren<Renderer>();
        if (_meshRenderer.Length > 0)
        {
            _isVisible = _meshRenderer[0].enabled;
        }
        else
        {
            Debug.Log($"no renderer on {this.name}");
        }
    }
    void OnDestroy()
    {
        if(_tinyWizHideableManager)
        {
            _tinyWizHideableManager.Remove(this);
        }
    }
    private void Update()
    {
        if(!Observant)
        {
            return;
        }
        var movedDist = (transform.position - _lastPosition).sqrMagnitude;
        if (movedDist > 0.01f)
        {
            _lastPosition = transform.position;
            _visibilityDirty = true;
        }
    }

    
    public virtual bool IsLocalPlayer() { return false; }
    public virtual bool Observant { get { return true; } }

    public void Show() 
    { 
        if (IsVisible()) 
            return;
        _isVisible = true; 
        MakeMeshVisible(_isVisible); 
    }
    public virtual void Hide() 
    { 
        if (!IsVisible()) 
            return; 
        _isVisible = false; 
        MakeMeshVisible(_isVisible); 
    }
    public bool CanShow() { return true; }
    public bool CanHide() { return true; }

    public bool IsVisible() { return _isVisible; }

    public void MakeMeshVisible(bool visible) 
    {
        if (_meshRenderer == null)
            return;

        for(int i = 0; i< _meshRenderer.Length; i++)
        {
            _meshRenderer[i].enabled = visible;
        }
    }

    internal bool HasInvisibilityEffectActive()
    {
        if(invisioStack == 0)
            return false;
        return true;
    }

    internal bool HasSuperVision()
    {
        if (superVisionStack == 0)
            return false;
        return true;
    }
}