using UnityEngine;


[CreateAssetMenu(fileName = "New SpecialActionMeleeCardParams", menuName = "ClashFPS/SpecialActionMeleeCardParams")]
public class SpecialActionMeleeCardParams : MeleeCardParams
{
	public float specialActionRate;
	public bool isSpecialActionOnPlayers;
	public float specialActionPlayerMinDistance;
	public float specialActionPlayerMaxDistance;
	public Color specialActionPlayerGlowColor;
	public float specialActionPlayerGlowIntensity;
	public float specialActionPlayerGlowiness;
}