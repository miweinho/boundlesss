using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;               // assign player
    public Vector3 offset = Vector3.zero;  // e.g. (0,0,-10)

    void Start()
    {
        if (target == null) TryFindTarget();

        if (target != null)
        {
            var p = target.position;
            transform.position = new Vector3(p.x, p.y, transform.position.z); // snap once
        }
    }

    void LateUpdate()
    {
        if (target == null) return;
        transform.position = new Vector3(target.position.x + offset.x,
                                         target.position.y + offset.y,
                                         transform.position.z + offset.z);
    }

    void TryFindTarget()
    {
        if (GameObject.FindGameObjectWithTag("Player"))
            target = GameObject.FindGameObjectWithTag("Player").transform;
    }
}