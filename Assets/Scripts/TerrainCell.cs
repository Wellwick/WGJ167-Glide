using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainCell : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Mesh mesh;

    private TerrainGeneration tg;

    public int vertexCount = 0;

    public void PrepareTerrainCell(TerrainGeneration tg) {
        this.tg = tg;
        PrepareMesh();
        CalculateMesh();
    }

    public Vector2 CellLocation() {
        return new Vector2(transform.position.x, transform.position.z);
    }

    private void PrepareMesh() {
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));

        meshFilter = gameObject.AddComponent<MeshFilter>();
    }

    public void CalculateMesh() {
        mesh = new Mesh();
        // Amount of vertices is equal to the width * depth / resolution
        int width = (int)((tg.size.x + tg.resolution) / tg.resolution);
        int depth = (int)((tg.size.y + tg.resolution) / tg.resolution);


        Random.InitState(tg.noiseOffsetSeed);
        Vector2 offset = Vector2.one;
        offset.x *= Random.value * 1000000;
        offset.y *= Random.value * 1000000;

        Vector3[] vertices = new Vector3[width * depth];
        int i = 0;
        for (float z = 0f; z <= tg.size.y; z += tg.resolution) {
            for (float x = 0f; x <= tg.size.x; x += tg.resolution, i++) {
                float localX = x + transform.position.x;
                float localZ = z + transform.position.z;
                float y = Mathf.Lerp(tg.minHeight, tg.maxHeight, Mathf.PerlinNoise((localX / tg.zoom) + offset.x, (localZ / tg.zoom) + offset.y));
                vertices[i] = new Vector3(x, y, z);
                //vertices[i] = new Vector3(x, 0f, z);
            }
        }
        mesh.vertices = vertices;
        vertexCount = vertices.Length;

        // For each 'quad' there is 2 triangles, or 6 triangle points
        int[] tris = new int[width * depth * 6];

        i = 0;
        for (int z = 0; z < depth - 1; z++, i++) {
            for (int x = 0; x < width - 1; x++, i++) {
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
        mesh.RecalculateNormals();

        Vector2[] uv = new Vector2[width * depth];
        for (i = 0; i < uv.Length; i++) {
            uv[i] = new Vector2(vertices[i].x, vertices[i].z);
        }
        mesh.uv = uv;

        meshFilter.mesh = mesh;
    }
}
