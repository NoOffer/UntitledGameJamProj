using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

struct MineralInfo
{
    public MineralInfo(float param_OreRate, float param_Ore1Rate, float param_Ore2Rate)
    {
        oreRate = param_OreRate;
        ore1Rate = param_Ore1Rate;
        ore2Rate = param_Ore2Rate;
        xPipe = false;
        yPipe = false;
        coverage = false;
    }

    public float oreRate;
    public float ore1Rate;
    public float ore2Rate;
    public bool xPipe;
    public bool yPipe;
    public bool coverage;

    public float Output()
    {
        return coverage ? oreRate : 0;
    }
}

public class TileManager : MonoBehaviour
{
    [SerializeField] private int gridSize;
    [SerializeField] private float tileSideLen;

    [SerializeField] private Tilemap pipeTilemapX;
    [SerializeField] private Tilemap pipeTilemapY;
    [SerializeField] private Sprite pipeSpriteX;
    [SerializeField] private Sprite pipeSpriteY;

    [SerializeField] private GameObject previewX;
    [SerializeField] private GameObject previewY;
    [SerializeField] private Transform testTransform;

    private Tile pipeTileX;
    private Tile pipeTileY;

    private float oreAmount;
    private float ore1Amount;
    private float ore2Amount;
    private float oreUpgradeRate;
    private float ore1UpgradeRate;
    private float ore2UpgradeRate;

    private MineralInfo[,] mineralGrid;

    // Start is called before the first frame update
    void Start()
    {
        previewX.SetActive(false);
        previewY.SetActive(false);

        pipeTileX = ScriptableObject.CreateInstance<Tile>();
        pipeTileX.sprite = pipeSpriteX;
        pipeTileY = ScriptableObject.CreateInstance<Tile>();
        pipeTileY.sprite = pipeSpriteY;

        mineralGrid = new MineralInfo[gridSize, gridSize];
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                mineralGrid[i, j] = new MineralInfo(3f, 2f, 1f);
            }
        }
        mineralGrid[50, 100].xPipe = true;
        mineralGrid[50, 100].yPipe = true;
        mineralGrid[50, 100].coverage = true;
        mineralGrid[49, 100].coverage = true;
        mineralGrid[50, 99].coverage = true;
        pipeTilemapX.SetTile(new Vector3Int(0, 0, 0), pipeTileX);
        pipeTilemapY.SetTile(new Vector3Int(0, 0, 0), pipeTileY);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 rawPos = (Camera.main.ScreenToWorldPoint(Input.mousePosition)) / tileSideLen;
        Vector2 pos = new Vector2(Mathf.Round(rawPos.x + 0.5f) - 0.5f, Mathf.Round(rawPos.y + 0.5f) - 0.5f);
        testTransform.position = new Vector3(pos.x, pos.y, 1);

        // Set pipe
        {
            bool CheckSurrounding(Vector2Int checkPos, bool isHorizontal)
            {
                if (isHorizontal)
                {
                    return !mineralGrid[checkPos.x, checkPos.y].xPipe &&
                        (mineralGrid[checkPos.x, checkPos.y].yPipe || mineralGrid[checkPos.x - 1, checkPos.y].xPipe ||
                        mineralGrid[checkPos.x, checkPos.y - 1].yPipe || mineralGrid[checkPos.x + 1, checkPos.y - 1].yPipe ||
                        mineralGrid[checkPos.x + 1, checkPos.y].xPipe || mineralGrid[checkPos.x + 1, checkPos.y].yPipe);
                }
                else
                {
                    return !mineralGrid[checkPos.x, checkPos.y].yPipe &&
                        (mineralGrid[checkPos.x, checkPos.y].xPipe || mineralGrid[checkPos.x - 1, checkPos.y].xPipe ||
                        mineralGrid[checkPos.x, checkPos.y - 1].yPipe || mineralGrid[checkPos.x - 1, checkPos.y + 1].xPipe ||
                        mineralGrid[checkPos.x, checkPos.y + 1].xPipe || mineralGrid[checkPos.x, checkPos.y + 1].yPipe);
                }
            }

            //Avoid index out of bound
            Vector2Int tilePos = new Vector2Int((int)(pos.x - 0.5f), (int)(pos.y - 0.5f));
            Vector2Int tileIdx = new Vector2Int(tilePos.x + gridSize / 2, tilePos.y + gridSize - 2);
            if (tileIdx.x > 0 && tileIdx.x < gridSize - 1 && tileIdx.y > 0 && tileIdx.y < gridSize - 1)
            {
                if (Mathf.Abs(rawPos.x - pos.x) > Mathf.Abs(rawPos.y - pos.y))
                {
                    previewY.SetActive(true);
                    previewX.SetActive(false);
                    if (rawPos.x > pos.x)
                    {
                        if (CheckSurrounding(tileIdx + new Vector2Int(1, 0), false))
                        {
                            previewY.GetComponent<SpriteRenderer>().color = Color.green;
                            if (Input.GetMouseButtonDown(0))
                            {
                                pipeTilemapY.SetTile(new Vector3Int(tilePos.x + 1, tilePos.y, 0), pipeTileY);
                                mineralGrid[tileIdx.x + 1, tileIdx.y].yPipe = true;
                                mineralGrid[tileIdx.x, tileIdx.y].coverage = true;
                                mineralGrid[tileIdx.x + 1, tileIdx.y].coverage = true;
                            }
                        }
                        else
                        {
                            previewY.GetComponent<SpriteRenderer>().color = Color.red;
                        }
                        previewY.transform.position = pos + new Vector2(tileSideLen / 2, 0);
                    }
                    else
                    {
                        if (CheckSurrounding(tileIdx, false))
                        {
                            previewY.GetComponent<SpriteRenderer>().color = Color.green;
                            if (Input.GetMouseButtonDown(0))
                            {
                                pipeTilemapY.SetTile(new Vector3Int(tilePos.x, tilePos.y, 0), pipeTileY);
                                mineralGrid[tileIdx.x, tileIdx.y].yPipe = true;
                                mineralGrid[tileIdx.x, tileIdx.y].coverage = true;
                                mineralGrid[tileIdx.x - 1, tileIdx.y].coverage = true;
                            }
                        }
                        else
                        {
                            previewY.GetComponent<SpriteRenderer>().color = Color.red;
                        }
                        previewY.transform.position = pos - new Vector2(tileSideLen / 2, 0);
                    }
                }
                else
                {
                    previewX.SetActive(true);
                    previewY.SetActive(false);
                    if (rawPos.y > pos.y)
                    {
                        if (CheckSurrounding(tileIdx + new Vector2Int(0, 1), true))
                        {
                            previewX.GetComponent<SpriteRenderer>().color = Color.green;
                            if (Input.GetMouseButtonDown(0))
                            {
                                pipeTilemapX.SetTile(new Vector3Int(tilePos.x, tilePos.y + 1, 0), pipeTileX);
                                mineralGrid[tileIdx.x, tileIdx.y + 1].xPipe = true;
                                mineralGrid[tileIdx.x, tileIdx.y].coverage = true;
                                mineralGrid[tileIdx.x, tileIdx.y + 1].coverage = true;
                            }
                        }
                        else
                        {
                            previewX.GetComponent<SpriteRenderer>().color = Color.red;
                        }
                        previewX.transform.position = pos + new Vector2(0, tileSideLen / 2);
                    }
                    else
                    {
                        if (CheckSurrounding(tileIdx, true))
                        {
                            previewX.GetComponent<SpriteRenderer>().color = Color.green;
                            if (Input.GetMouseButtonDown(0))
                            {
                                pipeTilemapX.SetTile(new Vector3Int(tilePos.x, tilePos.y, 0), pipeTileX);
                                mineralGrid[tileIdx.x, tileIdx.y].xPipe = true;
                                mineralGrid[tileIdx.x, tileIdx.y].coverage = true;
                                mineralGrid[tileIdx.x, tileIdx.y - 1].coverage = true;
                            }
                        }
                        else
                        {
                            previewX.GetComponent<SpriteRenderer>().color = Color.red;
                        }
                        previewX.transform.position = pos - new Vector2(0, tileSideLen / 2);
                    }
                }
            }
        }
        Debug.Log(pos);
    }

    //private void OnDrawGizmos()
    //{
    //    for (int i = 0; i < gridSize; i++)
    //    {
    //        for (int j = 0; j < gridSize; j++)
    //        {
    //            if (mineralGrid[i, j].Output() > 0)
    //            {
    //                Gizmos.color = Color.green;
    //            }
    //            else
    //            {
    //                Gizmos.color = Color.red;
    //            }
    //            Gizmos.DrawWireCube(new Vector3(i - gridSize / 2 + 0.5f, j - gridSize / 2 + 0.5f, 0), new Vector3(1, 1, 1));
    //        }
    //    }
    //}
}
