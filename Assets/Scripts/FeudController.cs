using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Feud {
	public string question;
	public List<AnswerScore> answerScores;
	
	public Feud() {
		answerScores = new List<AnswerScore>();
	}
}

public struct AnswerScore {
	public string answer;
	public int score;

	public AnswerScore(string answer, int score) {
		this.answer = answer;
		this.score = score;
	}
}

public class FeudController : MonoBehaviour {
	public Timer timer;
	public Question question;
	public Answer[] answers;
	private Queue<Feud> feuds;

	public void NextRound() {
		if (feuds.Count != 0) {
			Feud feud = feuds.Dequeue();
			question.Set(feud.question);
			for (int i = 0; i < feud.answerScores.Count; i++) {
				answers[i].Set(feud.answerScores[i].answer, feud.answerScores[i].score);
			}
		}

		nextSubround();
	}

	public void nextSubround() {
		timer.StopCountdown();
		timer.StartCountdown();
	}

	void Start() {
		feuds = new Queue<Feud>();
		using (StreamReader reader = new StreamReader(@"/Users/prasetyaa/test_feuds.csv")) {
			Feud feud = null;
			while (!reader.EndOfStream) {
				string line = reader.ReadLine();
				string[] values = line.Split(',');
				switch (values.Length) {
					case 1:
						// New question, flush old feud (if any)
						if (feud != null) {
							feuds.Enqueue(feud);
						}
						// Create new feud
						feud = new Feud();
						feud.question = values[0];
						break;
					case 2:
						string answer = values[0];
						int score = Int32.Parse(values[1]);
						feud.answerScores.Add(new AnswerScore(answer, score));
						break;
					default:
						break;
				}
			}
		}
	}
}
