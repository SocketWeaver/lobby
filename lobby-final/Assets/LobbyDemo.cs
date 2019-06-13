using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SWNetwork;

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

    void Start()
    {
        // Subscribe to Lobby events
        NetworkClient.Lobby.OnNewPlayerJoinRoomEvent += Lobby_OnNewPlayerJoinRoomEvent;
        NetworkClient.Lobby.OnPlayerLeaveRoomEvent += Lobby_OnPlayerLeaveRoomEvent;
        NetworkClient.Lobby.OnRoomCustomDataChangeEvent += Lobby_OnRoomCustomDataChangeEvent;

        NetworkClient.Lobby.OnRoomMessageEvent += Lobby_OnRoomMessageEvent;
        NetworkClient.Lobby.OnPlayerMessageEvent += Lobby_OnPlayerMessageEvent;

        NetworkClient.Lobby.OnLobbyConncetedEvent += Lobby_OnLobbyConncetedEvent;
    }

    void onDestroy()
    {
        // Unsubscrible to Lobby events
        NetworkClient.Lobby.OnNewPlayerJoinRoomEvent -= Lobby_OnNewPlayerJoinRoomEvent;
        NetworkClient.Lobby.OnPlayerLeaveRoomEvent -= Lobby_OnPlayerLeaveRoomEvent;
        NetworkClient.Lobby.OnRoomCustomDataChangeEvent -= Lobby_OnRoomCustomDataChangeEvent;

        NetworkClient.Lobby.OnRoomMessageEvent -= Lobby_OnRoomMessageEvent;
        NetworkClient.Lobby.OnPlayerMessageEvent -= Lobby_OnPlayerMessageEvent;

        NetworkClient.Lobby.OnLobbyConncetedEvent -= Lobby_OnLobbyConncetedEvent;
    }

    public void RegisterPlayer()
    {
        GUI.ShowRegisterPlayerPopup((bool ok, string playerName) =>
        {
            if (ok)
            {
                // store the playerName
                // playerName also used to register local player to the lobby server
                playerName_ = playerName;
                NetworkClient.Instance.CheckIn(playerName, (bool successful, string error) =>
                {
                    if (!successful)
                    {
                        Debug.LogError(error);
                    }
                });
            }
        });
    }

    public void GetPlayersInCurrentRoom()
    {
        NetworkClient.Lobby.GetPlayersInRoom((successful, reply, error) =>
        {
            if (successful)
            {
                Debug.Log("Got players " + reply);

                // store the playerIds and player names in a dictionary.
                // The dictionary is later used to populate the player list.
                playersDict_ = new Dictionary<string, string>();
                foreach (SWPlayer player in reply.players)
                {
                    playersDict_[player.id] = player.GetCustomDataString();
                }

                // fetch the room custom data.
                GetRoomCustomData();
            }
            else
            {
                Debug.Log("Failed to get players " + error);
            }
        });
    }

    public void GetRoomCustomData()
    {
        NetworkClient.Lobby.GetRoomCustomData((successful, reply, error) =>
        {
            if (successful)
            {
                Debug.Log("Got room custom data " + reply);

                // Deserialize the room custom data.
                roomData_ = reply.GetCustomData<RoomCustomData>();
                if (roomData_ != null)
                {
                    RefreshPlayerList();
                }
            }
            else
            {
                Debug.Log("Failed to get room custom data " + error);
            }
        });
    }

    public void CreateNewRoom()
    {
        GUI.ShowNewGamePopup((bool ok, string gameName) =>
        {
            if (ok)
            {
                roomData_ = new RoomCustomData();
                roomData_.name = gameName;
                roomData_.team1 = new TeamCustomData();
                roomData_.team2 = new TeamCustomData();
                roomData_.team1.players.Add(NetworkClient.Lobby.PlayerId);

                // use the serializable roomData_ object as room's custom data.
                NetworkClient.Lobby.CreateRoom(roomData_, true, 4, (successful, reply, error) =>
                {
                    if (successful)
                    {
                        Debug.Log("Room created " + reply);
                        // refresh the room list
                        GetRooms();

                        // refresh the player list
                        GetPlayersInCurrentRoom();
                    }
                    else
                    {
                        Debug.Log("Failed to create room " + error);
                    }
                });
            }
        });
    }

    public void SendRoomMessage(string message)
    {
        Debug.Log("Send room message " + message);
        NetworkClient.Lobby.MessageRoom(message, (bool successful, SWLobbyError error) =>
        {
            if (successful)
            {
                Debug.Log("Sent room message");
                string msg = "Sent to room: " + message;
                GUI.AddRowForMessage(msg, null, null);
            }
            else
            {
                Debug.Log("Failed to send room messagem " + error);
            }
        });
    }

    public void OnRoomSelected(string roomId)
    {
        Debug.Log("OnRoomSelected: " + roomId);
        // Join the selected room
        NetworkClient.Lobby.JoinRoom(roomId, (successful, reply, error) =>
        {
            if (successful)
            {
                Debug.Log("Joined room " + reply);
                // refresh the player list
                GetPlayersInCurrentRoom();
            }
            else
            {
                Debug.Log("Failed to Join room " + error);
            }
        });
    }

    public void OnPlayerSelected(string playerId)
    {
        Debug.Log("OnPlayerSelected: " + playerId);

        // demonstrate player message API
        GUI.ShowMessagePlayerPopup(playerId, (bool ok, string targetPlayerId, string message) =>
        {
            if (ok)
            {
                Debug.Log("Send player message " + "playerId= " + targetPlayerId + " message= " + message);
                NetworkClient.Lobby.MessagePlayer(playerId, message, (bool successful, SWLobbyError error) =>
                {
                    if (successful)
                    {
                        Debug.Log("Sent player message");
                        string msg = "Sent to " + targetPlayerId + ": " + message;
                        GUI.AddRowForMessage(msg, null, null);
                    }
                    else
                    {
                        Debug.Log("Failed to send player messagem " + error);
                    }
                });
            }
        });
    }

    public void GetRooms()
    {
        // Get the rooms for the current page.
        NetworkClient.Lobby.GetRooms(currentRoomPageIndex_, 6, (successful, reply, error) =>
        {
            if (successful)
            {
                Debug.Log("Got rooms " + reply);

                // Remove rooms in the rooms list
                GUI.ClearRoomList();

                foreach (SWRoom room in reply.rooms)
                {
                    Debug.Log(room);
                    // Deserialize the room custom data.
                    RoomCustomData rData = room.GetCustomData<RoomCustomData>();
                    if (rData != null)
                    {
                        // Add rooms to the rooms list.
                        GUI.AddRowForRoom(rData.name, room.id, OnRoomSelected);
                    }
                }
            }
            else
            {
                Debug.Log("Failed to get rooms " + error);
            }
        });
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
        NetworkClient.Lobby.LeaveRoom((successful, error) =>
        {
            if (successful)
            {
                Debug.Log("Left room");
                GUI.ClearPlayerList();
                GetRooms();
            }
            else
            {
                Debug.Log("Failed to leave room " + error);
            }
        });
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

    // lobby delegate events
    void Lobby_OnLobbyConncetedEvent()
    {
        Debug.Log("Lobby_OnLobbyConncetedEvent");
        // Register the player using the entered player name.
        NetworkClient.Lobby.Register(playerName_, (successful, reply, error) =>
        {
            if (successful)
            {
                Debug.Log("Lobby registered " + reply);
                if (reply.started)
                {
                    // Player is in a room and the room has started.
                    // Call NetworkClient.Instance.ConnectToRoom to connect to the game servers of the room.
                }
                else if (reply.roomId != null)
                {
                    // Player is in a room.
                    GetRooms();
                    GetPlayersInCurrentRoom();
                }
                else
                {
                    // Player is not in a room.
                    GetRooms();
                }
            }
            else
            {
                Debug.Log("Lobby failed to register " + error);
            }
        });
    }


    void Lobby_OnNewPlayerJoinRoomEvent(SWJoinRoomEventData eventData)
    {
        Debug.Log("Player joined room");
        Debug.Log(eventData);

        // Store the new playerId and player name pair
        playersDict_[eventData.newPlayerId] = eventData.GetString();

        if (NetworkClient.Lobby.IsOwner)
        {
            // Find the smaller team and assign the new player to it.
            if (roomData_.team1.players.Count < roomData_.team2.players.Count)
            {
                roomData_.team1.players.Add(eventData.newPlayerId);
            }
            else
            {
                roomData_.team2.players.Add(eventData.newPlayerId);
            }

            // Update the room custom data
            NetworkClient.Lobby.ChangeRoomCustomData(roomData_, (bool successful, SWLobbyError error) =>
            {
                if (successful)
                {
                    Debug.Log("ChangeRoomCustomData successful");
                    RefreshPlayerList();
                }
                else
                {
                    Debug.Log("ChangeRoomCustomData failed: " + error);
                }
            });
        }
    }

    void Lobby_OnPlayerLeaveRoomEvent(SWLeaveRoomEventData eventData)
    {
        Debug.Log("Player left room: " + eventData);

        if (NetworkClient.Lobby.IsOwner)
        {
            // Remove the players from both team.
            roomData_.team2.players.RemoveAll(eventData.leavePlayerIds.Contains);
            roomData_.team1.players.RemoveAll(eventData.leavePlayerIds.Contains);

            // Update the room custom data
            NetworkClient.Lobby.ChangeRoomCustomData(roomData_, (bool successful, SWLobbyError error) =>
            {
                if (successful)
                {
                    Debug.Log("ChangeRoomCustomData successful");
                    RefreshPlayerList();
                }
                else
                {
                    Debug.Log("ChangeRoomCustomData failed: " + error);
                }
            });
        }
    }

    void Lobby_OnRoomCustomDataChangeEvent(SWRoomCustomDataChangeEventData eventData)
    {
        Debug.Log("Room custom data changed: " + eventData);

        SWRoom room = NetworkClient.Lobby.RoomData;
        roomData_ = room.GetCustomData<RoomCustomData>();

        // Room custom data changed, refresh the player list.
        RefreshPlayerList();
    }

    void Lobby_OnRoomMessageEvent(SWMessageRoomEventData eventData)
    {
        string msg = "Room message: " + eventData.data;
        GUI.AddRowForMessage(msg, null, null);
    }

    void Lobby_OnPlayerMessageEvent(SWMessagePlayerEventData eventData)
    {
        string msg = eventData.playerId + ": " + eventData.data;
        GUI.AddRowForMessage(msg, null, null);
    }
}
