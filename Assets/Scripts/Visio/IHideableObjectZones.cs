using System.Collections.Generic;
using UnityEngine;

public abstract partial class IHideableObject: MonoBehaviour
{
    int _zoneIAmIn;
    List<int> _zonesISee = new List<int>();

    public List<int> ZonesISee => _zonesISee;

    public void SetZone(int zone) { _zoneIAmIn = zone; AddVisibleZone(_zoneIAmIn); }
    public int GetZone() { return _zoneIAmIn; }

    public void AddVisibleZone(int zone) { if(!_zonesISee.Contains(zone)) _zonesISee.Add(zone); }
    public void AddVisibleZones(int zoneId, int [] zones) 
    {
        if (!_zonesISee.Contains(zoneId))
        {
            _zonesISee.Add(zoneId);
        }
        foreach (var zone in zones)
        {
            if (!_zonesISee.Contains(zoneId))
                _zonesISee.Add(zone);
        }

    }
    public void ClearAllVisibleZones() { _zonesISee.Clear(); }

    public bool IsInZone(int zone) { if(_zoneIAmIn == zone) return true;  return false; }
    public bool IsInZones(List<int> testZones)
    {
        return testZones.Contains(_zoneIAmIn);
    }
    public List<int> GetZonesISee() { return _zonesISee; }
    public bool AreZonesTheSame(List<int> testZones) 
    {
        if (!testZones.Contains(_zoneIAmIn)) 
            return false;
        return true;
    }
}