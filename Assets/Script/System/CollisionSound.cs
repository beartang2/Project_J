using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CollisionSound : MonoBehaviour
{
    public AudioClip collisionClip;
    public float minVelocity = 1f; // 너무 약한 충돌은 무시
    public float startTime = 0f; // 재생 시작 지점
    public float stopTime = 0f; // 재생 중지 지점 (0이면 끝까지 재생)
    public float volume = 1.0f; // 볼륨 조절

    public AudioSource audioSource;
    private Rigidbody rb;

    void Awake()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        audioSource.playOnAwake = false;

        StartCoroutine(muteDuringSpawn()); // 3초 후 음소거 해제
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > minVelocity && collisionClip != null)
        {
            audioSource.clip = collisionClip;
            audioSource.time = startTime; // 지정된 시간부터 재생
            audioSource.pitch = Random.Range(1.0f, 1.5f);
            if (gameObject.name.Contains("Bone") || gameObject.name.Contains("Carrot") || gameObject.name.Contains("Root"))
            {
                audioSource.PlayOneShot(collisionClip, volume);
            }
            else
            {
                audioSource.Play();
            }
            //Debug.Log($"Collision sound played with clip: {collisionClip.name}, volume: {volume}");
            if(audioSource != null)
            {
                StartCoroutine(StopAfterTime(stopTime));
            }
        }
    }

    IEnumerator StopAfterTime(float duringTime)
    {
        yield return new WaitForSeconds(duringTime);
        audioSource.Stop();
    }

    public IEnumerator muteDuringSpawn()
    {
        if(audioSource != null)
        {
            audioSource.mute = true; // 초기 음소거
            yield return new WaitForSeconds(3f); // 음소거
            audioSource.mute = false;
        }
    }
}
