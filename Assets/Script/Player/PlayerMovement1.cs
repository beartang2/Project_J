using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.XR;

public class PlayerMovement1 : NetworkBehaviour
{
    private Rigidbody rb;

    private float horizontal;
    private float vertical;
    public float moveSpeed = 3.5f;
    public float jumpForce = 5.0f;

    private Vector3 moveVec3;
    private InputDevice rightHandDevice;

    private bool isGrounded = true; // 땅에 닿아있는지 확인하는 변수

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        if (!rightHandDevice.isValid)
        {
            TryInitializeRightHand();
        }

        GetKey();

        if (rightHandDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool isPressed) && isPressed && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    private void FixedUpdate()
    {
        Movement();

        Vector3 moveVec3 = new Vector3(horizontal, 0, vertical).normalized;
        rb.MovePosition(transform.position + moveVec3 * moveSpeed * Time.fixedDeltaTime);
    }

    //  키 입력
    void GetKey()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
    }

    // 이동 구현
    void Movement()
    {
        //플레이어 이동
        moveVec3 = new Vector3(horizontal, 0, vertical).normalized;

        transform.Translate(moveVec3 * moveSpeed * Time.deltaTime);
    }

    void TryInitializeRightHand()
    {
        var rightHandDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightHandDevices);
        if (rightHandDevices.Count > 0)
            rightHandDevice = rightHandDevices[0];
    }

    private void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
        // 바닥에 닿으면 점프 가능
        /*
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
         */
    }
}