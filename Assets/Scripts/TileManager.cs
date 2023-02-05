using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

enum OreType
{
    Basic, Mana, Advanced, None
}

struct MineralInfo
{
    public MineralInfo(OreType type, int amount)
    {
        oreType = type;
        oreAmount = amount;
        coverage = false;
        advanced = false;
    }

    public OreType oreType;
    public int oreAmount;
    public bool coverage;
    public bool advanced;

    public int Output()
    {
        if (coverage && oreAmount > 0 && (oreType != OreType.Advanced || advanced))
        {
            oreAmount--;
            return (int)oreType;
        }
        else
        {
            return (int)OreType.None;
        }
    }
}

[Serializable]
struct RoomPlacement
{
    public GameObject prefab;
}

public class TileManager : MonoBehaviour
{
    [SerializeField] private float tInterval;
    private float timer = 0;

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
    [SerializeField] private LayerMask whatIsBasicRoomSupport;
    [SerializeField] private LayerMask whatIsMachining;
    [SerializeField] private LayerMask whatIsAdvancedMachining;
    [SerializeField] private LayerMask whatIsResearch;
    [SerializeField] private LayerMask whatIsAdvancedResearch;
    [SerializeField] private LayerMask whatIsResidence;
    [SerializeField] private LayerMask whatIsAdvancedResidence;
    [SerializeField] private GameObject roomPreview;

    [SerializeField] private Transform testTransform;

    [SerializeField] private Tilemap oreMap;

    private bool isPipeHorizontal = false;
    [SerializeField] private int roomType = 0;
    [SerializeField] private bool updateMode = false;

    private MineralInfo[,] mineralGrid;
    [SerializeField] private Sprite basicOreSprite;
    [SerializeField] private Sprite manaOreSprite;
    [SerializeField] private Sprite advancedOreSprite;
    private int[] oreStorage;
    private int[] oreRate;
    private int[] alloyStorage;
    private int population;

    [SerializeField] private Text oreDisplay;
    [SerializeField] private Text alloyDisplay;
    [SerializeField] private Text populationDisplay;
    [SerializeField] private Image constructionType;

    // Start is called before the first frame update
    void Start()
    {
        pipePreview.SetActive(false);

        mineralGrid = new MineralInfo[gridSize, gridSize];
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                float p = UnityEngine.Random.Range(0f, 1f);
                if (p < 0.05)
                {
                    mineralGrid[i, j] = new MineralInfo(OreType.Advanced, 100);
                }
                else if (p < 0.15)
                {
                    mineralGrid[i, j] = new MineralInfo(OreType.Mana, 100);
                }
                else if (p < 0.3)
                {
                    mineralGrid[i, j] = new MineralInfo(OreType.Basic, 100);
                }
                else
                {
                    mineralGrid[i, j] = new MineralInfo(OreType.None, 100);
                }
                SetOreTile(i, j);
            }
        }

        mineralGrid[50, 99] = new MineralInfo(OreType.Basic, 100);
        mineralGrid[49, 99] = new MineralInfo(OreType.Basic, 100);
        mineralGrid[50, 99].coverage = true;
        mineralGrid[49, 99].coverage = true;
        SetOreTile(50, 99);
        SetOreTile(49, 99);

        oreStorage = new int[] { 0, 0, 0 };
        oreRate = new int[] { 1, 0, 0 };
        alloyStorage = new int[] { 0, 0, 0 };
        population = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 rawPos = (Camera.main.ScreenToWorldPoint(Input.mousePosition));
        Vector2 pos;

        // Set pipe
        if (rawPos.y < 1)
        {
            roomPreview.SetActive(false);
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
                                pos = c.transform.position - new Vector3(0f, 0f, 1f);
                                Instantiate(advancedPipePrefab, pos, c.transform.rotation, pipesRoot);
                                mineralGrid[(int)(pos.x + gridSize / 2), (int)(pos.y + gridSize)].advanced = true;
                                mineralGrid[(int)(pos.x + gridSize / 2 - 1), (int)(pos.y + gridSize)].advanced = true;
                                mineralGrid[(int)(pos.x + gridSize / 2 - 1), (int)(pos.y + gridSize - 1)].advanced = true;
                                mineralGrid[(int)(pos.x + gridSize / 2), (int)(pos.y + gridSize - 1)].advanced = true;
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
                        else if (alloyStorage[0] > 4 && (
                            Physics2D.OverlapBox(pos, new Vector2(0.1f, 0.1f), 0f, whatIsPipe) ||
                            Physics2D.OverlapBox(pos + new Vector2(1f, 0f), new Vector2(0.1f, 0.1f), 0f, whatIsPipe) ||
                            Physics2D.OverlapBox(pos - new Vector2(1f, 0f), new Vector2(0.1f, 0.1f), 0f, whatIsPipe)))
                        {
                            pipePreview.GetComponent<SpriteRenderer>().color = Color.green;

                            if (Input.GetMouseButtonDown(0))
                            {
                                Instantiate(basicPipePrefab, new Vector3(pos.x, pos.y, 2f), Quaternion.Euler(0f, 0f, 90f), pipesRoot);
                                mineralGrid[(int)(pos.x + gridSize / 2), (int)(pos.y + gridSize)].coverage = true;
                                mineralGrid[(int)(pos.x + gridSize / 2 - 1), (int)(pos.y + gridSize)].coverage = true;
                                mineralGrid[(int)(pos.x + gridSize / 2 - 1), (int)(pos.y + gridSize - 1)].coverage = true;
                                mineralGrid[(int)(pos.x + gridSize / 2), (int)(pos.y + gridSize - 1)].coverage = true;
                                alloyStorage[0] -= 5;
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
                        else if (alloyStorage[0] > 4 && (
                            Physics2D.OverlapBox(pos, new Vector2(0.1f, 0.1f), 0f, whatIsPipe) ||
                            Physics2D.OverlapBox(pos + new Vector2(0f, 1f), new Vector2(0.1f, 0.1f), 0f, whatIsPipe) ||
                            Physics2D.OverlapBox(pos - new Vector2(0f, 1f), new Vector2(0.1f, 0.1f), 0f, whatIsPipe)))
                        {
                            pipePreview.GetComponent<SpriteRenderer>().color = Color.green;

                            if (Input.GetMouseButtonDown(0))
                            {
                                Instantiate(basicPipePrefab, new Vector3(pos.x, pos.y, 2f), Quaternion.identity, pipesRoot);
                                mineralGrid[(int)(pos.x + gridSize / 2), (int)(pos.y + gridSize)].coverage = true;
                                mineralGrid[(int)(pos.x + gridSize / 2 - 1), (int)(pos.y + gridSize)].coverage = true;
                                mineralGrid[(int)(pos.x + gridSize / 2 - 1), (int)(pos.y + gridSize - 1)].coverage = true;
                                mineralGrid[(int)(pos.x + gridSize / 2), (int)(pos.y + gridSize - 1)].coverage = true;
                                alloyStorage[0] -= 5;
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
            pipePreview.SetActive(false);
            roomPreview.SetActive(true);
            //Vector3Int tilePos = new Vector3Int((int)(pos.x), (int)(pos.y - 0.5f), 0);

            void RoomPreviewAndSetup()
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Instantiate(rooms[roomType].prefab, new Vector3(pos.x, pos.y, roomType), Quaternion.identity, roomsRoot);
                    switch (roomType)
                    {
                        case 0:
                            oreRate[0]++;
                            break;
                        case 1:
                            oreRate[1]++;
                            break;
                        case 2:
                            population += 3;
                            break;
                        case 3:
                            oreRate[2]++;
                            break;
                        case 4:
                            oreRate[1]++;
                            break;
                        case 5:
                            population += 3;
                            break;
                    }
                    if (updateMode)
                    {
                        alloyStorage[2] -= 5;
                    }

                    else
                    {
                        alloyStorage[0] -= 5;
                    }
                    if (roomType < 2)
                    {
                        population--;
                    }
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                roomType = (roomType + 1) % 6;
            }
            updateMode = roomType > 2;
            pos = new Vector2(Mathf.Round(rawPos.x / 2) * 2, Mathf.Round(rawPos.y / 2) * 2);

            if (updateMode)
            {
                if (roomType == 3)
                {
                    Collider2D c = Physics2D.OverlapBox(pos, new Vector2(0.1f, 0.1f), 0f, whatIsMachining);
                    if (c)
                    {
                        pos = c.transform.position;

                        //Debug.Log(pos);
                        if (Physics2D.OverlapBox(pos, new Vector2(0.2f, 0.2f), 0, whatIsAdvancedMachining))
                        {
                            roomPreview.GetComponent<SpriteRenderer>().color = Color.red;
                        }
                        else
                        {
                            if (alloyStorage[2] > 4)
                            {
                                roomPreview.GetComponent<SpriteRenderer>().color = Color.green;
                                RoomPreviewAndSetup();
                            }
                            else
                            {
                                roomPreview.GetComponent<SpriteRenderer>().color = Color.red;
                            }
                        }
                    }
                    else
                    {
                        roomPreview.GetComponent<SpriteRenderer>().color = Color.white;
                    }
                }
                else if (roomType == 4)
                {
                    Collider2D c = Physics2D.OverlapBox(pos, new Vector2(0.1f, 0.1f), 0f, whatIsResearch);
                    if (c)
                    {
                        if (Physics2D.OverlapBox(pos, new Vector2(0.2f, 0.2f), 0, whatIsAdvancedResearch))
                        {
                            roomPreview.GetComponent<SpriteRenderer>().color = Color.red;
                        }
                        else
                        {
                            if (alloyStorage[2] > 4)
                            {
                                roomPreview.GetComponent<SpriteRenderer>().color = Color.green;
                                RoomPreviewAndSetup();
                            }
                            else
                            {
                                roomPreview.GetComponent<SpriteRenderer>().color = Color.red;
                            }
                        }
                    }
                    else
                    {
                        roomPreview.GetComponent<SpriteRenderer>().color = Color.white;
                    }
                }
                else
                {
                    Collider2D c = Physics2D.OverlapBox(pos, new Vector2(0.1f, 0.1f), 0f, whatIsResidence);
                    if (c)
                    {
                        if (Physics2D.OverlapBox(pos, new Vector2(0.2f, 0.2f), 0, whatIsAdvancedResidence))
                        {
                            roomPreview.GetComponent<SpriteRenderer>().color = Color.red;
                        }
                        else
                        {
                            if (alloyStorage[2] > 4)
                            {
                                roomPreview.GetComponent<SpriteRenderer>().color = Color.green;
                                RoomPreviewAndSetup();
                            }
                            else
                            {
                                roomPreview.GetComponent<SpriteRenderer>().color = Color.red;
                            }
                        }
                    }
                    else
                    {
                        roomPreview.GetComponent<SpriteRenderer>().color = Color.white;
                    }
                }
            }
            else
            {
                if ((roomType == 2 || population > 0) && alloyStorage[0] > 4)
                {
                    if (!Physics2D.OverlapBox(pos, new Vector2(0.2f, 0.2f), 0, whatIsBasicRoomSupport) &&
                    Physics2D.OverlapBox(pos - new Vector2(0f, 1f), new Vector2(0.2f, 0.2f), 0, whatIsBasicRoomSupport))
                    {
                        roomPreview.GetComponent<SpriteRenderer>().color = Color.green;
                        RoomPreviewAndSetup();
                    }
                    else
                    {
                        roomPreview.GetComponent<SpriteRenderer>().color = Color.red;
                    }
                }
                else
                {
                    roomPreview.GetComponent<SpriteRenderer>().color = Color.red;
                }
            }
            roomPreview.transform.position = pos;
        }
    }

    private void FixedUpdate()
    {
        if (timer > tInterval)
        {
            timer -= tInterval;

            // Harvest ore
            Debug.Log(oreRate[0] + " " + oreRate[1] + " " + oreRate[2]);
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    int res = mineralGrid[i, j].Output();
                    if (res < 3)
                    {
                        oreStorage[res]++;
                    }
                }
            }
            for (int i = 0; i < 3; i++)
            {
                alloyStorage[i] += Mathf.Min(oreRate[i], oreStorage[i]);
                oreStorage[i] -= Mathf.Min(oreRate[i], oreStorage[i]);
            }
        }
        timer += Time.fixedDeltaTime;
    }

    private void LateUpdate()
    {
        oreDisplay.text = oreStorage[0] +
            "\n" + oreStorage[1] +
            "\n" + oreStorage[2];
        alloyDisplay.text = alloyStorage[0] +
            "\n" + alloyStorage[1] +
            "\n" + alloyStorage[2];
        populationDisplay.text = population + "";
        constructionType.sprite = rooms[roomType].prefab.GetComponent<SpriteRenderer>().sprite;
    }

    void SetOreTile(int i, int j)
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        switch (mineralGrid[i, j].oreType)
        {
            case OreType.Basic:
                tile.sprite = basicOreSprite;
                break;
            case OreType.Mana:
                tile.sprite = manaOreSprite;
                break;
            case OreType.Advanced:
                tile.sprite = advancedOreSprite;
                break;
        }

        oreMap.SetTile(new Vector3Int(i - gridSize / 2, j - gridSize, 10), tile);
    }

    //private void OnDrawGizmos()
    //{
    //    for (int i = 0; i < gridSize; i++)
    //    {
    //        for (int j = 0; j < gridSize; j++)
    //        {
    //            if (mineralGrid[i, j].coverage)
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
