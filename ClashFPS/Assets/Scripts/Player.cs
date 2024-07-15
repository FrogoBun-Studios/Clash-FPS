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
    private void updateAnimatorRpc(bool[] AnimatorParams){
        animator.SetFloat("MovingBlend", AnimatorParams[0] ? 1 : 0);
        animator.SetFloat("AttackBlend", AnimatorParams[1] ? 1 : 0);
        animator.SetFloat("JumpBlend", AnimatorParams[2] ? 1 : 0);

        animator.SetBool("Idle", !(AnimatorParams[0] || AnimatorParams[1] || AnimatorParams[2]));

        if(AnimatorParams[3])
            animator.SetTrigger("Death");
    }

    public void updateAnimator(bool[] AnimatorParams){
        updateAnimatorRpc(AnimatorParams);
    }

    private void OnDrawGizmos(){
        Gizmos.DrawWireSphere(transform.position - transform.right * 0.75f, 0.05f);
        Gizmos.DrawWireSphere(transform.position + transform.right * 0.75f, 0.05f);
    }
}