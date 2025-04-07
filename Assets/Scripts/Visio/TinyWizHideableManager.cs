using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using Sirenix.OdinInspector;

public class KeyDoesNotExistException : SystemException
{
    public KeyDoesNotExistException() { }
    public KeyDoesNotExistException(string message) { }
    public KeyDoesNotExistException(string message, Exception inner) { }
    protected KeyDoesNotExistException(SerializationInfo info, StreamingContext context) { }
}

public class TinyWizHideableManager : MonoBehaviour
{
    public Dictionary<int, IHideableObject> AllObjects => _allHidableObjects;
    public Dictionary<int, IHideableObject> AttentiveObjects => _allHidableObjects.Where(kv => kv.Value.Observant == true).ToDictionary(kv => kv.Key, kv => kv.Value);
    public Dictionary<int, IHideableObject> MoveableObjects => _allHidableObjects.Where(kv => kv.Value.Moveable == true).ToDictionary(kv => kv.Key, kv => kv.Value);
    public List<IHideableObject> NewlySpawnedObjects => _newlySpawnedObjects;

    internal int incrementingHidableId = 1;

    //todo: create a dictionary/list for the "observant"/ attentive objects
    List<IHideableObject> spawnedHistory;
    Dictionary<int, IHideableObject> _allHidableObjects = new Dictionary<int, IHideableObject>();
    List<IHideableObject> _newlySpawnedObjects = new List<IHideableObject>();
    
    int spawnedIndex = 100;
    bool hasFinishedInit = false;
    public bool HasFinishedInit => hasFinishedInit;
    [ShowInInspector, FoldoutGroup("Debug")] List<IHideableObject> DebugAttentiveObjects => AttentiveObjects.Values.ToList();

    void Start()
    {
        hasFinishedInit = true;
    }


    internal IHideableObject GetObjectById(int id)
    {
        if (_allHidableObjects.ContainsKey(id))
        {
            return _allHidableObjects[id];
        }
        return null;
    }
    public void Register(IHideableObject obj)
    {
        if(obj.HideableId == 0)
        {
            obj.Hide();
            obj.HideableId = incrementingHidableId++;
            _allHidableObjects.Add(obj.HideableId, obj);
            _newlySpawnedObjects.Add(obj);
        }
    }
    public void Remove(IHideableObject obj)
    {
        if (obj.HideableId != 0)
        {
            if (_allHidableObjects.ContainsKey(obj.HideableId))
            {
                _allHidableObjects.Remove(obj.HideableId);
            }
            if (_newlySpawnedObjects.Contains(obj))
                _newlySpawnedObjects.Remove(obj);
        }
    }


    [Button]
    void SpawnObj()
    {
        if (spawnedHistory == null)
            spawnedHistory = new List<IHideableObject>();
        var choice = UnityEngine.Random.Range(0, AllObjects.Count);
        var choosenItem = _allHidableObjects.Skip(choice).First();

        Vector3 pos = choosenItem.Value.transform.position;
        Quaternion rot = choosenItem.Value.transform.rotation;
        Vector2 randomXY = UnityEngine.Random.insideUnitCircle * 3;
        pos += new Vector3(randomXY.x, 0, randomXY.y);
        var r = UnityEngine.Random.rotation;

        var obj = GameObject.Instantiate(choosenItem.Value, pos, rot * r);
        obj.name = $"spawn {choosenItem.Value.name}-{spawnedIndex++}";
        obj.Show();
        obj.transform.parent = this.transform;// hierarchy setup

        spawnedHistory.Add(obj);
    }

    [Button]
    void DeleteSpawnedObject()
    {
        if(spawnedHistory == null)
        {
            Debug.LogError("nothing spawned");
            return;
        }
        if(spawnedHistory.Count < 1)
        {
            Debug.LogError("nothing to despawn");
            return;
        }
        var obj = spawnedHistory[0];
        DestroyImmediate(obj.gameObject);
        spawnedHistory.RemoveAt(0);
    }

    internal void GetAllPlayersInZone(int zoneId,
        HashSet<int> objectsInZone)
    {
        foreach(var obj in _allHidableObjects)
        {
            if(obj.Value.GetZone() == zoneId)
            {
                objectsInZone.Add(obj.Key);
            }
        }
    }

    internal void GetAllObjectsInZones(List<int> zoneIds,
        HashSet<int> objectsInZone)
    {
        foreach (var obj in _allHidableObjects)
        {
            if (zoneIds.Contains(obj.Value.GetZone()))
            {
                objectsInZone.Add(obj.Key);
            }
        }
    }

    internal void InformObjectThatIAmVisible(int hidableId, HashSet<int> audience)
    {
        if (_allHidableObjects.ContainsKey(hidableId) == false)
            throw new KeyDoesNotExistException();

        var hider = _allHidableObjects[hidableId];
        
        foreach(var objId in audience)
        {
            if (_allHidableObjects.ContainsKey(objId) == false)
                continue;
            if (_allHidableObjects[objId].Observant)
            {
                _allHidableObjects[objId].ObjectBecameVisible(hidableId);
            }
            // send notification
        }
    }

    internal void ShowLocalObjects(HashSet<int> objectsToShow)
    {
        foreach(var objId in objectsToShow)
        {
            if (_allHidableObjects.ContainsKey(objId) == false)
                continue;
            _allHidableObjects[objId].Show();
        }
    }

    public bool ShouldShowLocallyUsingGameplayRules(IHideableObject localHideable, IHideableObject other)
    {
        if (other.HasInvisibilityEffectActive())
        {
            return false;
        }
        if (other.IgnoreDistance)
        {
            return true;
        }
        var dist = Vector3.Distance(other.Position, localHideable.Position);
        if (dist > localHideable.normalVisionRange)
        {
            return false;
        }

        if (dist > localHideable.restrictedVisionRange)
        {
            return !other.IsInZone(localHideable.GetZone());
        }
        return true;
    }
}
