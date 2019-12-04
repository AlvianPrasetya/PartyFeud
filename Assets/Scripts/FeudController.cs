using System;
using UnityEngine;

[Serializable]
public struct AnswerScore {
	public string answer;
	public int score;
}

public class FeudController : MonoBehaviour {
	public AnswerBar[] answerBars;
	public AnswerScore[] answerScores;
}
