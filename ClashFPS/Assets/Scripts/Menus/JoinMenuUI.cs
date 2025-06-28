using System.Collections;
using System.Threading.Tasks;

using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class JoinMenuUI : MonoBehaviour
{
	[SerializeField] private Button hostBtn;
	[SerializeField] private Button joinBtn;
	[SerializeField] private TMP_InputField joinField;
	private string joinText;

	private void Start()
	{
		DontDestroyOnLoad(this);

		hostBtn.onClick.AddListener(() => StartCoroutine(LoadArena(true)));
		joinBtn.onClick.AddListener(() => StartCoroutine(LoadArena(false)));
	}

	private IEnumerator LoadArena(bool host)
	{
		yield return new WaitUntil(() => GameObject.Find("RelayManager").GetComponent<RelayManager>().IsReady());

		joinText = joinField.text;

		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1);
		yield return new WaitUntil(() => asyncLoad.isDone);

		GameObject.Find("LoadingBar").GetComponent<Slider>().value = 0;
		GameObject.Find("LoadingBar").SetActive(true);

		hostBtn.gameObject.SetActive(false);
		joinBtn.gameObject.SetActive(false);
		joinField.gameObject.SetActive(false);
		GameObject.Find("LoadingBar").GetComponent<Slider>().value = 0.5f;

		if (host)
			Host();
		else
			Join();
	}

	private async Task Host()
	{
		await GameObject.Find("RelayManager").GetComponent<RelayManager>().CreateRelay();
		Destroy(gameObject);
	}

	private async Task Join()
	{
		await GameObject.Find("RelayManager").GetComponent<RelayManager>().JoinRelay(joinText);
		Destroy(gameObject);
	}
}