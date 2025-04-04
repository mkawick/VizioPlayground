using UnityEngine;

public abstract partial class IHideableObject: MonoBehaviour
{
    static protected TinyWizHideableManager _tinyWizHideableManager;


    [SerializeField] public Transform root;
    bool alwaysSearchForRendersOnStart = true;

    protected Renderer[] _meshRenderer;

    protected bool _isVisible;
    protected Vector3 _lastPosition;
    protected bool _visibilityDirty;
    public bool shouldReconsiderVisibilityOnSameZones { get; internal set; }
    protected int invisioStack = 0;
    protected int superVisionStack = 0;

    public int HideableId { get; set; }

    public bool VisibilityDirty 
    { 
        get { return _visibilityDirty; } 
    }
    public void ClearDirty() 
    {
        _visibilityDirty = false; 
    }

    private void Start()
    {
        if (root == null)
        {
            root = transform;
        }

        _visibilityDirty = true;
        shouldReconsiderVisibilityOnSameZones = true;
        _isVisible = true;
        _lastPosition = root.position;

        FindRenderersInChildren(alwaysSearchForRendersOnStart, transform);
    }
    void OnDestroy()
    {
        if(_tinyWizHideableManager)
        {
            _tinyWizHideableManager.Remove(this);
        }
    }

    void Update()
    {
        CheckDirty();
    }

    void CheckDirty()
    {
        ValidateRegister();

        if (!Moveable)
        {
            return;
        }

        var movedDist = (root.position - _lastPosition).sqrMagnitude;
        if (movedDist > 0.01f)
        {
            _lastPosition = root.position;
            _visibilityDirty = true;
        }
    }
    
    // TODO:Ideally this is done on start, however we are currently having initialization issues 
    void ValidateRegister()
    {
        if (HideableId == 0)
        {
            if (_tinyWizHideableManager == null)
            {
                _tinyWizHideableManager = GameObject.FindAnyObjectByType<TinyWizHideableManager>();
            }

            _tinyWizHideableManager?.Register(this);
        }
    }

    public virtual bool IsLocalPlayer() 
    {
        return false;
    }

    public virtual Vector3 Position
    {
        get { return root.transform.position; }
    }

    public virtual bool Observant 
    {
        get { return true; } 
    }

    public virtual bool Moveable
    {
        get { return false; }
        set { }
    }

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

    public bool IsVisible() 
    {
        return _isVisible;
    }

    public virtual void MakeMeshVisible(bool visible) 
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

    public virtual bool IgnoreDistance => false;

    internal bool HasSuperVision()
    {
        if (superVisionStack == 0)
            return false;
        return true;
    }

    protected virtual void FindRenderersInChildren(bool invalidatePrevious, Transform search)
    {
        if (_meshRenderer == null
            || _meshRenderer.Length == 0
            || invalidatePrevious)
        {
            _meshRenderer = search.GetComponentsInChildren<Renderer>(true);
        }
    }
}