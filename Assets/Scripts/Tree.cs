using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Mesh mesh;

    private GameObject leavesObject;
    private MeshFilter leafMeshFilter;

    private TerrainGeneration tg;

    [SerializeField]
    private Material wood, leaves;

    [SerializeField]
    private float height;

    [SerializeField, Range(0,1)]
    private float trunkThickness;

    [SerializeField, Range(1, 5)]
    private float rootLength;

    [SerializeField, Range(3, 8)]
    private int roots;


    private bool requestCalculate;

    public void PrepareMesh(float height, float trunkThickness, int roots, float rootLength, TerrainGeneration tg) {
        this.height = height;
        this.trunkThickness = trunkThickness;
        this.roots = roots;
        this.rootLength = rootLength;
        this.tg = tg;
        wood = tg.treeWoodMaterial;
        leaves = tg.treeLeafMaterial;
        
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = wood;

        meshFilter = gameObject.AddComponent<MeshFilter>();
    }

    public void CalculateMesh() {
        // We will convert this into an array at the end!
        List<Vector3> vertices = new List<Vector3>();
        int index = 0;
        List<int> triangles = new List<int>();
        // Divide the 360 degrees into the amount of roots, then randomly select a root end point in that range.
        for (int i = 0; i < roots; i++, index += 4) {
            float angle = Random.Range((360f / roots)*i, (360f / roots)*(i+1));
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

    public void PrepareLeafMesh() {
        leavesObject = Instantiate(new GameObject("Leaves"), transform.position + Vector3.up * height, Quaternion.identity, transform);
        leavesObject.AddComponent<MeshRenderer>().sharedMaterial = leaves;

        leafMeshFilter = leavesObject.AddComponent<MeshFilter>();
    }

    public void CalculateLeafMesh() {
        Vector3[] vertices = new Vector3[18];
        vertices[0] = new Vector3(Random.Range(-0.1f, 0.1f)*height, Random.Range(-height*0.45f, -height*0.3f), Random.Range(-0.1f, 0.1f) * height);
        float width = height * Random.Range(0.4f, 0.8f);
        for (int i = 0; i < 4; i++) {
            // Four layers, building out then back in
            float yPos = ((i - 1.5f) * height * 0.15f) + (Random.Range(-0.02f, 0.02f)*height);
            float localWidth = width;
            if (i % 3 == 0) {
                localWidth *= 0.5f;
            }
            for (int j = 0; j < 4; j++) {
                float angle = (90f * j) + Random.Range(15f, 75f);
                Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
                vertices[(i * 4) + j + 1] = direction * localWidth + Vector3.up * yPos;
            }
        }


        vertices[17] = new Vector3(Random.Range(-0.1f, 0.1f) * height, Random.Range(height*0.3f, height*0.45f), Random.Range(-0.1f, 0.1f) * height);

        // Add a yOffset to all of them just a bit
        float yOffset = Random.Range(-0.5f, 0.5f);
        for (int i=0; i < vertices.Length; i++) {
            vertices[i] += Vector3.up * yOffset;
        }

        int[] triangles = new int[32*3];

        triangles = new int[32*3] {
            0, 2, 1,
            0, 3, 2,
            0, 4, 3,
            0, 1, 4,
            1, 2, 6,
            2, 3, 7,
            3, 4, 8,
            4, 1, 5,
            4, 5, 8,
            1, 6, 5,
            2, 7, 6,
            3, 8, 7,
            5, 6, 9,
            6, 7, 10,
            7, 8, 11,
            8, 5, 12,
            5, 9, 12,
            6, 10, 9,
            7, 11, 10,
            8, 12, 11,
            13, 9, 14,
            14, 10, 15,
            15, 11, 16,
            16, 12, 13,
            13, 12, 9,
            14, 9, 10,
            15, 10, 11,
            16, 11, 12,
            17, 13, 14,
            17, 14, 15,
            17, 15, 16,
            17, 16, 13
        };

        Vector2[] uv = new Vector2[vertices.Length];
        uv[0] = Vector2.zero;
        uv[1] = new Vector2(0f, 0.5f);
        uv[2] = new Vector2(0.5f, 0.5f);
        uv[3] = new Vector2(1f, 0.5f);
        uv[4] = new Vector2(0.5f, 0.5f);
        for (int i=5; i < uv.Length-1; i++) {
            uv[i] = new Vector2(i%2/2, (int)i/8);
        }
        uv[17] = Vector2.one;

        Mesh leafMesh = new Mesh();
        leafMesh.vertices = vertices;
        leafMesh.triangles = triangles;
        leafMesh.RecalculateNormals();
        leafMesh.uv = uv;


        leafMeshFilter.mesh = leafMesh;
    }

    // Start is called before the first frame update
    void Start()
    {
        requestCalculate = false;
    }

    private void OnValidate() {
        requestCalculate = true;
    }

    // Update is called once per frame
    void Update() {
        if (requestCalculate) {
            CalculateMesh();
            CalculateLeafMesh();
            leavesObject.transform.localPosition = Vector3.up * height;
            requestCalculate = false;
        }
    }
}
