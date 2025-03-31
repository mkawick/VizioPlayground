using Sirenix.OdinInspector;
using System;
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

    public List<ZoneInfo> ZoneList { get; private set; }

    [SerializeField, Range(0, 600)] int numFramesAfterLeavingZoneBeforeHide = 120;
    [SerializeField, Range(0, 600)] private int gameplayRulesFrequency = 10;
    [SerializeField] bool clearHistoryOnMoveFlag = true;
    [SerializeField] bool waitForArenaToStart = false;
    [SerializeField] int debugZone = 1;

    [FormerlySerializedAs("_ListOfVisibleZones")]
    public List<int> _zonesThatSeeOutside;

    [ShowInInspector] LayerMask _layerToSearch;

    TinyWizHideableManager _tinyWizPlayerManager;
    IHideableObject _localPlayer;
    Collider[] _collidersTracker;
    Dictionary<Collider, VizZone> _colliderToZoneMap = new();
    
    private bool hasFinishedInit = false;
    private Dictionary<int, HideableZoneGroup> hideableZones;


    //----------------------------------------------------
    void Start()
    {
        hideableZones = new Dictionary<int, HideableZoneGroup>();
        InitializeVisio();

        hasFinishedInit = true;
    }


#if UNITY_EDITOR 
    [Button(ButtonSizes.Medium)]
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
    [Button(ButtonSizes.Medium)]
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
    [Button(ButtonSizes.Medium)]
    void ConnectLinesForZoneIdInScene()
    {
        var zones = GetComponentsInChildren<VizZone>();
        List<GameObject> foundZone = new List<GameObject>();
        foreach (var zone in zones)
        {
            if (zone.ZoneId == debugZone)
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
            if (zone.ZoneId == debugZone)
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

#endif

    void InitializeHiddenZones()
    {
        foreach(var zone in ZoneList)
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
                    // auto add roofs to visible outside 
                    if (_zonesThatSeeOutside.Contains(zoneId) == false)
                    {
                        _zonesThatSeeOutside.Add(zoneId);
                    }

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
        UpdateZonesOfMovedItems();

        // each hidable saves the list of hidables that they used to know (used to notify later) ... only those that Observe
        // look at old Observers and tell them that I left

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
                if (thoseToHide.ContainsKey(prevObj))
                {
                    thoseToHide[prevObj].MakeMeshVisible(false);
                }
            }
        }
    }

    private void HideObjectsFromObjectsInHistory()
    {
        if (clearHistoryOnMoveFlag == false)
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

    private void InformHidablesThatMyVisibilityHasChanged(IHideableObject hideable, List<int> listOfVisibleZones, bool makeVisible)
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

                ChangeVisibility(makeVisible, newObj, hidableId);
            }
        }
    }

    List<int> ListOfZones(int zoneIdThatTheySee)
    {
        var list = new List<int>();
        list.Add(zoneIdThatTheySee);

        for(int i = 0; i < ZoneList.Count; i++)
        {
            var visibleZones = ZoneList[i].zone.visibleZonesICanSee;

            for (int j = 0; j < visibleZones.Count; j++) 
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

    private void HandleMoveZone(int previousZoneId, IHideableObject attentive, VizZone vizZone, int newZoneId)
    {
        if (previousZoneId != OUTSIDE_ZONEID)
        {
            VizZone oldVizZone = GetZone(previousZoneId);

            if (oldVizZone != null)
            {
                InformHidablesThatMyVisibilityHasChanged(attentive, oldVizZone.externalZonesThatSeeMe, false);
            }
        }
        else
        {
            InformHidablesThatMyVisibilityHasChanged(attentive, _zonesThatSeeOutside, false);
        }

        if (vizZone != null)
        {
            attentive.MoveZones(newZoneId, vizZone.visibleZonesICanSee);
            attentive.ApplyZoneSettings(vizZone.definition);
            InformHidablesThatMyVisibilityHasChanged(attentive, vizZone.externalZonesThatSeeMe, true);
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

    //TODO: we better move this out of Visio
    bool CheckDistancesOnly(IHideableObject attentive)
    {
        bool foundSomething = false;
        foreach (var otherObjId in attentive.ObjectsISee)
        {
            bool becameInvisible = false;
            var otherObject = _tinyWizPlayerManager.AllObjects[otherObjId];
            var dist = Vector3.Distance(otherObject.transform.position, attentive.transform.position);
            if (dist > attentive.normalVisionRange)
            {
                becameInvisible = true;
            }
            else if (dist > attentive.restrictedVisionRange)
            {
                becameInvisible = !otherObject.IsInZone(attentive.GetZone());
            }

            if (becameInvisible)
            {
                attentive.ObjectBecameInvisible(otherObjId);
                foundSomething = true;
            }
        }

        var last = attentive.objectsIUsedToSee.LastOrDefault()._objectsISee;

        foreach (var historical in last)
        {
            var obj = _tinyWizPlayerManager.AllObjects[historical];

            if (obj.IsVisible())
            {
                continue;
            }
            bool becameVisible = false;
            var dist = Vector3.Distance(obj.transform.position, attentive.transform.position);
            if (dist < attentive.normalVisionRange)
            {
                becameVisible = true;
            }
            else if (dist < attentive.restrictedVisionRange)
            {
                becameVisible = !obj.IsInZone(attentive.GetZone());
            }

            if (becameVisible)
            {
                attentive.ObjectBecameVisible(historical);
                foundSomething = true;
            }
        }

        return foundSomething;
    }

    private void SetInitialZone(IHideableObject hideable)
    {
        int zoneToSet;
        VizZone vizZone;

        FindMostLikelyContainingZone(hideable, _layerToSearch, ZONE_SEARCH_RADIUS, out zoneToSet, out vizZone);
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

        for (int i = 0; i < ZoneList.Count; i++)
        {
            var zone = ZoneList[i].zone;
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

    VizZone GetZone(int zoneId)
    {
        foreach (var zone in ZoneList)
        {
            if (zone.zone.ZoneId == zoneId)
                return zone.zone;
        }
        return null;
    }
    List<VizZone> GetZones(int zoneId)
    {
        List<VizZone> returnList = new();
        foreach (var zone in ZoneList)
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
