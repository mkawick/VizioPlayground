using System.Collections.Generic;
using UnityEngine;

public abstract partial class IHideableObject: MonoBehaviour
{
    Renderer[] _meshRenderer;
    static TinyWizHideableManager _tinyWizHideableManager;
    bool _isVisible;

    Vector3 _lastPosition;
    bool _hasMoved;
    public int HideableId { get; set; }

    public bool HasMoved { get { return _hasMoved; } }
    public void ClearMoved() { _hasMoved = false; }

    private void Start()
    {
        _hasMoved = false;
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
        if(Observant)
        {
            return;
        }
        if((transform.position- _lastPosition).sqrMagnitude > 0.01f)
        {
            _lastPosition = transform.position;
            _hasMoved = true;
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
}