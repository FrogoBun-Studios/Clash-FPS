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
		float jumpHeight = 2;

		float f(float x)
		{
			float mid = dist / 2;
			float a = jumpHeight / (mid - 0) / (mid - dist);
			Debug.Log($"The height func is {a}(x)(x - {dist})");
			return a * (x - 0) * (x - dist);
		}

		float stepSize = 0.05f;
		float wait = 1f / (dist / stepSize);

		playerScript.EnableCardControl(false);
		movementController.EnableController(false);
		for (float v = 0; Vector2.Distance(startPos, endPos) > stepSize; v += stepSize)
		{
			Debug.Log($"At {v}/{Vector2.Distance(startPos, endPos)}");
			Debug.Log($"With dir {dir}");
			Debug.Log($"Moving {dir * v} at xz");
			Debug.Log($"Calculated height is {f(v)}");
			Debug.Log($"Start pos is {startPos}");
			Debug.Log($"End pos is {endPos}");
			Debug.Log($"Going to {startPos + dir * v + Vector3.up * f(v)}");
			Debug.Log($"Waiting {wait}");
			player.position = startPos + dir * v + Vector3.up * f(v);
			yield return new WaitForSeconds(wait);
		}

		player.position = endPos;
		playerScript.EnableCardControl(true);
		movementController.EnableController(true);
	}
}