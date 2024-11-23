#if UNITY_EDITOR

using System.IO;
using System.Linq;

using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;

using UnityEditor;
using UnityEditor.Animations;

using UnityEngine;

public class CardCreator : EditorWindow
{
	private readonly AnimationClip[] _animations = new AnimationClip[8];
	private Avatar _avatar;
	private bool _cardCreated;
	private GameObject _cardPrefab;
	private GameObject _model;
	private GameObject _modelPrefab;
	private string _name = "";
	private NetworkBehaviour _script = null;

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
			_cardCreated = true;
		}

		if (_cardCreated)
			GUILayout.Label($"Please attach a card script to {_name}Card.prefab in Assets/Resources/Cards/{_name}.");

		GUILayout.Space(20);
		GUILayout.Label("Card Remover", headlineStyle);
		_name = EditorGUILayout.TextField("Card name", _name);
		GUILayout.Space(10);

		if (GUILayout.Button("Remove Card"))
		{
			RemoveCard();
			_cardCreated = false;
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
		_name = EditorGUILayout.TextField("Card name", _name);
		_model = (GameObject)EditorGUILayout.ObjectField("3D Model(Blender File)", _model, typeof(GameObject), false);

		GUILayout.Label("Card Animations", EditorStyles.boldLabel);
		_animations[0] =
			(AnimationClip)EditorGUILayout.ObjectField("Idle", _animations[0], typeof(AnimationClip), false);
		_animations[1] =
			(AnimationClip)EditorGUILayout.ObjectField("Running", _animations[1], typeof(AnimationClip), false);
		_animations[2] =
			(AnimationClip)EditorGUILayout.ObjectField("Jump", _animations[2], typeof(AnimationClip), false);
		_animations[3] =
			(AnimationClip)EditorGUILayout.ObjectField("Attack", _animations[3], typeof(AnimationClip), false);
		_animations[4] =
			(AnimationClip)EditorGUILayout.ObjectField("RunAttack", _animations[4], typeof(AnimationClip), false);
		_animations[5] =
			(AnimationClip)EditorGUILayout.ObjectField("RunJump", _animations[5], typeof(AnimationClip), false);
		_animations[6] =
			(AnimationClip)EditorGUILayout.ObjectField("RunJumpAttack", _animations[6], typeof(AnimationClip), false);
		_animations[7] =
			(AnimationClip)EditorGUILayout.ObjectField("Death", _animations[7], typeof(AnimationClip), false);

		_avatar = (Avatar)EditorGUILayout.ObjectField("Animator Avatar", _avatar, typeof(Avatar), false);
	}

	private void CreateFolder()
	{
		string folderPath = $"Assets/Resources/Cards/{_name}";

		if (!AssetDatabase.IsValidFolder(folderPath))
		{
			AssetDatabase.CreateFolder("Assets/Resources/Cards", $"{_name}");
			AssetDatabase.Refresh();
			Debug.Log("Card Creator: Folder created at " + folderPath);
		}
		else
		{
			Debug.LogError("Card Creator: Folder already exists at " + folderPath);
		}
	}

	private void CreateAnimator()
	{
		bool success = AssetDatabase.CopyAsset("Assets/Resources/CardsAssets/Valkyrie/Animator.controller",
			$"Assets/Resources/CardsAssets/{_name}/Animator.controller");
		if (success)
			Debug.Log("Card Creator: Animator controller created.");
		else
			Debug.LogError("Card Creator: Failed to create animator controller.");

		AssetDatabase.Refresh();

		AnimatorController animator = Resources.Load<AnimatorController>($"CardsAssets/{_name}/Animator");

		animator.layers[0].stateMachine.states[0].state.motion = _animations[0];
		animator.layers[0].stateMachine.states[1].state.motion = _animations[7];
		animator.layers[0].stateMachine.states[2].state.motion = _animations[3];
		animator.layers[0].stateMachine.states[3].state.motion = _animations[2];
		animator.layers[0].stateMachine.states[4].state.motion = _animations[1];
		animator.layers[0].stateMachine.states[5].state.motion = _animations[6];
		animator.layers[0].stateMachine.states[6].state.motion = _animations[5];
		animator.layers[0].stateMachine.states[7].state.motion = _animations[4];

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		Debug.Log("Card Creator: Animator controller set.");
	}

	private void CreateModelPrefab()
	{
		_modelPrefab = PrefabUtility.SaveAsPrefabAsset(_model,
			AssetDatabase.GenerateUniqueAssetPath($"Assets/Resources/CardsAssets/{_name}/{_name}ModelPrefab.prefab"));
		AssetDatabase.Refresh();

		Debug.Log("Card Creator: Model prefab created.");

		_modelPrefab.GetComponent<Animator>().runtimeAnimatorController =
			Resources.Load<AnimatorController>($"CardsAssets/{_name}/Animator");
		_modelPrefab.GetComponent<Animator>().avatar = _avatar;
		_modelPrefab.AddComponent<NetworkObject>();
		_modelPrefab.AddComponent<ClientNetworkTransform>().SyncRotAngleX = false;
		_modelPrefab.GetComponent<ClientNetworkTransform>().SyncRotAngleZ = false;
		_modelPrefab.GetComponent<ClientNetworkTransform>().SyncScaleX = false;
		_modelPrefab.GetComponent<ClientNetworkTransform>().SyncScaleY = false;
		_modelPrefab.GetComponent<ClientNetworkTransform>().SyncScaleZ = false;
		_modelPrefab.AddComponent<ClientNetworkAnimator>().Animator = _modelPrefab.GetComponent<Animator>();
		_modelPrefab.tag = "Model";

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		Debug.Log("Card Creator: Model prefab set.");
	}

	private void CreateCardScript()
	{
		string scriptTemplate =
			@"using UnityEngine;

public class " + _name + @"Card : Card
{
    public override void StartCard(Transform player)
    {
        base.StartCard(player, new CardParams(
            health: CardParamHelper.Health.Heavy,
            damage: CardParamHelper.Damage.MediumHigh,
            speed: CardParamHelper.Speed.Medium,
            JumpStrength: CardParamHelper.JumpStrength.MediumHigh,
            jumps: 2,
            flying: false,
            AttackRate: CardParamHelper.AttackRate.Medium,
            side: Side.Blue,
            ColliderRadius: CardParamHelper.Collider.Radius,
            ColliderHeight: CardParamHelper.Collider.Height,
            ColliderYOffset: CardParamHelper.Collider.YOffset
        ), """ + _name + @""");
    }

    public override int GetElixerCost() => 5;
}";

		File.WriteAllText($"Assets/Scripts/Cards/{_name}Card.cs", scriptTemplate);
		AssetDatabase.Refresh();

		Debug.Log($"Card Creator: {_name}Card.cs created.");
	}

	private void CreateCardPrefab()
	{
		GameObject cardPrefab = new();
		cardPrefab.AddComponent<NetworkObject>();
		cardPrefab.tag = "Card";

		_cardPrefab = PrefabUtility.SaveAsPrefabAsset(cardPrefab,
			AssetDatabase.GenerateUniqueAssetPath($"Assets/Resources/CardsPrefabs/{_name}/{_name}Card.prefab"));
		Debug.Log($"Card Creator: {_name}Card.prefab created.");

		Undo.DestroyObjectImmediate(cardPrefab);
		Debug.Log(
			"Card Creator: Deleted \"New GameObject\" from scene, if something important was deleted, you can undo with ctrl-z");

		AssetDatabase.Refresh();

		Debug.Log($"Card Creator: {_name}Card.prefab created.");
		Debug.Log($"Card Creator: Don't forget to add a card script to the card- {_name}Card.prefab.");
	}

	private void EditNetworkPrefabs()
	{
		Resources.Load<NetworkPrefabsList>("NetworkPrefabs").Add(new NetworkPrefab { Prefab = _modelPrefab });
		Resources.Load<NetworkPrefabsList>("NetworkPrefabs").Add(new NetworkPrefab { Prefab = _cardPrefab });
	}

	private void CreateCard()
	{
		CreateFolder();
		CreateAnimator();
		CreateModelPrefab();
		CreateCardPrefab();
		EditNetworkPrefabs();
	}

	#endregion

	#region CardRemoval

	private void RemoveFolder()
	{
		string folderPath = $"Assets/Resources/CardsAssets/{_name}";

		if (Directory.Exists(folderPath))
		{
			Directory.Delete(folderPath, true);
			Debug.Log($"Card Creator: Card folder and its contents deleted at path {folderPath}.");
		}
		else
		{
			Debug.LogWarning($"Card folder does not exist at path {folderPath}.");
		}

		AssetDatabase.Refresh();
	}

	private void RemoveCardScript()
	{
		string path = $"Assets/Scripts/Cards/{_name}Card.cs";
		File.Delete(path);

		Debug.Log("Card Creator: Card script deleted.");
	}

	private void RemoveFromNetworkPrefabs()
	{
		NetworkPrefabsList list = Resources.Load<NetworkPrefabsList>("NetworkPrefabs");
		list.Remove(list.PrefabList.ToList().Find(x => x.Prefab == _modelPrefab));
		list.Remove(list.PrefabList.ToList().Find(x => x.Prefab == _cardPrefab));

		Debug.Log("Card Creator: Prefabs removed from NetworkPrefabs list.");
	}

	private void RemoveCard()
	{
		RemoveFolder();
		RemoveFromNetworkPrefabs();
	}

	#endregion
}
#endif