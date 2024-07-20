using Unity.Netcode;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.UI;
using TMPro;

public class Player : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float friction;
    [SerializeField] private Transform cameraFollow;
    [SerializeField] private Slider HealthSlider;
    [SerializeField] private TextMeshProUGUI Name;

    private Card card;
    private bool spawned = false;

    public override void OnNetworkSpawn(){
        Chat.Singleton.Log($"Player {OwnerClientId} logged in");

        Name.text = $"Player {OwnerClientId}";
        HealthSlider.name = $"Slider{OwnerClientId}";

        if(!IsOwner)
            return;

        Application.targetFrameRate = 120;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GameObject.Find("CineCam").GetComponent<CinemachineCamera>().Follow = cameraFollow;

        transform.position = new Vector3(0, 2, -34);

        if(OwnerClientId == 0)
            ChooseCard(CardTypes.Valkyrie);
        else
            ChooseCard(CardTypes.Wizard);
    }

#region CardCreation
    private void ChooseCard(string cardName){
        spawned = false;

        SpawnCardRpc(cardName);
    }

    [Rpc(SendTo.Server)]
    private void SpawnCardRpc(string cardName){
        GameObject card = Instantiate(CardTypes.StringToCardPrefab(cardName));
        card.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId, true);

        SetCardRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void SetCardRpc(){
        int i = 0;
        foreach(GameObject card in GameObject.FindGameObjectsWithTag("Card")){
            card.name = $"Card{i}";
            i++;
        }

        card = GameObject.Find($"Card{OwnerClientId}").transform.GetComponent<Card>();
        card.StartCard(transform);
        
        card.SetSliderRpc($"Slider{OwnerClientId}");
    }
#endregion

    private void Update()
    {
        if(!IsOwner || !spawned)
            return;

        card.UpdateCard(rb, friction, cameraFollow);
    }

    public Card GetCard(){
        return card;
    }

    public void Spawned(){
        spawned = true;
    }
}