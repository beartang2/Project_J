using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MergeObjects;

public enum MergeType
{
    None,
    Body,
    HeadType1,
    HeadType2,
    KnightChessman,
    RookChessman,
    // 추가 가능
}

public class MergeTypeInfo : MonoBehaviour
{
    public MergeType mergeType;
}
