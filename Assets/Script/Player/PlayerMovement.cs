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

    private bool isGrounded = true; // 바닥 체크
    private bool isJumping = false; // 중복 점프 방지용

    public float groundCheckDistance = 0.1f; // Ray 길이
    public float jumpCooldown = 0.1f; // 점프 쿨타임 (중복 방지)

    public AudioSource jumpSource;
    public AudioClip jumpSound;   // 점프 사운드 (추가 가능)

    private bool ignoreGroundCheck = false;
    private float ignoreGroundTime = 0.05f;

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

        if (!isJumping && isGrounded && rightHandDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool isPressed)
            && isPressed)
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        Movement();

        /*
        Vector3 moveVec3 = new Vector3(horizontal, 0, vertical).normalized;

        Vector3 targetPosition = rb.position + moveVec3 * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
        Vector3 moveVec3 = new Vector3(horizontal, 0, vertical).normalized;
        rb.MovePosition(transform.position + moveVec3 * moveSpeed * Time.fixedDeltaTime);*/
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
        if (ignoreGroundCheck)
            return;

        Vector3 offset = new Vector3(0, 0.001f, 0);
        isGrounded = Physics.Raycast(transform.position + offset, Vector3.down, groundCheckDistance + 0.07f);
        Debug.DrawRay(transform.position + offset, Vector3.down * (groundCheckDistance + 0.05f), isGrounded ? Color.green : Color.red);
    }

    void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        ignoreGroundCheck = true;
        Invoke(nameof(ResetGroundCheck), ignoreGroundTime);

        StartCoroutine(JumpCooldownCoroutine());
        // 점프 효과음 실행
        if (jumpSource != null)
        {
            jumpSource.clip = jumpSound; // 점프 사운드 설정
            jumpSource.time = 0.3f; // 사운드 시작 지점
            jumpSource.pitch = 1.7f;
            //audioSource.Play();
            jumpSource.PlayOneShot(jumpSound, 0.8f); // 점프 사운드 재생
        }
    }

    void ResetGroundCheck()
    {
        ignoreGroundCheck = false;
    }


    IEnumerator JumpCooldownCoroutine()
    {
        isJumping = true;
        isGrounded = false; // 강제로 false 설정해서 다음 프레임도 못 뛰게
        yield return new WaitForSeconds(jumpCooldown);
        isJumping = false;
    }
}
