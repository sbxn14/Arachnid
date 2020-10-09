using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcedurallyAnimatedLeg : MonoBehaviour
{
    [SerializeField]
    bool isGrounded = false;

    [SerializeField]
    Transform target, pole;
    [SerializeField]
    Transform bodyTarget;
    [SerializeField]
    float stepRadius = .25f;
    [SerializeField]
    LayerMask probeMask;

    [SerializeField]
    float distanceBetweenTargets = 0f;

    public Vector3 restPosition;

    Vector3 stepPosition, stepNormal;

    Vector3 worldVelocity;

    private void Start()
    {
        restPosition = bodyTarget.position;
    }

    void Update()
    {
        SphereCast(restPosition + worldVelocity);
        CalculateDistanceBetweenTargets();
        UpdateTarget(stepPosition);
    }

    private void LateUpdate()
    {
        if (distanceBetweenTargets > 1.5f)
        {
            UpdateTarget(bodyTarget.position);
            restPosition = target.position;
        }
        AdjustNormals();
    }

    void CalculateDistanceBetweenTargets()
    {
        distanceBetweenTargets = Vector3.Distance(target.position, bodyTarget.position);
    }

    void SphereCast(Vector3 position)
    {
        Vector3 direction = position - pole.position;
        RaycastHit hit;

        if (Physics.SphereCast(pole.position, stepRadius, direction, out hit, direction.magnitude * 2f, probeMask))
        {
            //Debug.DrawLine(pole.position, hit.point, Color.green);
            stepPosition = hit.point;
            stepNormal = hit.normal;
            isGrounded = true;
        }
        else
        {
            //Debug.DrawLine(pole.position, restPosition, Color.red);
            stepPosition = restPosition;
            stepNormal = Vector3.zero;
            isGrounded = false;
        }
    }

    void AdjustNormals()
    {
        RaycastHit hit;

        if (Physics.Raycast(bodyTarget.position, -stepNormal, out hit, probeMask))
        {
            Vector3 newPos = bodyTarget.position;
            newPos.y = hit.point.y;
            //bodyTarget.position = Vector3.Lerp(bodyTarget.position, newPos, 1f);

            bodyTarget.position = Vector3.Lerp(bodyTarget.position, hit.point, 1f);
        }
    }

    void UpdateTarget(Vector3 position)
    {
        target.position = Vector3.Lerp(target.position, position, 1f);
    }

    public void SetVelocity(Vector3 newVelocity)
    {
        worldVelocity = Vector3.Lerp(worldVelocity, newVelocity, 1f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(pole.position, target.position);
        Gizmos.DrawSphere(target.position, .15f);
        Gizmos.DrawSphere(pole.position, .15f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(bodyTarget.position, .15f);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(target.position, bodyTarget.position);
    }
}
