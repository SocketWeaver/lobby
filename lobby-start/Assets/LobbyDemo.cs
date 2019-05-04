using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LobbyDemo : MonoBehaviour
{
    public LobbyGUI GUI;

    /// <summary>
    /// Used to display players in different teams.
    /// </summary>
    Dictionary<string, string> playersDict_;

    /// <summary>
    /// Current room's custom data.
    /// </summary>
    RoomCustomData roomData_;

    /// <summary>
    /// Current page index of the room list. 
    /// </summary>
    int currentRoomPageIndex_ = 0;

    /// <summary>
    /// Player entered name
    /// </summary>
    string playerName_;

    public void RegisterPlayer()
    {
        GUI.ShowRegisterPlayerPopup((bool ok, string playerName) =>
        {
            if (ok)
            {
                Debug.Log("Register player: " + playerName);
            }
        });
    }

    public void CreateNewRoom()
    {
        GUI.ShowNewGamePopup((bool ok, string gameName) =>
        {
            if (ok)
            {
                Debug.Log("Create new room: " + gameName);
            }
        });
    }

    public void SendRoomMessage(string message)
    {
        Debug.Log("Send room message " + message);
    }

    public void OnRoomSelected(string roomId)
    {
        Debug.Log("OnRoomSelected: " + roomId);
    }

    public void OnPlayerSelected(string playerId)
    {
        Debug.Log("OnPlayerSelected: " + playerId);
    }

    public void GetRooms()
    {
        Debug.Log("GetRooms");
    }

    public void NextPage()
    {
        currentRoomPageIndex_++;
        GetRooms();
    }

    public void PreviousPage()
    {
        currentRoomPageIndex_--;
        GetRooms();
    }

    public void LeaveRoom()
    {
        Debug.Log("LeaveRoom");
    }

    void RefreshPlayerList()
    {
        // Use the room custom data, and the playerId and player Name dictionary to populate the player lsit
        if (playersDict_ != null)
        {
            GUI.ClearPlayerList();
            GUI.AddRowForTeam("Team 1");
            foreach (string pId in roomData_.team1.players)
            {
                String playerName = playersDict_[pId];
                GUI.AddRowForPlayer(playerName, pId, OnPlayerSelected);
            }

            GUI.AddRowForTeam("Team 2");
            foreach (string pId in roomData_.team2.players)
            {
                String playerName = playersDict_[pId];
                GUI.AddRowForPlayer(playerName, pId, OnPlayerSelected);
            }
        }
    }
}
