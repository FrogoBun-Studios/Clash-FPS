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
    private Transform model;
    private Animator animator;

    public override void OnNetworkSpawn(){
        Chat.Singleton.Log($"Player {OwnerClientId} logged in");

        if(!IsOwner)
            return;

        Application.targetFrameRate = 120;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GameObject.Find("CineCam").GetComponent<CinemachineCamera>().Follow = cameraFollow;

        if(OwnerClientId == 0)
            ChooseCardRpc(CardTypes.Valkyrie);
        else
            ChooseCardRpc(CardTypes.Wizard);
    }

    [Rpc(SendTo.Everyone)]
    private void ChooseCardRpc(string cardName){
        spawned = false;

        card = CardTypes.StringToCard(cardName);
        card.StartCard(transform, IsOwner, OwnerClientId);

        CreateModelRpc();
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
        int i = 0;
        foreach(GameObject model in GameObject.FindGameObjectsWithTag("Model")){
            model.name = $"Model{i}";
            i++;
        }

        model = GameObject.Find($"Model{OwnerClientId}").transform;
        animator = model.GetComponent<Animator>();
        
        spawned = true;
    }

    [Rpc(SendTo.Server)]
    private void updateModelRpc(){
        model.position = transform.position;
        model.localEulerAngles = transform.localEulerAngles;
    }

    public void updateModel(){
        updateModelRpc();
    }

    [Rpc(SendTo.Server)]
    private void updateAnimatorRpc(bool Moving, bool Attack, bool Death, float AttackBlend){
        animator.SetBool("Moving", Moving);
        animator.SetFloat("AttackBlend", AttackBlend);

        if(Attack)
            animator.SetTrigger("Attack");

        if(Death)
            animator.SetTrigger("Death");
    }

    public void updateAnimator(bool Moving, bool Attack, bool Death, float AttackBlend){
        updateAnimatorRpc(Moving, Attack, Death, AttackBlend);
    }

    private void OnDrawGizmos(){
        Gizmos.DrawWireSphere(transform.position - transform.right * 0.75f - transform.up * 0.1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position + transform.right * 0.75f - transform.up * 0.1f, 0.25f);
    }
}