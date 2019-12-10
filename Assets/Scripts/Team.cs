using UnityEngine;
using UnityEngine.UI;

public class Team : MonoBehaviour {
	public Image image;
	public Text text;
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

	void Awake() {
		score = 0;
		isPlaying = true;
	}
}
