using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour
{
    [SerializeField]
    private int cellRangeBuffer = 12;

    [SerializeField]
    public Vector2 size = new Vector2(100f,100f);

    [SerializeField, Range(0.05f, 2f)]
    public float resolution = 1f;

    [SerializeField]
    public float minHeight, maxHeight;

    [SerializeField]
    public int noiseOffsetSeed = 12345;

    [SerializeField, Range(0.001f, 20f)]
    public float zoom = 1.0f;

    [SerializeField]
    private Transform player;

    [SerializeField]
    public Material grass;

    private float xCellBuffer;
    
    public bool requestCalculate;

    private List<TerrainCell> cells;

    // Start is called before the first frame update
    void Start() {
        cells = new List<TerrainCell>();
        xCellBuffer = player.position.x;
        float startX = xCellBuffer - (xCellBuffer % size.x);
        float startZ = player.position.z - (player.position.z % size.y);
        for (float x = startX - (cellRangeBuffer*size.x); x < startX + (cellRangeBuffer*size.x); x += size.x) {
            for (float z = startZ-size.y; z < startZ +(size.y*15f); z += size.y) {
                AddCell(new Vector2(x, z));
            }
        }
        requestCalculate = false;
    }

    private void AddCell(Vector2 position) {
        GameObject newObject = Instantiate(new GameObject("Cell"), new Vector3(position.x, 0f, position.y), Quaternion.identity, transform);
        TerrainCell tc = newObject.AddComponent<TerrainCell>();
        tc.PrepareTerrainCell(this);
        cells.Add(tc);
    }

    private void StepForward() {
        WipeLastColumn();
        AddColumn();
    }

    private void WipeLastColumn() {
        float xPos = xCellBuffer - (xCellBuffer % size.x) - (cellRangeBuffer * size.x);
        List<TerrainCell> clearing = new List<TerrainCell>();
        foreach (TerrainCell tc in cells) {
            if (tc.CellLocation().x == xPos) {
                clearing.Add(tc);
            }
        }
        foreach (TerrainCell tc in clearing) {
            cells.Remove(tc);
            Destroy(tc.gameObject);
        }
    }

    private void AddColumn() {

        float x = xCellBuffer - (xCellBuffer % size.x) + (cellRangeBuffer * size.x);
        float startZ = player.position.z - (player.position.z % size.y);
        for (float z = startZ - size.y; z < startZ + (size.y * 15f); z += size.y) {
            AddCell(new Vector2(x, z));
        }
        xCellBuffer += size.x;
    }

    private void RemoveCell(TerrainCell cell) {
        cells.Remove(cell);
        Destroy(cell);
    }

    private void CalculateAllMeshes() {
        foreach (TerrainCell tc in cells) {
            tc.CalculateMesh();
        }
    }

    private void OnValidate() {
        requestCalculate = true;
    }

    // Update is called once per frame
    void Update()
    {
        while (xCellBuffer + 10 < player.position.x) {
            StepForward();
        }
        if (requestCalculate) {
            CalculateAllMeshes();
            requestCalculate = false;
        }
    }
}
