using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameNetworkManager : MonoBehaviourPunCallbacks
{
    private static int PlayerIndex;
    private static GameNetworkManager Instance;
    
    [SerializeField] private Text networkInfoText;
    [SerializeField] private Text[] playerNicknameTexts;
    [SerializeField] private Controller controller;

    private Photon.Realtime.Player[] playersInRoom;
    
    public static int GetPlayerIndex() => PlayerIndex;
    public static Photon.Realtime.Player[] GetPlayersInRoom() => Instance.playersInRoom;
    public static void UpdatePlayers() => Instance.UpdatePlayersInRoom();

    private void Start()
    {
        Instance = this;
        
        PlayerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        Debug.Log(PlayerIndex);

        PhotonPeer.RegisterType(typeof(Vector2Int), 0, SerializeVector2Int, DeserializeVector2Int);
        UpdatePlayersInRoom();
    }

    public override void OnConnected()
    {
        UpdatePlayersInRoom();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        UpdatePlayersInRoom();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        UpdatePlayersInRoom();
        Leave();
    }

    private void UpdatePlayersInRoom()
    {
        playersInRoom = PhotonNetwork.PlayerList;
        networkInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name;
        for (int i = 0; i < playersInRoom.Length; i++)
        {
            playerNicknameTexts[i].text = playersInRoom[i].NickName;
            playerNicknameTexts[i].color = Map.Players[i].GetColor();
        }

        controller.UpdateInfoText();
    }

    // private void UpdatePlayersText()
    // {
    //     foreach (var player in PhotonNetwork.CurrentRoom.Players)
    //     {
    //         playersText.text += player.Value.NickName + "\n";
    //     }
    // }

    public void Leave()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    public static object DeserializeVector2Int(byte[] data)
    {
        return new Vector2Int(BitConverter.ToInt32(data, 0), BitConverter.ToInt32(data, 4));
    }
    public static byte[] SerializeVector2Int(object obj)
    {
        Vector2Int vector2Int = (Vector2Int) obj;
        byte[] result = new byte[8];
        
        BitConverter.GetBytes(vector2Int.x).CopyTo(result, 0);
        BitConverter.GetBytes(vector2Int.y).CopyTo(result, 4);
        
        return result;
    }
}
