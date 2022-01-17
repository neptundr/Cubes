using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Map : MonoBehaviour
{
    public static Player[] Players;

    private static Map instance;
    private const int MaxFrequency = 11;

    [Header("2 or 4")] [SerializeField] private int playersCount;
    [Range(1, 100)] [SerializeField] private int minStartLevel;
    [SerializeField] private int startLevelHandicap;
    [Range(1, 100)] [SerializeField] private int maxLevel = 8;
    [Header("How many colors are there so many players")] [SerializeField] private List<Color> playerColors;
    [Header("X > Y")] [SerializeField] private Vector2Int size;
    [SerializeField] private int seed;
    [Range(1, MaxFrequency - 1)] [SerializeField] private int frequency;
    [Range(0, 100)] [SerializeField] private int diagonalChance;
    [Range(1, 100)] [SerializeField] private int baseResourceIncome;
    [Range(0, 100)] [SerializeField] private int expensiveTowerChance;
    [Range(1, 20)] [SerializeField] private int expensiveTowerAddition;
    [Range(0, 100)] [SerializeField] private int nullTowerChance;
    [SerializeField] private bool symmetric;
    [SerializeField] private bool snapHorizontal;
    [Space]
    [SerializeField] private Text[] resourcesTexts;
    [SerializeField] private int density;
    [SerializeField] private TowerVisualizer towerVisualizer;
    [SerializeField] private Controller controller;

    private static Tower[,] map;

    public static Tower MapIn(int x, int y) => map[x, y];
    public static bool GetSimmetric() => instance.symmetric;
    public static bool GetSnapHorizontal() => instance.snapHorizontal;
    public static int GetMaxLevel() => instance.maxLevel;
    public static int GetBaseResourceIncome() => instance.baseResourceIncome;
    public static int GetExpensiveTowerChance() => instance.expensiveTowerChance;
    public static int GetNullTowerChance() => instance.nullTowerChance;
    public static int GetExpensiveTowerAddition() => instance.expensiveTowerAddition;
    public static int GetDensity() => instance.density;
    public static int GetDiagonalChance() => instance.diagonalChance;
    public static Vector2Int GetSize() => instance.size;

    private void Start()
    {
        /*playersCount = MenuConfigurator.playersCount;
        minStartLevel = MenuConfigurator.minStartLevel;
        startLevelHandicap = MenuConfigurator.startLevelHandicap;
        maxLevel = MenuConfigurator.maxLevel;
        size = MenuConfigurator.size;
        seed = MenuConfigurator.seed;
        frequency = MenuConfigurator.frequency;
        diagonalChance = MenuConfigurator.diagonalChance;
        baseResourceIncome = MenuConfigurator.baseResourceIncome;
        expensiveTowerChance = MenuConfigurator.expensiveTowerChance;
        expensiveTowerAddition = MenuConfigurator.expensiveTowerAddition;
        simmetric = MenuConfigurator.simmetric;
        snapHorizontal = MenuConfigurator.snapHorizontal;*/

        LoadAll();
        
        instance = this;

        if (size.x % 2 != 0 || size.y % 2 != 0) throw new ArgumentException();

        if (playersCount == 4) symmetric = true;

        Players = new Player[playersCount];

        UnityEngine.Random.InitState(seed);

        map = new Tower[size.x, size.y];
        int xBorder = (size.x % 2 == 0) ? (size.x / 2) : ((size.x + 1) / 2);
        int yBorder = 0;
        if (symmetric) yBorder = (size.y % 2 == 0) ? (size.y / 2) : ((size.y + 1) / 2);
        else yBorder = size.y;

        Debug.Log(xBorder + " " + yBorder);

        Tower[] startTowers = new Tower[playersCount];
        int startTowerX = 0; int startTowerY = 0;

        for (int x = 0; x < xBorder; x++)
        {
            for (int y = 0; y < yBorder; y++)
            {
                if (UnityEngine.Random.Range(1, MaxFrequency) >= MaxFrequency - frequency)
                {
                    SpawnTower(new Vector2Int(x, y), new Vector2Int(x, y));

                    if (startTowers[0] == null)
                    {
                        startTowers[0] = map[x, y];
                        startTowerX = x; startTowerY = y;
                    }
                }
            }
        }

        if (symmetric)
        {
            for (int x = 0; x < xBorder; x++)
            {
                for (int y = yBorder; y < size.y; y++)
                {
                    if (map[x, size.y - 1 - y] != null)
                    {
                        SpawnTower(new Vector2Int(x, y), new Vector2Int(x, size.y - 1 - y));
                    }
                }
            }
        }

        for (int x = xBorder; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (map[size.x - 1 - x, snapHorizontal ? y : (size.y - 1 - y)] != null) SpawnTower(new Vector2Int(x, y), 
                    new Vector2Int(size.x - 1 - x, snapHorizontal ? y : (size.y - 1 - y)));
            }
        }

        if (Players.Length == 2)
        {
            resourcesTexts[1].gameObject.SetActive(false);
            resourcesTexts[3].gameObject.SetActive(false);
            if (snapHorizontal) startTowers[1] = map[size.x - 1 - startTowerX, startTowerY];
            else startTowers[1] = map[size.x - 1 - startTowerX, size.y - 1 - startTowerY];
        }
        else if (Players.Length == 4)
        {
            startTowers[1] = map[size.x - 1 - startTowerX, size.y - 1 - startTowerY];
            startTowers[2] = map[size.x - 1 - startTowerX, startTowerY];
            startTowers[3] = map[startTowerX, size.y - 1 - startTowerY];
        }

        int StartLevel(int index)
        {
            if (playersCount == 2)
            {
                if (index == 0) return minStartLevel;
                else return minStartLevel + startLevelHandicap;
            }
            else
            {
                if (index <= 1) return minStartLevel;
                else return minStartLevel + startLevelHandicap;
            }
        }

        for (int i = 0; i < Players.Length; i++)
        {
            Players[i] = new Player(startTowers[i], playerColors[i], i, StartLevel(i), resourcesTexts[(i == 1 && Players.Length == 2) ? 2: i]);
        }

        MakeConnections(xBorder, yBorder);

        controller.transform.position = new Vector3(transform.position.x + size.x * (density - 1),
            controller.transform.position.y, transform.position.z + size.y * (density - 1) - 5);
        controller.DelayedStart();
    }

    private void MakeConnections(int xBorder, int yBorder)
    {
        for (int x = 0; x < xBorder; x++)
        {
            for (int y = 0; y < yBorder; y++)
            {
                if (map[x, y] != null) map[x, y].MakeConnections();
            }
        }

        if (symmetric)
        {
            for (int x = 0; x < xBorder; x++)
            {
                for (int y = yBorder; y < size.y; y++)
                {
                    if (map[x, y] != null) map[x, y].MakeConnections();
                }
            }
        }

        for (int x = xBorder; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (map[x, y] != null) map[x, y].MakeConnections();
            }
        }
    }

    private void LoadAll()
    {
        // 0) PlayersCount
        playersCount = Convert.ToInt32(MenuConfigurator.LoadValue(0));
        // 1) MinStartLevel
        minStartLevel = Convert.ToInt32(MenuConfigurator.LoadValue(1));
        // 2) StartLevelHandicap
        startLevelHandicap = Convert.ToInt32(MenuConfigurator.LoadValue(2));
        // 3) MaxLevel
        maxLevel = Convert.ToInt32(MenuConfigurator.LoadValue(3));
        // 4) Symmetric
        symmetric = Convert.ToInt32(MenuConfigurator.LoadValue(4)) == 1;
        // 5) SnapHorizontal
        snapHorizontal = Convert.ToInt32(MenuConfigurator.LoadValue(5)) == 1;
        // 6) Seed
        seed = Convert.ToInt32(MenuConfigurator.LoadValue(6));
        // 7) SizeX
        // 8) SizeY
        size = new Vector2Int(Convert.ToInt32(MenuConfigurator.LoadValue(7)), Convert.ToInt32(MenuConfigurator.LoadValue(8)));
        // 9) Frequency
        frequency = Convert.ToInt32(MenuConfigurator.LoadValue(9));
        // 10) DiagonalChance
        diagonalChance = Convert.ToInt32(MenuConfigurator.LoadValue(10));
        // 11) BaseResourceIncome
        baseResourceIncome = Convert.ToInt32(MenuConfigurator.LoadValue(11));
        // 12) ExpensiveTowerChance
        expensiveTowerChance = Convert.ToInt32(MenuConfigurator.LoadValue(12));
        // 13) ExpensiveTowerAddition
        expensiveTowerAddition = Convert.ToInt32(MenuConfigurator.LoadValue(13));
        // 14) NullTowerChance
        nullTowerChance = Convert.ToInt32(MenuConfigurator.LoadValue(14));
    }

    private void SpawnTower(Vector2Int point, Vector2Int representationPoint)
    {
        map[point.x, point.y] = new Tower(point, 
            Instantiate(towerVisualizer.gameObject, (transform.position + new Vector3(point.x, 0, point.y)) * density,
            Quaternion.identity, transform).GetComponent<TowerVisualizer>(), map[representationPoint.x, representationPoint.y]);
    }
}