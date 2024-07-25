using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine;
using System.Collections;

public abstract class Card : NetworkBehaviour
{
    private GameObject ModelPrefab;
    protected Transform player;
    protected Player PlayerScript;
    private Transform model;
    private Animator animator;
    protected CardParams Params;
    private Slider HealthSlider;
    
    protected float attackTimer;

    public virtual void StartCard(Transform player, CardParams Params, string ModelName){
        this.Params = Params;
        this.ModelPrefab = Resources.Load($"{ModelName}/ModelPrefab") as GameObject;
        this.player = player;
        this.PlayerScript = player.GetComponent<Player>();

        if(!IsOwner)
            return;

        CreateModelRpc();

        attackTimer = 1 / Params.AttackRate;
    }

    public abstract void StartCard(Transform player);

    public virtual void UpdateCard(){
        if(Params.health <= 0)
            return;

        PlayerScript.ControlCharacter(Params.speed, Params.jumps, Params.JumpStrength);

        attackTimer -= Time.deltaTime;
        if(Input.GetButtonDown("Fire") && attackTimer <= 0){
            attackTimer = 1 / Params.AttackRate;
            Attack();
        }

        model.position = player.position;
        model.localEulerAngles = player.localEulerAngles;
    }

#region Misc
    [Rpc(SendTo.NotOwner)]
    public void SetSliderRpc(string name){
        HealthSlider = GameObject.Find(name).GetComponent<Slider>();
        HealthSlider.maxValue = Params.health;
        HealthSlider.value = Params.health;
    }

    public Side GetSide(){
        return Params.side;
    }

    public Animator GetAnimator(){
        return animator;
    }

    protected IEnumerator UpdateSlider(float value){
        if(IsOwner)
            yield break;

        float StepSize = 0.5f;
        float dir = value > HealthSlider.value ? StepSize : -StepSize;
        float wait = 0.01f / (Mathf.Abs(HealthSlider.value - value) / StepSize);

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
        PlayerScript.SetAnimatorRpc(model.name);
        
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
