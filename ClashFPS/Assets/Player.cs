using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float speed = 50f;
    [SerializeField] private float friction = 0.1f;
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private Card card;

    private void Start(){
        if(!IsOwner)
            return;

        Application.targetFrameRate = 120;
        Chat.Singleton.Log("Player logged in");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        card = new WizardCard();
        card.StartCard(transform);
    }

    private void Update()
    {
        if(!IsOwner)
            return;

        card.UpdateCard(transform, rb, friction, cameraHolder);
    }
}