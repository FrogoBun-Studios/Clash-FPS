using Unity.Netcode;
using UnityEngine;
using Unity.Cinemachine;
using System;

public class Player : NetworkBehaviour
{
    public Rigidbody rb;
    public float friction;
    public Card card;
    public Transform cameraFollow;
    private bool spawned = false;

    public override void OnNetworkSpawn(){
        if(!IsOwner)
            return;

        Application.targetFrameRate = 120;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GameObject.Find("CineCam").GetComponent<CinemachineCamera>().Follow = cameraFollow;

        Chat.Singleton.Log("before start rpc: " + OwnerClientId);
        StartRpc(OwnerClientId);
        Chat.Singleton.Log("after start rpc: " + OwnerClientId);
        
        spawned = true;
    }

    [Rpc(SendTo.Everyone)]
    private void StartRpc(ulong playerId){
        Chat.Singleton.Log($"Player {playerId} logged in");

        card = gameObject.AddComponent<WizardCard>();
        card.StartCard(OwnerClientId);
    }

    private void Update()
    {
        if(!IsOwner || !spawned)
            return;

        card.UpdateCard();
    }

    [Rpc(SendTo.Server)]
    public void CreateModelRpc(ulong id){
        card.CreateModel(id);
    }
}