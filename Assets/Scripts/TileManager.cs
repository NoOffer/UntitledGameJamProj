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
    [SerializeField] private bool pipeUpdateMode = false;
    [SerializeField] private bool roomUpdateMode = false;

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
    [SerializeField] private Image[] constructionTypes;

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
        mineralGrid[49, 99] = new MineralInfo(OreType.Mana, 100);
        mineralGrid[50, 99].coverage = true;
        mineralGrid[49, 99].coverage = true;
        SetOreTile(50, 99);
        SetOreTile(49, 99);

        oreStorage = new int[] { 0, 0, 0 };
        oreRate = new int[] { 1, 1, 0 };
        alloyStorage = new int[] { 0, 0, 0 };
        population = 1;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 rawPos = (Camera.main.ScreenToWorldPoint(Input.mousePosition));
        Vector2 pos;

        // Set pipe
        if (rawPos.y < 1)
        {
            for (int i = 0; i < 6; i++)
            {
                constructionTypes[i].color = new Color(0.5f, 0.5f, 0.5f);
            }
            if (pipeUpdateMode)
            {
                constructionTypes[6].color = new Color(0.5f, 0.5f, 0.5f);
                constructionTypes[7].color = new Color(1f, 1f, 1f);
            }
            else
            {
                constructionTypes[6].color = new Color(1f, 1f, 1f);
                constructionTypes[7].color = new Color(0.5f, 0.5f, 0.5f);
            }

            if (Input.mouseScrollDelta.y != 0)
            {
                pipeUpdateMode = !pipeUpdateMode;
            }

            roomPreview.SetActive(false);
            pos = new Vector2(Mathf.Round(rawPos.x), Mathf.Round(rawPos.y));

            if (gridSize / 2 > pos.x && pos.x > -gridSize / 2 && 0 > pos.y && pos.y > -gridSize)
            {
                pipePreview.SetActive(true);
                if (pipeUpdateMode)
                {
                    Collider2D c = Physics2D.OverlapBox(rawPos, new Vector2(0.1f, 0.1f), 0f, whatIsPipe);
                    if (c)
                    {
                        pipePreview.transform.position = c.transform.position;
                        pipePreview.transform.rotation = c.transform.rotation;
                        if (Physics2D.OverlapCircle(rawPos, 0.1f, whatIsAdvancedPipe) || alloyStorage[1] < 5)
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
                                alloyStorage[1] -= 5;
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
                    if (Input.GetMouseButtonDown(1))
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
            Debug.Log(roomType);
            pipePreview.SetActive(false);
            roomPreview.SetActive(true);
            //Vector3Int tilePos = new Vector3Int((int)(pos.x), (int)(pos.y - 0.5f), 0);

            void RoomPreviewAndSetup()
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Instantiate(rooms[roomType].prefab, new Vector3(pos.x, pos.y, -roomType), Quaternion.identity, roomsRoot);
                    switch (roomType)
                    {
                        case 0:
                            population--;
                            oreRate[0]++;
                            alloyStorage[0] -= 5;
                            break;
                        case 1:
                            population--;
                            oreRate[1]++;
                            alloyStorage[0] -= 5;
                            break;
                        case 2:
                            population += 3;
                            alloyStorage[1] -= 5;
                            break;
                        case 3:
                            oreRate[2]++;
                            alloyStorage[0] -= 5;
                            break;
                        case 4:
                            oreRate[1]++;
                            alloyStorage[2] -= 5;
                            break;
                        case 5:
                            population += 3;
                            alloyStorage[2] -= 5;
                            break;
                    }
                }
            }

            if (Input.mouseScrollDelta.y < 0)
            {
                roomType = (roomType + 1) % 6;
            }
            else if(Input.mouseScrollDelta.y > 0)
            {
                roomType = (roomType + 5) % 6;
            }
            for (int i = 0; i < 6; i++)
            {
                if (i == roomType)
                {
                    constructionTypes[roomType].color = new Color(1f, 1f, 1f);
                }
                else
                {
                    constructionTypes[i].color = new Color(0.5f, 0.5f, 0.5f);
                }
            }
            constructionTypes[6].color = new Color(0.5f, 0.5f, 0.5f);
            constructionTypes[7].color = new Color(0.5f, 0.5f, 0.5f);

            roomUpdateMode = roomType > 2;
            pos = new Vector2(Mathf.Round(rawPos.x / 2) * 2, Mathf.Round(rawPos.y / 2) * 2);

            if (roomUpdateMode)
            {
                if (roomType == 3)
                {
                    Collider2D c = Physics2D.OverlapBox(pos, new Vector2(0.1f, 0.1f), 0f, whatIsMachining);
                    if (c)
                    {
                        pos = c.transform.position;

                        if (Physics2D.OverlapBox(pos, new Vector2(0.2f, 0.2f), 0, whatIsAdvancedMachining))
                        {
                            roomPreview.GetComponent<SpriteRenderer>().color = Color.red;
                        }
                        else
                        {
                            if (alloyStorage[0] > 4)
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
                switch (roomType)
                {
                    case 0:
                        if (population > 0 && alloyStorage[0] > 4)
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
                        break;
                    case 1:
                        if (population > 0 && alloyStorage[0] > 4)
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
                        break;
                    case 2:
                        if (alloyStorage[1] > 4)
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
                        break;
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
            //Debug.Log(roomType);
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
