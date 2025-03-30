using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class XRControllerInput : MonoBehaviour
{
    [SerializeField] XRController left;
    [SerializeField] XRController right;

    public bool isLPressed = false;
    public bool isRPressed = false;

    private void Awake()
    {
        //left = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        //right = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    private void Update()
    {
        left.inputDevice.TryGetFeatureValue(CommonUsages.grip, out float leftTriggerValue);
        right.inputDevice.TryGetFeatureValue(CommonUsages.grip, out float rightTriggerValue);

        
        if (leftTriggerValue > 0.3f)
        {
            isLPressed = true;
            Debug.Log("Ltrigger pressed");
        }
        else if (leftTriggerValue < 0.2f)
        {
            isLPressed = false;
        }

        if (rightTriggerValue > 0.3f)
        {
            isRPressed = true;
            Debug.Log("Rtrigger pressed");
        }
        else if(rightTriggerValue < 0.2f)
        {
            isRPressed = false;
        }
    }
}
