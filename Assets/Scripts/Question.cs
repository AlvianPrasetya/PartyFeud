using UnityEngine;
using UnityEngine.UI;

public class Question : MonoBehaviour {
	public Text questionText;

	public void Set(string question) {
		questionText.text = question;
		gameObject.SetActive(true);
	}

	public void Unset() {
		gameObject.SetActive(false);
	}
}
