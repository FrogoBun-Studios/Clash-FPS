using UnityEngine;


public abstract class SpecialActionMeleeCard : MeleeCard
{
	private float specialAttackTimer;
	private Player selectedPlayer;

	private new void OnDrawGizmos()
	{
		base.OnDrawGizmos();

		if (GetParams().isSpecialActionOnPlayers)
		{
			RaycastHit[] hits = Physics.SphereCastAll(movementController.GetCameraTransform().position, 0.5f,
				movementController.GetCameraFollowTransform().forward, GetParams().specialActionPlayerMaxDistance);
			foreach (RaycastHit hit in hits)
			{
				if (hit.transform == player)
					continue;

				if (Vector3.Distance(player.position, hit.transform.position) <
				    GetParams().specialActionPlayerMinDistance)
					continue;

				Gizmos.color = Color.yellow;
				Gizmos.DrawWireSphere(hit.point, 0.5f);

				// break;
			}
		}
	}

	public override void UpdateCard(bool enableCardControl)
	{
		base.UpdateCard(enableCardControl);

		specialAttackTimer -= Time.deltaTime;
		if (specialAttackTimer > 0)
		{
			if (selectedPlayer != null)
			{
				SkinnedMeshRenderer renderer =
					GetActualModelChild(selectedPlayer.GetModel()).GetComponent<SkinnedMeshRenderer>();
				UnglowPlayer(renderer);
				selectedPlayer = null;
			}

			return;
		}

		if (GetParams().isSpecialActionOnPlayers)
		{
			RaycastHit[] hits = Physics.SphereCastAll(movementController.GetCameraTransform().position, 0.5f,
				movementController.GetCameraFollowTransform().forward, GetParams().specialActionPlayerMaxDistance);
			bool hitPlayer = false;
			foreach (RaycastHit hit in hits)
			{
				if (!hit.transform.CompareTag("Player"))
					continue;

				if (hit.transform == player)
					continue;

				if (Vector3.Distance(player.position, hit.transform.position) <
				    GetParams().specialActionPlayerMinDistance)
					continue;

				selectedPlayer = hit.transform.GetComponent<Player>();
				SkinnedMeshRenderer renderer =
					GetActualModelChild(selectedPlayer.GetModel()).GetComponent<SkinnedMeshRenderer>();
				GlowPlayer(renderer, GetParams().specialActionPlayerGlowColor,
					GetParams().specialActionPlayerGlowIntensity, GetParams().specialActionPlayerGlowiness);
				hitPlayer = true;

				break;
			}

			if (!hitPlayer && selectedPlayer != null)
			{
				SkinnedMeshRenderer renderer =
					GetActualModelChild(selectedPlayer.GetModel()).GetComponent<SkinnedMeshRenderer>();
				UnglowPlayer(renderer);
				selectedPlayer = null;
			}
		}

		if (enableCardControl && Input.GetButtonDown("SpecialAction"))
		{
			if (GetParams().isSpecialActionOnPlayers && selectedPlayer != null)
			{
				specialAttackTimer = 1 / GetParams().specialActionRate;
				specialAction(selectedPlayer.OwnerClientId);
			}
			else if (!GetParams().isSpecialActionOnPlayers)
			{
				specialAttackTimer = 1 / GetParams().specialActionRate;
				specialAction(Constants.nonPlayerID);
			}
		}
	}

	private Transform GetActualModelChild(Transform model)
	{
		Transform child0 = model.GetChild(0);
		Transform child1 = model.GetChild(1);
		if (child0.gameObject.name != "mixamorig:Hips")
			return child0;
		return child1;
	}

	protected void GlowPlayer(SkinnedMeshRenderer modelRenderer, Color color, float intensity, float glowiness)
	{
		foreach (Material mat in modelRenderer.materials)
		{
			Texture2D tex = new(1, 1, TextureFormat.RGBA32, false);
			tex.SetPixel(0, 0, color);
			tex.Apply();

			mat.SetTexture("_EmissiveColorMap", tex);
			mat.SetColor("_EmissiveColor", Color.white);
			mat.SetFloat("_UseEmissiveIntensity", intensity);
			mat.SetFloat("_EmissiveIntensity", intensity);
			mat.SetFloat("_EmissiveExposureWeight", glowiness);
			mat.EnableKeyword("_EMISSIVE_COLOR_MAP");
		}
	}

	protected void UnglowPlayer(SkinnedMeshRenderer modelRenderer)
	{
		foreach (Material mat in modelRenderer.materials) mat.DisableKeyword("_EMISSIVE_COLOR_MAP");
	}

	protected abstract void specialAction(ulong selectedPlayerID);

	protected SpecialActionMeleeCardParams GetParams()
	{
		return (SpecialActionMeleeCardParams)cardParams;
	}
}