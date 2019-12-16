using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviourPunCallbacks {
	public Text timerText;
	public AudioSource sfx;
	public AudioClip tick;

	public IEnumerator CountDown(int startTime) {
		photonView.RPC("RPCReveal", RpcTarget.All);
		for (int i = startTime; i > 0; i--) {
			photonView.RPC("RPCSetTime", RpcTarget.All, i);
			yield return new WaitForSecondsRealtime(1);
			sfx.PlayOneShot(tick);
		}
		photonView.RPC("RPCHide", RpcTarget.All);
	}

	[PunRPC]
	private void RPCReveal() {
		gameObject.SetActive(true);
	}

	[PunRPC]
	private void RPCHide() {
		gameObject.SetActive(false);
	}

	[PunRPC]
	private void RPCSetTime(int time) {
		timerText.text = time.ToString();
	}
}
