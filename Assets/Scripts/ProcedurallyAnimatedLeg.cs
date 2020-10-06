using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcedurallyAnimatedLeg : MonoBehaviour
{
    [SerializeField]
    Transform target, pole;
    [SerializeField]
    float stepRadius = 1f;
    [SerializeField]
    LayerMask probeMask;
    [SerializeField]
    Vector3 restPosition = Vector3.right;

    Vector3 stepPoint;

    Vector3 currentPosition = Vector3.zero;

    private void Start()
    {
        target.position = restPosition;
    }

    void Update()
    {
        //currentPosition = target.position;
        CalculateIKTarget();
        target.position = Vector3.Lerp(target.position, stepPoint, 1f);
    }

    void CalculateIKTarget()
    {
        Vector3 direction = currentPosition - pole.position;
        RaycastHit hit;
        if (Physics.SphereCast(pole.position, stepRadius, direction, out hit, direction.magnitude * 2f, probeMask))
        {
            Debug.DrawLine(pole.position, hit.point, Color.green);
            //stepPoint = hit.point;
            stepPoint = restPosition;
        }
        else
        {
            Debug.DrawLine(pole.position, restPosition, Color.red);
            stepPoint = restPosition;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(restPosition, .25f);
    }
}
