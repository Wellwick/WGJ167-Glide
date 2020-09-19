using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour
{
    [SerializeField]
    private Vector2 size = new Vector2(100f,100f);

    [SerializeField, Range(0.001f, 2f)]
    private float resolution = 1f;

    [SerializeField]
    private float minHeight, maxHeight;

    [SerializeField]
    private int noiseOffsetSeed = 12345;

    [SerializeField, Range(0.001f, 20f)]
    private float zoom = 1.0f;

    public int vertexCount = 0;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Mesh mesh;
    
    private bool requestCalculate;

    private void PrepareMesh() {
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));

        meshFilter = gameObject.AddComponent<MeshFilter>();
        
    }

    // Start is called before the first frame update
    void Start() {
        PrepareMesh();
        CalculateMesh();

        requestCalculate = false;
    }

    private void CalculateMesh() {
        mesh = new Mesh();
        // Amount of vertices is equal to the width * depth / resolution
        int width = (int)((size.x + resolution) / resolution);
        int depth = (int)((size.y + resolution) / resolution);


        Random.InitState(noiseOffsetSeed);
        Vector2 offset = Vector2.one;
        offset.x *= Random.value * 1000000;
        offset.y *= Random.value * 1000000;

        Vector3[] vertices = new Vector3[width * depth];
        int i = 0;
        for (float z=0f; z <= size.y; z += resolution) {
            for (float x = 0f; x <= size.x; x += resolution, i++) {
                float y = Mathf.Lerp(minHeight, maxHeight, Mathf.PerlinNoise((x/zoom) + offset.x, (z/zoom) + offset.y));
                vertices[i] = new Vector3(x, y, z);
                //vertices[i] = new Vector3(x, 0f, z);
            }
        }
        mesh.vertices = vertices;
        vertexCount = vertices.Length;

        // For each 'quad' there is 2 triangles, or 6 triangle points
        int[] tris = new int[width * depth * 6];

        i = 0;
        for (int z = 0; z < depth-1; z++, i++) {
            for (int x = 0; x < width-1; x++, i++) {
                int localIndex = i * 6;
                tris[localIndex] = i;
                tris[localIndex + 1] = i + width;
                tris[localIndex + 2] = i + 1;

                tris[localIndex + 3] = i + width;
                tris[localIndex + 4] = i + width + 1;
                tris[localIndex + 5] = i + 1;

            }
        }
        mesh.triangles = tris;

        Vector3[] normals = new Vector3[width * depth];
        /*
        for (i = 0; i < normals.Length; i++) {
            // Need to figure out the average plane from the nearby triangles
            // and then use that normal!
            normals[i] = Vector3.up;
        }
        mesh.normals = normals;
        */
        mesh.RecalculateNormals();

        Vector2[] uv = new Vector2[width * depth];
        for (i = 0; i < uv.Length; i++) {
            uv[i] = new Vector2(vertices[i].x, vertices[i].z);
        }
        mesh.uv = uv;

        meshFilter.mesh = mesh;
    }

    private void OnValidate() {
        requestCalculate = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (requestCalculate) {
            CalculateMesh();
            requestCalculate = false;
        }
    }
}
