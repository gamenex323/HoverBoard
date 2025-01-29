using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PhotonAuth : MonoBehaviourPunCallbacks
{
    public static PhotonAuth Instance;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }
    private string _playFabPlayerIdCache;

    //Run the entire thing on awake
    

    /*
     * Step 1
     * We authenticate a current PlayFab user normally.
     * In this case we use the LoginWithCustomID API call for simplicity.
     * You can absolutely use any Login method you want.
     * We use PlayFabSettings.DeviceUniqueIdentifier as our custom ID.
     * We pass RequestPhotonToken as a callback to be our next step, if
     * authentication was successful.
     */
    public void AuthenticateWithPlayFab()
    {
        LogMessage("PlayFab authenticating using Custom ID...");

        PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest()
        {
            CreateAccount = true,
            CustomId = PlayFabSettings.DeviceUniqueIdentifier
        }, RequestPhotonToken, OnPlayFabError);
    }

    /*
    * Step 2
    * We request a Photon authentication token from PlayFab.
    * This is a crucial step, because Photon uses different authentication tokens
    * than PlayFab. Thus, you cannot directly use PlayFab SessionTicket and
    * you need to explicitly request a token. This API call requires you to
    * pass a Photon App ID. The App ID may be hard coded, but in this example,
    * we are accessing it using convenient static field on PhotonNetwork class.
    * We pass in AuthenticateWithPhoton as a callback to be our next step, if
    * we have acquired the token successfully.
    */
    private void RequestPhotonToken(LoginResult obj)
    {
        LogMessage("PlayFab authenticated. Requesting photon token...");

        //We can player PlayFabId. This will come in handy during next step
        _playFabPlayerIdCache = obj.PlayFabId;

        PlayFabClientAPI.GetPhotonAuthenticationToken(new GetPhotonAuthenticationTokenRequest()
        {
            PhotonApplicationId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime
        }, AuthenticateWithPhoton, OnPlayFabError);
    }

    /*
     * Step 3
     * This is the final and the simplest step. We create a new AuthenticationValues instance.
     * This class describes how to authenticate a player inside the Photon environment.
     */
    private void AuthenticateWithPhoton(GetPhotonAuthenticationTokenResult obj)
    {
        LogMessage("Photon token acquired: " + obj.PhotonCustomAuthenticationToken + "  Authentication complete.");

        //We set AuthType to custom, meaning we bring our own, PlayFab authentication procedure.
        var customAuth = new Photon.Realtime.AuthenticationValues { AuthType = CustomAuthenticationType.Custom };

        //We add "username" parameter. Do not let it confuse you: PlayFab is expecting this parameter to contain player PlayFab ID (!) and not username.
        customAuth.AddAuthParameter("username", _playFabPlayerIdCache);    // expected by PlayFab custom auth service

        //We add "token" parameter. PlayFab expects it to contain Photon Authentication Token issues to your during previous step.
        customAuth.AddAuthParameter("token", obj.PhotonCustomAuthenticationToken);

        //We finally tell Photon to use this authentication parameters throughout the entire application.
        PhotonNetwork.AuthValues = customAuth;
        PhotonNetwork.ConnectUsingSettings();
    }

    private void OnPlayFabError(PlayFabError obj)
    {
        LogMessage(obj.GenerateErrorReport());
    }

    public void LogMessage(string message)
    {
        Debug.Log("PlayFab + Photon Example: " + message);
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server.");
        PhotonNetwork.JoinLobby(); // Join the default lobby
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined the lobby successfully.");
        
        PhotonNetwork.LocalPlayer.NickName = GlobalData.PlayerName;
        Debug.Log("Nick Name " + PhotonNetwork.LocalPlayer.NickName);
        SceneManager.LoadSceneAsync(1);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined a room successfully.");
        Debug.Log("Current Room " +PhotonNetwork.CurrentRoom.Name);

        if (GameManager.Instance)
        {
            ExitGames.Client.Photon.Hashtable playerCustomProperties = new ExitGames.Client.Photon.Hashtable
            {
                { "CarName", GameManager.Instance.GetSelectedPlayerName()}
            };
            // Set the player's custom properties
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerCustomProperties);
        }
        if (UIManager.Instance)
            UIManager.Instance.RoomPlayerList();
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //UIManager.instance.RoomPlayerList();
        if (PhotonNetwork.IsMasterClient)
        {
            if (UIManager.Instance)
                UIManager.Instance.RoomPlayerList();
        }
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //UIManager.instance.RoomPlayerList();
        if (PhotonNetwork.IsMasterClient)
        {
            if (UIManager.Instance)
                UIManager.Instance.RoomPlayerList();
        }
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("Room List Update");
        //UIManager.instance.RoomPlayerList();

        foreach (var room in roomList)
        {
            Debug.Log($"Room Name: {room.Name}, Player Count: {room.PlayerCount}/{room.MaxPlayers}");
        }
    }


    public void JoinOrCreateRoom()
    {
        // Attempt to join a random room
        PhotonNetwork.JoinRandomRoom();
    }
    public string ScenceName;
    // Callback if joining a random room fails
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to join a random room. Creating a new room...");
        // Create a new room with some basic options
        // Validate scene name and car name
        string sceneName = ScenceName; // Ensure ScenceName is initialized
        
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("SceneName is null or empty. Cannot create a room.");
            return;
        }

       
        // Room options
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 6,
            IsVisible = true,
            IsOpen = true,
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "SceneName", sceneName }
           
        },
            CustomRoomPropertiesForLobby = new string[] { "SceneName" }
        };

        PhotonNetwork.CreateRoom(null, roomOptions, TypedLobby.Default);
    }

    // Callback when a room is created successfully
    public override void OnCreatedRoom()
    {
        Debug.Log("Successfully created a new room!");
    }
   

}
