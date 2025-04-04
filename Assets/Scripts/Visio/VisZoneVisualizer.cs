using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class VisZoneVisualizer
{
    public int zoneId;
    [ShowInInspector] public VizZoneTag zoneTag => zone?.type??VizZoneTag.Outside;
    [ShowInInspector] public string definitionKey => zone?.definitionKey??string.Empty;
    VizZone _zone;

    [ShowInInspector] public VizZone zone
    {
        get
        {
            if (_zone == null)
            {
                _zone = GameObject.FindObjectsOfType<VizZone>().FirstOrDefault(x => x.ZoneId == zoneId);
            }

            return _zone;
        }
    }

    public VisZoneVisualizer(int inZoneId)
    {
        zoneId = inZoneId;
    }
}
