using Unity.Netcode;
using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class Player : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float friction;
    [SerializeField] private Transform cameraFollow;

    private Card card;
    private bool spawned = false;

    public override void OnNetworkSpawn(){
        Chat.Singleton.Log($"Player {OwnerClientId} logged in");

        if(!IsOwner)
            return;

        Application.targetFrameRate = 120;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GameObject.Find("CineCam").GetComponent<CinemachineCamera>().Follow = cameraFollow;

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
    }
#endregion

    private void Update()
    {
        if(!IsOwner || !spawned)
            return;

        card.UpdateCard(rb, friction, cameraFollow);
    }

    public void Spawned(){
        spawned = true;
    }

    private void OnDrawGizmos(){
        Gizmos.DrawWireSphere(transform.position - transform.right * 0.75f, 0.05f);
        Gizmos.DrawWireSphere(transform.position + transform.right * 0.75f, 0.05f);
    }
}