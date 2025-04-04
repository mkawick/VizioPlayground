using System.Collections.Generic;
using UnityEngine;

public abstract partial class IHideableObject: MonoBehaviour
{
    public struct HistoricalListOfObjectsISaw
    {

        public HashSet<int> _objectsISee;
        public float _timestamp;

    }

    // TODO should be private
    public Queue<HistoricalListOfObjectsISaw> objectsIUsedToSee = new Queue<HistoricalListOfObjectsISaw>();
    
    protected HashSet<int> _objectsISee = new HashSet<int>();

    internal HashSet<int> ObjectsISee => _objectsISee;


    public void CacheAllVisibleObjects(float timestamp, HashSet<int> objectsISee)
    {
        if (objectsISee.Count != 0)
        {
            objectsIUsedToSee.Enqueue(new HistoricalListOfObjectsISaw { _objectsISee = objectsISee, _timestamp = timestamp });
        }
    }

    public void InitializeObjectsThatISee()
    {
        if (Observant)
        {
            _objectsISee = CollectAllObjectsThatISee();
        }
    }

    public void MoveZones(int newZoneId, List<int> visibleZones)
    {
        List<int> OldZones = _zonesISee;
        int myOldZone = _zoneIAmIn;
        HashSet<int> prevZoneObjs = CollectAllObjectsThatISee();

        // set the current ones
        _zonesISee = visibleZones;
        _zoneIAmIn = newZoneId;
        HashSet<int> newlySeenObjs = CollectAllObjectsThatISee();

        if (Observant)
        {
            // reduce the set of old ones
            RemoveCurrentlyVisibleItemsFromHistory(newlySeenObjs, prevZoneObjs);

            // save the old ones
            CacheAllVisibleObjects(Time.frameCount, prevZoneObjs);

            _objectsISee = newlySeenObjs;
        }
    }

    public virtual void ClearHistory(int timeStamp)
    {
        // this is a queue and is ordered by time.. fall out early 
        while (objectsIUsedToSee.Count > 0)
        {
            if (objectsIUsedToSee.Peek()._timestamp < timeStamp)
            {
                HashSet<int> currentlySeenObjs = CollectAllObjectsThatISee();
                var history = objectsIUsedToSee.Dequeue();
                RemoveCurrentlyVisibleItemsFromHistory(currentlySeenObjs, history._objectsISee);

                foreach (var objId in history._objectsISee)
                {
                    _objectsISee.Remove(objId);
                }
            }
            else
            {
                break;
            }
        }
    }

    protected void AddHidableToHistory(int objId, bool validate)
    {
        if (objId == HideableId)// don't save self
        { 
            return; 
        }

        if (validate)
        {
            if (_objectsISee.Contains(objId) == false)
                return;
        }
        HashSet<int> objIdsThatIKnew = new HashSet<int> { objId };
        CacheAllVisibleObjects(Time.frameCount, objIdsThatIKnew);
    }

    public virtual void ObjectBecameVisible(int objId)
    {
        if (Observant)
        {
            if (objId == HideableId)
            {
                return; // don't add self
            }
            _objectsISee.Add(objId);
        }
    }

    public virtual void ObjectBecameInvisible(int objId)
    {
        if (Observant)
        {
            AddHidableToHistory(objId, true);
        }
    }

    public void ShowAllVisibleItems()
    {
        _tinyWizHideableManager.ShowLocalObjects(_objectsISee);
    }

    HashSet<int> CollectAllObjectsThatISee()
    {
        HashSet<int> objectsISee = new HashSet<int>();
        if (Observant)
        {
            foreach (var zoneId in _zonesISee)
            {
                _tinyWizHideableManager.GetAllPlayersInZone(zoneId, objectsISee);
            }

            _tinyWizHideableManager.GetAllPlayersInZone(_zoneIAmIn, objectsISee);

            // no need to keep yourself in the list
            objectsISee.Remove(HideableId);
        }
        return objectsISee;
    }

    void RemoveCurrentlyVisibleItemsFromHistory(HashSet<int> currentlySeenObjs, HashSet<int> history)
    {
        List<int> objectsIStillSee = new List<int>();
        foreach (var prevZoneObj in history)
        {
            if (currentlySeenObjs.Contains(prevZoneObj))
            {
                objectsIStillSee.Add(prevZoneObj);
            }
        }
        foreach (var item in objectsIStillSee)
        {
            history.Remove(item);
        }
    }

}