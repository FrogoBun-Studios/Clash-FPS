using Unity.Netcode;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.UI;
using TMPro;

public class Player : NetworkBehaviour
{
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform cameraFollow;
    [SerializeField] private Slider HealthSlider;
    [SerializeField] private TextMeshProUGUI Name;

    private Card card;
    private Animator animator;
    private float yVelocity = 0;
    private bool spawned = false;
    private int jumpsLeft;
    
    public override void OnNetworkSpawn(){
        Chat.Singleton.Log($"Player {OwnerClientId} logged in");

        Name.text = $"Player {OwnerClientId}";
        HealthSlider.name = $"Slider{OwnerClientId}";

        if(!IsOwner)
            return;

        Destroy(HealthSlider.gameObject);
        Destroy(Name.gameObject);

        Application.targetFrameRate = 120;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GameObject.Find("CineCam").GetComponent<CinemachineCamera>().Follow = cameraFollow;

        Teleport(new Vector3(0, 2, -34));

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
        foreach(GameObject cardGO in GameObject.FindGameObjectsWithTag("Card")){
            cardGO.name = $"Card{i}";
            Card card = cardGO.GetComponent<Card>();
            GameObject.FindGameObjectsWithTag("Player")[i].GetComponent<Player>().SetCard(card);

            if(!card.IsStarted()){
                card.StartCard(GameObject.FindGameObjectsWithTag("Player")[i].transform);
                card.SetSliders($"Slider{i}");
            }

            i++;
        }
    }

    [Rpc(SendTo.Everyone)]
    public void SetAnimatorRpc(string name){
        animator = GameObject.Find(name).GetComponent<Animator>();
    }
#endregion

    private void Update()
    {
        if(!IsOwner || !spawned)
            return;

        card.UpdateCard();
    }

#region Movement
    public void ControlCharacter(float speed, int jumps, float JumpStrength){
        Move(speed);
        Look();

        if(Input.GetButtonDown("Jump")){
            if(controller.isGrounded)
                jumpsLeft = jumps;

            if(jumpsLeft > 0){
                yVelocity = JumpStrength;
                animator.SetTrigger("Jump");
                jumpsLeft--;
            }
        }
    }

    private void Move(float speed){
        Vector3 movementDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        yVelocity += Physics.gravity.y * Time.deltaTime;
        controller.Move(transform.right * movementDir.x * speed * Time.deltaTime
                        + Vector3.up * yVelocity * Time.deltaTime
                        + transform.forward * movementDir.z * speed * Time.deltaTime);

        if(controller.isGrounded)
            yVelocity = 0;
        
        animator.SetBool("Moving", movementDir != Vector3.zero);
        if(movementDir.z != 0)
            animator.SetFloat("Speed", Utils.MagnitudeInDirection(controller.velocity, transform.forward) / 6.6f);
        else
            animator.SetFloat("Speed", Mathf.Abs(Utils.MagnitudeInDirection(controller.velocity, transform.right)) >= 0.2f ? 1 : 0);
    }

    private void Look(){
        transform.localEulerAngles = new Vector3(0, transform.rotation.eulerAngles.y + Input.GetAxis("Mouse X"), 0);

        float xAngle = cameraFollow.rotation.eulerAngles.x;
        if(xAngle >= 180)
            xAngle -= 360;

        cameraFollow.localEulerAngles = new Vector3(Mathf.Clamp(xAngle - Input.GetAxis("Mouse Y"), -40, 75), 0, 0);
    }

    private void Teleport(Vector3 pos){
        controller.enabled = false;
        transform.position = pos;
        controller.enabled = true;
    }
#endregion

    public Card GetCard(){
        return card;
    }

    public void SetCard(Card card){
        this.card = card;
    }

    public Quaternion GetCameraRotation(){
        return cameraFollow.rotation;
    }

    public Vector3 GetCameraForward(){
        return cameraFollow.forward;
    }

    public void Spawned(){
        spawned = true;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit){
        if(IsOwner && hit.gameObject.CompareTag("WaterCols"))
            card.DamageRpc(Mathf.Infinity);
    }
}