using UnityEngine;


public struct PlayerSettings
{
	public string playerName;
	public float volume;
	public float mouseSensitivity;
	public int quality;
	public float FOV;

	public override bool Equals(object obj)
	{
		if (obj == null)
			return false;

		if (typeof(PlayerSettings) != obj.GetType())
			return false;

		PlayerSettings other = (PlayerSettings)obj;

		return playerName == other.playerName
		       && EqualFloats(volume, other.volume)
		       && EqualFloats(mouseSensitivity, other.mouseSensitivity)
		       && quality == other.quality
		       && EqualFloats(FOV, other.FOV);
	}

	public override string ToString()
	{
		return $"({playerName}, {volume}, {mouseSensitivity}, {quality}, {FOV})";
	}

	private bool EqualFloats(float a, float b)
	{
		return Mathf.Abs(a - b) < 0.001f;
	}
}