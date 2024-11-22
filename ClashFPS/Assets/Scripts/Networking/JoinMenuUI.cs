using System.Collections;

using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class JoinMenuUI : MonoBehaviour
{
	[SerializeField] private Button hostBtn;
	[SerializeField] private Button joinBtn;
	[SerializeField] private TMP_InputField joinField;
	private string _joinText;

	private void Start()
	{
		DontDestroyOnLoad(this);

		hostBtn.onClick.AddListener(() => StartCoroutine(LoadArena(true)));

		joinBtn.onClick.AddListener(() => StartCoroutine(LoadArena(false)));
	}

	private IEnumerator LoadArena(bool host)
	{
		_joinText = joinField.text;

		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Arena");
		while (!asyncLoad!.isDone)
			yield return null;

		if (host)
			Host();
		else
			Join();
	}

	private async void Host()
	{
		GameObject.Find("LoadingBar").GetComponent<Slider>().value = 0;
		GameObject.Find("LoadingBar").SetActive(true);
		hostBtn.gameObject.SetActive(false);
		joinBtn.gameObject.SetActive(false);
		joinField.gameObject.SetActive(false);
		RelayManager relayManager = FindFirstObjectByType<RelayManager>();
		await relayManager.StartManager();
		GameObject.Find("LoadingBar").GetComponent<Slider>().value = 0.25f;

		relayManager.CreateRelay();
		GameObject.Find("LoadingBar").GetComponent<Slider>().value = 0.5f;
		Destroy(gameObject);
	}

	private async void Join()
	{
		GameObject.Find("LoadingBar").GetComponent<Slider>().value = 0;
		GameObject.Find("LoadingBar").SetActive(true);
		hostBtn.gameObject.SetActive(false);
		joinBtn.gameObject.SetActive(false);
		joinField.gameObject.SetActive(false);
		RelayManager relayManager = FindFirstObjectByType<RelayManager>();
		await relayManager.StartManager();
		GameObject.Find("LoadingBar").GetComponent<Slider>().value = 0.25f;

		relayManager.JoinRelay(_joinText);
		GameObject.Find("LoadingBar").GetComponent<Slider>().value = 0.5f;
		Destroy(gameObject);
	}
}