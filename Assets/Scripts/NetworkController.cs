using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkController : MonoBehaviourPunCallbacks {
	public override void OnConnectedToMaster() {
		Debug.Log("Connected to master server");
		if (!PhotonNetwork.JoinRandomRoom()) {
			Debug.Log("Failed to join random room");
		}
	}

	public override void OnJoinedRoom() {
		Debug.LogFormat("Joined room {0}", PhotonNetwork.CurrentRoom.Name);
		SceneManager.LoadScene("Intro", LoadSceneMode.Single);
		if (PhotonNetwork.IsMasterClient) {
			photonView.RPC("RPCBegin", RpcTarget.All);
		}
	}

	public override void OnJoinRandomFailed(short returnCode, string message) {
		Debug.LogFormat("Failed to join random room: {0}", message);

		Photon.Realtime.RoomOptions roomOptions = new Photon.Realtime.RoomOptions();
		roomOptions.MaxPlayers = 0;
		PhotonNetwork.CreateRoom(null, roomOptions);
	}

	void Awake() {
		DontDestroyOnLoad(gameObject);
	}

	void Start() {
		if (!PhotonNetwork.ConnectUsingSettings()) {
			Debug.Log("Failed to connect to master server");
		}
	}

	[PunRPC]
	private void RPCBegin() {
		SceneManager.LoadScene("Feud", LoadSceneMode.Single);
	}
}
