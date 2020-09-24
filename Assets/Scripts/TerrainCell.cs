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

    private float resolution;

    public void PrepareTerrainCell(TerrainGeneration tg) {
        this.tg = tg;
        if (transform.position.z >= 30f) {
            resolution = tg.resolution * 4f;
        } else {
            resolution = tg.resolution;
        }
        PrepareMesh();
        CalculateMesh();
    }

    public Vector2 CellLocation() {
        return new Vector2(transform.position.x, transform.position.z);
    }

    private void PrepareMesh() {
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = tg.grass;

        meshFilter = gameObject.AddComponent<MeshFilter>();
    }

    public void CalculateMesh() {
        mesh = new Mesh();
        // Amount of vertices is equal to the width * depth / resolution
        int width = (int)((tg.size.x + resolution) / resolution);
        int depth = (int)((tg.size.y + resolution) / resolution);


        Random.InitState(tg.noiseOffsetSeed);
        Vector2 offset = Vector2.one;
        offset.x *= Random.value * 1000000;
        offset.y *= Random.value * 1000000;

        Vector2 secondaryOffset = Vector2.one;
        secondaryOffset.x *= Random.value * 1000000;
        secondaryOffset.y *= Random.value * 1000000;

        Vector3[] vertices = new Vector3[width * depth];
        int i = 0;
        for (float z = 0f; z <= tg.size.y; z += resolution) {
            float extraY = 0f;
            float weighting = 0f;
            // Scale y between 0 and 30 z so we have something in foreground lower than the place the player is moving through
            float globalZ = z + transform.position.z;
            if (globalZ <= 30f && globalZ >= 0f) {
                extraY = ((30 * globalZ) - (globalZ * globalZ)) * 0.025f;
            } else {
                // We add on the secondary mesh
                weighting = Mathf.Clamp((globalZ - 30), 0f, tg.maxHeight * tg.maxHeight * 100);
            }

            for (float x = 0f; x <= tg.size.x; x += resolution, i++) {
                float localX = x + transform.position.x;
                float localZ = z + transform.position.z;
                if (globalZ > 30f) {
                    extraY = weighting * Mathf.PerlinNoise((localX / (tg.zoom*10)) + secondaryOffset.x, (localZ / (tg.zoom * 10)) + secondaryOffset.y);
                }
                float y = Mathf.Lerp(tg.minHeight, tg.maxHeight, Mathf.PerlinNoise((localX / tg.zoom) + offset.x, (localZ / tg.zoom) + offset.y)) + extraY;
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
        
        mesh.RecalculateNormals();
        // We need to do more work for this tbh
        for (int z = 0; z < depth; z++) {
            mesh.normals[z * width] = Vector3.up;
            mesh.normals[z * width+width-1] = Vector3.up;
        }
        for (int x = 0; x < width; x++) {
            mesh.normals[x * depth] = Vector3.up;
            mesh.normals[x * depth + depth - 1] = Vector3.up;
        }

        Vector2[] uv = new Vector2[width * depth];
        for (i = 0; i < uv.Length; i++) {
            uv[i] = new Vector2(vertices[i].x, vertices[i].z);
        }
        mesh.uv = uv;

        meshFilter.mesh = mesh;
    }
}
