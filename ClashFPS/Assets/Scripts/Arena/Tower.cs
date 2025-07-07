using System.Collections;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.UI;


public class Tower : NetworkBehaviour
{
	[SerializeField] private float startingHealth = 1000f;
	[SerializeField] private NetworkObject deathPrefab;
	[SerializeField] private bool isKing;
	[SerializeField] private Side side;
	[SerializeField] private Slider healthSlider;
	[SerializeField] private Pose pose;

	private readonly NetworkVariable<float> health = new();

	public override void OnNetworkSpawn()
	{
		healthSlider.maxValue = startingHealth;
		healthSlider.value = startingHealth;

		if (IsServer)
			health.Value = startingHealth;

		health.OnValueChanged += (value, newValue) => UpdateSliderRpc();

		UpdateSliderRpc();
	}

	[ServerRpc(RequireOwnership = false)]
	public void DamageServerRpc(ulong sourcePlayerID, float amount)
	{
		health.Value -= amount;

		if (health.Value <= 0)
		{
			GameManager.Get.GetPlayerByID(sourcePlayerID).GetCard().OnDestroyedTower();
			GameManager.Get.OnTowerDestroy(side, isKing);

			deathPrefab =
				Instantiate(deathPrefab.gameObject, transform.position + Vector3.down * (isKing ? 8.3f : 5.8f),
					Quaternion.identity).GetComponent<NetworkObject>();
			deathPrefab.Spawn();
			GetComponent<NetworkObject>().Despawn();
		}
	}

	[Rpc(SendTo.Everyone)]
	private void UpdateSliderRpc()
	{
		StartCoroutine(UpdateSlider(health.Value));
	}

	private IEnumerator UpdateSlider(float value)
	{
		if (value <= 0)
		{
			healthSlider.value = 0;
			yield break;
		}

		float stepSize = 5f;
		float dir = value > healthSlider.value ? stepSize : -stepSize;
		float wait = 0.25f / (Mathf.Abs(healthSlider.value - value) / stepSize);

		for (float v = healthSlider.value; Mathf.Abs(value - v) > stepSize; v += dir)
		{
			healthSlider.value = v;
			yield return new WaitForSeconds(wait);
		}

		healthSlider.value = value;
	}

	public Side GetSide()
	{
		return side;
	}
}