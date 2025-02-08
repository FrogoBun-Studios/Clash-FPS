using System.Collections;

using UnityEngine;
using UnityEngine.UI;


public class Tower : MonoBehaviour
{
	[SerializeField] private float health = 1000f;
	[SerializeField] private GameObject deathPrefab;
	[SerializeField] private bool isKing;
	[SerializeField] private Side side;
	[SerializeField] private Slider healthSlider;

	private void Start()
	{
		healthSlider.maxValue = health;
		healthSlider.value = health;
	}

	public bool Damage(float amount)
	{
		health -= amount;

		StartCoroutine(UpdateSlider(health));

		if (health <= 0)
		{
			Instantiate(deathPrefab, transform.position + Vector3.down * (isKing ? 8.3f : 5.8f), Quaternion.identity);
			Destroy(gameObject, 0.01f);
			return true;
		}

		return false;
	}

	protected IEnumerator UpdateSlider(float value)
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