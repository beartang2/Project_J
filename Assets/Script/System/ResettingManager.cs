using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResettingManager : MonoBehaviour
{
    public static ResettingManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetAllTriggers()
    {
        var allResettable = FindObjectsOfType<MonoBehaviour>(true);

        foreach (var comp in allResettable)
        {
            if (!comp.gameObject.activeSelf) continue;

            if (comp is IResettable resettable)
            {
                resettable.ResetTrigger();
            }
        }

        Debug.Log("모든 트리거 초기화 완료");
    }
}
