using UnityEngine;


public class MegaKnightCard : SpecialActionMeleeCard
{
	protected override void specialAction(ulong selectedPlayerID)
	{
		Debug.Log(selectedPlayerID);
	}
}