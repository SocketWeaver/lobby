using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyGUI : MonoBehaviour
{
    public GameObject roomRowPrefab;
    public GameObject roomList;

    public GameObject playerRowPrefab;
    public GameObject teamHeaderRowPrefab;
    public GameObject playerList;

    public GameObject messageRowPrefab;
    public GameObject messageList;

    public InputField newRoomText;
    public GameObject newRoomPopup;

    public InputField playerNameText;
    public GameObject registerPlayerPopup;

    public InputField playerIdText;
    public InputField messageText;
    public GameObject messagePlayerPopup;

    /// <summary>
    /// The current message row count.
    /// </summary>
    int currentMessageRowCount = 0;
    int MAX_MESSAGE_ROW_COUNT = 5;

    /// <summary>
    /// Callbak to invoke when NewGamePopup is closed.
    /// </summary>
    Action<bool, string> newGamePopupCloseCallback;

    /// <summary>
    /// Callbak to invoke when RegisterPlayerPopup is closed.
    /// </summary>
    Action<bool, string> registerPlayerPopupCloseCallback;

    /// <summary>
    /// Callback to invoke when MessagePlayerPopup is closed.
    /// </summary>
    Action<bool, string, string> messagePlayerPopupCloseCallback;

    /// <summary>
    /// Remove all the rows in the Player list.
    /// </summary>
    public void ClearPlayerList()
    {
        RemoveAllChildren(playerList.transform);
    }

    /// <summary>
    /// Remove all the rows in the Room list.
    /// </summary>
    public void ClearRoomList()
    {
        RemoveAllChildren(roomList.transform);
    }

    /// <summary>
    /// Add a row to the Player list.
    /// </summary>
    public void AddRowForPlayer(string title, string objectId, TableRow.SelectedHandler callback)
    {
        AddRowToTable(playerList.transform, playerRowPrefab, title, objectId, callback);
    }

    /// <summary>
    /// Add a row to the Team list.
    /// </summary>
    public void AddRowForTeam(string title)
    {
        AddRowToTable(playerList.transform, teamHeaderRowPrefab, title, null, null);
    }

    /// <summary>
    /// Add a row to the Room list.
    /// </summary>
    public void AddRowForRoom(string title, string objectId, TableRow.SelectedHandler callback)
    {
        AddRowToTable(roomList.transform, roomRowPrefab, title, objectId, callback);
    }

    /// <summary>
    /// Add a row to the Message list.
    /// </summary>
    public void AddRowForMessage(string title, string objectId, TableRow.SelectedHandler callback)
    {
        if(currentMessageRowCount == MAX_MESSAGE_ROW_COUNT)
        {
            //remove the first message when MAX_MESSAGE_ROW_COUNT is reached.
            RemoveChild(messageList.transform);
            currentMessageRowCount--;
        }

        currentMessageRowCount++;
        AddRowToTable(messageList.transform, messageRowPrefab, title, objectId, callback);
    }

    public void ShowNewGamePopup(Action<bool, string> callback)
    {
        newRoomPopup.SetActive(true);
        newRoomText.text = "";
        newGamePopupCloseCallback = callback;
    }

    public void ShowRegisterPlayerPopup(Action<bool, string> callback)
    {
        registerPlayerPopup.SetActive(true);
        playerNameText.text = "";
        registerPlayerPopupCloseCallback = callback;
    }

    public void ShowMessagePlayerPopup(string targetPlayer, Action<bool, string, string> callback)
    {
        messagePlayerPopup.SetActive(true);
        playerIdText.text = targetPlayer;
        messageText.text = "";
        messagePlayerPopupCloseCallback = callback;
    }

    // Helper methods
    void AddRowToTable(Transform table, GameObject rowPrefab, string title, string objectId, TableRow.SelectedHandler callback)
    {
        GameObject newRow = Instantiate(rowPrefab, table);
        TableRow tableRow = newRow.GetComponent<TableRow>();
        tableRow.OnSelected += callback;
        tableRow.SetTitle(title);
        tableRow.SetObjectId(objectId);
    }

    void RemoveAllChildren(Transform parent)
    {
        foreach (Transform childTransform in parent)
        {
            Destroy(childTransform.gameObject);
        }
    }

    void RemoveChild(Transform parent)
    {
        foreach (Transform childTransform in parent)
        {
            Destroy(childTransform.gameObject);
            return;
        }
    }

    void HandleCreateGameOK()
    {
        if (newRoomText.text.Length > 0)
        {
            newRoomPopup.SetActive(false);
            if (newGamePopupCloseCallback != null)
            {
                newGamePopupCloseCallback(true, newRoomText.text);
            }
        }
        else
        {
            // must enter a game name
            Debug.LogWarning("Game name is empty.");
        }
    }

    void HandleCreateGameCancel()
    {
        newRoomPopup.SetActive(false);
        if (newGamePopupCloseCallback != null)
        {
            newGamePopupCloseCallback(false, null);
        }
    }

    void HandleRegisterPlayerOK()
    {
        if (playerNameText.text.Length > 0)
        {
            registerPlayerPopup.SetActive(false);
            if (registerPlayerPopupCloseCallback != null)
            {
                registerPlayerPopupCloseCallback(true, playerNameText.text);
            }
        }
        else
        {
            // must enter a player name
            Debug.LogWarning("Player name is empty.");
        }
    }

    void HandleRegisterPlayerCancel()
    {
        registerPlayerPopup.SetActive(false);
        if (registerPlayerPopupCloseCallback != null)
        {
            registerPlayerPopupCloseCallback(false, null);
        }
    }

    void HandleMessagePlayerOK()
    {
        if (playerIdText.text.Length > 0)
        {
            messagePlayerPopup.SetActive(false);
            if (messagePlayerPopupCloseCallback != null)
            {
                messagePlayerPopupCloseCallback(true, playerIdText.text, messageText.text);
            }
        }
        else
        {
            Debug.LogWarning("Player id is empty.");
        }
    }

    void HandleMessagePlayerCancel()
    {
        messagePlayerPopup.SetActive(false);
        if (messagePlayerPopupCloseCallback != null)
        {
            messagePlayerPopupCloseCallback(false, null, null);
        }
    }
}
