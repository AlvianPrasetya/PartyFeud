using UnityEngine;
using UnityEngine.UI;

public class AnswerBar : MonoBehaviour {
	public Text answerText;
	public Text scoreText;

	public void Set(string answer, int score) {
		answerText.text = answer;
		scoreText.text = score.ToString();
	}
}
