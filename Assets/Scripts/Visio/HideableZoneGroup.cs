
using UnityEngine;
using System.Collections.Generic;

public class HideableZoneGroup
{
    public int ZoneId { get; set; }
    public List<VizZone> ContainingZones = new();
    protected List<Renderer> _meshRenderers;
    protected List<Renderer> _oppositeDayMeshRenderers;

    public void Init()
    {
        if (_meshRenderers == null)
            _meshRenderers = new List<Renderer>();
        if (_oppositeDayMeshRenderers == null)
            _oppositeDayMeshRenderers = new List<Renderer>();

        foreach (var vizZone in ContainingZones)
        {
            var renderers = vizZone.optionalRoot.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                _meshRenderers.Add(renderer);
            }
        }

        foreach (var vizZone in ContainingZones)
        {
            if (vizZone.optionalAppearingObjects != null)
            {
                var renderers = vizZone.optionalAppearingObjects.GetComponentsInChildren<Renderer>(true);
                foreach (var renderer in renderers)
                {
                    _oppositeDayMeshRenderers.Add(renderer);
                }
            }
        }
        // Default is off
        EnableMeshes(false, _oppositeDayMeshRenderers);
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

        EnableMeshes(true, _meshRenderers);
        EnableMeshes(false, _oppositeDayMeshRenderers);
    }
    public void ZoneHide()
    {
        if (ContainingZones.Count == 0)
            return;

        EnableMeshes(false, _meshRenderers);
        EnableMeshes(true, _oppositeDayMeshRenderers);
    }

    void EnableMeshes(bool enable, List<Renderer> meshRenderers)
    {
        foreach (var mesh in meshRenderers)
        {
            mesh.enabled = enable;
        }
    }
}
