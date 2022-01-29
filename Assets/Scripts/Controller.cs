using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Controller : MonoBehaviour, IPunObservable, IOnEventCallback
{
    [SerializeField] private LayerMask whatIsTower;
    [SerializeField] private Text infoText;
    [SerializeField] private Camera camera;

    private Tower selectedTower;
    private bool resourceDistribution = false;
    private bool gameEnded = false;
    private bool gameStarted = false;
    private int turn = 0;
    private int sentFrom;
    private PhotonView photonView;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        Invoke(nameof(DelayedStart), 1);
    }

    public void DelayedStart()
    {
        gameStarted = true;
        UpdateInfoText();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && Map.GetIsLocal()) GoToMenu();

        // if (Input.GetKey(KeyCode.E)) transform.position = new Vector3(transform.position.x, transform.position.y + movementSpeed, transform.position.z);
        // if (Input.GetKey(KeyCode.Q)) transform.position = new Vector3(transform.position.x, transform.position.y - movementSpeed, transform.position.z);

        if (Map.GetIsLocal() || (GameNetworkManager.GetPlayerIndex() == turn && Map.GetPlayersCount() == GameNetworkManager.GetPlayersInRoom().Length))
        {
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
                        NetworkStartDistribution();
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
                        NetworkNextTurn();
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
    }

    private void GoToTower(Tower preSelectedTower)
    {
        if (selectedTower != preSelectedTower)
        {
            if (preSelectedTower.GetOwner() is null || preSelectedTower.GetOwner().index != turn)
            {
                if (selectedTower.IsConnected(preSelectedTower))
                {
                    if (GameNetworkManager.GetPlayerIndex() == turn)
                    {
                        RaiseEventOptions raiseEventOptions = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };
                        SendOptions sendOptions = new SendOptions() {Reliability = true};
                        PhotonNetwork.RaiseEvent(0, new Vector2Int[]{ selectedTower.GetPosition(), preSelectedTower.GetPosition()},
                            raiseEventOptions, sendOptions);
                    }

                    selectedTower.GoTo(preSelectedTower, out bool captured);

                    selectedTower.GetTowerVisualizer().SetVisual(VisualType.None);
                    selectedTower = null;
                    
                    if (captured && preSelectedTower.GetLevel() > 1)
                    {
                        selectedTower = preSelectedTower;
                        selectedTower.GetTowerVisualizer().SetVisual(VisualType.Selected);
                        CheckForWin();
                    }
                        
                    CheckForMoves();
                }
            }
        }
    }

    private void CheckForWin()
    {
        int playersDefeated = 0;
        foreach (Player player in Map.Players)
        {
            if (player.GetMyTowersCount() == 0) playersDefeated += 1;
        }

        if (playersDefeated == Map.Players.Length - 1)
        {
            gameEnded = true;
            infoText.text = ((Map.GetIsLocal() ? "Player " + (turn + 1) : GameNetworkManager.GetPlayersInRoom()[turn].NickName)) + " wins!";
            infoText.color = Map.Players[turn].GetColor();
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
                    
                    if (GameNetworkManager.GetPlayerIndex() == turn)
                    {
                        RaiseEventOptions raiseEventOptions = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };
                        SendOptions sendOptions = new SendOptions() { Reliability = true };
                        PhotonNetwork.RaiseEvent(1, new Vector2Int[]{ tower.GetPosition(), 
                                new Vector2Int(resourcesToGive, resourcesToGive)}, raiseEventOptions, sendOptions);
                    }
                    
                    tower.SetLevel(tower.GetLevel() + resourcesToGive);
                    Map.Players[turn].SetResource(Map.Players[turn].GetResources() - resourcesToGive);
                }
            }
        }
    }

    private void CheckForMoves()
    {
        if (!Map.Players[turn].CanMove()) NetworkStartDistribution();
    }

    private void NetworkStartDistribution()
    {
        if (!Map.GetIsLocal())
        {
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions() {Receivers = ReceiverGroup.Others};
            SendOptions sendOptions = new SendOptions() { Reliability = true };
            PhotonNetwork.RaiseEvent(3, null, raiseEventOptions, sendOptions);
        }
        StartDistribution();
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

    private void NetworkNextTurn()
    {
        if (!Map.GetIsLocal())
        {
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions() {Receivers = ReceiverGroup.Others};
            SendOptions sendOptions = new SendOptions() { Reliability = true };
            PhotonNetwork.RaiseEvent(4, null, raiseEventOptions, sendOptions);
        }
        NextTurn();
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

    public void UpdateInfoText()
    {
        if (Map.GetIsLocal() || Map.GetPlayersCount() == GameNetworkManager.GetPlayersInRoom().Length)
        {
            infoText.text = (Map.GetIsLocal() ? "Player " + (turn + 1) : GameNetworkManager.GetPlayersInRoom()[turn].NickName) +
                            (resourceDistribution ? " distributes resources" : " moves");
            infoText.color = Map.Players[turn].GetColor();
        }
        else
        {
            infoText.text = "Waiting for players...";
        }
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // if (stream.IsWriting)
        // {
        //     stream.SendNext(turn);
        //     stream.SendNext(resourceDistribution);
        // }
        // else
        // {
        //     turn = (int) stream.ReceiveNext();
        //     resourceDistribution = (bool) stream.ReceiveNext();
        // }
    }

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case 0:
                Vector2Int[] data0 = (Vector2Int[]) photonEvent.CustomData;
                Map.MapIn(data0[0].x, data0[0].y).GoTo(Map.MapIn(data0[1].x, data0[1].y), out bool captured);
                Debug.Log(0 + " " + data0);
                break;
            case 1:
                Vector2Int[] data1 = (Vector2Int[]) photonEvent.CustomData;
                Map.MapIn(data1[0].x, data1[0].y).SetLevel(Map.MapIn(data1[0].x, data1[0].y).GetLevel() + data1[1].x);
                Map.Players[turn].SetResource(Map.Players[turn].GetResources() - data1[1].x);
                Debug.Log(1 + " " + data1[0] + " " + data1[0]);
                break;
            case 3:
                StartDistribution();
                Debug.Log(3);
                break;
            case 4:
                NextTurn();
                Debug.Log(4);
                break;
        }
    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }
    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
}