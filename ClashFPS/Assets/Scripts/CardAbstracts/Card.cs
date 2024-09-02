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
    private bool Started = false;

    public virtual void StartCard(Transform player, CardParams Params, string ModelName){
        Started = true;

        this.Params = Params;
        this.ModelPrefab = Resources.Load($"{ModelName}/ModelPrefab") as GameObject;
        this.player = player;
        this.PlayerScript = player.GetComponent<Player>();

        if(!IsOwner)
            return;

        PlayerScript.SetColliderSizeRpc(Params.ColliderRadius, Params.ColliderHeight, Params.ColliderYOffset);
        CreateModelRpc();

        PlayerScript.SetCameraFollow(new Vector3(0, 4.625f * model.localScale.y - 2.375f, -2.5f * model.localScale.y + 2.5f));

        attackTimer = 1 / Params.AttackRate;
    }

    public bool IsStarted(){
        return Started;
    }

    public abstract void StartCard(Transform player, Side side);

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
    public void SetSliders(string topSlider){
        if(!IsOwner)
            HealthSlider = GameObject.Find(topSlider).GetComponent<Slider>();
        else
            HealthSlider = GameObject.Find("HealthSliderUI").GetComponent<Slider>();

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
        if(value <= 0){
            HealthSlider.value = 0;
            yield break;
        }

        float StepSize = 0.5f;
        float dir = value > HealthSlider.value ? StepSize : -StepSize;
        float wait = 0.01f / (Mathf.Abs(HealthSlider.value - value) / StepSize);

        for(float v = HealthSlider.value; Mathf.Abs(value - v) > StepSize; v += dir){
            HealthSlider.value = v;
            yield return new WaitForSeconds(wait);
        }
    }

    [Rpc(SendTo.Everyone)]
    protected void UpdateSliderRpc(float value){
        StartCoroutine(UpdateSlider(value));
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
    }

    [Rpc(SendTo.Everyone)]
    protected void AttackTowerRpc(string TowerName){
        Tower t = GameObject.Find(TowerName).GetComponent<Tower>();

        if(t.GetSide() != Params.side)
            t.Damage(Params.damage);
    }

    [Rpc(SendTo.Owner)]
    public virtual void DamageRpc(float amount){
        Params.health -= amount;

        UpdateSliderRpc(Params.health);

        if(Params.health <= 0)
            animator.SetTrigger("Death");
    }

    public virtual void Heal(float amount){
        DamageRpc(-amount);
    }

    [Rpc(SendTo.Everyone)]
    protected void SetDamageRpc(float newDamage){
        Params.damage = newDamage;
    }
#endregion
}
