using System.Collections;

using UnityEngine;


public class MegaKnightCard : SpecialActionMeleeCard
{
	protected override void specialAction(ulong selectedPlayerID)
	{
		Debug.Log(selectedPlayerID);

		StartCoroutine(Jump(GameManager.Get.GetPlayerByID(selectedPlayerID)));
	}

	private IEnumerator Jump(Player target)
	{
		Vector3 startPos = player.position;
		Vector3 endPos = target.transform.position;
		float dist = Vector2.Distance(new Vector2(startPos.x, startPos.z), new Vector2(endPos.x, endPos.z));
		Vector3 dir = (new Vector3(endPos.x, 0, endPos.z) - new Vector3(startPos.x, 0, startPos.z)).normalized;
		float jumpHeight = 7.5f;

		float f(float x)
		{
			float mid = dist / 2;
			float a = jumpHeight / (mid - 0) / (mid - dist);
			return a * (x - 0) * (x - dist);
		}

		float stepSize = 0.1f;
		float wait = 0.1f / (dist / stepSize);

		playerScript.EnableCardControl(false);
		movementController.EnableController(false);
		movementController.SetAnimatorTriggerRpc("Jump");

		for (float v = 0; v < dist; v += stepSize)
		{
			player.position = startPos + dir * v + Vector3.up * f(v);
			yield return new WaitForSeconds(wait);
		}

		AttackServerRpc(cardParams.damage * 5);

		player.position = endPos;
		playerScript.EnableCardControl(true);
		movementController.EnableController(true);
	}
}