using UnityEngine;
using UnityEngine.UI;

public class Team : MonoBehaviour {
	public Image image;
	public Text text;
	private bool isPlaying;

	public void TogglePlaying() {
		isPlaying = !isPlaying;
		if (isPlaying) {
			image.color = new Color(1.0f, 1.0f, 1.0f);
			text.gameObject.SetActive(true);
		} else {
			image.color = new Color(0.5f, 0.5f, 0.5f);
			text.gameObject.SetActive(false);
		}
	}

	void Awake() {
		isPlaying = true;
	}
}
