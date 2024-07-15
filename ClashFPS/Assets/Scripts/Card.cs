using Unity.Netcode;
using UnityEngine;

public abstract class Card
{
    protected GameObject ModelPrefab;
    protected Transform player;
    protected Player PlayerScript;
    protected bool IsOwner;
    protected ulong OwnerClientId;
    
    protected CardParams Params;

    protected float attackTimer;
    // Moving, Attack, Jump, Death
    protected bool[] AnimatorParams = new bool[4];
    protected float AnimatorAttackBlend = 1;
    protected int jumpsLeft;

    public virtual void StartCard(Transform player, bool IsOwner, ulong OwnerClientId, CardParams Params = new CardParams(), string ModelName = ""){
        this.Params = Params;
        this.ModelPrefab = Resources.Load($"{ModelName}/ModelPrefab") as GameObject;
        this.player = player;
        this.PlayerScript = player.GetComponent<Player>();
        this.IsOwner = IsOwner;
        this.OwnerClientId = OwnerClientId;
        this.jumpsLeft = Params.jumps;

        if(!IsOwner)
            return;

        attackTimer = 1 / Params.attackRate;
    }

    public void CreateModel(){
        Transform model = GameObject.Instantiate(ModelPrefab, new Vector3(), Quaternion.identity, player).transform;
        model.GetComponent<NetworkObject>().Spawn(true);
    }

    public virtual void UpdateCard(Rigidbody rb, float friction, Transform cameraFollow){
        Move(rb, friction);
        Look(cameraFollow);

        attackTimer -= Time.deltaTime;
        if(Input.GetMouseButtonDown(0) && attackTimer <= 0){
            attackTimer = 1 / Params.attackRate;
            Attack();
        }
        
        if(OnGround())
            AnimatorParams[2] = false;

        if(Input.GetKeyDown(KeyCode.Space)){
            if(OnGround())
                jumpsLeft = Params.jumps;

            if(jumpsLeft > 0){
                rb.AddForce(Params.JumpStrength * player.up, ForceMode.Impulse);
                AnimatorParams[2] = true;
                jumpsLeft--;
            }
        }

        PlayerScript.updateModel();
        PlayerScript.updateAnimator(AnimatorParams);

        AnimatorParams[1] = false;
        AnimatorParams[3] = false;
    }

    protected bool OnGround(){
        return Physics.OverlapSphere(player.position - player.right * 0.75f, 0.05f).Length > 0
            || Physics.OverlapSphere(player.position + player.right * 0.75f, 0.05f).Length > 0;
    }

    protected virtual void Move(Rigidbody rb, float friction){
        Vector3 movementDir = new Vector3();

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

        AnimatorParams[0] = movementDir != Vector3.zero;
    }

    protected void Look(Transform cameraFollow){
        player.localEulerAngles = new Vector3(0, player.rotation.eulerAngles.y + Input.GetAxis("Mouse X"), 0);

        float xAngle = cameraFollow.rotation.eulerAngles.x;
        if(xAngle >= 180)
            xAngle -= 360;

        cameraFollow.localEulerAngles = new Vector3(Mathf.Clamp(xAngle - Input.GetAxis("Mouse Y"), -40, 75), 0, 0);
    }

    public virtual void Attack(){
        AnimatorParams[1] = true;
        Chat.Singleton.Log("Attacking");
    }

    public virtual void Damage(float amount){
        Params.health -= amount;

        if(Params.health <= 0)
            AnimatorParams[3] = true;
    }

    public virtual void Heal(float amount){
        Params.health += amount;
    }

    public void setDamage(float newDamage){
        Params.damage = newDamage;
    }
}
