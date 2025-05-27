using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.XR;

public class PlayerMovement : NetworkBehaviour
{
    private Rigidbody rb;

    private float horizontal;
    private float vertical;
    public float moveSpeed = 3.5f;
    public float jumpForce = 5.0f;

    private Vector3 moveVec3;
    private InputDevice rightHandDevice;

    private bool isGrounded = true; // 땅에 닿아있는지 확인하는 변수

    public float groundCheckDistance = 0.1f; // Ray 길이
    public LayerMask groundLayer; // 땅 레이어

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

        CheckIfGrounded();

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

    void GetKey()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
    }

    void Movement()
    {
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

    void CheckIfGrounded()
    {
        // Ray를 아래로 쏴서 Ground 레이어와 충돌 검사
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance + 0.1f);
    }
}
