using System;
using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
   [SerializeField] private float speed = 8f;
   [SerializeField] private float jumpForce = 12f;
   [SerializeField] private CharacterController controller;
   [SerializeField] private InputReader inputReader;
   [SerializeField] private Animator anim;
   [SerializeField]private GameObject camTarget;

   [SerializeField]private float horizontalMouseSensitivity = 10f;
   [SerializeField]private float verticalMouseSensitivity = 100f;
   [SerializeField]private float upperYBound = -90f;
   [SerializeField]private float lowerYBound = 90f;
   private float pitch;
   
   [SerializeField]private float viewBobSpeed = 5f;
   [SerializeField]private float amplitude = 0.2f;
   [SerializeField]private float frequency = 0.5f;

   private void Start()
   {
    
   }
   private void Update()
   {
      Movement();
      Turn();
      //CameraBounce();
      anim.SetBool("IsMoving", inputReader._moveComposite.magnitude > 0);
   }

   private void Movement()
   {
      Vector3 moveDir = transform.forward * inputReader._moveComposite.y + transform.right  * inputReader._moveComposite.x;

      controller.Move(moveDir * speed *  Time.deltaTime);
   }

   private void Turn()
   {
      
      float mouseX = inputReader._mouseDelta.x * horizontalMouseSensitivity; 
      transform.Rotate(Vector3.up * mouseX);
      
      float mouseY = inputReader._mouseDelta.y * verticalMouseSensitivity;  
      pitch -= mouseY;                                    
      pitch = Mathf.Clamp(pitch, upperYBound, lowerYBound);

      camTarget.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
   }

   private void CameraBounce()
   {
      float speed = controller.velocity.magnitude;
      float sin = amplitude * Mathf.Sin(viewBobSpeed * speed);
      camTarget.transform.localPosition = Vector3.up * sin;
   }

}
