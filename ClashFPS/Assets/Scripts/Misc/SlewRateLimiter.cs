using UnityEngine;


public class SlewRateLimiter
{
	private float lastTime;
	private float lastValue;
	private float rateLimit;

	public SlewRateLimiter(float rateLimit)
	{
		this.rateLimit = rateLimit;
		lastValue = 0f;
		lastTime = Time.time;
	}

	public float Calculate(float input)
	{
		float currentTime = Time.time;
		float deltaTime = currentTime - lastTime;

		float maxDelta = rateLimit * deltaTime;
		float delta = input - lastValue;

		if (delta > maxDelta)
			delta = maxDelta;
		else if (delta < -maxDelta)
			delta = -maxDelta;

		lastValue += delta;
		lastTime = currentTime;

		return lastValue;
	}

	public void SetRateLimit(float rateLimit)
	{
		this.rateLimit = rateLimit;
	}

	public void Reset(float value = 0f)
	{
		lastValue = value;
		lastTime = Time.time;
	}
}