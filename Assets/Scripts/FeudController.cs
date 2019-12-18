using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct Feud {
	public string question;
	public AnswerScore[] answerScores;
	
	public Feud(string question, List<AnswerScore> answerScores) {
		this.question = question;
		this.answerScores = answerScores.ToArray();
	}

	public static byte[] Serialize(object feud) {
		return Encoding.ASCII.GetBytes(JsonUtility.ToJson(feud));
	}

	public static object Deserialize(byte[] data) {
		return JsonUtility.FromJson<Feud>(Encoding.UTF8.GetString(data));
	}
}

[Serializable]
public struct AnswerScore {
	public string answer;
	public int score;

	public AnswerScore(string answer, int score) {
		this.answer = answer;
		this.score = score;
	}

	public static byte[] Serialize(object feud) {
		return Encoding.ASCII.GetBytes(JsonUtility.ToJson(feud));
	}

	public static object Deserialize(byte[] data) {
		return JsonUtility.FromJson<Feud>(Encoding.UTF8.GetString(data));
	}
}

public class FeudController : MonoBehaviourPunCallbacks {
	private enum FeudState {
		ToLoad, Ready, Play, Wait, RevealAnswers, ToStandings, ToReset
	}
	
	public Timer timer;
	public Question question;
	public Button nextButton;
	public Answer[] answers;
	public Image wrongIcon;
	public TeamController teamController;
	public AudioSource bgm;
	public AudioSource sfx;
	public AudioClip reveal;
	public AudioClip wrong;
	private Queue<Feud> feuds;
	private FeudState state;
	private Mutex wait = new Mutex();
	private int numUnanswered;
	private Coroutine timerCoroutine;
	private int roundIndex;

	public void Next() {
		switch (state) {
			case FeudState.ToLoad:
				state = FeudState.Wait;
				NextRound();
				break;
			case FeudState.Ready:
				state = FeudState.Wait;
				StartCoroutine(StartRoundCoroutine());
				break;
			case FeudState.Play:
				state = FeudState.Wait;
				WrongAnswer();
				break;
			case FeudState.ToStandings:
				state = FeudState.Wait;
				photonView.RPC("RPCResetRound", RpcTarget.All);
				teamController.Reset();
				teamController.ShowStandings();
				timer.Hide();
				if (roundIndex == 1) {
					state = FeudState.ToReset;
				} else {
					state = FeudState.ToLoad;
				}
				break;
			case FeudState.ToReset:
				state = FeudState.Wait;
				teamController.ResetStandings();
				state = FeudState.ToLoad;
				break;
			default:
				break;
		}
	}

	public void RevealAnswer(int index) {
		if (state != FeudState.Play && state != FeudState.RevealAnswers) {
			return;
		}

		sfx.PlayOneShot(reveal);
		photonView.RPC("RPCRevealAnswer", RpcTarget.All, index);
		numUnanswered--;
		switch (state) {
			case FeudState.Play:
				teamController.AddScore(teamController.playingTeamIndex, answers[index].score);
				teamController.NextTeam();
				NextSubround();
				break;
			case FeudState.RevealAnswers:
				if (numUnanswered == 0) {
					state = FeudState.ToStandings;
				}
				break;
		}
	}

	void Awake() {
		if (PhotonNetwork.IsMasterClient) {
			feuds = new Queue<Feud>();
			using (StreamReader reader = new StreamReader(@"./feuds.csv")) {
				string question = "";
				List<AnswerScore> answerScores = null;
				while (!reader.EndOfStream) {
					string line = reader.ReadLine();
					string[] values = line.Split(',');
					switch (values.Length) {
						case 1:
							// New question, flush old feud (if any)
							if (answerScores != null) {
								feuds.Enqueue(new Feud(question, answerScores));
							}
							// Create new feud
							question = values[0];
							answerScores = new List<AnswerScore>();
							break;
						case 2:
							string answer = values[0];
							int score = Int32.Parse(values[1]);
							answerScores.Add(new AnswerScore(answer, score));
							break;
						default:
							break;
					}
				}
				// Flush last feud (if any)
				if (answerScores != null) {
					feuds.Enqueue(new Feud(question, answerScores));
				}
			}
		}
	}

	void Start() {
		if (PhotonNetwork.IsMasterClient) {
			bgm.Play();
			nextButton.gameObject.SetActive(true);
			state = FeudState.ToLoad;
		}
	}

	[PunRPC]
	private void RPCNextRound(Feud feud) {
		question.Set(feud.question);
		for (int i = 0; i < feud.answerScores.Length; i++) {
			answers[i].Set(feud.answerScores[i].answer, feud.answerScores[i].score, PhotonNetwork.IsMasterClient);
		}
		for (int i = feud.answerScores.Length; i < answers.Length; i++) {
			answers[i].Unset();
		}
	}

	[PunRPC]
	private void RPCResetRound() {
		question.Unset();
		foreach (Answer answer in answers) {
			answer.Unset();
		}
	}

	[PunRPC]
	private void RPCRevealAnswer(int index) {
		if (PhotonNetwork.IsMasterClient) {
			StartCoroutine(answers[index].Hide());
		} else {
			StartCoroutine(answers[index].Reveal());
		}
	}

	[PunRPC]
	private void RPCWrongAnswer() {
		StartCoroutine(WrongAnswerCoroutine());
	}

	private void NextRound() {
		if (feuds.Count != 0) {
			Feud feud = feuds.Dequeue();
			photonView.RPC("RPCNextRound", RpcTarget.All, feud);
			teamController.Reset();
			numUnanswered = feud.answerScores.Length;
			state = FeudState.Ready;
			roundIndex++;
		} else {
			teamController.ShowWinner();
		}
	}

	private void StartRound() {
		StartCoroutine(StartRoundCoroutine());
	}

	private IEnumerator StartRoundCoroutine() {
		yield return timer.CountDown(10);
		if (roundIndex == 1) {
			// Demo round, select first team to start
			yield return teamController.SelectTeam(0);
		} else if (roundIndex <= teamController.teams.Length + 1) {
			// First 10 rounds, select starting team in sequence
			yield return teamController.SelectTeam(roundIndex - 2);
		} else {
			yield return teamController.SelectTeamRandom();
		}
		NextSubround();
	}

	private void WrongAnswer() {
		sfx.PlayOneShot(wrong);
		photonView.RPC("RPCWrongAnswer", RpcTarget.All);
		teamController.Eliminate();
		teamController.NextTeam();
		NextSubround();
	}

	private IEnumerator WrongAnswerCoroutine() {
		yield return new WaitForSeconds(0.5f);
		wrongIcon.gameObject.SetActive(true);
		for (float progress = 0; progress < 1; progress = Mathf.Min(1, progress + Time.deltaTime*2)) {
			wrongIcon.rectTransform.sizeDelta = new Vector2(
				360 + progress * 120,
				360 + progress * 120
			);
			yield return null;
		}
		for (float progress = 0; progress < 1; progress = Mathf.Min(1, progress + Time.deltaTime*2)) {
			wrongIcon.rectTransform.sizeDelta = new Vector2(
				480 - progress * 120,
				480 - progress * 120
			);
			yield return null;
		}
		wrongIcon.gameObject.SetActive(false);
	}

	private void NextSubround() {
		if (timerCoroutine != null) {
			StopCoroutine(timerCoroutine);
			timerCoroutine = null;
		}
		
		if (numUnanswered == 0) {
			state = FeudState.ToStandings;
		} else if (teamController.numPlayingTeams == 0) {
			state = FeudState.RevealAnswers;
		} else {
			state = FeudState.Play;
			timerCoroutine = StartCoroutine(timer.CountDown(3));
		}
	}
}
