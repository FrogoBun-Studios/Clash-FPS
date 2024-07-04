using Unity.Netcode;
using UnityEngine;
using Unity.Cinemachine;

public class Player : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float friction;
    [SerializeField] private Card card;
    [SerializeField] private Transform cameraFollow;
    private bool spawned = false;

    public override void OnNetworkSpawn(){
        if(!IsOwner)
            return;

        Application.targetFrameRate = 120;
        Chat.Singleton.Log($"Player {OwnerClientId} logged in");
        gameObject.name = "Player" + OwnerClientId;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GameObject.Find("CineCam").GetComponent<CinemachineCamera>().Follow = cameraFollow;

        Chat.Singleton.Log($"Player {OwnerClientId} is Creating a card");
        card = new WizardCard();
        card.StartCard(transform);

        spawned = true;
    }

    private void Update()
    {
        if(!IsOwner || !spawned)
            return;

        card.UpdateCard(transform, rb, friction, cameraFollow);
    }
}