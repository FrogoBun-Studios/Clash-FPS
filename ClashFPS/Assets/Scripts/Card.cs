using Unity.Netcode;
using UnityEngine;

public abstract class Card
{
    protected GameObject ModelPrefab;
    protected Transform model;
    // protected Animator animator;
    protected Transform player;
    protected Player PlayerScript;
    protected bool IsOwner;
    protected ulong OwnerClientId;
    
    protected CardParams Params;

    protected float attackTimer;
    protected bool AnimatorMoving = false;
    protected bool AnimatorAttack = false;
    protected bool AnimatorDeath = false;

    public virtual void StartCard(Transform player, bool IsOwner, ulong OwnerClientId, CardParams Params = new CardParams(), string ModelName = ""){
        this.Params = Params;
        this.ModelPrefab = Resources.Load($"{ModelName}/ModelPrefab") as GameObject;
        this.player = player;
        this.PlayerScript = player.GetComponent<Player>();
        this.IsOwner = IsOwner;
        this.OwnerClientId = OwnerClientId;

        if(!IsOwner)
            return;

        attackTimer = 1 / Params.attackRate;
    }

    public void CreateModel(){
        model = GameObject.Instantiate(ModelPrefab, new Vector3(), Quaternion.identity, player).transform;
        model.GetComponent<NetworkObject>().Spawn(true);
    }

    public void SetModel(){
        int i = 0;
        foreach(GameObject model in GameObject.FindGameObjectsWithTag("Model")){
            model.name = $"Model{i}";
            i++;
        }

        model = GameObject.Find($"Model{OwnerClientId}").transform;
        // animator = model.GetComponent<Animator>();
        PlayerScript.Spawned();
    }

    public virtual void UpdateCard(Rigidbody rb, float friction, Transform cameraFollow){
        Move(rb, friction);
        Look(cameraFollow);

        attackTimer -= Time.deltaTime;
        if(Input.GetMouseButtonDown(0) && attackTimer <= 0){
            attackTimer = 1 / Params.attackRate;
            Attack();
        }

        PlayerScript.updateModel();
        PlayerScript.updateAnimator(AnimatorMoving, AnimatorAttack, AnimatorDeath);

        AnimatorAttack = false;
        AnimatorDeath = false;
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

        // AnimatorMoving = Math.Abs(rb.linearVelocity.x) > 0.1f || Math.Abs(rb.linearVelocity.z) > 0.1f;
        AnimatorMoving = movementDir != Vector3.zero;
    }

    protected void Look(Transform cameraFollow){
        player.localEulerAngles = new Vector3(0, player.rotation.eulerAngles.y + Input.GetAxis("Mouse X"), 0);

        float xAngle = cameraFollow.rotation.eulerAngles.x;
        if(xAngle >= 180)
            xAngle -= 360;

        cameraFollow.localEulerAngles = new Vector3(Mathf.Clamp(xAngle - Input.GetAxis("Mouse Y"), -40, 75), 0, 0);
    }

    public virtual void Attack(){
        AnimatorAttack = true;
        Chat.Singleton.Log("Attacking");
    }

    public virtual void Damage(float amount){
        Params.health -= amount;
    }

    public virtual void Heal(float amount){
        Params.health += amount;
    }

    public void setDamage(float newDamage){
        Params.damage = newDamage;
    }
}
