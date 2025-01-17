using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviourPunCallbacks
{
    [SerializeField] public GameObject CreateRoomPanel;

    public static UIManager Instance;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }
    #region

    [SerializeField] GameObject PlayerPrefab;
    [SerializeField] Transform PlayerContent;
    List<GameObject> playerList = new List<GameObject>();
    public void RoomPlayerList()
    {
        ClearPlayerList();
        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            GameObject Init = Instantiate(PlayerPrefab, PlayerContent);
            Init.GetComponent<PlayerInfo>().TextPlayerNumber.text = (i + 1).ToString();
            Init.GetComponent<PlayerInfo>().TextPlayerName.text = players[i].NickName.ToString();
            playerList.Add(Init);
        }
        if (PhotonNetwork.IsMasterClient)
        {
            LetStart.SetActive(true);
        }
        else
        {
            LetStart.SetActive(false);
        }
    }
    void ClearPlayerList()
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            Destroy(playerList[i]);
        }
        playerList = new List<GameObject>();
    }
#endregion
    [SerializeField] GameObject LetStart;
    public void OnClickStart()
    {
        photonView.RPC(nameof(StartGame), RpcTarget.All);
    }
    [PunRPC]
    void StartGame()
    {
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("SceneName", out object sceneName))
        {
            Debug.Log($"Loading scene: {sceneName}");
            PhotonNetwork.LoadLevel(sceneName.ToString());
        }
        else
        {
            Debug.LogError("Scene name not found in room properties!");
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
