using Sirenix.OdinInspector;
using UnityEngine;

public abstract partial class IHideableObject : MonoBehaviour
{
    public enum VisibleState
    {

        Initializing,
        Visible,
        Hidden,
        Disabled
    }

    protected static TinyWizHideableManager _tinyWizHideableManager;

    [SerializeField] public VisibleState startAs = VisibleState.Visible;
    [SerializeField] public Transform root;

    [Header(
        "This will ignore previous setup and search for renders on start. Disable this if you want to precache complex structures")]
    [SerializeField] bool alwaysSearchForRendersOnStart = true;

    protected Renderer[] _meshRenderers;

    VisibleState visibleState = VisibleState.Initializing;
    Vector3 _lastPosition;
    bool _visibilityDirty;
    protected int invisioStack = 0;
    protected int superVisionStack = 0;

    public int HideableId { get; private set; }

    public bool VisibilityDirty
    {
        get { return _visibilityDirty; }
    }

    public void ClearDirty()
    {
        _visibilityDirty = false;
    }

    private void OnEnable()
    {
        if (root == null)
        {
            root = transform;
        }
        _lastPosition = root.position;

        FindRenderersInChildren(alwaysSearchForRendersOnStart, transform);

        //MakeMeshVisible(true); // TODO .... only show if local player says yes
    }

    void OnDisable()
    {
        if (_tinyWizHideableManager)
        {
            HideableId = _tinyWizHideableManager.Remove(this);
        }
        visibleState = VisibleState.Disabled;
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


            HideableId = _tinyWizHideableManager?.Register(this) ?? 0;
            if (HideableId != 0)
            {
                _visibilityDirty = true;
                shouldReconsiderVisibilityOnSameZones = true;
            }
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

    [ShowInInspector]
    public virtual bool Observant
    {
        get { return true; }
    }

    [ShowInInspector]
    public virtual bool Moveable
    {
        get { return false; }
        set { }
    }

    public bool shouldReconsiderVisibilityOnSameZones { get; internal set; }

    public void Show()
    {
        if (visibleState == VisibleState.Visible)
        {
            return;
        }
        visibleState = VisibleState.Visible;
        MakeMeshVisible(true);
    }

    public void Hide()
    {
        if (visibleState == VisibleState.Hidden)
        {
            return;
        }
        visibleState = VisibleState.Hidden;
        MakeMeshVisible(false);
    }

    public bool IsVisible()
    {
        return visibleState == VisibleState.Visible;
    }

    public virtual void MakeMeshVisible(bool visible)
    {
        if (_meshRenderers != null && _meshRenderers.Length > 0)
        {
            for (int i = 0; i < _meshRenderers.Length; i++)
            {
                _meshRenderers[i].enabled = visible;
            }
        }
    }

    internal bool HasInvisibilityEffectActive()
    {
        return invisioStack != 0;
    }

    public virtual bool IgnoreDistance => false;

    protected virtual void FindRenderersInChildren(bool invalidatePrevious, Transform search)
    {
        if (_meshRenderers == null
            || _meshRenderers.Length == 0
            || invalidatePrevious)
        {
            _meshRenderers = search.GetComponentsInChildren<Renderer>(true);
        }
    }

}