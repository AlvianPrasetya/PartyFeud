using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Answer : MonoBehaviour {
	public Button button;
	public Image bar;
	public Text answerText;
	public Text scoreText;

	public void Set(string answer, int score, bool revealed = false) {
		answerText.text = answer;
		scoreText.text = score.ToString();
		gameObject.SetActive(true);
		if (revealed) {
			answerText.gameObject.SetActive(true);
			scoreText.gameObject.SetActive(true);
			bar.color = new Color(0.375f, 1.0f, 1.0f);
			button.interactable = true;
		} else {
			answerText.gameObject.SetActive(false);
			scoreText.gameObject.SetActive(false);
			bar.color = new Color(1.0f, 1.0f, 1.0f);
			button.interactable = false;
		}
	}

	public void Unset() {
		gameObject.SetActive(false);
	}

	public IEnumerator Reveal() {
		yield return new WaitForSeconds(0.5f);
		answerText.gameObject.SetActive(true);
		scoreText.gameObject.SetActive(true);
		bar.color = new Color(0.375f, 1.0f, 1.0f);
	}

	public IEnumerator Hide() {
		yield return new WaitForSeconds(0.5f);
		answerText.gameObject.SetActive(false);
		scoreText.gameObject.SetActive(false);
		bar.color = new Color(1.0f, 1.0f, 1.0f);
		button.interactable = false;
	}
}
