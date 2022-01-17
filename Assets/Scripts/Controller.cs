using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Controller : MonoBehaviour
{
    [SerializeField] private LayerMask whatIsTower;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private Text infoText;

    private Tower selectedTower;
    private bool resourceDistribution = false;
    private bool gameEnded = false;
    private bool gameStarted = false;
    private int turn = 0;
    private Camera camera;

    private void Start()
    {
        camera = GetComponent<Camera>();
    }

    public void DelayedStart()
    {
        gameStarted = true;
        UpdateInfoText();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.A)) transform.position = new Vector3(transform.position.x - movementSpeed, transform.position.y, transform.position.z);
        if (Input.GetKey(KeyCode.D)) transform.position = new Vector3(transform.position.x + movementSpeed, transform.position.y, transform.position.z);
        if (Input.GetKey(KeyCode.S)) transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - movementSpeed);
        if (Input.GetKey(KeyCode.W)) transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + movementSpeed);
        if (Input.GetKey(KeyCode.E) && camera.orthographicSize - zoomSpeed > zoomSpeed) camera.orthographicSize -= zoomSpeed;
        if (Input.GetKey(KeyCode.Q)) camera.orthographicSize += zoomSpeed;
        if (Input.GetKeyDown(KeyCode.Escape)) GoToMenu();

        // if (Input.GetKey(KeyCode.E)) transform.position = new Vector3(transform.position.x, transform.position.y + movementSpeed, transform.position.z);
        // if (Input.GetKey(KeyCode.Q)) transform.position = new Vector3(transform.position.x, transform.position.y - movementSpeed, transform.position.z);

        if (!gameEnded && gameStarted)
        {
            bool CheckIfHitsTower(out RaycastHit hit)
            {
                return Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit, whatIsTower);
            }

            if (!resourceDistribution)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    StartDistribution();
                }

                if (Input.GetMouseButtonDown(0))
                {
                    if (CheckIfHitsTower(out RaycastHit hit))
                    {
                        SelectTower(hit.transform.GetComponent<TowerVisualizer>().GetTower());
                    }
                }
                else if (Input.GetMouseButtonDown(1) && selectedTower != null)
                {
                    if (CheckIfHitsTower(out RaycastHit hit))
                    {
                        GoToTower(hit.transform.GetComponent<TowerVisualizer>().GetTower());
                    }
                }
            }
            else
            {
                if (Map.Players[turn].resources <= 0 || Input.GetKeyDown(KeyCode.Space))
                {
                    NextTurn();
                }

                if (Input.GetMouseButtonDown(0))
                {
                    if (CheckIfHitsTower(out RaycastHit hit))
                    {
                        GiveResource(hit.transform.GetComponent<TowerVisualizer>().GetTower(), ResourcesNeeded.one);
                    }
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    if (CheckIfHitsTower(out RaycastHit hit))
                    {
                        GiveResource(hit.transform.GetComponent<TowerVisualizer>().GetTower(), ResourcesNeeded.max);
                    }
                }
            }
        }
    }

    private void GoToTower(Tower preSelectedTower)
    {
        if (selectedTower != preSelectedTower)
        {
            if (preSelectedTower.GetOwner() is null || preSelectedTower.GetOwner().index != turn)
            {
                if (selectedTower.IsConnected(preSelectedTower))
                {
                    selectedTower.GoTo(preSelectedTower, out bool captured);

                    selectedTower.GetTowerVisualizer().SetVisual(VisualType.None);
                    selectedTower = null;
                    
                    if (captured && preSelectedTower.GetLevel() > 1)
                    {
                        selectedTower = preSelectedTower;
                        selectedTower.GetTowerVisualizer().SetVisual(VisualType.Selected);
                    }
                        
                    CheckForMoves();
                }
            }
        }
    }

    private void SelectTower(Tower attendedToSelectTower)
    {
        if (!(attendedToSelectTower is null || attendedToSelectTower.GetOwner() is null ||
              attendedToSelectTower.GetOwner().index != turn || attendedToSelectTower.GetLevel() <= 1))
        {
            if (selectedTower != null) selectedTower.GetTowerVisualizer().SetVisual(VisualType.None);
            selectedTower = attendedToSelectTower;
            selectedTower.GetTowerVisualizer().SetVisual(VisualType.Selected);
        }
    }

    private enum ResourcesNeeded
    {
        one,
        max
    }
    
    private void GiveResource(Tower tower, ResourcesNeeded resourcesNeeded)
    {
        if (tower.GetLevel() < Map.GetMaxLevel())
        {
            if (tower.GetOwner() != null && tower.GetOwner().index == turn)
            {
                if (Map.Players[turn].resources >= 1)
                {
                    int resourcesToGive = resourcesNeeded == ResourcesNeeded.one ? 1 : Map.GetMaxLevel() - tower.GetLevel();

                    if (resourcesToGive > Map.Players[turn].resources) resourcesToGive = Map.Players[turn].resources;

                    tower.SetLevel(tower.GetLevel() + resourcesToGive);
                    Map.Players[turn].SetResource(Map.Players[turn].GetResources() - resourcesToGive);
                }
            }
        }
    }

    private void UpdateInfoText()
    {
        infoText.text = "Player " + (turn + 1) + (resourceDistribution ? " distributes resources" : " moves");
        infoText.color = Map.Players[turn].GetColor();
    }

    private void CheckForMoves()
    {
        if (!Map.Players[turn].CanMove()) StartDistribution();
    }

    private void StartDistribution()
    {
        if (selectedTower != null) selectedTower.GetTowerVisualizer().SetVisual(VisualType.None);
        selectedTower = null;
        Debug.Log("Resource distribution starts");
        resourceDistribution = true;
        Map.Players[turn].GainResource();
        UpdateInfoText();
    }

    private void NextTurn()
    {
        if (selectedTower != null) selectedTower.GetTowerVisualizer().SetVisual(VisualType.None);
        selectedTower = null;
        resourceDistribution = false;
        turn += 1;
        if (turn >= Map.Players.Length) turn = 0;
        Debug.Log((turn + 1) + " player turn");
        CheckForMoves();
        UpdateInfoText();
    }

    private void GoToMenu()
    {
        SceneManager.LoadScene(0);
    }
}