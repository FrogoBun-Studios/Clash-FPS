using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using Unity.Netcode;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using System.IO;
using System.Linq;

public class CardCreator : EditorWindow
{
    private string name = "";
    private GameObject model = null;
    private AnimationClip[] animations = new AnimationClip[8];
    private Avatar avatar = null;
    private NetworkBehaviour script = null;
    private GameObject modelPrefab = null;
    private GameObject cardPrefab = null;
    private bool cardCreated = false;

    [MenuItem("Window/Card Creator")]
    public static void ShowWindow(){
        GetWindow<CardCreator>("Card Creator");
    }

    #region CardCreation

    private void GetInfo(){
        name = EditorGUILayout.TextField("Card name", name);
        model = (GameObject)EditorGUILayout.ObjectField("3D Model(Blender File)", model, typeof(GameObject), false);

        GUILayout.Label("Card Animations", EditorStyles.boldLabel);
        animations[0] = (AnimationClip)EditorGUILayout.ObjectField("Idle", animations[0], typeof(AnimationClip), false);
        animations[1] = (AnimationClip)EditorGUILayout.ObjectField("Running", animations[1], typeof(AnimationClip), false);
        animations[2] = (AnimationClip)EditorGUILayout.ObjectField("Jump", animations[2], typeof(AnimationClip), false);
        animations[3] = (AnimationClip)EditorGUILayout.ObjectField("Attack", animations[3], typeof(AnimationClip), false);
        animations[4] = (AnimationClip)EditorGUILayout.ObjectField("RunAttack", animations[4], typeof(AnimationClip), false);
        animations[5] = (AnimationClip)EditorGUILayout.ObjectField("RunJump", animations[5], typeof(AnimationClip), false);
        animations[6] = (AnimationClip)EditorGUILayout.ObjectField("RunJumpAttack", animations[6], typeof(AnimationClip), false);
        animations[7] = (AnimationClip)EditorGUILayout.ObjectField("Death", animations[7], typeof(AnimationClip), false);

        avatar = (Avatar)EditorGUILayout.ObjectField("Animator Avatar", avatar, typeof(Avatar), false);
    }

    private void CreateFolder(){
        string folderPath = $"Assets/Resources/Cards/{name}";

        if (!AssetDatabase.IsValidFolder(folderPath)){
            AssetDatabase.CreateFolder("Assets/Resources/Cards", $"{name}");
            AssetDatabase.Refresh();
            Debug.Log("Card Creator: Folder created at " + folderPath);
        }
        else
            Debug.LogError("Card Creator: Folder already exists at " + folderPath);
    }

    private void CreateAnimator(){
        bool success = AssetDatabase.CopyAsset("Assets/Resources/Cards/Valkyrie/Animator.controller", $"Assets/Resources/{name}/Animator.controller");
        if (success)
            Debug.Log("Card Creator: Animator controller created.");
        else
            Debug.LogError("Card Creator: Failed to create animator controller.");

        AssetDatabase.Refresh();

        AnimatorController animator = Resources.Load<AnimatorController>($"Cards/{name}/Animator");

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

    private void CreateModelPrefab(){
        modelPrefab = PrefabUtility.SaveAsPrefabAsset(model, AssetDatabase.GenerateUniqueAssetPath($"Assets/Resources/Cards/{name}/{name}ModelPrefab.prefab"));
        AssetDatabase.Refresh();

        Debug.Log("Card Creator: Model prefab created.");

        modelPrefab.GetComponent<Animator>().runtimeAnimatorController = Resources.Load<AnimatorController>($"Cards/{name}/Animator");
        modelPrefab.GetComponent<Animator>().avatar = avatar;
        modelPrefab.AddComponent<NetworkObject>();
        modelPrefab.AddComponent<ClientNetworkTransform>().SyncRotAngleX = false;
        modelPrefab.GetComponent<ClientNetworkTransform>().SyncRotAngleZ = false;
        modelPrefab.GetComponent<ClientNetworkTransform>().SyncScaleX = false;
        modelPrefab.GetComponent<ClientNetworkTransform>().SyncScaleY = false;
        modelPrefab.GetComponent<ClientNetworkTransform>().SyncScaleZ = false;
        modelPrefab.AddComponent<ClientNetworkAnimator>().Animator = modelPrefab.GetComponent<Animator>();
        modelPrefab.tag = "Model";

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Card Creator: Model prefab set.");
    }

    private void CreateCardScript(){
        string scriptTemplate = 
@"using UnityEngine;

public class " + name + @"Card : Card
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
        ), """ + name + @""");
    }

    public override int GetElixerCost() => 5;
}";

        File.WriteAllText($"Assets/Scripts/Cards/{name}Card.cs", scriptTemplate);
        AssetDatabase.Refresh();

        Debug.Log($"Card Creator: {name}Card.cs created.");
    }

    private void CreateCardPrefab(){
        GameObject cardPrefab = new GameObject();
        cardPrefab.AddComponent<NetworkObject>();
        cardPrefab.tag = "Card";

        this.cardPrefab = PrefabUtility.SaveAsPrefabAsset(cardPrefab, AssetDatabase.GenerateUniqueAssetPath($"Assets/Resources/Cards/{name}/{name}Card.prefab"));
        Debug.Log($"Card Creator: {name}Card.prefab created.");

        Undo.DestroyObjectImmediate(cardPrefab);
        Debug.Log($"Card Creator: Deleted \"New GameObject\" from scene, if something important was deleted, you can undo with ctrl-z");

        AssetDatabase.Refresh();

        Debug.Log($"Card Creator: {name}Card.prefab created.");
        Debug.Log($"Card Creator: Don't forget to add a card script to the card- {name}Card.prefab.");
    }

    private void EditNetworkPrefabs(){
        Resources.Load<NetworkPrefabsList>("NetworkPrefabs").Add(new NetworkPrefab{ Prefab = modelPrefab });
        Resources.Load<NetworkPrefabsList>("NetworkPrefabs").Add(new NetworkPrefab{ Prefab = cardPrefab });
    }

    private void EditCardTypes(){
        string path = $"Assets/Scripts/Helpers/CardTypes.cs";

        string scriptContent = File.ReadAllText(path);
        
        int lastConstIndex = scriptContent.LastIndexOf("public const string ");
        int insertPosition = scriptContent.IndexOf(';', lastConstIndex) + 1;
        string newConstant = $"\n    public const string {name} = \"{name}\";";

        scriptContent = scriptContent.Insert(insertPosition, newConstant);

        File.WriteAllText(path, scriptContent);
        AssetDatabase.Refresh();

        Debug.Log("Card Creator: CardTypes edited successfully.");
    }

    private void CreateCard(){
        CreateFolder();
        CreateAnimator();
        CreateModelPrefab();
        CreateCardPrefab();
        EditNetworkPrefabs();
    }

    #endregion
    #region CardRemoval

    private void RemoveFolder(){
        string folderPath = $"Assets/Resources/Cards/{name}";

        if (Directory.Exists(folderPath))
        {
            Directory.Delete(folderPath, true);
            Debug.Log($"Card Creator: Card folder and its contents deleted at path {folderPath}.");
        }
        else
            Debug.LogWarning($"Card folder does not exist at path {folderPath}.");

        AssetDatabase.Refresh();
    }

    private void RemoveCardScript(){
        string path = $"Assets/Scripts/Cards/{name}Card.cs";
        File.Delete(path);

        Debug.Log("Card Creator: Card script deleted.");
    }

    private void RemoveFromNetworkPrefabs(){
        NetworkPrefabsList list = Resources.Load<NetworkPrefabsList>("NetworkPrefabs");
        list.Remove(list.PrefabList.ToList().Find(x => x.Prefab == modelPrefab));
        list.Remove(list.PrefabList.ToList().Find(x => x.Prefab == cardPrefab));

        Debug.Log("Card Creator: Prefabs removed from NetworkPrefabs list.");
    }

    private void RemoveCardType(){
        string path = $"Assets/Scripts/Helpers/CardTypes.cs";
        string scriptContent = File.ReadAllText(path);

        int lastConstIndex = scriptContent.LastIndexOf("public const string ");
        int endOfLineIndex = scriptContent.IndexOf(';', lastConstIndex) + 1;

        scriptContent = scriptContent.Remove(lastConstIndex, endOfLineIndex - lastConstIndex);

        File.WriteAllText(path, scriptContent);
        AssetDatabase.Refresh();

        Debug.Log("Card Creator: CardTypes edited successfully.");
    }

    private void RemoveCard(){
        RemoveFolder();
        RemoveFromNetworkPrefabs();
    }

    #endregion

    private void OnGUI(){
        GUIStyle headlineStyle = new GUIStyle(GUI.skin.label)
        {
            padding = new RectOffset(10, 10, 10, 10),
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        GUILayout.Label("Card Creator", headlineStyle);
        GetInfo();
        GUILayout.Space(10);

        if(GUILayout.Button("Create Card")){
            CreateCard();
            cardCreated = true;
        }
        if(cardCreated)
            GUILayout.Label($"Please attach a card script to {name}Card.prefab in Assets/Resources/Cards/{name}.");

        GUILayout.Space(20);
        GUILayout.Label("Card Remover", headlineStyle);
        name = EditorGUILayout.TextField("Card name", name);
        GUILayout.Space(10);

        if(GUILayout.Button("Remove Card")){
            RemoveCard();
            cardCreated = false;
        }
    }
}
