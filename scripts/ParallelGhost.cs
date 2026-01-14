using UnityEngine;

public class ParallelGhost : MonoBehaviour
{
    Transform target;
    Vector3 offset;

    public void Init(Transform target, Vector3 offset)
    {
        this.target = target;
        this.offset = offset;
    }

    void Update()
    {
        if (target == null) return;
        transform.position = target.position + offset;
    }
}
