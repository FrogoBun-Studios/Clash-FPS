using System.Collections;

using TMPro;

using UnityEngine;
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
	private Player _playerScript;
	private PlayerSettings _playerSettings;
	private bool _showen;

	private void Update()
	{
		if (_playerScript == null)
			return;

		GetUISettings();
		saveButton.interactable = !_playerScript.PlayerSettings.Equals(_playerSettings);
	}

	public IEnumerator Show()
	{
		_showen = true;

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		_playerSettings = _playerScript.PlayerSettings;
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
		_showen = false;

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
		return _showen;
	}

	public void Set(Player playerScript)
	{
		_playerScript = playerScript;
	}

	private void GetUISettings()
	{
		_playerSettings.playerName = playerNameInput.text;
		_playerSettings.volume = volumeInput.value;
		_playerSettings.mouseSensitivity = mouseSensitivityInput.value;
		_playerSettings.quality = qualityInput.value;
		_playerSettings.FOV = fovInput.value;
	}

	private void SetUISettings()
	{
		playerNameInput.text = _playerSettings.playerName;
		volumeInput.value = _playerSettings.volume;
		mouseSensitivityInput.value = _playerSettings.mouseSensitivity;
		qualityInput.value = _playerSettings.quality;
		fovInput.value = _playerSettings.FOV;
	}

	public void SaveButton()
	{
		GetUISettings();
		_playerScript.UpdateSettings(_playerSettings);

		PlayerPrefs.SetString("playerName", _playerSettings.playerName);
		PlayerPrefs.SetFloat("volume", _playerSettings.volume);
		PlayerPrefs.SetFloat("mouseSensitivity", _playerSettings.mouseSensitivity);
		PlayerPrefs.SetInt("quality", _playerSettings.quality);
		PlayerPrefs.SetFloat("FOV", _playerSettings.FOV);
		PlayerPrefs.Save();

		StartCoroutine(Hide());
	}

	public void ChangeSideButton()
	{
		if (_playerScript.Side == Side.Blue)
			GameManager.Get.UpdateBluePlayersCountRpc(-1);
		else
			GameManager.Get.UpdateRedPlayersCountRpc(-1);

		StartCoroutine(Hide());
		_playerScript.ChooseSide();
	}
}