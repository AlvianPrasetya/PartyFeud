using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamController : MonoBehaviourPunCallbacks {
	public Team[] teams;
	public AudioSource sfx;
	public AudioClip tick;
	public int numPlayingTeams {
		get; private set;
	}
	public int playingTeamIndex {
		get; private set;
	}

	public void Reset() {
		photonView.RPC("RPCReset", RpcTarget.All);
	}

	public IEnumerator SelectTeamRandom() {
		playingTeamIndex = Random.Range(0, 10);
		int steps = 5 * teams.Length + playingTeamIndex;

		for (int i = 0; i < steps - 10; i++) {
			sfx.PlayOneShot(tick);
			photonView.RPC("RPCHighlight", RpcTarget.All, i % teams.Length);
			yield return new WaitForSeconds(0.1f);
			photonView.RPC("RPCUnhighlight", RpcTarget.All, i % teams.Length);
		}
		for (int i = steps - 10; i < steps; i++) {
			sfx.PlayOneShot(tick);
			photonView.RPC("RPCHighlight", RpcTarget.All, i % teams.Length);
			yield return new WaitForSeconds(1.0f / (steps - i));
			photonView.RPC("RPCUnhighlight", RpcTarget.All, i % teams.Length);
		}
		sfx.PlayOneShot(tick);
		photonView.RPC("RPCHighlight", RpcTarget.All, playingTeamIndex);
	}

	public void NextTeam() {
		sfx.PlayOneShot(tick);
		photonView.RPC("RPCUnhighlight", RpcTarget.All, playingTeamIndex);

		if (numPlayingTeams == 0) {
			return;
		}

		playingTeamIndex = (playingTeamIndex + 1) % teams.Length;
		while (!teams[playingTeamIndex].isPlaying) {
			playingTeamIndex = (playingTeamIndex + 1) % teams.Length;
		}
		photonView.RPC("RPCHighlight", RpcTarget.All, playingTeamIndex);
	}

	public int Eliminate() {
		photonView.RPC("RPCEliminate", RpcTarget.All, playingTeamIndex);
		return --numPlayingTeams;
	}

	public void AddScore(int index, int score) {
		teams[index].score += score;
	}

	[PunRPC]
	private void RPCReset() {
		foreach (Team team in teams) {
			team.Uneliminate();
			team.Unhighlight();
		}
		numPlayingTeams = teams.Length;
	}

	[PunRPC]
	private void RPCHighlight(int index) {
		teams[index].Highlight();
	}

	[PunRPC]
	private void RPCUnhighlight(int index) {
		teams[index].Unhighlight();
	}

	[PunRPC]
	private void RPCEliminate(int index) {
		teams[index].Eliminate();
	}
}
