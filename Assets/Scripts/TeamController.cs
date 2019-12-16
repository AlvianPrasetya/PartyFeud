using Photon.Pun;
using System;
using System.Collections;
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
		playingTeamIndex = UnityEngine.Random.Range(0, 10);
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

	public void ShowStandings() {
		photonView.RPC("RPCShowStandings", RpcTarget.All);
	}

	[PunRPC]
	private void RPCReset() {
		foreach (Team team in teams) {
			team.Uneliminate();
			team.Unhighlight();
			team.HideScore();
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

	[PunRPC]
	private void RPCShowStandings() {
		StartCoroutine(ShowStandingsCoroutine());
	}

	private IEnumerator ShowStandingsCoroutine() {
		int minScore = Int32.MaxValue, maxScore = 0;
		foreach (Team team in teams) {
			minScore = Mathf.Min(minScore, team.score);
			maxScore = Mathf.Max(maxScore, team.score);
			team.Highlight();
		}
		int rangeScore = maxScore - minScore;
		if (rangeScore == 0) {
			rangeScore = Int32.MaxValue;
		}

		for (float progress = 0; progress < 1; progress = Mathf.Min(1, progress + Time.deltaTime / 2)) {
			foreach (Team team in teams) {
				float progressScore = Mathf.Lerp(0, team.score, progress);
				team.ShowScore((float)(progressScore - minScore) / rangeScore, (int) progressScore);
			}
			yield return null;
		}
		foreach (Team team in teams) {
			team.ShowScore((float)(team.score - minScore) / rangeScore, team.score);
		}
	}
}
