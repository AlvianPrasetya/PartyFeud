using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviourPunCallbacks {
	public Text timerText;
	public AudioSource sfx;
	public AudioClip tick;

	public IEnumerator CountDown(int startTime) {
		timerText.text = startTime.ToString();
		for (int i = startTime-1; i >= 0; i--) {
			yield return new WaitForSecondsRealtime(1);
			sfx.PlayOneShot(tick);
			photonView.RPC("RPCSetTime", RpcTarget.All, i);
		}
	}

	[PunRPC]
	private void RPCSetTime(int time) {
		timerText.text = time.ToString();
	}
}
