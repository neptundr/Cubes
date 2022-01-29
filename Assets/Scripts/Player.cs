using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Player
{
    [NonSerialized] public int index;
    [NonSerialized] public int resources;
    
    private List<Tower> myTowers;
    private Text resourcesText;
    private Color color;

    public Player(Tower startTower, Color playerColor, int playerIndex, int startLevel, Text text)
    {
        myTowers = new List<Tower>();
        color = playerColor;
        index = playerIndex;
        resourcesText = text;
        resourcesText.color = color;
        startTower.ChangeOwner(this);
        startTower.SetLevel(startLevel);
    }

    public int GetMyTowersCount()
    {
        return myTowers.Count;
    }

    public void AddTower(Tower tower)
    {
        myTowers.Add(tower);
        UpdateText();
    }
    public void RemoveTower(Tower tower)
    {
        myTowers.Remove(tower);
        UpdateText();
    }
    
    public bool CanMove()
    {
        foreach (Tower tower in myTowers)
        {
            if (tower.GetLevel() > 1)
            {
                foreach (Tower connectedTower in tower.GetConnectedTower())
                {
                    if (connectedTower.GetOwner() != this) return true;
                }
            }
        }

        return false;
    }

    public void GainResource()
    {
        Tower.GiveResourceToPlayer.Invoke(this);
        UpdateText();
    }

    public int GetResources()
    {
        return resources;
    }

    public void SetResource(int to)
    {
        resources = to;
        UpdateText();
    }

    public Color GetColor()
    {
        return color;
    }

    private void UpdateText()
    {
        resourcesText.text = resources.ToString() + " (+" + ResourceIncome() + ")";
    }

    private int ResourceIncome()
    {
        return myTowers.Sum(tower => tower.GetResourceIncome());
    }
}