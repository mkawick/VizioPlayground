using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Visio : MonoBehaviour
{
    public struct ZoneInfo
    {
        public VizZone zone;
        public Collider collider;
    }

    [SerializeField, Range(0, 600)] int numFramesAfterLeavingZoneBeforeHide = 120;
    [SerializeField] bool clearHistory = true;
    [SerializeField] int zoneToSelect = 1;
    public List<int> _ListOfVisibleZones;

    const int outsideZone = -1;
    const int zoneSearchRadius = 8;
    LayerMask _layerToSearch;
    TinyWizHideableManager _tinyWizPlayerManager;
    IHideableObject _localPlayer;
    Collider[] _collidersTracker;

    public List<ZoneInfo> ZoneList { get; private set; }
    List<int> zonesThatSeeOutside;

    

    //----------------------------------------------------
    void Start()
    {
        InitializeZoneList();
        if (_ListOfVisibleZones == null)
            _ListOfVisibleZones = new List<int>();
        if (_ListOfVisibleZones.Contains(outsideZone) == false)
            _ListOfVisibleZones.Add(outsideZone);
        _tinyWizPlayerManager = GameObject.FindAnyObjectByType<TinyWizHideableManager>();
    }

    [GUIColor(0.8f, 0.9f, 0.2f)]
    [Button(ButtonSizes.Medium)]
    void ConnectLinesForZoneIdInScene()
    {
        var zones = GetComponentsInChildren<VizZone>();
        List<GameObject> foundZone = new List<GameObject>();
        foreach (var zone in zones)
        {
            if (zone.ZoneId == zoneToSelect)
            {
                foundZone.Add(zone.gameObject);
            }
        }
        if(foundZone.Count > 0)
        {
            if(foundZone.Count > 1)
            {
                for (int i = 0, j = 1; i < foundZone.Count; i++)
                {
                    Debug.DrawLine(foundZone[i].gameObject.transform.position, foundZone[j].gameObject.transform.position, Color.yellow, 5);
                   
                    if (j < foundZone.Count - 1)
                    {
                        j++;
                    }
                    else
                    {
                        j = 0;
                    }
                }
            }
        }
    }
    [Button]
    void ZoomToZoneIdInScene()
    {
        var zones = GetComponentsInChildren<VizZone>();
        List<GameObject> foundZone = new List<GameObject>();
        foreach (var zone in zones)
        {
            if (zone.ZoneId == zoneToSelect)
            {
                foundZone.Add(zone.gameObject);
            }
        }
        if (foundZone.Count > 0)
        {
            var prevObj = Selection.activeGameObject;
            Selection.objects = foundZone.ToArray();
            SceneView.FrameLastActiveSceneView();
            Selection.activeGameObject = prevObj;
        }
    }
    [GUIColor(1, 0.6f, 0.4f)]
    [Button()]
    void AddMissingVizZones()
    {
        var layerId = LayerMask.NameToLayer("Visio");
        int highestZoneId = -1;
        var zones = GetComponentsInChildren<VizZone>();
        Material anyHighlight = null;
        foreach (var zone in zones)
        {
            if (zone._OverrideZoneId > highestZoneId)
                highestZoneId = zone._OverrideZoneId + 1;
            zone.gameObject.layer = layerId;
            if(anyHighlight == null)
            {
                if (zone.highlightMaterial != null)
                    anyHighlight = zone.highlightMaterial;
            }
        }

        ErrorText = "";
        int numModified = 0;
        Transform[] childArray = GetComponentsInChildren<Transform>();
        foreach(var child in childArray)
        {
            if (child.GetComponent<VizZone>() != null ||
                child.transform == this.transform || 
                child.GetComponent<Canvas>() != null ||
                child.GetComponent<TextMeshProUGUI>() != null)
                continue;
            numModified++;
            var newComp = child.gameObject.AddComponent<VizZone>();
            newComp._OverrideZoneId = highestZoneId++;
            newComp.highlightMaterial = anyHighlight;
            child.gameObject.layer = layerId;
            ErrorText += $"{child.name} added VizZone comp id={newComp._OverrideZoneId}\n";
        }       
    }
    [GUIColor(0.4f, 0.4f, 0.8f)]
    [Button()]
    void CheckForMissingColliders()
    {
        ErrorText = "";
        int numModified = 0;
        Transform[] childArray = GetComponentsInChildren<Transform>();
        foreach (var child in childArray)
        {
            if (child.GetComponent<Collider>() != null ||
                child.transform == this.transform ||
                child.GetComponent<Canvas>() != null ||
                child.GetComponent<TextMeshProUGUI>() != null)
                continue;

            ErrorText += $"{child.name} is missing collider\n";
        }

    }
    [GUIColor(0.8f, 0.8f, 0.0f)]
    [Multiline(10)]
    public string ErrorText;

    void ClearHighlightOnZones(VizZone[] zones)
    {
        /* foreach (var zone in zones)
         {
             zone.ShowSelected(false);
         }*/
    }
    

    private void AddNewObjectsToZones()
    {
        var hideables = _tinyWizPlayerManager.NewlySpawnedObjects;
        if (hideables.Count == 0)
            return;

        foreach (var hideable in hideables)
        {
            SetInitialZone(hideable);
            if (hideable.IsLocalPlayer())
            {
                _localPlayer = hideable;
                hideable.Show();
            }
        }

        MakeAllObjectsAwareOfOneAnother();
        ShowInitialObjectsToLocalPlayer();

        _tinyWizPlayerManager.NewlySpawnedObjects.Clear();
    }


    void Update()
    {
        if (_tinyWizPlayerManager &&
            _tinyWizPlayerManager.HasFinishedInit)
        {
            AddNewObjectsToZones();
        }

        // update everyone's zones
        // save a list of those who have moved
        var movables = _tinyWizPlayerManager.AttentiveObjects;
        UpdateZonesOfMovedItems();

        // each hidable saves the list of hidables that they used to know (used to notify later) ... only those that Observe
        // look at old Observers and tell them that I left

        // clear old visibility from each player
        HideObjectsFromObjectsInHistory();
    }

    private void HideMeshesForLocalPlayer(IHideableObject hideable, 
        HashSet<int> _objectsISee, 
        Dictionary<int, IHideableObject> thoseToHide)
    {
        if (hideable == _localPlayer)
        {
            foreach (var prevObj in _objectsISee)
            {
                if (thoseToHide.ContainsKey(prevObj))
                {
                    thoseToHide[prevObj].MakeMeshVisible(false);
                }
            }
        }
    }

    private void HideObjectsFromObjectsInHistory()
    {
        if (clearHistory == false)
            return;

        var hideables = _tinyWizPlayerManager.AttentiveObjects;
        foreach (var hideable in hideables.Values)
        {
            var history = hideable.objectsIUsedToSee;

            if(history.Count() > 0)
            {
                hideable.ClearHistory(Time.frameCount - numFramesAfterLeavingZoneBeforeHide);
            }
        }
    }

    private void InformHidablesThatMyVisibilityHasChanged(IHideableObject hideable, List<int> listOfVisibleZones, bool makeVisible, bool changeMeshState)
    {
        var zoneId = hideable.GetZone();        
        var objectsInZone = GrabAllCharactersInZones(zoneId, listOfVisibleZones);

        bool hasInvisoOn = hideable.HasInvisibilityEffectActive();

        var hidableId = hideable.HideableId;
        foreach (var newObjId in objectsInZone)
        {
            bool newVisibilityState = makeVisible;
            if (_tinyWizPlayerManager.AllObjects.ContainsKey(newObjId))
            {
                var newObj = _tinyWizPlayerManager.AllObjects[newObjId];
                bool canSeeAnyway = newObj.HasSuperVision();

                if (makeVisible == false && canSeeAnyway)
                    newVisibilityState = true;
                if (hasInvisoOn == true)
                    newVisibilityState = false;

                ChangeVisibility(makeVisible, newObj, hidableId, changeMeshState);
            }
        }
    }

    List<int> ListOfZones(int zoneIdThatTheySee)
    {
        var list = new List<int>();
        list.Add(zoneIdThatTheySee);
        for(int i = 0; i < ZoneList.Count; i++)
        {
            var visibleZones = ZoneList[i].zone._ListOfVisibleZones;
            for (int j = 0; j < visibleZones.Length; j++) 
            {
                if (visibleZones[j] == zoneIdThatTheySee)
                {
                    list.Add(ZoneList[i].zone.ZoneId);
                    break;
                }
            }
        }
        return list;
    }

    void UpdateZonesOfMovedItems()
    {
        var hideables = _tinyWizPlayerManager.AttentiveObjects;
        foreach (var hideable in hideables.Values)
        {
            if (hideable.VisibilityDirty == false)
                continue;

            hideable.ClearDirty();

            int newZoneId;
            VizZone vizZone;
            FindMostLikelyContainingZone(hideable, _layerToSearch, zoneSearchRadius, out newZoneId, out vizZone);

            if (newZoneId == hideable.GetZone())
                continue;
            bool isLocalPlayer = hideable == _localPlayer;
            if (hideable.GetZone() != outsideZone)
            {
                VizZone oldVizZone = GetZone(hideable.GetZone());
                if (oldVizZone != null)
                {
                    InformHidablesThatMyVisibilityHasChanged(hideable, oldVizZone.externalZonesThatSeeMe, false, isLocalPlayer);
                }
            }
            else
            {
                InformHidablesThatMyVisibilityHasChanged(hideable, _ListOfVisibleZones, false, isLocalPlayer);
            }

            if (vizZone != null)
            {
                hideable.MoveZones(newZoneId, vizZone._ListOfVisibleZones.ToList());
                InformHidablesThatMyVisibilityHasChanged(hideable, vizZone.externalZonesThatSeeMe, true, isLocalPlayer);
            }
            else
            {
                hideable.MoveZones(newZoneId, _ListOfVisibleZones);
                InformHidablesThatMyVisibilityHasChanged(hideable, zonesThatSeeOutside , true, isLocalPlayer);
            }
        }

        // show visible 
        if (_localPlayer)
        {
            _localPlayer.ShowAllVisibleItems();
        }
    }

    List<IHideableObject> TrackHidablesInZones(List<int> zoneIds)
    {
        List<IHideableObject> subset = new List<IHideableObject>();
        var hideables = _tinyWizPlayerManager.AllObjects;
        foreach(var hidable in hideables.Values)
        {
            if (hidable.IsInZones(zoneIds))
                subset.Add(hidable);
        }
        return subset;
    }

    private void SetInitialZone(IHideableObject hideable)
    {
        int zoneToSet;
        VizZone vizZone;

        FindMostLikelyContainingZone(hideable, _layerToSearch, zoneSearchRadius, out zoneToSet, out vizZone);
        hideable.SetZone(zoneToSet);
        if (vizZone != null)
        {
            hideable.AddVisibleZones(vizZone.ZoneId, vizZone._ListOfVisibleZones);
        }
        else
        {
            hideable.AddVisibleZones(zoneToSet, _ListOfVisibleZones.ToArray());
        }
    }

    private void MakeAllObjectsAwareOfOneAnother()
    {
        var hideables = _tinyWizPlayerManager.AllObjects;
        
        foreach (var hideable in hideables.Values)
        {
            hideable.InitializeObjectsThatISee();
        }
    }

    public List<Collider> GetCollidersOrderedByDistOnLayer(Vector3 position, float searchRadius, int layerMask)
    {
        int num = Physics.OverlapSphereNonAlloc(position, searchRadius, _collidersTracker, layerMask);

        return _collidersTracker.Take(num).OrderBy((d) => (d.transform.position - position).sqrMagnitude).ToList();
    }

    private void InitializeZoneList()
    {
        ZoneList = new List<ZoneInfo>();
        var zones = GetComponentsInChildren<VizZone>();
        _layerToSearch = LayerMask.GetMask("Visio");
        _collidersTracker = new Collider[zones.Length];

        for (int i = 0; i < zones.Length; i++)
        {
            var zone = zones[i];
            var collider = zone.GetComponent<Collider>();

            ZoneList.Add(new ZoneInfo { zone = zone, collider = collider });
        }
        ClearHighlightOnZones(zones);

        for (int i = 0; i < ZoneList.Count; i++)
        {
            var zone = ZoneList[i].zone;
            zone.externalZonesThatSeeMe = ListOfZones(zone.ZoneId);
        }

        zonesThatSeeOutside = ListOfZones(outsideZone);
    }

    private void ShowInitialObjectsToLocalPlayer()
    {
        var hideables = _tinyWizPlayerManager.AllObjects;

        // this is the most efficient way to do this
        var localPlayerZones = _localPlayer.GetZonesISee();
        foreach (var hideable in hideables.Values)
        {
            if (hideable.IsLocalPlayer())
            {
                continue;
            }
            if (hideable.IsInZones(localPlayerZones))
            {
                hideable.Show();
            }
            else
            {
                hideable.Hide();
            }
        }
    }

    private void FindMostLikelyContainingZone(IHideableObject you, LayerMask layerToSearch, int searchRadius, out int zoneToSet, out VizZone vizZone)
    {
        List<Collider> colliders = GetCollidersOrderedByDistOnLayer(you.transform.position, searchRadius, layerToSearch);

        Collider innerMostZone = null;
        zoneToSet = outsideZone;
        vizZone = null;
        foreach (var collider in colliders)
        {
            // TODO... deal with negative bounds
            if (collider.bounds.Contains(you.transform.position))// TODO: we need a better comparison
            {
                vizZone = collider.GetComponent<VizZone>();
                zoneToSet = vizZone.ZoneId;
                innerMostZone = collider;
                break;// inner most zone
            }
        }
    }

    private HashSet<int> GrabAllCharactersInZones(int zoneId, List<int> listOfVisibleZones)
    {
        HashSet<int> objectsInZone = new HashSet<int>();

        // ensure the current zone is considered
        _tinyWizPlayerManager.GetAllPlayersInZone(zoneId, objectsInZone);
        foreach (var zoneThatSeeThisOne in listOfVisibleZones)
        {
            if (zoneId == zoneThatSeeThisOne)// prevent bad setup bug where designer lists a zone in it's visible zones
                continue;
            _tinyWizPlayerManager.GetAllPlayersInZone(zoneThatSeeThisOne, objectsInZone);
        }
        return objectsInZone;
    }

    VizZone GetZone(int zoneId)
    {
        foreach (var zone in ZoneList)
        {
            if (zone.zone.ZoneId == zoneId)
                return zone.zone;
        }
        return null;
    }

    private void ChangeVisibility(bool makeVisible, IHideableObject newObj, int hidableId, bool changeMeshState)
    {
        if (makeVisible)
        {
            newObj.ObjectBecameVisible(hidableId);
        }
        else
        {
            newObj.ObjectBecameInvisible(hidableId);
        }
    }
}
