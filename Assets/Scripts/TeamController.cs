using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;

public class TeamController : MonoBehaviourPunCallbacks {
	public Team[] teams;
	public AudioSource sfx;
	public AudioClip tick;
	public AudioClip drumRoll;
	public AudioClip applause;
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

	public IEnumerator SelectTeam(int index) {
		sfx.PlayOneShot(tick);
		photonView.RPC("RPCUnhighlight", RpcTarget.All, playingTeamIndex);
		playingTeamIndex = index;
		photonView.RPC("RPCHighlight", RpcTarget.All, playingTeamIndex);
		yield return null;
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
		sfx.PlayOneShot(drumRoll);
		int[] scores = new int[teams.Length];
		for (int i = 0; i < teams.Length; i++) {
			scores[i] = teams[i].score;
		}
		photonView.RPC("RPCShowStandings", RpcTarget.All, scores);
	}

	public void ResetStandings() {
		photonView.RPC("RPCResetStandings", RpcTarget.All);
	}

	public void ShowWinner() {
		sfx.PlayOneShot(applause);
		int winningTeamIndex = 0;
		for (int i = 1; i < teams.Length; i++) {
			if (teams[i].score > winningTeamIndex) {
				winningTeamIndex = i;
			}
		}
		photonView.RPC("RPCShowWinner", RpcTarget.All, winningTeamIndex);
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
	private void RPCShowStandings(int[] scores) {
		StartCoroutine(ShowStandingsCoroutine(scores));
	}

	[PunRPC]
	private void RPCResetStandings() {
		StartCoroutine(ResetStandingsCoroutine());
	}

	[PunRPC]
	private void RPCShowWinner(int index) {
		teams[index].Win();
	}

	private IEnumerator ShowStandingsCoroutine(int[] scores) {
		yield return new WaitForSeconds(0.5f);

		int minScore = Int32.MaxValue, maxScore = 0;
		for (int i = 0; i < teams.Length; i++) {
			teams[i].score = scores[i];
			minScore = Mathf.Min(minScore, teams[i].score);
			maxScore = Mathf.Max(maxScore, teams[i].score);
			teams[i].Highlight();
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

	private IEnumerator ResetStandingsCoroutine() {
		int minScore = Int32.MaxValue, maxScore = 0;
		for (int i = 0; i < teams.Length; i++) {
			minScore = Mathf.Min(minScore, teams[i].score);
			maxScore = Mathf.Max(maxScore, teams[i].score);
			teams[i].Highlight();
		}
		int rangeScore = maxScore - minScore;
		if (rangeScore == 0) {
			rangeScore = Int32.MaxValue;
		}

		for (float progress = 0; progress < 1; progress = Mathf.Min(1, progress + Time.deltaTime / 2)) {
			foreach (Team team in teams) {
				float progressScore = Mathf.Lerp(team.score, 0, progress);
				team.ShowScore((float)(progressScore - minScore) / rangeScore, (int) progressScore);
			}
			yield return null;
		}
		foreach (Team team in teams) {
			team.ShowScore(0, 0);
			team.score = 0;
		}
	}
}
