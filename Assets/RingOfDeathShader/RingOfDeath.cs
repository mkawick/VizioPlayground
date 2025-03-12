using TMPro;
using UnityEngine;

public class RingOfDeath : MonoBehaviour
{
    [SerializeField] float ringStartSize;
    [SerializeField] float ringEndSize;

    [SerializeField, Range(0.01f, 3)] float sizeChange;
    //  Mesh mesh;
    Renderer meshRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // mesh = GetComponent<Mesh>();
        meshRenderer = GetComponent<Renderer>();

        var b = meshRenderer.bounds;
        var extents = b.extents;
        extents.x = ringStartSize;
        meshRenderer.bounds.SetMinMax(-extents, extents);
        // renderer.ResetBounds();

        Debug.Log("start: " + meshRenderer.bounds);
    }

    // Update is called once per frame
    void Update()
    {
        var b = meshRenderer.bounds;
        if (b.extents.x > 4) //(transform.localScale.x > 16)
        {
            Vector3 scaleChange = new Vector3(-sizeChange * Time.deltaTime, 0f, -sizeChange * Time.deltaTime);
            transform.localScale += scaleChange;
        }
        Debug.Log(b.extents);
        return;

        b = meshRenderer.bounds;
        var extents = b.extents;
        Debug.Log(extents);
        extents.x -= 0.012f;
        extents.z -= 0.012f;

        if (extents.x < ringEndSize)
            extents.x = ringEndSize;
        if (extents.z < ringEndSize)
            extents.z = ringEndSize;
        extents *= 2;
        meshRenderer.bounds = new Bounds(Vector3.zero, extents);
        
    }
}
