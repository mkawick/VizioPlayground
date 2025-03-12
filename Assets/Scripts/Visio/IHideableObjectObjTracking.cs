using Sirenix.OdinInspector.Editor.Validation;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEditor.VersionControl;
using UnityEngine;

public abstract partial class IHideableObject: MonoBehaviour
{
    protected HashSet<int> _objectsISee = new HashSet<int>();
    internal HashSet<int> ObjectsISee => _objectsISee;

    public struct HistoricalListOfObjectsISaw
    {
        public HashSet<int> _objectsISee;
        public float _timestamp;
    }

    // TODO should be private
    public Queue<HistoricalListOfObjectsISaw> objectsIUsedToSee = new Queue<HistoricalListOfObjectsISaw>();

    public void SaveAllVisibleObjects(float timestamp)
    {
        CacheAllVisibleObjects(timestamp, _objectsISee);
    }
    public void CacheAllVisibleObjects(float timestamp, HashSet<int> objectsISee)
    {
        if (objectsISee.Count != 0)
        {
            objectsIUsedToSee.Enqueue(new HistoricalListOfObjectsISaw { _objectsISee = objectsISee, _timestamp = timestamp });
        }
    }

    HashSet<int> CollectAllObjectsThatISee()
    {
        HashSet<int> objectsISee = new HashSet<int>();
        if (Observant)
        {
            foreach (var zoneId in _zonesISee)
            {
                _tinyWizHideableManager.GetAllPlayersInRoom(zoneId, objectsISee);
            }
            _tinyWizHideableManager.GetAllPlayersInRoom(_zoneIAmIn, objectsISee);
            // no need to keep yourself in the list
            objectsISee.Remove(HideableId);
        }
        return objectsISee;
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
        HashSet<int> objIdsThatIKnew = CollectAllObjectsThatISee();

        // set the current ones
        _zonesISee = visibleZones;
        _zoneIAmIn = newZoneId;
        HashSet<int> setOfNewObjIds = CollectAllObjectsThatISee();

        if (Observant)
        {
            // reduce the set of old ones
            List<int> itemsToRemove = new List<int>();
            foreach (var obj in objIdsThatIKnew)
            {
                if (setOfNewObjIds.Contains(obj))
                {
                    itemsToRemove.Add(obj);
                }
            }
            foreach (var item in itemsToRemove)
            {
                objIdsThatIKnew.Remove(item);
            }

            // save the old ones
            CacheAllVisibleObjects(Time.frameCount, objIdsThatIKnew);

            _objectsISee = setOfNewObjIds;
            // walk the list of new ones to let them know that they can see me
            // make sure that you are visible to them
            //_tinyWizHideableManager.InformObjectThatIAmVisible(HideableId, setOfNewObjIds);
        }
    }

    public virtual void ClearHistory(int timeStamp)
    {
        // this is a queue and is ordered by time.. fall out early 
        while (objectsIUsedToSee.Count > 0)
        {
            if (objectsIUsedToSee.Peek()._timestamp < timeStamp)
            {
                var history = objectsIUsedToSee.Dequeue();
                foreach(var objId in history._objectsISee)
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
        if(validate)
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
}