using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkController : MonoBehaviourPunCallbacks {
	public Text debugText;
	public Button startButton;
	public AudioSource bgm;

	public override void OnConnectedToMaster() {
		debugText.text = "Joining random room";
		if (!PhotonNetwork.JoinRandomRoom()) {
			debugText.text = "Failed to join random room";
		}
	}

	public override void OnJoinedRoom() {
		debugText.text = string.Format("Joined room {0}", PhotonNetwork.CurrentRoom.Name);

		if (PhotonNetwork.IsMasterClient) {
			DebugNumClients(PhotonNetwork.CurrentRoom.Players.Count - 1);
			startButton.gameObject.SetActive(true);
			bgm.Play();
		}
	}

	public override void OnJoinRandomFailed(short returnCode, string message) {
		debugText.text = string.Format("Creating room");	
		Photon.Realtime.RoomOptions roomOptions = new Photon.Realtime.RoomOptions();
		roomOptions.MaxPlayers = 0;
		PhotonNetwork.CreateRoom(null, roomOptions);
	}

	public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) {
		if (PhotonNetwork.IsMasterClient) {
			DebugNumClients(PhotonNetwork.CurrentRoom.Players.Count - 1);
		}
	}

	public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) {
		if (PhotonNetwork.IsMasterClient) {
			DebugNumClients(PhotonNetwork.CurrentRoom.Players.Count - 1);
		}
	}

	public void StartGame() {
		debugText.text = "Starting game";
		StartCoroutine(StartGameCoroutine());
		PhotonNetwork.CurrentRoom.IsOpen = false;
		PhotonNetwork.CurrentRoom.IsVisible = false;
	}

	void Start() {
		debugText.text = "Connecting to master server";
		if (!PhotonNetwork.ConnectUsingSettings()) {
			debugText.text = "Failed to connect to master server";
		}
	}

	[PunRPC]
	private void RPCStartGame() {
		SceneManager.LoadScene("Feud", LoadSceneMode.Single);
	}
	
	private void DebugNumClients(int numClients) {
		debugText.text = string.Format("{0} clients joined", numClients);
	}

	private IEnumerator StartGameCoroutine() {
		while (bgm.volume != 0) {
			bgm.volume -= Time.deltaTime / 2;
			yield return null;
		}
		photonView.RPC("RPCStartGame", RpcTarget.All);
	}
}
