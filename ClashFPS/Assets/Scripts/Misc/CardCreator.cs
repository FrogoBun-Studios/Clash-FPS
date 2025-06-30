#if UNITY_EDITOR

using System.IO;
using System.Linq;

using Unity.Netcode;
using Unity.Netcode.Components;

using UnityEditor;
using UnityEditor.Animations;

using UnityEngine;

public class CardCreator : EditorWindow
{
	private readonly AnimationClip[] animations = new AnimationClip[8];
	private Avatar avatar;
	private bool cardCreated;
	private GameObject cardPrefab;
	private GameObject model;
	private GameObject modelPrefab;
	private string cardName = "";
	private NetworkBehaviour script = null;

	private void OnGUI()
	{
		GUIStyle headlineStyle = new(GUI.skin.label)
		{
			padding = new RectOffset(10, 10, 10, 10),
			fontSize = 20,
			fontStyle = FontStyle.Bold,
			alignment = TextAnchor.MiddleCenter
		};

		GUILayout.Label("Card Creator", headlineStyle);
		GetInfo();
		GUILayout.Space(10);

		if (GUILayout.Button("Create Card"))
		{
			CreateCard();
			cardCreated = true;
		}

		if (cardCreated)
		{
			GUILayout.Label("Card created,");
			GUILayout.Label(
				$"Please attach a card script to {cardName}Card.prefab at Assets/Resources/CardsPrefabs/{cardName}.");
		}

		GUILayout.Space(20);
		GUILayout.Label("Card Remover", headlineStyle);
		cardName = EditorGUILayout.TextField("Card name", cardName);
		GUILayout.Space(10);

		if (GUILayout.Button("Remove Card"))
		{
			RemoveCard();
			cardCreated = false;
		}
	}

	[MenuItem("Window/Card Creator")]
	public static void ShowWindow()
	{
		GetWindow<CardCreator>("Card Creator");
	}

	#region CardCreation

	private void GetInfo()
	{
		cardName = EditorGUILayout.TextField("Card name", cardName);
		model = (GameObject)EditorGUILayout.ObjectField("3D Model(Blender File)", model, typeof(GameObject), false);

		GUILayout.Label("Card Animations", EditorStyles.boldLabel);
		animations[0] =
			(AnimationClip)EditorGUILayout.ObjectField("Idle", animations[0], typeof(AnimationClip), false);
		animations[1] =
			(AnimationClip)EditorGUILayout.ObjectField("Running", animations[1], typeof(AnimationClip), false);
		animations[2] =
			(AnimationClip)EditorGUILayout.ObjectField("Jump", animations[2], typeof(AnimationClip), false);
		animations[3] =
			(AnimationClip)EditorGUILayout.ObjectField("Attack", animations[3], typeof(AnimationClip), false);
		animations[4] =
			(AnimationClip)EditorGUILayout.ObjectField("RunAttack", animations[4], typeof(AnimationClip), false);
		animations[5] =
			(AnimationClip)EditorGUILayout.ObjectField("RunJump", animations[5], typeof(AnimationClip), false);
		animations[6] =
			(AnimationClip)EditorGUILayout.ObjectField("RunJumpAttack", animations[6], typeof(AnimationClip), false);
		animations[7] =
			(AnimationClip)EditorGUILayout.ObjectField("Death", animations[7], typeof(AnimationClip), false);

		avatar = (Avatar)EditorGUILayout.ObjectField("Animator Avatar", avatar, typeof(Avatar), false);
	}

	private void CreateFolder(string path, string name)
	{
		string folderPath = "Assets/Resources/" + path + name;

		if (!AssetDatabase.IsValidFolder(folderPath))
		{
			AssetDatabase.CreateFolder("Assets/Resources/" + path, $"{name}");
			AssetDatabase.Refresh();
			Debug.Log("Card Creator: Folder created at " + folderPath);
		}
		else
			Debug.LogError("Card Creator: Folder already exists at " + folderPath);
	}

	private void CreateAnimator()
	{
		bool success = AssetDatabase.CopyAsset("Assets/Resources/CardsAssets/Valkyrie/Animator.controller",
			$"Assets/Resources/CardsAssets/{cardName}/Animator.controller");
		if (success)
			Debug.Log("Card Creator: Animator controller created.");
		else
			Debug.LogError("Card Creator: Failed to create animator controller.");

		AssetDatabase.Refresh();

		AnimatorController animator = Resources.Load<AnimatorController>($"CardsAssets/{cardName}/Animator");

		animator.layers[0].stateMachine.states[0].state.motion = animations[0];
		animator.layers[0].stateMachine.states[1].state.motion = animations[7];
		animator.layers[0].stateMachine.states[2].state.motion = animations[3];
		animator.layers[0].stateMachine.states[3].state.motion = animations[2];
		animator.layers[0].stateMachine.states[4].state.motion = animations[1];
		animator.layers[0].stateMachine.states[5].state.motion = animations[6];
		animator.layers[0].stateMachine.states[6].state.motion = animations[5];
		animator.layers[0].stateMachine.states[7].state.motion = animations[4];

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		Debug.Log("Card Creator: Animator controller set.");
	}

	private void CreateModelPrefab()
	{
		modelPrefab = PrefabUtility.SaveAsPrefabAsset(model,
			AssetDatabase.GenerateUniqueAssetPath(
				$"Assets/Resources/CardsAssets/{cardName}/{cardName}ModelPrefab.prefab"));
		AssetDatabase.Refresh();

		Debug.Log("Card Creator: Model prefab created.");

		modelPrefab.GetComponent<Animator>().runtimeAnimatorController =
			Resources.Load<AnimatorController>($"CardsAssets/{cardName}/Animator");
		modelPrefab.GetComponent<Animator>().avatar = avatar;
		modelPrefab.AddComponent<NetworkObject>();
		modelPrefab.AddComponent<NetworkTransform>().SyncRotAngleX = false;
		modelPrefab.GetComponent<NetworkTransform>().SyncRotAngleZ = false;
		modelPrefab.GetComponent<NetworkTransform>().SyncScaleX = false;
		modelPrefab.GetComponent<NetworkTransform>().SyncScaleY = false;
		modelPrefab.GetComponent<NetworkTransform>().SyncScaleZ = false;
		modelPrefab.AddComponent<NetworkAnimator>().Animator = modelPrefab.GetComponent<Animator>();
		modelPrefab.tag = "Model";

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		Debug.Log("Card Creator: Model prefab set.");
	}

	private void CreateCardPrefab()
	{
		GameObject cardPrefab = new();
		cardPrefab.AddComponent<NetworkObject>();
		cardPrefab.tag = "Card";

		this.cardPrefab =
			PrefabUtility.SaveAsPrefabAsset(cardPrefab, $"Assets/Resources/CardsPrefabs/{cardName}.prefab");
		Debug.Log($"Card Creator: {cardName}Card.prefab created.");

		Undo.DestroyObjectImmediate(cardPrefab);
		Debug.Log(
			"Card Creator: Deleted \"New GameObject\" from scene, if something important was deleted, you can undo with ctrl-z");

		AssetDatabase.Refresh();

		Debug.Log($"Card Creator: {cardName}Card.prefab created.");
		Debug.Log($"Card Creator: Don't forget to add a card script to the card- {cardName}Card.prefab.");
	}

	private void EditNetworkPrefabs()
	{
		Resources.Load<NetworkPrefabsList>("NetworkPrefabs").Add(new NetworkPrefab { Prefab = modelPrefab });
		Resources.Load<NetworkPrefabsList>("NetworkPrefabs").Add(new NetworkPrefab { Prefab = cardPrefab });
	}

	private void CreateCard()
	{
		CreateFolder("CardAssets/", cardName);
		CreateAnimator();
		CreateModelPrefab();
		CreateCardPrefab();
		EditNetworkPrefabs();
	}

	#endregion

	#region CardRemoval

	private void RemoveFolder(string path)
	{
		string folderPath = "Assets/Resources/" + path;

		if (Directory.Exists(folderPath))
		{
			Directory.Delete(folderPath, true);
			Debug.Log($"Card Creator: Card folder and its contents deleted at path {folderPath}.");
		}
		else
			Debug.LogWarning($"Card folder does not exist at path {folderPath}.");

		AssetDatabase.Refresh();
	}

	private void RemoveCardPrefab()
	{
		if (AssetDatabase.DeleteAsset($"Assets/Resources/CardsPrefabs/{cardName}.prefab"))
			Debug.Log("Card Creator: Card prefab successfully deleted.");
		else
			Debug.LogWarning("Card Creator: Failed to delete card prefab.");

		if (AssetDatabase.DeleteAsset($"Assets/Resources/CardsPrefabs/{cardName}Params.asset"))
			Debug.Log("Card Creator: Card params successfully deleted.");
		else
			Debug.LogWarning("Card Creator: Failed to delete card params.");
	}

	private void RemoveFromNetworkPrefabs()
	{
		NetworkPrefabsList list = Resources.Load<NetworkPrefabsList>("NetworkPrefabs");
		list.Remove(list.PrefabList.ToList().Find(x => x.Prefab == modelPrefab));
		list.Remove(list.PrefabList.ToList().Find(x => x.Prefab == cardPrefab));

		Debug.Log("Card Creator: Prefabs removed from NetworkPrefabs list.");
	}

	private void RemoveCard()
	{
		RemoveFolder($"CardAssets/{cardName}");
		RemoveCardPrefab();
		RemoveFromNetworkPrefabs();
	}

	#endregion
}
#endif