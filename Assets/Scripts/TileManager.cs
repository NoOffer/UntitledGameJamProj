using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.PlayerSettings;

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
        return coverage ? 1 : 0;
    }
}

[Serializable]
struct RoomPlacement
{
    public GameObject prefab;
}

public class TileManager : MonoBehaviour
{
    [SerializeField] private int gridSize;
    //[SerializeField] private float tileSideLen;

    [SerializeField] private Transform pipesRoot;
    [SerializeField] private GameObject basicPipePrefab;
    [SerializeField] private GameObject advancedPipePrefab;
    [SerializeField] private LayerMask whatIsPipe;
    [SerializeField] private LayerMask whatIsAdvancedPipe;
    [SerializeField] private GameObject pipePreview;

    [SerializeField] private Transform roomsRoot;
    [SerializeField] private RoomPlacement[] rooms;
    [SerializeField] private LayerMask whatIsRoomSupport;
    [SerializeField] private LayerMask whatIsMachining;
    [SerializeField] private LayerMask whatIsAdvancedMachining;
    [SerializeField] private LayerMask whatIsResearch;
    [SerializeField] private LayerMask whatIsAdvancedResearch;
    [SerializeField] private GameObject roomPreview;

    [SerializeField] private Transform testTransform;

    private Tile pipeTile;

    [SerializeField] private bool isPipeHorizontal = false;
    [SerializeField] private int roomType = 0;
    [SerializeField] private bool updateMode = false;

    private MineralInfo[,] mineralGrid;

    // Start is called before the first frame update
    void Start()
    {
        pipePreview.SetActive(false);

        mineralGrid = new MineralInfo[gridSize, gridSize];
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                mineralGrid[i, j] = new MineralInfo(3f, 2f, 1f);
            }
        }

        mineralGrid[50, 99].coverage = true;
        mineralGrid[49, 99].coverage = true;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 rawPos = (Camera.main.ScreenToWorldPoint(Input.mousePosition));
        Vector2 pos;

        // Set pipe
        if (true)
        {
            pos = new Vector2(Mathf.Round(rawPos.x), Mathf.Round(rawPos.y));

            if (gridSize / 2 > pos.x && pos.x > -gridSize / 2 && 0 > pos.y && pos.y > -gridSize)
            {
                pipePreview.SetActive(true);
                if (updateMode)
                {
                    Collider2D c = Physics2D.OverlapBox(rawPos, new Vector2(0.1f, 0.1f), 0f, whatIsPipe);
                    if (c)
                    {
                        pipePreview.transform.position = c.transform.position;
                        pipePreview.transform.rotation = c.transform.rotation;
                        if (Physics2D.OverlapCircle(rawPos, 0.1f, whatIsAdvancedPipe))
                        {
                            pipePreview.GetComponent<SpriteRenderer>().color = Color.red;
                        }
                        else
                        {
                            pipePreview.GetComponent<SpriteRenderer>().color = Color.green;

                            if (Input.GetMouseButtonDown(0))
                            {
                                Instantiate(advancedPipePrefab, c.transform.position - new Vector3(0f, 0f, 1f), c.transform.rotation, pipesRoot);
                            }
                        }
                    }
                    else
                    {
                        pipePreview.SetActive(false);
                    }
                }
                else
                {
                    pipePreview.transform.position = pos;
                    if (Input.mouseScrollDelta.y != 0)
                    {
                        isPipeHorizontal = !isPipeHorizontal;
                    }
                    if (isPipeHorizontal)
                    {
                        pipePreview.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                        if (Physics2D.OverlapBox(pos + new Vector2(0.5f, 0f), new Vector2(0.1f, 0.1f), 0f, whatIsPipe) ||
                            Physics2D.OverlapBox(pos - new Vector2(0.5f, 0f), new Vector2(0.1f, 0.1f), 0f, whatIsPipe))
                        {
                            pipePreview.GetComponent<SpriteRenderer>().color = Color.red;
                        }
                        else if (Physics2D.OverlapBox(pos, new Vector2(0.1f, 0.1f), 0f, whatIsPipe) ||
                            Physics2D.OverlapBox(pos + new Vector2(1f, 0f), new Vector2(0.1f, 0.1f), 0f, whatIsPipe) ||
                            Physics2D.OverlapBox(pos - new Vector2(1f, 0f), new Vector2(0.1f, 0.1f), 0f, whatIsPipe))
                        {
                            pipePreview.GetComponent<SpriteRenderer>().color = Color.green;

                            if (Input.GetMouseButtonDown(0))
                            {
                                Instantiate(basicPipePrefab, new Vector3(pos.x, pos.y, 2f), Quaternion.Euler(0f, 0f, 90f), pipesRoot);
                                mineralGrid[(int)(pos.x + gridSize / 2), (int)(pos.y + gridSize)].coverage = true;
                                mineralGrid[(int)(pos.x + gridSize / 2 - 1), (int)(pos.y + gridSize)].coverage = true;
                                mineralGrid[(int)(pos.x + gridSize / 2 - 1), (int)(pos.y + gridSize - 1)].coverage = true;
                                mineralGrid[(int)(pos.x + gridSize / 2), (int)(pos.y + gridSize - 1)].coverage = true;
                            }
                        }
                        else
                        {
                            pipePreview.GetComponent<SpriteRenderer>().color = Color.red;
                        }
                    }
                    else
                    {
                        pipePreview.transform.rotation = Quaternion.identity;
                        if (Physics2D.OverlapBox(pos + new Vector2(0f, 0.5f), new Vector2(0.1f, 0.1f), 0f, whatIsPipe) ||
                            Physics2D.OverlapBox(pos - new Vector2(0f, 0.5f), new Vector2(0.1f, 0.1f), 0f, whatIsPipe))
                        {
                            pipePreview.GetComponent<SpriteRenderer>().color = Color.red;
                        }
                        else if (Physics2D.OverlapBox(pos, new Vector2(0.1f, 0.1f), 0f, whatIsPipe) ||
                            Physics2D.OverlapBox(pos + new Vector2(0f, 1f), new Vector2(0.1f, 0.1f), 0f, whatIsPipe) ||
                            Physics2D.OverlapBox(pos - new Vector2(0f, 1f), new Vector2(0.1f, 0.1f), 0f, whatIsPipe))
                        {
                            pipePreview.GetComponent<SpriteRenderer>().color = Color.green;

                            if (Input.GetMouseButtonDown(0))
                            {
                                Instantiate(basicPipePrefab, new Vector3(pos.x, pos.y, 2f), Quaternion.identity, pipesRoot);
                                mineralGrid[(int)(pos.x + gridSize / 2), (int)(pos.y + gridSize)].coverage = true;
                                mineralGrid[(int)(pos.x + gridSize / 2 - 1), (int)(pos.y + gridSize)].coverage = true;
                                mineralGrid[(int)(pos.x + gridSize / 2 - 1), (int)(pos.y + gridSize - 1)].coverage = true;
                                mineralGrid[(int)(pos.x + gridSize / 2), (int)(pos.y + gridSize - 1)].coverage = true;
                            }
                        }
                        else
                        {
                            pipePreview.GetComponent<SpriteRenderer>().color = Color.red;
                        }
                    }
                }
            }
            else
            {
                pipePreview.SetActive(false);
            }

            //testTransform.position = new Vector3(pos.x, pos.y, 1);
            //Debug.Log(pos);
        }


        // Set room
        else
        {
            //Vector3Int tilePos = new Vector3Int((int)(pos.x), (int)(pos.y - 0.5f), 0);

            void RoomPreviewAndSetup()
            {
                roomPreview.SetActive(true);
                roomPreview.transform.position = pos;
                if (Input.GetMouseButtonDown(0))
                {
                    Instantiate(rooms[roomType].prefab, new Vector3(pos.x, pos.y, roomType), Quaternion.identity, roomsRoot);
                }
            }
            if (updateMode)
            {
                if (roomType == 0)
                {
                    Collider2D c = Physics2D.OverlapBox(rawPos, new Vector2(0.1f, 0.1f), 0f, whatIsMachining);
                    if (c)
                    {
                        pos = c.transform.position;

                        Debug.Log(pos);
                        if (!Physics2D.OverlapBox(pos, new Vector2(0.2f, 0.2f), 0, whatIsAdvancedMachining) &&
                            Physics2D.OverlapBox(pos - new Vector2(1f, 0f), new Vector2(0.2f, 0.2f), 0, whatIsMachining) &&
                            Physics2D.OverlapBox(pos + new Vector2(1f, 0f), new Vector2(0.2f, 0.2f), 0, whatIsMachining))
                        {
                            RoomPreviewAndSetup();
                        }
                    }
                }
                else if (roomType == 1)
                {
                    Collider2D c = Physics2D.OverlapBox(rawPos, new Vector2(0.1f, 0.1f), 0f, whatIsResearch);
                    if (c)
                    {
                        pos = c.transform.position;
                        if (!Physics2D.OverlapBox(pos, new Vector2(0.2f, 0.2f), 0, whatIsAdvancedResearch) &&
                            Physics2D.OverlapBox(pos - new Vector2(1f, 0f), new Vector2(0.2f, 0.2f), 0, whatIsResearch) &&
                            Physics2D.OverlapBox(pos + new Vector2(1f, 0f), new Vector2(0.2f, 0.2f), 0, whatIsResearch))
                        {
                            RoomPreviewAndSetup();
                        }
                    }
                }
                else
                {
                    roomPreview.SetActive(false);
                }
            }
            else
            {
                pos = new Vector2(Mathf.Round(rawPos.x / 2) * 2, Mathf.Round(rawPos.y / 2 + 0.5f) * 2);

                if (!Physics2D.OverlapBox(pos, new Vector2(0.2f, 0.2f), 0, whatIsRoomSupport) &&
                Physics2D.OverlapBox(pos - new Vector2(0f, 1f), new Vector2(0.2f, 0.2f), 0, whatIsRoomSupport))
                {
                    RoomPreviewAndSetup();
                }
                else
                {
                    roomPreview.SetActive(false);
                }
            }
        }
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
    //            Gizmos.DrawWireCube(new Vector3(i - gridSize / 2 + 0.5f, j - gridSize + 0.5f, 0), new Vector3(1, 1, 1));
    //        }
    //    }
    //}
}
