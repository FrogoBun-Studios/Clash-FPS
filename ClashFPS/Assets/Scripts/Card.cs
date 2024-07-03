using System;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Card
{
    protected float health;
    protected float damage;
    protected GameObject model;
    protected Animator animator;
    protected float speed;
    protected float JumpStrength;
    protected int jumps;
    protected int elixer;
    protected bool flying;
    protected float attackRate;
    protected float attackTimer;

    public virtual void StartCard(Transform transform){
        animator = GameObject.Instantiate(model, new Vector3(0, 0, 0), Quaternion.identity, transform).GetComponent<Animator>();

        attackTimer = 1 / attackRate;
    }

    public virtual void UpdateCard(Transform transform, Rigidbody rb, float friction, Transform cameraHolder){
        Move(transform, rb, friction);
        Look(transform, cameraHolder);

        attackTimer -= Time.deltaTime;
        if(Input.GetMouseButtonDown(0) && attackTimer <= 0){
            attackTimer = 1 / attackRate;
            Attack();
        }
    }

    protected virtual void Move(Transform transform, Rigidbody rb, float friction){
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
    }

    protected virtual void Look(Transform transform, Transform cameraHolder){
        transform.rotation = Quaternion.Euler(0, cameraHolder.rotation.eulerAngles.y + Input.GetAxis("Mouse X"), 0);
        cameraHolder.rotation = Quaternion.Euler(Mathf.Clamp(cameraHolder.rotation.eulerAngles.x - Input.GetAxis("Mouse Y"), -90, 90), 0, 0);
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

    public virtual void setCardParams(float health, float damage, float speed, float JumpStrength, int jumps, int elixer, bool flying, float attackRate){
        this.health = health;
        this.damage = damage;
        this.speed = speed;
        this.JumpStrength = JumpStrength;
        this.jumps = jumps;
        this.elixer = elixer;
        this.flying = flying;
        this.attackRate = attackRate;
    }
}
