using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeChessman : CheckHandTransform
{
    private GameObject thisObject;
    [SerializeField] GameObject chessman;

    private void Awake()
    {
        thisObject = gameObject;
    }

    private void Update()
    {
        GameObject chessBody = GameObject.FindWithTag("merged_key");

        if (chessBody != null)
        {
            CheckDistanceNCreate(thisObject, chessBody, chessman);
            Debug.Log("Found chessBody");
        }
    }
}
