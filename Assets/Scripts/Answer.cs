using UnityEngine;
using UnityEngine.UI;

public class Answer : MonoBehaviour {
	public RectTransform bar;
	public Text answerText;
	public Text scoreText;

	public void Set(string answer, int score) {
		answerText.text = answer;
		scoreText.text = score.ToString();
		gameObject.SetActive(true);
	}

	public void Unset() {
		gameObject.SetActive(false);
	}

	public void Reveal() {
	}
}
