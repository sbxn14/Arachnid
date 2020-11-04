using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

public class ProcedurallyAnimatedLeg : MonoBehaviour
{
    public bool isGrounded = false;

    [SerializeField]
    Transform target, pole;
    [SerializeField]
    float stepRadius = .25f;
    [SerializeField]
    LayerMask probeMask;

    public Vector3 restPosition;

    public Vector3 restingPosition
    {
        get
        {
            return transform.TransformPoint(restPosition);
        }
    }

    public Vector3 stepPosition;

    public Vector3 stepNormal;

    public Vector3 worldVelocity;

    Vector3 worldTarget = Vector3.zero;

    public Vector3 desiredPosition
    {
        get
        {
            return restingPosition + worldVelocity + (Random.insideUnitSphere * placementRandomization);
        }
    }

    public float stepCooldown = 1f;
    public float stepDuration = 0.5f;
    [SerializeField]
    float placementRandomization = 0;
    float lastStep = 0;
    [SerializeField]
    float stepOffset;

    public AnimationCurve stepHeightCurve;
    public float stepHeightMultiplier = 0.25f;

    float percent
    {
        get
        {
            return Mathf.Clamp01((Time.time - lastStep) / stepDuration);
        }
    }

    private void Start()
    {
        worldVelocity = Vector3.zero;
        lastStep = Time.time + stepCooldown * stepOffset;
        target.position = restingPosition;
        Step();
        //GetNewDesiredPosition();
    }

    void Update()
    {
        stepPosition = SphereCast(worldTarget + worldVelocity);
        UpdateTarget(stepPosition);

        //if (Time.time > lastStep + stepCooldown)
        //{
        //    Step();
        //}
    }

    public void Step()
    {
        //GetNewDesiredPosition();
        //CheckIfValid();
        stepPosition = worldTarget = SphereCast(desiredPosition);
        lastStep = Time.time;
    }

    //void GetNewDesiredPosition()
    //{
    //    desiredPosition = restingPosition + worldVelocity + (Random.insideUnitSphere * placementRandomization);
    //    DistanceFromPoint = Vector3.Distance(desiredPosition, stepPosition);
    //}

    //[SerializeField]
    //float searchRadius = 4f;
    //[SerializeField]
    //float newDistance = 0f;
    //[SerializeField]
    //Vector3 newPoint;

    //[SerializeField]
    //Color color;

    //void CheckIfValid()
    //{
    //    float bestDistance = float.PositiveInfinity;
    //    Collider bestCollider = null;
    //    RaycastHit hit;
    //    //GetNewDesiredPosition();

    //    Vector3 direction = -transform.up;

    //    if (Physics.SphereCast(restingPosition, searchRadius, direction, out hit, direction.magnitude * 2f, probeMask))
    //    {
    //        newPoint = hit.point;
    //    }

    //    //if (!Physics.SphereCast(restingPosition, searchRadius, stepNormal, out hit, probeMask))
    //    //{
    //    //    Debug.DrawLine(restingPosition, stepNormal, color);
    //    //    //find new position
    //    //    Collider[] colliders = Physics.OverlapSphere(restingPosition, searchRadius, probeMask);

    //    //    foreach (var collider in colliders)
    //    //    {
    //    //        newDistance = Vector3.Distance(restingPosition, collider.transform.position);

    //    //        if (newDistance < bestDistance)
    //    //        {
    //    //            bestDistance = newDistance;
    //    //            bestCollider = collider;
    //    //        }
    //    //    }
    //    //    if (bestCollider != null)
    //    //    {
    //    //        Debug.Log("Desired Position changed");
    //    //        newPoint = bestCollider.ClosestPointOnBounds(desiredPosition);
    //    //        //desiredPosition = Vector3.Lerp(desiredPosition, newPoint, percent);
    //    //    }
    //    //}
    //    //else
    //    //{
    //    //    desiredPosition = Vector3.Lerp(desiredPosition, hit.point, percent);
    //    //}
    //}



    Vector3 SphereCast(Vector3 position)
    {
        Vector3 direction = position - pole.position;
        RaycastHit hit;
        //distance = direction.magnitude * 2f;

        if (Physics.SphereCast(pole.position, stepRadius, direction, out hit, direction.magnitude * 2f, probeMask))
        {
            //Debug.DrawLine(pole.position, hit.point, Color.green);
            //distance = hit.distance;
            position = hit.point;
            stepNormal = hit.normal;
            isGrounded = true;
        }
        else
        {
            //Debug.DrawLine(pole.position, restingPosition, Color.red);
            //distance = 0;
            position = restingPosition;
            stepNormal = Vector3.zero;
            isGrounded = false;
        }
        return position;
    }

    void UpdateTarget(Vector3 position)
    {
        target.position = Vector3.Lerp(target.position, position, percent) + stepNormal * stepHeightCurve.Evaluate(percent) * stepHeightMultiplier;
    }

    public void SetVelocity(Vector3 newVelocity)
    {
        worldVelocity = Vector3.Lerp(worldVelocity, newVelocity, 1f - percent);
    }

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(stepPosition, stepRadius);
        //Gizmos.DrawLine(target.position, restingPosition);
        //Gizmos.DrawSphere(target.position, .15f);
        //Gizmos.DrawSphere(pole.position, .15f);
        //Gizmos.color = Color.yellow;
        ////Gizmos.DrawSphere(bodyTarget.position, .15f);
        Gizmos.color = Color.blue;
        //Gizmos.DrawWireSphere(worldTarget, stepRadius);
        //Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(restingPosition, stepRadius);

        //Gizmos.DrawLine(restingPosition, newPoint);
        //Gizmos.color = Color.black;
        //Gizmos.DrawWireSphere(desiredPosition, stepRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(desiredPosition, stepRadius);
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(restingPosition, desiredPosition);
        //Gizmos.color = Color.green;
        //Gizmos.DrawWireSphere(restingPosition, searchRadius);
    }
}
