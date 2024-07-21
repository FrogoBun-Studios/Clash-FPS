using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine;
using System.Collections;

public abstract class Card : NetworkBehaviour
{
    private GameObject ModelPrefab;
    protected Transform player;
    private Player PlayerScript;
    private Transform model;
    private Animator animator;
    protected CardParams Params;
    private Slider HealthSlider;
    
    protected float attackTimer;
    protected int jumpsLeft;

    public virtual void StartCard(Transform player, CardParams Params, string ModelName){
        this.Params = Params;
        this.ModelPrefab = Resources.Load($"{ModelName}/ModelPrefab") as GameObject;
        this.player = player;
        this.PlayerScript = player.GetComponent<Player>();
        this.jumpsLeft = Params.jumps;

        if(!IsOwner)
            return;

        CreateModelRpc();

        attackTimer = 1 / Params.AttackRate;
    }

    public abstract void StartCard(Transform player);

    
#region Update
    public virtual void UpdateCard(Rigidbody rb, float friction, Transform cameraFollow, LayerMask NotPlayerLayer){
        if(Params.health <= 0)
            return;

        Move(rb, friction);
        StepClimb(rb, NotPlayerLayer);
        Look(cameraFollow);

        attackTimer -= Time.deltaTime;
        if(Input.GetMouseButtonDown(0) && attackTimer <= 0){
            attackTimer = 1 / Params.AttackRate;
            Attack();
        }
        

        if(Input.GetKeyDown(KeyCode.Space)){
            if(OnGround())
                jumpsLeft = Params.jumps;

            if(jumpsLeft > 0){
                rb.AddForce(Params.JumpStrength * player.up, ForceMode.Impulse);
                animator.SetTrigger("Jump");
                jumpsLeft--;
            }
        }

        model.position = player.position;
        model.localEulerAngles = player.localEulerAngles;
    }

    protected virtual void Move(Rigidbody rb, float friction){
        Vector3 movementDir = new Vector3();//new Vector3(MoveInput.action.ReadValue<Vector2>().x, 0, MoveInput.action.ReadValue<Vector2>().y);

        if(Input.GetKey(KeyCode.W))
            movementDir.z = 1;
        else if(Input.GetKey(KeyCode.S))
            movementDir.z = -1;
        if(Input.GetKey(KeyCode.D))
            movementDir.x = 1;
        else if(Input.GetKey(KeyCode.A))
            movementDir.x = -1;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x * friction, rb.linearVelocity.y, rb.linearVelocity.z * friction);
        rb.linearVelocity += player.forward * movementDir/*.normalized*/.z * Params.speed
                           + player.right * movementDir/*.normalized*/.x * Params.speed;

        animator.SetBool("Moving", movementDir != Vector3.zero);

        if(Mathf.Abs(MagnitudeInDirection(rb.linearVelocity, player.transform.forward)) >= 0.2f)
            animator.SetFloat("Speed", MagnitudeInDirection(rb.linearVelocity, player.transform.forward) / 6.6f);
        else
            animator.SetFloat("Speed", Mathf.Abs(MagnitudeInDirection(rb.linearVelocity, player.transform.right)) >= 0.2f ? 1 : 0);
    }

    protected void Look(Transform cameraFollow){
        player.localEulerAngles = new Vector3(0, player.rotation.eulerAngles.y + Input.GetAxis("Mouse X"), 0);

        float xAngle = cameraFollow.rotation.eulerAngles.x;
        if(xAngle >= 180)
            xAngle -= 360;

        cameraFollow.localEulerAngles = new Vector3(Mathf.Clamp(xAngle - Input.GetAxis("Mouse Y"), -40, 75), 0, 0);
    }

    protected void StepClimb(Rigidbody rb, LayerMask NotPlayerLayer){
        if (Physics.OverlapBox(player.position - player.up * 0.55f, new Vector3(1.5f, 0.3f, 1.5f) / 2, Quaternion.identity, NotPlayerLayer).Length > 0)
        {
            Vector3 targetVector = new Vector3(rb.position.x, rb.position.y + 0.01f, rb.position.z);
            // rb.position = Vector3.Lerp(rb.position, targetVector, Time.deltaTime / 0.01f);
            rb.position = targetVector;
        }
    }
#endregion

#region Misc
    protected bool OnGround(){
        return Physics.OverlapSphere(player.position - player.right * 0.75f - player.up * 0.65f, 0.25f).Length > 0
            || Physics.OverlapSphere(player.position + player.right * 0.75f - player.up * 0.65f, 0.25f).Length > 0;
    }

    protected float MagnitudeInDirection(Vector3 v, Vector3 direction)
    {
        return Vector3.Dot(v, direction);
    }

    [Rpc(SendTo.NotOwner)]
    public void SetSliderRpc(string name){
        HealthSlider = GameObject.Find(name).GetComponent<Slider>();
        HealthSlider.maxValue = Params.health;
        HealthSlider.value = Params.health;
    }

    public Side GetSide(){
        return Params.side;
    }

    protected IEnumerator UpdateSlider(float value){
        if(IsOwner)
            yield break;

        float StepSize = 0.1f;
        float dir = value > HealthSlider.value ? StepSize : -StepSize;
        float wait = 0.1f / (Mathf.Abs(HealthSlider.value - value) / StepSize);

        for(float v = HealthSlider.value; Mathf.Abs(value - v) > StepSize; v += dir){
            HealthSlider.value = v;
            yield return new WaitForSeconds(wait);
        }
    }
#endregion

#region ModelCreation
    [Rpc(SendTo.Server)]
    private void CreateModelRpc(){
        GameObject model = Instantiate(ModelPrefab, new Vector3(), Quaternion.identity, player);
        model.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId, true);

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
        
        PlayerScript.Spawned();
    }
#endregion

#region CardMethods
    protected virtual void Attack(){
        animator.SetTrigger("Attack");
        Chat.Singleton.Log("Attacking");
    }

    [Rpc(SendTo.Everyone)]
    protected void AttackCastleRpc(string CastleName){
        Castle c = GameObject.Find(CastleName).GetComponent<Castle>();

        if(c.GetSide() != Params.side)
            c.Damage(Params.damage);
    }

    [Rpc(SendTo.Everyone)]
    public virtual void DamageRpc(float amount){
        Params.health -= amount;

        StartCoroutine(UpdateSlider(Params.health));

        if(Params.health <= 0)
            animator.SetTrigger("Death");
    }

    [Rpc(SendTo.Everyone)]
    public virtual void HealRpc(float amount){
        Params.health += amount;

        StartCoroutine(UpdateSlider(Params.health));
    }

    [Rpc(SendTo.Everyone)]
    protected void SetDamageRpc(float newDamage){
        Params.damage = newDamage;
    }
#endregion
}
