using System.Collections;

using TMPro;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class SettingsMenu : MonoBehaviour
{
	[SerializeField] private CanvasGroup canvasGroup;
	[SerializeField] private TMP_InputField playerNameInput;
	[SerializeField] private Slider volumeInput;
	[SerializeField] private Slider mouseSensitivityInput;
	[SerializeField] private TMP_Dropdown qualityInput;
	[SerializeField] private Slider fovInput;
	[SerializeField] private Button saveButton;
	private Player playerScript;
	private PlayerSettings playerSettings;
	private bool showen;

	private void Update()
	{
		if (playerScript == null)
			return;

		GetUISettings();
		saveButton.interactable = !playerScript.playerSettings.Equals(playerSettings);
	}

	public IEnumerator Show()
	{
		Debug.Log("Player opened settings menu");

		showen = true;

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		playerSettings = playerScript.playerSettings;
		SetUISettings();
		saveButton.interactable = false;

		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;

		float timeToOpen = 0.2f;
		float timeStep = 0.01f;
		for (float t = 0; t < 1; t += timeStep / timeToOpen)
		{
			canvasGroup.alpha = t;
			yield return new WaitForSeconds(timeStep);
		}

		canvasGroup.alpha = 1;
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;
	}

	public IEnumerator Hide()
	{
		Debug.Log("Player closed settings menu");

		showen = false;

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;

		for (float t = 1; t > 0; t -= 0.05f)
		{
			canvasGroup.alpha = t;
			yield return new WaitForSeconds(0.01f);
		}

		canvasGroup.alpha = 0;
	}

	public bool IsShowen()
	{
		return showen;
	}

	public void Set(Player playerScript)
	{
		this.playerScript = playerScript;
	}

	private void GetUISettings()
	{
		playerSettings.playerName = playerNameInput.text;
		playerSettings.volume = volumeInput.value;
		playerSettings.mouseSensitivity = mouseSensitivityInput.value;
		playerSettings.quality = qualityInput.value;
		playerSettings.FOV = fovInput.value;
	}

	private void SetUISettings()
	{
		playerNameInput.text = playerSettings.playerName;
		volumeInput.value = playerSettings.volume;
		mouseSensitivityInput.value = playerSettings.mouseSensitivity;
		qualityInput.value = playerSettings.quality;
		fovInput.value = playerSettings.FOV;
	}

	public void SaveButton()
	{
		GetUISettings();
		playerScript.UpdateGameToSettings(playerSettings);

		PlayerPrefs.SetString("playerName", playerSettings.playerName);
		PlayerPrefs.SetFloat("volume", playerSettings.volume);
		PlayerPrefs.SetFloat("mouseSensitivity", playerSettings.mouseSensitivity);
		PlayerPrefs.SetInt("quality", playerSettings.quality);
		PlayerPrefs.SetFloat("FOV", playerSettings.FOV);
		PlayerPrefs.Save();

		Debug.Log("Saved settings");

		StartCoroutine(Hide());
	}

	public void ChangeSideButton()
	{
		if (playerScript.GetSide() == Side.Blue)
			GameManager.Get.UpdateBluePlayersCountRpc(-1);
		else
			GameManager.Get.UpdateRedPlayersCountRpc(-1);

		StartCoroutine(Hide());
		playerScript.ChooseSide();
	}

	public void LeaveButton()
	{
		if (playerScript.GetSide() == Side.Blue)
			GameManager.Get.UpdateBluePlayersCountRpc(-1);
		else
			GameManager.Get.UpdateRedPlayersCountRpc(-1);

		NetworkManager.Singleton.Shutdown();
		SceneManager.LoadScene("JoinMenu");
	}
}