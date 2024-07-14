using Unity.Netcode;
using UnityEngine;
using Unity.Cinemachine;

public class Player : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float friction;
    [SerializeField] private Transform cameraFollow;

    private Card card;
    private bool spawned = false;

    public override void OnNetworkSpawn(){
        if(!IsOwner)
            return;

        Application.targetFrameRate = 120;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GameObject.Find("CineCam").GetComponent<CinemachineCamera>().Follow = cameraFollow;

        StartRpc(OwnerClientId);
        CreateModelRpc();
        
        // spawned = true;
    }

    [Rpc(SendTo.Everyone)]
    private void StartRpc(ulong playerId){
        Chat.Singleton.Log($"Player {playerId} logged in");

        card = new WizardCard();
        card.StartCard(transform, IsOwner, OwnerClientId);
    }

    private void Update()
    {
        if(!IsOwner || !spawned)
            return;

        card.UpdateCard(rb, friction, cameraFollow);
    }

    [Rpc(SendTo.Server)]
    private void CreateModelRpc(){
        card.CreateModel();
        SetModelRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void SetModelRpc(){
        card.SetModel();
    }

    [Rpc(SendTo.Server)]
    private void updateModelRpc(){
        GameObject.Find($"Model{OwnerClientId}").transform.position = transform.position;
        GameObject.Find($"Model{OwnerClientId}").transform.localEulerAngles = transform.localEulerAngles;
    }

    public void updateModel(){
        updateModelRpc();
    }

    [Rpc(SendTo.Server)]
    private void updateAnimatorRpc(bool Moving, bool Attack, bool Death){
        GameObject.Find($"Model{OwnerClientId}").GetComponent<Animator>().SetBool("Moving", Moving);

        if(Attack)
            GameObject.Find($"Model{OwnerClientId}").GetComponent<Animator>().SetTrigger("Attack");

        if(Death)
            GameObject.Find($"Model{OwnerClientId}").GetComponent<Animator>().SetTrigger("Death");
    }

    public void updateAnimator(bool Moving, bool Attack, bool Death){
        updateAnimatorRpc(Moving, Attack, Death);
    }

    public void Spawned(){
        spawned = true;
    }
}