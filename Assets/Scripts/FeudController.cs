using ExitGames.Client.Photon;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

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
	public static FeudController instance;
	public Timer timer;
	public Question question;
	public Answer[] answers;
	public Team[] teams;
	private Queue<Feud> feuds;

	public void NextRound() {
		if (feuds.Count != 0) {
			photonView.RPC("RPCNextRound", RpcTarget.All, feuds.Dequeue());
		}
	}

	public void NextSubround() {
		photonView.RPC("RPCNextSubround", RpcTarget.All);
	}

	public void ToggleTeam(int index) {
		photonView.RPC("RPCToggleTeam", RpcTarget.All, index);
	}

	void Awake() {
		PhotonPeer.RegisterType(typeof(Feud), 0, Feud.Serialize, Feud.Deserialize);
		PhotonPeer.RegisterType(typeof(AnswerScore), 1, AnswerScore.Serialize, AnswerScore.Deserialize);

		instance = this;

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
		}
	}

	[PunRPC]
	private void RPCNextRound(Feud feud) {
		question.Set(feud.question);
		for (int i = 0; i < feud.answerScores.Length; i++) {
			answers[i].Set(feud.answerScores[i].answer, feud.answerScores[i].score);
		}
		for (int i = feud.answerScores.Length; i < answers.Length; i++) {
			answers[i].Unset();
		}
	}

	[PunRPC]
	private void RPCNextSubround() {
		timer.StopCountdown();
		timer.StartCountdown();
	}

	[PunRPC]
	private void RPCToggleTeam(int index) {
		teams[index].TogglePlaying();
	}
}
