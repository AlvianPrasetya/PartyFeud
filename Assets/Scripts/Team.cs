using UnityEngine;
using UnityEngine.UI;

public class Team : MonoBehaviour {
	public Image image;
	public Text text;
	public Text scoreText;
	public bool isPlaying;
	public int score;

	public void Highlight() {
		image.color = new Color(1.0f, 1.0f, 1.0f);
	}

	public void Unhighlight() {
		image.color = new Color(0.75f, 0.75f, 0.75f);
	}

	public void Eliminate() {
		isPlaying = false;
		gameObject.SetActive(false);
	}

	public void Uneliminate() {
		isPlaying = true;
		gameObject.SetActive(true);
	}

	public void ShowScore(float progress, int score) {
		float height = progress * 630 + 320;
		image.rectTransform.sizeDelta = new Vector2(
			image.rectTransform.sizeDelta.x,
			height
		);
		text.rectTransform.anchoredPosition = new Vector2(
			text.rectTransform.anchoredPosition.x,
			-40 * height / 230
		);
		scoreText.rectTransform.anchoredPosition = new Vector2(
			scoreText.rectTransform.anchoredPosition.x,
			-40 * height / 230 - 100
		);
		scoreText.text = score.ToString();
	}

	public void HideScore() {
		image.rectTransform.sizeDelta = new Vector2(
			image.rectTransform.sizeDelta.x,
			230
		);
		text.rectTransform.anchoredPosition = new Vector2(
			text.rectTransform.anchoredPosition.x,
			-40
		);
		scoreText.rectTransform.anchoredPosition = new Vector2(
			scoreText.rectTransform.anchoredPosition.x,
			-230
		);
	}

	void Awake() {
		score = 0;
		isPlaying = true;
	}
}
