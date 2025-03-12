using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using Sirenix.OdinInspector;
using static UnityEditor.FilePathAttribute;
using Unity.VisualScripting;

public class KeyDoesNotExistException : SystemException
{
    public KeyDoesNotExistException() { }
    public KeyDoesNotExistException(string message) { }
    public KeyDoesNotExistException(string message, Exception inner) { }
    protected KeyDoesNotExistException(SerializationInfo info, StreamingContext context) { }
}

public class TinyWizHideableManager : MonoBehaviour
{
    Dictionary<int, IHideableObject> allHidableObjects = new Dictionary<int, IHideableObject>();
    List<IHideableObject> newlySpawnedObjects = new List<IHideableObject>();

    bool hasFinishedInit = false;
    internal int incrementingHidableId = 1;

    void Start()
    {
        hasFinishedInit = true;
    }

    public Dictionary<int, IHideableObject> AllObjects => allHidableObjects;
    public Dictionary<int, IHideableObject> AttentiveObjects => allHidableObjects.Where(kv => kv.Value.Observant == true).ToDictionary(kv => kv.Key, kv => kv.Value);
    public List<IHideableObject> NewlySpawnedObjects => newlySpawnedObjects;

    public bool HasFinishedInit => hasFinishedInit;

    internal IHideableObject GetObjectById(int id)
    {
        if (allHidableObjects.ContainsKey(id))
        {
            return allHidableObjects[id];
        }
        return null;
    }
    public void Register(IHideableObject obj)
    {
        if(obj.HideableId == 0)
        {
            obj.Hide();
            obj.HideableId = incrementingHidableId++;
            allHidableObjects.Add(obj.HideableId, obj);
            newlySpawnedObjects.Add(obj);
        }
    }
    public void Remove(IHideableObject obj)
    {
        if (obj.HideableId != 0)
        {
            if (allHidableObjects.ContainsKey(obj.HideableId))
            {
                allHidableObjects.Remove(obj.HideableId);
            }
            if (newlySpawnedObjects.Contains(obj))
                newlySpawnedObjects.Remove(obj);
        }
    }

    int spawnedIndex = 100;
    List<IHideableObject> spawnedHistory;

    [Button]
    void SpawnObj()
    {
        if (spawnedHistory == null)
            spawnedHistory = new List<IHideableObject>();
        var choice = UnityEngine.Random.Range(0, AllObjects.Count);
        var choosenItem = allHidableObjects.Skip(choice).First();

        Vector3 pos = choosenItem.Value.transform.position;
        Quaternion rot = choosenItem.Value.transform.rotation;
        Vector2 randomXY = UnityEngine.Random.insideUnitCircle * 3;
        pos += new Vector3(randomXY.x, 0, randomXY.y);
        var r = UnityEngine.Random.rotation;

        var obj = GameObject.Instantiate(choosenItem.Value, pos, rot * r);
        obj.name = $"spawn {choosenItem.Value.name}-{spawnedIndex++}";
        obj.Show();
        obj.transform.parent = this.transform;// heirarchy setup

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
        spawnedHistory.RemoveAt(0);
    }

    internal void GetAllPlayersInRoom(int zoneId,
        HashSet<int> objectsInZone)
    {
        foreach(var obj in allHidableObjects)
        {
            if(obj.Value.GetZone() == zoneId)
            {
                objectsInZone.Add(obj.Key);
            }
        }
    }

    internal void InformObjectThatIAmVisible(int hidableId, HashSet<int> audience)
    {
        if (allHidableObjects.ContainsKey(hidableId) == false)
            throw new KeyDoesNotExistException();

        var hider = allHidableObjects[hidableId];
        
        foreach(var objId in audience)
        {
            if (allHidableObjects.ContainsKey(objId) == false)
                continue;
            if (allHidableObjects[objId].Observant)
            {
                allHidableObjects[objId].ObjectBecameVisible(hidableId);
            }
            // send notification
        }
    }

    internal void ShowLocalObjects(HashSet<int> objectsToShow)
    {
        foreach(var objId in objectsToShow)
        {
            if (allHidableObjects.ContainsKey(objId) == false)
                continue;
            allHidableObjects[objId].Show();
        }
    }
}
