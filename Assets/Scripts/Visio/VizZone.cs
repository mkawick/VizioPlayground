using System.Text.RegularExpressions;
using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using UnityEditor;

public enum VizZoneTag
{
    None = 0,
    Outside,
    InsideBuilding,
    Bushes,
    Roof,
    Doorway
}

public class VizZone : MonoBehaviour
{
    public List<int> visibleZonesICanSee;
    public VisibilityZoneDefinition definition = VisibilityZoneDefinition.DefaultDefinition;
    public string definitionKey => _definitionKey;

    internal List<int> externalZonesThatSeeMe;
    public int ZoneId;

    [SerializeField] string _definitionKey;
    [SerializeField] VizZoneTag _zoneType = VizZoneTag.InsideBuilding;
    public VizZoneTag type => _zoneType;
    bool showSelection;

    [SerializeField] public Transform root;

    public int[] hiddenZones;

    Material normalMaterial;
    public Material highlightMaterial;
    public TextMeshProUGUI text;

    public HideableZoneGroup ZoneHider { get; set; }

    void Start()
    {
        int value = int.Parse(text.text);
        if (value != 0 && ZoneId == 0)
            ZoneId = value;

        string num = Regex.Match(name, @"\d+").Value;
        text.text = num.TrimStart(new Char[] { '0' });
        normalMaterial = this.GetComponent<MeshRenderer>().material;

        externalZonesThatSeeMe = new List<int>();
        if (!visibleZonesICanSee.Contains(ZoneId))
        {
            visibleZonesICanSee.Add(ZoneId);
        }
        showSelection = false;

        if (root == null)
            root = this.transform;
    }

    private Color GetGizmoColor(VizZoneTag vizZoneTag)
    {
        float alpha = 0.5f;
        switch (vizZoneTag)
        {
            case VizZoneTag.Bushes:
                return new Color(0f, 1f, 0f, alpha);
            case VizZoneTag.Doorway:
                return new Color(0f, 0f, 1f, alpha);
            case VizZoneTag.InsideBuilding:
                return new Color(1f, 1f, 1f, alpha);
            case VizZoneTag.Roof:
                return new Color(1f, 0f, 0f, alpha);
            default:
                break;
        }
        return Color.white;
    }

    void OnDrawGizmos()
    {
        if (showSelection)
        {
            Color oldColor = Gizmos.color;

            var one = new Vector3(1, 1, 1);
            Gizmos.color = new Color(0.98f, 0.98f, 0.07f, 0.2f);
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, one);
            Gizmos.DrawCube(Vector3.zero, transform.localScale);
            //Gizmos.DrawWireSphere(Vector3.zero, 2);

            Gizmos.color = oldColor;
        }
    }
    private BoxCollider boxCollider = null;
    void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        if (Selection.count > 10)
        {
            return;
        }
#endif
        if (!boxCollider)
        {
            boxCollider = GetComponent<BoxCollider>();
            if (!boxCollider)
            {
                return;
            }
        }

        Color oldColor = Gizmos.color;

        Gizmos.color = GetGizmoColor(_zoneType);
        Vector3 centerOffset = transform.TransformPoint(boxCollider.center);
        Vector3 v1 = transform.lossyScale;
        Vector3 v2 = boxCollider.size;
        Gizmos.matrix = Matrix4x4.TRS(centerOffset, transform.rotation, new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z));
        Gizmos.DrawCube(Vector3.zero, Vector3.one);

        Gizmos.color = oldColor;

        if (visibleZonesICanSee.Count == 0)
        {
            return;
        }

        Gizmos.matrix = Matrix4x4.identity;
        var vizZones = FindObjectsByType<VizZone>(FindObjectsSortMode.None);
        foreach (var visibleZoneId in visibleZonesICanSee)
        {
            foreach (var vizZone in vizZones)
            {
                if (vizZone.ZoneId == visibleZoneId && vizZone.ZoneId != ZoneId)
                {
                    Gizmos.color = GetGizmoColor(vizZone.type);
                    Gizmos.DrawLine(transform.position, vizZone.transform.position);
                    Gizmos.DrawSphere(vizZone.transform.position, 1.5f);
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(vizZone.transform.position, 1.5f);
                }
            }
        }
        Gizmos.color = oldColor;
    }

    public void ShowSelected(bool show)
    {
        showSelection = show;
          if (show)
          {
              //this.GetComponent<MeshRenderer>().material = highlightMaterial;
          }
          else
          {
              //this.GetComponent<MeshRenderer>().material = normalMaterial;
          }
    }
    void OnValidate()
    {
        if (ZoneId < 0)
        {
            UnityEngine.Debug.Log("VizZone: negative values not allowed");
            ZoneId = Mathf.Clamp(ZoneId, 0, int.MaxValue); // or int.MaxValue, if you need to use an int but can't use uint.
        }
    }

  /*  [Conditional("UNITY_EDITOR")]
    public void SetupZoneEditorTime(VizZoneGroupHelper helper)
    {
        ZoneId = helper.desiredZoneId;
        _definitionKey = helper._visibilitySettingsKey;
        _zoneType = helper.zoneType;
    }*/
}
