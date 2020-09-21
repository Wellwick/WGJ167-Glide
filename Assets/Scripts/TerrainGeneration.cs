using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour
{
    [SerializeField]
    private int closeCells = 5;

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
    
    public bool requestCalculate;

    private List<TerrainCell> cells;

    // Start is called before the first frame update
    void Start() {
        cells = new List<TerrainCell>();
        AddCell(Vector2.zero);
        AddCell(new Vector2(10f,0f));
        requestCalculate = false;
    }

    private void AddCell(Vector2 position) {
        GameObject newObject = Instantiate(new GameObject("Cell"), new Vector3(position.x, 0f, position.y), Quaternion.identity, transform);
        TerrainCell tc = newObject.AddComponent<TerrainCell>();
        tc.PrepareTerrainCell(this);
        cells.Add(tc);
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
        if (requestCalculate) {
            CalculateAllMeshes();
            requestCalculate = false;
        }
    }
}
