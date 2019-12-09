using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour {
	public Text timerText;
	public AudioSource sfx;
	public AudioClip tick;
	public AudioClip correct;
	public AudioClip wrong;
	public int time;
	private Coroutine countdown;

	public void StartCountdown() {
		countdown = StartCoroutine(CountDown(time));
	}

	public void StopCountdown() {
		if (countdown != null) {
			StopCoroutine(countdown);
			countdown = null;
		}
	}

	private IEnumerator CountDown(int startTime) {
		timerText.text = startTime.ToString();
		for (int i = startTime-1; i >= 0; i--) {
			sfx.PlayOneShot(tick);
			yield return new WaitForSecondsRealtime(1);
			timerText.text = i.ToString();
		}
		sfx.PlayOneShot(wrong);
	}
}
