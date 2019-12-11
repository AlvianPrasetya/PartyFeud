using ExitGames.Client.Photon;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
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
		Idle, Ready, Play, Wait
	}
	
	public Timer timer;
	public Question question;
	public Button nextButton;
	public Answer[] answers;
	public TeamController teamController;
	public AudioSource sfx;
	public AudioClip reveal;
	public AudioClip wrong;
	private Queue<Feud> feuds;
	private FeudState state;
	private Mutex wait = new Mutex();
	private int numUnanswered;

	public void Next() {
		switch (state) {
			case FeudState.Idle:
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
			default:
				break;
		}
	}

	public void RevealAnswer(int index) {
		if (state != FeudState.Play) {
			return;
		}
		state = FeudState.Wait;

		sfx.PlayOneShot(reveal);
		photonView.RPC("RPCRevealAnswer", RpcTarget.All, index);
		teamController.AddScore(index, answers[index].score);
		teamController.NextTeam();
		numUnanswered--;
		if (numUnanswered == 0) {
			state = FeudState.Idle;
		} else {
			state = FeudState.Play;
		}
	}

	void Awake() {
		PhotonPeer.RegisterType(typeof(Feud), 0, Feud.Serialize, Feud.Deserialize);
		PhotonPeer.RegisterType(typeof(AnswerScore), 1, AnswerScore.Serialize, AnswerScore.Deserialize);

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

	void Start() {
		if (PhotonNetwork.IsMasterClient) {
			nextButton.gameObject.SetActive(true);
		}
		NextRound();
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
	private void RPCRevealAnswer(int index) {
		if (PhotonNetwork.IsMasterClient) {
			StartCoroutine(answers[index].Hide());
		} else {
			StartCoroutine(answers[index].Reveal());
		}
	}

	private void NextRound() {
		if (feuds.Count != 0) {
			Feud feud = feuds.Dequeue();
			photonView.RPC("RPCNextRound", RpcTarget.All, feud);
			teamController.Reset();
			numUnanswered = feud.answerScores.Length;
			state = FeudState.Ready;
		}
	}

	private void StartRound() {
		StartCoroutine(StartRoundCoroutine());
	}

	private IEnumerator StartRoundCoroutine() {
		yield return timer.CountDown(10);
		yield return teamController.SelectTeamRandom();
		state = FeudState.Play;
	}

	private void WrongAnswer() {
		sfx.PlayOneShot(wrong);
		int numPlayingTeams = teamController.Eliminate();
		teamController.NextTeam();
		if (numPlayingTeams == 0) {
			state = FeudState.Idle;
		} else {
			state = FeudState.Play;
		}
	}
}
