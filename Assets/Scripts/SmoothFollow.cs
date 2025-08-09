using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    [Header("Target to follow")]
    public Transform target;

    [Header("Offset from the target")]
    public Vector3 offset = new Vector3(0f, 5f, -10f);

    [Header("Smooth follow settings")]
    [Range(0.01f, 10f)]
    public float followDelay = 5f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * followDelay);
    }
}
