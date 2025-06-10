using UnityEngine;

public enum SpawnArea
{
    Room1,
    Room2,
    Room3,
    Room4,
}

public class SpawnPoint : MonoBehaviour
{
    public SpawnArea areaType;
}