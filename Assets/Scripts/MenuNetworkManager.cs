using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MenuNetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private InputField nicknameInput;
    [SerializeField] private InputField roomNameInput;
    [SerializeField] private Button multiplayerButton;
    [SerializeField] private Button host;
    [SerializeField] private Button join;
    [SerializeField] private MenuConfigurator menuConfigurator;

    private string path;
    
    private void Start()
    {
        Directory.CreateDirectory(Application.streamingAssetsPath + "/Network/");

        path = Application.streamingAssetsPath + "/Network/" + "nickname" + ".txt";
        
        if (!File.Exists(path))
        {
            File.WriteAllText(path, "");
        }

        multiplayerButton.interactable = false;

        if (SaveLoader.LoadValueString(path, 0) == "")
        {
            nicknameInput.text = "Player" + Random.Range(1000, 9999);
            UpdateNickname();
        }
        else 
        {
            nicknameInput.text = SaveLoader.LoadValueString(path, 0);
            PhotonNetwork.NickName = nicknameInput.text;
        }
        Debug.Log(SaveLoader.LoadValueString(path, 0));

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "1";

        PhotonNetwork.LogLevel = PunLogLevel.Full;
        
        menuConfigurator.SetLocal();

        if (PhotonNetwork.IsConnected) multiplayerButton.interactable = true;
        else PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        multiplayerButton.interactable = false;
        menuConfigurator.SetLocal();
    }
    
    public override void OnConnected()
    {
        multiplayerButton.interactable = true;
    }

    public void CreateRoom()
    {
        SetInteractable(false);
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = Convert.ToByte(SaveLoader.LoadValueNumber(MenuConfigurator.GetPath(), 0))
        };
        if (roomNameInput.text == "") roomNameInput.text = Random.Range(100000, 999999).ToString();
        PhotonNetwork.CreateRoom(roomNameInput.text, roomOptions);
    }

    public void JoinRoom()
    {
        // PhotonNetwork.JoinRandomRoom();
        if (roomNameInput.text != "")
        {
            SetInteractable(false);
            Debug.Log(roomNameInput.text);
            Debug.Log(PhotonNetwork.CountOfRooms);
            PhotonNetwork.JoinRoom(roomNameInput.text);
        }
    }

    private void SetInteractable(bool to)
    {
        host.interactable = to;
        join.interactable = to;
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel(2);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log(message);
        SetInteractable(true);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log(message);
        SetInteractable(true);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("ConnectedToMaster");
        multiplayerButton.interactable = true;
        PhotonNetwork.JoinLobby();
    }
    
    public void UpdateNickname()
    {
        PhotonNetwork.NickName = nicknameInput.text;
        SaveLoader.SaveValue(path, 0, nicknameInput.text);
    }
}