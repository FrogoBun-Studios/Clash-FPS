using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float speed = 50f;
    [SerializeField] private float friction = 0.1f;

    private void Start(){
        Application.targetFrameRate = 120;

        if(IsOwner)
            Chat.Singleton.Log("Player logged in");
    }

    private void Update()
    {
        if(!IsOwner)
            return;

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
    }
}