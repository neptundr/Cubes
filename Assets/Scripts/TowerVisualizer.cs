using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerVisualizer : MonoBehaviour
{
    [SerializeField] private ParticleSystem goToMarker;
    [SerializeField] private GameObject selectedMarker;
    [SerializeField] private GameObject expensiveMarker;
    [SerializeField] private GameObject nullTowerMarker;
    [SerializeField] private GameObject teleportMarker;
    [SerializeField] private GameObject diagonal;
    [SerializeField] private Renderer model;
    [SerializeField] private Transform modelPivot;
    [SerializeField] private Text levelText;
    [SerializeField] private float minModelSize;
    [SerializeField] private float maxModelSize;
    
    private Tower tower;
    private Color standardColor;

    public void Init(Tower towerToSet)
    {
        tower = towerToSet;
        standardColor = model.material.color;

        selectedMarker.SetActive(false);
        goToMarker.gameObject.SetActive(false);
        expensiveMarker.SetActive(tower.IsExpensiveTower());
        nullTowerMarker.SetActive(tower.IsNullTower());
        teleportMarker.SetActive(tower.IsTeleportTower());
        
        UpdateLevel();
    }

    public void DrawDiagonal(Diagonal diagonalStruct)
    {
        for (int i = 0; i < diagonalStruct.length; i++)
        {
            Instantiate(diagonal,
                new Vector3(transform.position.x + i * diagonalStruct.direction.x * Map.GetDensity(),
                    transform.position.y - 0.5f, transform.position.z + i * diagonalStruct.direction.y * Map.GetDensity()),
                Quaternion.Euler(0, DirectionToAngle(diagonalStruct.direction), 0), transform);
        }
    }

    private int DirectionToAngle(Vector2Int direction)
    {
        if (direction.x == 1 && direction.y == 1) return 45;
        if (direction.x == 1 && direction.y == -1) return 135;
        if (direction.x == -1 && direction.y == -1) return -135;
        if (direction.x == -1 && direction.y == 1) return -45;

        throw new ArgumentException();
    }

    public Tower GetTower()
    {
        return tower;
    }

    public void SetColor()
    {
        if (tower.GetOwner() != null) model.material.color = tower.GetOwner().GetColor();
        else model.material.color = standardColor;
    }
    
    public void UpdateLevel()
    {
        if (tower.IsNullTower())
        {
            model.gameObject.SetActive(tower.GetOwner() != null);
        }

        modelPivot.localScale = new Vector3(transform.localScale.x, tower.GetLevel() * ((maxModelSize - minModelSize) / Map.GetMaxLevel()) 
                                                                    + minModelSize, transform.localScale.z);
        levelText.text = tower.GetLevel().ToString();
        levelText.gameObject.SetActive(tower.GetOwner() != null);
    }


    public void SetVisual(VisualType visualType)
    {
        SetVisual(visualType, 0);
    }
    
    public void SetVisual(VisualType visualType, int playerIndex)
    {
        switch (visualType)
        {
            case VisualType.Selected:
                Debug.Log("Selected " + tower.GetPosition());
                selectedMarker.SetActive(true);
                goToMarker.gameObject.SetActive(false);
                PreSelectConnectedTowers();
                break;
            case VisualType.GoTo:
                Debug.Log("GoTo " + tower.GetPosition());
                selectedMarker.SetActive(false);
                goToMarker.gameObject.SetActive(true);
                goToMarker.startColor = Map.Players[playerIndex].GetColor();
                break;
            case VisualType.None:
                Debug.Log("None " + tower.GetPosition());
                selectedMarker.SetActive(false);
                goToMarker.gameObject.SetActive(false);
                DePreSelectConnectedTowers();
                break;
            case VisualType.NotPreselected:
                Debug.Log("NotPreselected " + tower.GetPosition());
                selectedMarker.SetActive(false);
                goToMarker.gameObject.SetActive(false);
                break;
        }
    }
    
    public void PreSelectConnectedTowers()
    {
        foreach (Tower connectedTower in tower.GetConnectedTower())
        {
            if (connectedTower.GetOwner() != tower.GetOwner())
                connectedTower.GetTowerVisualizer().SetVisual(VisualType.GoTo, tower.GetOwner().index);
        }
    }
    public void DePreSelectConnectedTowers()
    {
        foreach (Tower connectedTower in tower.GetConnectedTower())
        {
            connectedTower.GetTowerVisualizer().SetVisual(VisualType.NotPreselected);
        }
    }
}

public enum VisualType
{
    Selected,
    GoTo,
    None,
    NotPreselected
}