using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class Visio : MonoBehaviour
{
    public struct ZoneInfo
    {
        public VizZone zone;
        public Collider collider;
    }

    const int OUTSIDE_ZONEID = -1;
    const int ZONE_SEARCH_RADIUS = 8;
    const string AUTOMATIC_SETUP = "Set On Runtime";
    const string MANUAL_SETUP = "Needs Manual Setup";
    const string DEBUG_SETUP = "DEBUG";
    const string OPTIONAL_SETUP = "Needs Manual Setup";
    const string INSPECTOR_ACTIONS = "Actions";

    

    [FoldoutGroup(MANUAL_SETUP, expanded: true, order: -1)]
    [SerializeField, Range(0, 600)] 
    int _numFramesAfterLeavingZoneBeforeHide = 120;
    [SerializeField, Range(0, 600)] 
    private int _gameplayRulesFrequency = 10;
    [SerializeField, FoldoutGroup(MANUAL_SETUP)] 
    bool _clearHistoryOnMoveFlag = true;
    [SerializeField, FoldoutGroup(MANUAL_SETUP)] 
    List<int> _zonesOutsideCanSee;

    [SerializeField, FoldoutGroup(MANUAL_SETUP)] 
    bool _waitForArenaToStart = false;
    [SerializeField] 
    int _debugZone = 1;


    [ShowInInspector, ReadOnly, FoldoutGroup(AUTOMATIC_SETUP)]
    LayerMask _layerToSearch;
    [ShowInInspector, ReadOnly, FoldoutGroup(AUTOMATIC_SETUP)]
    private bool hasFinishedInit = false;
    [ShowInInspector, ReadOnly, FoldoutGroup(AUTOMATIC_SETUP)]
    IHideableObject _localPlayer;
    [ShowInInspector, ReadOnly, FoldoutGroup(AUTOMATIC_SETUP)]
    List<ZoneInfo> _zoneList;
    [ShowInInspector, ReadOnly, FoldoutGroup(AUTOMATIC_SETUP)]
    public List<int> _zonesThatSeeOutside;

    [ShowInInspector, FoldoutGroup(DEBUG_SETUP)] 
    List<VisZoneVisualizer> _outsideCanSeeVisualizer => _zonesOutsideCanSee.Select(z => new VisZoneVisualizer(z)).ToList();

    TinyWizHideableManager _tinyWizPlayerManager;
    Collider[] _collidersTracker;
    Dictionary<Collider, VizZone> _colliderToZoneMap = new();
    private Dictionary<int, HideableZoneGroup> hideableZones;
    
    //private Dictionary<int, List<int>> externalZones;


    //----------------------------------------------------
    void Start()
    {
        hideableZones = new Dictionary<int, HideableZoneGroup>();
        InitializeVisio();

        hasFinishedInit = true;
    }


#if UNITY_EDITOR 
    [Button(ButtonSizes.Medium), FoldoutGroup(groupName: INSPECTOR_ACTIONS, order:1)]
    void InitializeVisio()
    {
        Debug.Log("Visio method InitializeVisio called");

        if (_tinyWizPlayerManager != null)
        {
            Debug.LogWarning("Visio already Initialized");

            return;
        }

        InitializeZoneList();

        if (_zonesThatSeeOutside == null)
            _zonesThatSeeOutside = new List<int>();
        if (_zonesThatSeeOutside.Contains(OUTSIDE_ZONEID) == false)
            _zonesThatSeeOutside.Add(OUTSIDE_ZONEID);

        _tinyWizPlayerManager = FindAnyObjectByType<TinyWizHideableManager>();

        InitializeHiddenZones();
    }

    [GUIColor(0, 1, 0.2f)]
    [Button(ButtonSizes.Medium), FoldoutGroup(groupName: INSPECTOR_ACTIONS)]
    void GrabOutsideVisibleZones()
    {
        HashSet<int> zonesIds = new();
        var zones = GameObject.FindObjectsOfType<VizZone>();
        foreach (var zone in zones)
        {
            switch (zone.type)
            {
                case VizZoneTag.Doorway:
                case VizZoneTag.Roof:
                    if (zone.visibleZonesICanSee.Contains(OUTSIDE_ZONEID))
                    {
                        zonesIds.Add(zone.ZoneId);
                    }
                    break;
            }
        }
        _zonesThatSeeOutside = zonesIds.ToList();
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    [GUIColor(0.8f, 0.9f, 0.2f)]
    [Button(ButtonSizes.Medium), FoldoutGroup(groupName: INSPECTOR_ACTIONS)]
    void ConnectLinesForZoneIdInScene()
    {
        var zones = GetComponentsInChildren<VizZone>();
        List<GameObject> foundZone = new List<GameObject>();
        foreach (var zone in zones)
        {
            if (zone.ZoneId == _debugZone)
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
    [Button, FoldoutGroup(groupName: INSPECTOR_ACTIONS)]
    void ZoomToZoneIdInScene()
    {
        var zones = GetComponentsInChildren<VizZone>();
        List<GameObject> foundZone = new List<GameObject>();
        foreach (var zone in zones)
        {
            if (zone.ZoneId == _debugZone)
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
    [GUIColor(1, 0.6f, 0.4f), FoldoutGroup(groupName: INSPECTOR_ACTIONS)]
    [Button()]
    void AddMissingVizZones()
    {
        var layerId = LayerMask.NameToLayer("Visio");
        int highestZoneId = -1;
        var zones = GetComponentsInChildren<VizZone>();
        Material anyHighlight = null;
        foreach (var zone in zones)
        {
            if (zone.ZoneId > highestZoneId)
                highestZoneId = zone.ZoneId + 1;
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
            newComp.ZoneId = highestZoneId++;
            newComp.highlightMaterial = anyHighlight;
            child.gameObject.layer = layerId;
            ErrorText += $"{child.name} added VizZone comp id={newComp.ZoneId}\n";
        }       
    }
    [GUIColor(0.4f, 0.4f, 0.8f), FoldoutGroup(groupName: INSPECTOR_ACTIONS)]
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
    [GUIColor(0.8f, 0.8f, 0.0f), FoldoutGroup(groupName: INSPECTOR_ACTIONS)]
    [Multiline(10)]
    public string ErrorText;

#endif

    void InitializeHiddenZones()
    {
        foreach(var zone in _zoneList)
        {
            if (zone.zone.hiddenZones != null && zone.zone.hiddenZones.Length > 0)
            {
                HideableZoneGroup group;
                int parentZoneId = zone.zone.ZoneId;
                if (hideableZones.ContainsKey(parentZoneId) == false)
                {
                    group = new HideableZoneGroup();
                    group.ZoneId = parentZoneId;
                    hideableZones.Add(parentZoneId, group);
                }
                else
                {
                    group = hideableZones[parentZoneId];
                }

                foreach (int zoneId in zone.zone.hiddenZones)
                {
                    var hiddenZones = GetZones(zoneId);
                    if (hiddenZones.Count == 0)
                        continue;

                    foreach (var hiddenZone in hiddenZones)
                    {
                        if (hiddenZone.ZoneHider == null)
                        {
                            hiddenZone.ZoneHider = group;
                        }
                        group.AddZone(hiddenZone);
                    }

                }
            }
        }
        foreach(var zone in hideableZones.Values)
        {
            zone.Init();
        }
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

        if (_localPlayer != null)
        {
            ShowInitialObjectsToLocalPlayer();
        }

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
        UpdateZonesOfMovedItems();

        // clear old visibility from each player
        HideObjectsFromObjectsInHistory();

        CheckForDistancesForLocalCharacter();
    }

    private void CheckForDistancesForLocalCharacter()
    {
        if (_localPlayer == null)
        {
            return;
        }

        foreach (var objId in _localPlayer.ObjectsISee)
        {
            if (!_tinyWizPlayerManager.AllObjects.TryGetValue(objId, out IHideableObject other))
            {
                continue;
            }

            var shouldShowLocallyUsingGameplayRules = false;// _tinyWizPlayerManager.ShouldShowLocallyUsingGameplayRules(_localPlayer, other);
            if (other.IsVisible() != shouldShowLocallyUsingGameplayRules)
            {
                other.shouldReconsiderVisibilityOnSameZones = true;
            }
        }
    }

    private void HideMeshesForLocalPlayer(IHideableObject hideable, 
        HashSet<int> _objectsISee, 
        Dictionary<int, IHideableObject> thoseToHide)
    {
        if (hideable == _localPlayer)
        {
            foreach (var prevObj in _objectsISee)
            {
                if (thoseToHide.TryGetValue(prevObj, out var hideableObject))
                {
                    hideableObject.MakeMeshVisible(false);
                }
            }
        }
    }

    private void HideObjectsFromObjectsInHistory()
    {
        if (_clearHistoryOnMoveFlag == false)
            return;

        var attentiveObjects = _tinyWizPlayerManager.AttentiveObjects;

        foreach (var attentiveObject in attentiveObjects.Values)
        {
            var history = attentiveObject.objectsIUsedToSee;

            if (history.Count() > 0)
            {
                attentiveObject.ClearHistory(Time.frameCount - _numFramesAfterLeavingZoneBeforeHide);
            }
        }
    }

    private void InformHidablesThatMyVisibilityHasChanged(IHideableObject attentive, List<int> listOfVisibleZones, bool makeVisible)
    {
        var zoneId = attentive.GetZone();
        var objectsInZone = GrabAllCharactersInZones(zoneId, listOfVisibleZones);

        var hidableId = attentive.HideableId;

        foreach (var newObjId in objectsInZone)
        {
            if (newObjId == attentive.HideableId)
                continue;

            if (_tinyWizPlayerManager.AllObjects.TryGetValue(newObjId, out var otherObject))
            {
                ChangeVisibility(makeVisible, otherObject, hidableId);
            }
        }
    }

    List<int> ListOfZones(int zoneIdThatTheySee)
    {
        var list = new List<int>();
        list.Add(zoneIdThatTheySee);

        for(int i = 0; i < _zoneList.Count; i++)
        {
            var visibleZones = _zoneList[i].zone.visibleZonesICanSee;

            for (int j = 0; j < visibleZones.Count; j++) 
            {
                if (visibleZones[j] == zoneIdThatTheySee)
                {
                    list.Add(_zoneList[i].zone.ZoneId);
                    break;
                }
            }
        }
        return list;
    }

    List<int>GetListOfExternalZonesThatSee(List<VizZone> zones)
    {
        HashSet<int> zoneIds = new HashSet<int>();
        foreach(var zone in zones)
        {
            for(int i=0; i< zone.externalZonesThatSeeMe.Count; i++)
            {
                zoneIds.Add(zone.externalZonesThatSeeMe[i]);
            }
        }
        return zoneIds.ToList();
    }

    private void HandleMoveZone(int previousZoneId, IHideableObject attentive, VizZone vizZone, int newZoneId)
    {
        if (previousZoneId != OUTSIDE_ZONEID)
        {
            List<int> externalZonesToSeePrevious = GetListOfExternalZonesThatSee(GetZones(previousZoneId));
            if (externalZonesToSeePrevious.Count != 0)
            {
                InformHidablesThatMyVisibilityHasChanged(attentive, externalZonesToSeePrevious, false);
            }
        }
        else
        {
            InformHidablesThatMyVisibilityHasChanged(attentive, _zonesThatSeeOutside, false);
        }

        if (vizZone != null)
        {
            List<int> externalZones = GetListOfExternalZonesThatSee(GetZones(vizZone.ZoneId));
            attentive.MoveZones(newZoneId, vizZone.visibleZonesICanSee);
            attentive.ApplyZoneSettings(vizZone.definition);
            InformHidablesThatMyVisibilityHasChanged(attentive, externalZones, true);
        }
        else
        {
            if (newZoneId != OUTSIDE_ZONEID)
            {
                Debug.LogError($"entering a zone that doesn't exist. newZoneId={newZoneId}");
            }

            attentive.MoveZones(newZoneId, _zonesThatSeeOutside);
            attentive.ApplyZoneSettings(VisibilityZoneDefinition.DefaultDefinition);
            InformHidablesThatMyVisibilityHasChanged(attentive, _zonesThatSeeOutside, true);
        }
    }

    void UpdateZonesOfMovedItems()
    {
        var attentiveObjects = _tinyWizPlayerManager.AttentiveObjects;
        bool didSomethingMove = false;

        foreach (var attentive in attentiveObjects.Values)
        {
            if (attentive.VisibilityDirty == false)
                continue;

            attentive.ClearDirty();

            int newZoneId;
            VizZone vizZone;
            FindMostLikelyContainingZone(
                you: attentive,
                layerToSearch: _layerToSearch,
                searchRadius: ZONE_SEARCH_RADIUS,
                out newZoneId,
                out vizZone);

            var previousZoneId = attentive.GetZone();
            if (newZoneId == previousZoneId)
            {
                didSomethingMove = attentive.shouldReconsiderVisibilityOnSameZones;
                continue;
            }

            bool isLocalPlayer = attentive == _localPlayer;
            HandleMoveZone(previousZoneId, attentive, vizZone, newZoneId);
            if(isLocalPlayer)
            {
                HandleHiddenZones(newZoneId, previousZoneId);
            }
            didSomethingMove = true;
        }

        // show visible 
        if (_localPlayer && didSomethingMove)
        {
            _localPlayer.ShowAllVisibleItems();
        }
    }

    private void HandleHiddenZones(int newZoneId, int previousZoneId)
    {
        if (hideableZones.ContainsKey(previousZoneId))
        {
            var prevZone = hideableZones[previousZoneId];
            prevZone.ZoneShow();
        }

        if (hideableZones.ContainsKey(newZoneId))
        {
            var nextZone = hideableZones[newZoneId];
            nextZone.ZoneHide();
        }
    }

    private void SetInitialZone(IHideableObject hideable)
    {
        int zoneToSet;
        VizZone vizZone;

        FindMostLikelyContainingZone(
            you: hideable,
            layerToSearch: _layerToSearch,
            searchRadius: ZONE_SEARCH_RADIUS,
            out zoneToSet,
            out vizZone); 
        hideable.SetZone(zoneToSet);
        if (vizZone != null)
        {
            hideable.AddVisibleZones(vizZone.ZoneId, vizZone.visibleZonesICanSee);
        }
        else
        {
            hideable.AddVisibleZones(zoneToSet, _zonesThatSeeOutside);
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

    public class DistanceComparer : IComparer<Collider>
    {
        private Transform target;

        public DistanceComparer(Transform distanceToTarget)
        {
            target = distanceToTarget;
        }

        public int Compare(Collider a, Collider b)
        {
            var targetPosition = target.position;
            return Vector3.Distance(a.transform.position, targetPosition).CompareTo(Vector3.Distance(b.transform.position, targetPosition));
        }
    }

    public List<Collider> GetCollidersOrderedByDistOnLayer(Vector3 position, float searchRadius, int layerMask)
    {
        int num = Physics.OverlapSphereNonAlloc(position, searchRadius, _collidersTracker, layerMask);

        DistanceComparer distanceComparer = new DistanceComparer(transform);
        System.Array.Sort(_collidersTracker, 0, num, distanceComparer);
        List<Collider> colliders = new List<Collider>();
        for (int i = 0; i < num; i++)
        {
            colliders.Add(_collidersTracker[i]);
        }

        return colliders;
    }

    private void InitializeZoneList()
    {
        _zoneList = new List<ZoneInfo>();
        _layerToSearch = LayerMask.GetMask("Visio");
        var zones = FindObjectsByType<VizZone>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        _collidersTracker = new Collider[zones.Length];

        for (int i = 0; i < zones.Length; i++)
        {
            var zone = zones[i];
            var collider = zone.GetComponent<Collider>();

            _zoneList.Add(new ZoneInfo { zone = zone, collider = collider });
        }

        for (int i = 0; i < _zoneList.Count; i++)
        {
            var zone = _zoneList[i].zone;
            zone.externalZonesThatSeeMe = ListOfZones(zone.ZoneId);
        }

        //_zonesThatSeeOutside = ListOfZones(OUTSIDE_ZONEID);
    }

    private void ShowInitialObjectsToLocalPlayer()
    {
        if (_localPlayer == null)
            return;

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

    VizZone GetOrCacheVizZone(Collider collider, VizZone vizZone)
    {
        if (!_colliderToZoneMap.TryGetValue(collider, out vizZone))
        {
            vizZone = collider.GetComponent<VizZone>();

            if (vizZone == null)
            {
                Debug.LogWarning("[VISIO] Collider without VizZone", collider.gameObject);
            }
            else
            {
                _colliderToZoneMap.Add(collider, vizZone);
            }
        }

        return vizZone;
    }

    private void FindMostLikelyContainingZone(IHideableObject you, LayerMask layerToSearch, int searchRadius, out int zoneToSet, out VizZone vizZone)
    {
        List<Collider> colliders = GetCollidersOrderedByDistOnLayer(you.transform.position, searchRadius, layerToSearch);

        Collider innerMostZone = null;
        zoneToSet = OUTSIDE_ZONEID;
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

    List<VizZone> GetZones(int zoneId)
    {
        List<VizZone> returnList = new();
        foreach (var zone in _zoneList)
        {
            if (zone.zone.ZoneId == zoneId)
                returnList.Add( zone.zone);
        }
        return returnList;
    }

    private void ChangeVisibility(bool makeVisible, IHideableObject newObj, int hidableId)
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
