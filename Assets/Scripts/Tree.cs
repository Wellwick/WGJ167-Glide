using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Mesh mesh;
    private TerrainGeneration tg;

    [SerializeField]
    private Material wood, leaves;

    [SerializeField]
    private float height;

    [SerializeField, Range(0,1)]
    private float trunkThickness;

    [SerializeField, Range(1, 5)]
    private float rootLength;


    private bool requestCalculate;

    private void PrepareMesh() {
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = wood;

        meshFilter = gameObject.AddComponent<MeshFilter>();
    }

    private void CalculateMesh() {
        // We will convert this into an array at the end!
        List<Vector3> vertices = new List<Vector3>();
        int index = 0;
        List<int> triangles = new List<int>();
        Random.InitState(1);
        int roots = Random.Range(3, 10);
        // Divide the 360 degrees into the amount of roots, then randomly select a root end point in that range.
        for (int i = 0; i < roots; i++, index += 4) {
            float angle = Random.Range(360f / roots*i, 360f / roots*i+1);
            Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
            vertices.Add(direction.normalized * rootLength);
            vertices.Add(direction.normalized * trunkThickness + Vector3.up*(0.2f*rootLength));
            direction = Quaternion.AngleAxis(angle + (5*rootLength), Vector3.up) * Vector3.forward;
            vertices.Add(direction.normalized * trunkThickness);
            direction = Quaternion.AngleAxis(angle - (5*rootLength), Vector3.up) * Vector3.forward;
            vertices.Add(direction.normalized * trunkThickness);
            triangles.Add(index);
            triangles.Add(index+2);
            triangles.Add(index+1);

            triangles.Add(index +0);
            triangles.Add(index + 1);
            triangles.Add(index + 3);

            // Mesh to the next root!
            triangles.Add(index + 2);
            triangles.Add((index + 3 + 4) % (roots * 4));
            triangles.Add(index + 1);

            triangles.Add(index + 1);
            triangles.Add((index + 3 + 4) % (roots * 4));
            triangles.Add((index + 1 + 4) % (roots * 4));
        }

        Vector3[] tempArray = vertices.ToArray();
        for (int i = 0; i < roots; i++, index += 1) {
            vertices.Add(tempArray[i * 4 + 1] + Vector3.up * (height - (0.2f * rootLength)));

            triangles.Add(index);
            triangles.Add(i * 4 + 1);
            int nextTop = ((index + 1 - roots * 4) % roots) + (roots * 4);
            triangles.Add(nextTop);

            triangles.Add(i * 4 + 1);
            triangles.Add(((i+1) * 4 + 1) % (roots*4));
            triangles.Add(nextTop);
        }

        // Do something similar with branches, just higher up and varying intervals
        int branch = Random.Range(4, 8);


        mesh = new Mesh();
        mesh.vertices = vertices.ToArray();

        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        Vector2[] uv = new Vector2[mesh.vertices.Length];
        for (int i = 0; i < uv.Length; i++) {
            // Take the angle around the y axis as x value, and y as y value
            // TODO
            uv[i] = new Vector2();
        }
        mesh.uv = uv;

        meshFilter.mesh = mesh;
    }

    // Start is called before the first frame update
    void Start()
    {
        requestCalculate = false;
        PrepareMesh();
        CalculateMesh();
    }

    private void OnValidate() {
        requestCalculate = true;
    }

    // Update is called once per frame
    void Update() {
        if (requestCalculate) {
            CalculateMesh();
            requestCalculate = false;
        }
    }
}
