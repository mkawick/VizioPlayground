using System.Text.RegularExpressions;
using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

public class VizZone : MonoBehaviour
{
    public TextMeshProUGUI text;
    public int ZoneId { get { if (_OverrideZoneId != 0) return _OverrideZoneId; return int.Parse(text.text); } }
    public int _OverrideZoneId;
    public Material highlightMaterial;
    public int[] _ListOfVisibleZones;
    internal List<int> externalZonesThatSeeMe;
    bool showSelection;

    Material normalMaterial;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string num = Regex.Match(name, @"\d+").Value;
        text.text = num.TrimStart(new Char[] { '0' });
        normalMaterial = this.GetComponent<MeshRenderer>().material;
        externalZonesThatSeeMe = new List<int>();
        showSelection = false;
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
        if (_OverrideZoneId < 0)
        {
            Debug.Log("VizZone: negative values not allowed");
            _OverrideZoneId = Mathf.Clamp(_OverrideZoneId, 0, int.MaxValue); // or int.MaxValue, if you need to use an int but can't use uint.
        }
    }
}
