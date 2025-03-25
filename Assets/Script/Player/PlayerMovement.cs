using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //private Rigidbody rb;
    private float horizontal;
    private float vertical;
    private float moveSpeed = 8.0f;

    private Vector3 moveVec3;

    private void Start()
    {
        //rb = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        GetKey();
    }

    private void FixedUpdate()
    {
        Movement();
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

    void VectorInfo_Update()
    {

    }
}