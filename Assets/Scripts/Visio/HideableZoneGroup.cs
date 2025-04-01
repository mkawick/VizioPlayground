
using UnityEngine;
using System.Collections.Generic;

public class HideableZoneGroup
{
    public int ZoneId { get; set; }
    public List<VizZone> ContainingZones = new();
    protected List<Renderer> _meshRenderers;

    public void Init()
	{
        if(_meshRenderers == null)
            _meshRenderers = new List<Renderer>();

        foreach (var vizZone in ContainingZones)
        {
            var renderers = vizZone.optionalRoot.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                _meshRenderers.Add(renderer);
            }
        }
    }
    public void AddZone(VizZone zone)
    {
        if (zone == null || ContainingZones.Contains(zone))// || zone.ZoneId != ZoneId)
            return;

        ContainingZones.Add(zone);
    }

    public void ZoneShow()
    {
        if (ContainingZones.Count == 0)
            return;
        foreach(var mesh in _meshRenderers)
        {
            mesh.enabled = true;
        }
    }
    public void ZoneHide()
    {
        if (ContainingZones.Count == 0)
            return;
        foreach (var mesh in _meshRenderers)
        {
            mesh.enabled = false;
        }
    }

}
