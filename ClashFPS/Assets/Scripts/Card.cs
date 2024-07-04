using System;
using Unity.Netcode;
using UnityEngine;

public abstract class Card : NetworkBehaviour
{
    protected float health;
    protected float damage;
    protected GameObject ModelPrefab;
    public Transform model;
    protected Animator animator;
    protected float speed;
    protected float JumpStrength;
    protected int jumps;
    protected int elixer;
    protected bool flying;
    protected float attackRate;
    protected float attackTimer;

    public virtual void StartCard(ulong id){
        if(!IsOwner)
            return;
            
        Chat.Singleton.Log($"Starting for {id}");
        GetComponent<Player>().CreateModelRpc(id);
        model = GameObject.Find($"Model{id}").transform;
        animator = model.GetComponent<Animator>();

        attackTimer = 1 / attackRate;
    }

    public void CreateModel(ulong id){
        Chat.Singleton.Log($"Creating model for {id}");
        model = GameObject.Instantiate(ModelPrefab, new Vector3(), Quaternion.identity, transform).transform;
        model.GetComponent<NetworkObject>().Spawn(true);

        model.name = $"Model{id}";
    }

    public virtual void UpdateCard(){
        Rigidbody rb = transform.GetComponent<Rigidbody>();
        float friction = transform.GetComponent<Player>().friction;
        Transform cameraFollow = transform.GetComponent<Player>().cameraFollow;

        Move(rb, friction);
        Look(cameraFollow);

        attackTimer -= Time.deltaTime;
        if(Input.GetMouseButtonDown(0) && attackTimer <= 0){
            attackTimer = 1 / attackRate;
            Attack();
        }
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
        rb.linearVelocity += transform.forward * movementDir/*.normalized*/.z * speed
                           + transform.right * movementDir/*.normalized*/.x * speed;

        if(Math.Abs(rb.linearVelocity.x) > 0.1f || Math.Abs(rb.linearVelocity.z) > 0.1f)
            animator.SetBool("Moving", true);
        else
            animator.SetBool("Moving", false);

        model.position = transform.position;
    }

    protected virtual void Look(Transform cameraFollow){
        transform.localEulerAngles = new Vector3(0, transform.rotation.eulerAngles.y + Input.GetAxis("Mouse X"), 0);
        model.localEulerAngles = transform.localEulerAngles;

        float xAngle = cameraFollow.rotation.eulerAngles.x;
        if(xAngle >= 180)
            xAngle -= 360;

        cameraFollow.localEulerAngles = new Vector3(Mathf.Clamp(xAngle - Input.GetAxis("Mouse Y"), -40, 75), 0, 0);
    }

    public virtual void Attack(){
        animator.SetTrigger("Attack");
        Chat.Singleton.Log("Attacking");
    }

    public virtual void Damage(float amount){
        health -= amount;
    }

    public virtual void Heal(float amount){
        health += amount;
    }

    public virtual void changeDamage(float newDamage){
        damage = newDamage;
    }

    public virtual void setCardParams(float health, float damage, float speed, float JumpStrength, int jumps, int elixer, bool flying, float attackRate, string ModelName){
        this.health = health;
        this.damage = damage;
        this.speed = speed;
        this.JumpStrength = JumpStrength;
        this.jumps = jumps;
        this.elixer = elixer;
        this.flying = flying;
        this.attackRate = attackRate;
        this.ModelPrefab = Resources.Load($"{ModelName}/{ModelName}Model") as GameObject;
    }
}
