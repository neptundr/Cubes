using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class Tower
{
    public static UnityEvent<Player> GiveResourceToPlayer = new UnityEvent<Player>();

    private List<Tower> connectedTowers;
    private int level = 1;
    private bool isExpensive;
    private bool isNullTower;
    private Player owner;
    private Tower representationOf;
    private TowerVisualizer towerVisualizer;
    private Vector2Int position;
    private List<Diagonal> diagonals;

    public Tower(Vector2Int point, TowerVisualizer towerVisualizerToSet, Tower representation)
    {
        position = point;
        towerVisualizer = towerVisualizerToSet;
        representationOf = representation;
        if (representationOf is null)
        {
            if (Map.GetExpensiveTowerChance() != 0 && Random.Range(Map.GetExpensiveTowerChance(), 100) == Map.GetExpensiveTowerChance())
            {
                isExpensive = true;
            }
            else if (Map.GetNullTowerChance() != 0 && Random.Range(Map.GetNullTowerChance(), 100) == Map.GetNullTowerChance())
            {
                isNullTower = true;
                level = 0;
            }
        }
        else
        {
            isExpensive = representationOf.isExpensive;
            isNullTower = representationOf.isNullTower; if (isNullTower) level = 0;
        }
        towerVisualizer.Init(this);
        GiveResourceToPlayer.AddListener(GiveResource);
    }

    public Vector2Int GetPosition() => position;
    public bool IsExpensive() => isExpensive;
    public bool IsNullTower() => isNullTower;
    public List<Tower> GetConnectedTower() => connectedTowers;
    public TowerVisualizer GetTowerVisualizer() => towerVisualizer;

    public int GetResourceIncome()
    {
        if (isExpensive) return Map.GetBaseResourceIncome() + Map.GetExpensiveTowerAddition();
        if (isNullTower) return 0;
        return Map.GetBaseResourceIncome();
    }

    public void SetLevel(int to)
    {
        level = to;
        if (level <= 0 && !isNullTower) level = 1;
        if (isNullTower && owner == null) level = 0;
        towerVisualizer.UpdateLevel();
    }
    public int GetLevel() => level;

    public void ChangeOwner(Player to)
    {
        owner?.RemoveTower(this);

        owner = to;
        owner?.AddTower(this);
        
        towerVisualizer.SetColor();
    }

    public Player GetOwner()
    {
        return owner;
    }

    public void GoTo(Tower preSelectedTower, out bool captured)
    {
        int NullTowerAdd() => isNullTower && !preSelectedTower.isNullTower ? 1 : 0;
        
        if (level < preSelectedTower.level)
        {
            preSelectedTower.SetLevel(preSelectedTower.level - level + NullTowerAdd());
            captured = false;
        }
        else
        {
            preSelectedTower.ChangeOwner(owner);
            preSelectedTower.SetLevel(level - preSelectedTower.level + NullTowerAdd());
            preSelectedTower.GetTowerVisualizer().SetColor();
            
            captured = true;
        }
        if (isNullTower) ChangeOwner(null);
        SetLevel(1);
    }

    public bool IsConnected(Tower tower)
    {
        return connectedTowers.Contains(tower);
    }

    public void MakeConnections()
    {
        connectedTowers = new List<Tower>();
        
        MakeConnectionTo(new Vector2Int(1, 0), Map.GetSize().x - 1 - position.x);
        MakeConnectionTo(new Vector2Int(-1, 0), position.x);
        MakeConnectionTo(new Vector2Int(0, 1), Map.GetSize().y - 1 - position.y);
        MakeConnectionTo(new Vector2Int(0, -1), position.y);
        
        diagonals = new List<Diagonal>();
        if (Map.GetDiagonalChance() != 0)
        {
            if (representationOf == null)
            {
                Debug.Log("a");
                for (int i = 0; i < 4; i++)
                {
                    if (Random.Range(Map.GetDiagonalChance(), 100) == Map.GetDiagonalChance())
                    {
                        Debug.Log("b");
                        Vector2Int direction = Vector2Int.zero;
                        int maxIndex = 0;
                        switch (i)
                        {
                            case 0:
                                direction = new Vector2Int(1, 1);
                                maxIndex = Mathf.Min((Map.GetSize().x - 1 - position.x),
                                    (Map.GetSize().y - 1 - position.y));
                                break;
                            case 1:
                                direction = new Vector2Int(-1, 1);
                                maxIndex = Mathf.Min(position.x, (Map.GetSize().y - 1 - position.y));
                                break;
                            case 2:
                                direction = new Vector2Int(1, -1);
                                maxIndex = Mathf.Min((Map.GetSize().x - 1 - position.x), position.y);
                                break;
                            case 3:
                                direction = new Vector2Int(-1, -1);
                                maxIndex = Mathf.Min(position.x, position.y);
                                break;
                        }

                        int length = MakeConnectionTo(direction, maxIndex);

                        if (length != 0)
                        {
                            Diagonal diagonal = new Diagonal(direction, length);
                            diagonals.Add(diagonal);
                            towerVisualizer.DrawDiagonal(diagonal);
                        }
                    }
                }
            }
            else
            {
                if (representationOf.diagonals.Count != 0)
                {
                    Vector2Int directionMultiplier;
                    if (position.x == representationOf.position.x) directionMultiplier = new Vector2Int(1, -1);
                    else if (position.y == representationOf.position.y) directionMultiplier = new Vector2Int(-1, 1);
                    else directionMultiplier = new Vector2Int(-1, -1);
                    
                    for (int i = 0; i < representationOf.diagonals.Count; i++)
                    {
                        connectedTowers.Add(Map.MapIn(
                            position.x + (representationOf.diagonals[i].length *
                                          representationOf.diagonals[i].direction.x * directionMultiplier.x),
                            position.y + (representationOf.diagonals[i].length *
                                          representationOf.diagonals[i].direction.y * directionMultiplier.y)));

                        Diagonal diagonal = new Diagonal(representationOf.diagonals[i].direction * directionMultiplier,
                        representationOf.diagonals[i].length);
                        diagonals.Add(diagonal);
                        towerVisualizer.DrawDiagonal(diagonal);
                    }
                }
            }
        }
    }

    private int MakeConnectionTo(Vector2Int direction, int maxIndex)
    {
        for (int i = 1; i <= maxIndex; i++)
        {
            if (Map.MapIn(position.x + (i * direction.x), position.y + (i * direction.y)) != null)
            {
                connectedTowers.Add(Map.MapIn(position.x + (i * direction.x), position.y + (i * direction.y)));
                return i;
            }            
        }
        return 0;
    }

    private void GiveResource(Player player)
    {
        if (player == owner) player.resources += GetResourceIncome();
    }
}

public struct Diagonal
{
    public Vector2Int direction;
    // public int maxIndex;
    public int length;

    public Diagonal(Vector2Int direction, /*int maxIndex,*/ int length)
    {
        this.direction = direction;
        // this.maxIndex = maxIndex;
        this.length = length;
    }
}