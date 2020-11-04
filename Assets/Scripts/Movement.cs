using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField]
    float moveSpeed = 10f;
    [SerializeField]
    float sprintSpeed = 20f;
    [SerializeField]
    float rotationSensitivity = 90f;
    [SerializeField]
    float rotationSpeed = 90f;

    Vector3 inputVelocity, localInput, worldVelocity;

    [SerializeField]
    float timeBetweenSteps = 1.5f;
    [SerializeField]
    float minimumTimeBetweenSteps = 0.5f;
    float lastStep = 0;
    [SerializeField]
    float stepDurationRatio = 2f;
    [SerializeField]
    float maxTargetDistance = 1f;

    [SerializeField]
    float rSpeed = 0f;

    [SerializeField]
    ProcedurallyAnimatedLeg[] legs;

    [SerializeField]
    bool isGrounded = false;
    [SerializeField]
    float averageRotationRadius = 2.95f;
    int index = 0;

    [SerializeField]
    int[] nextLegTri;
    [SerializeField]
    AnimationCurve sensitivityCurve;

    [SerializeField]
    float desiredSufaceDist = -2f;
    [SerializeField]
    float dist = 0f;

    [SerializeField]
    Rigidbody body;
    [SerializeField]
    CapsuleCollider capsuleCollider;

    float maxStepTries = 10f;
    float stepTries = 0f;

    //Vector3 rightAxis, forwardAxis;

    void Start()
    {
        for (int i = 0; i < legs.Length; i++)
        {
            averageRotationRadius += legs[i].restPosition.z;
        }
        averageRotationRadius /= legs.Length;
    }

    void Update()
    {
        Move();

        //rightAxis = ProjectDirectionOnPlane(Vector3.right, currentNormal);
        //forwardAxis = ProjectDirectionOnPlane(Vector3.forward, currentNormal);

        if (isGrounded)
        {
            if (sprinting)
            {
                timeBetweenSteps /= 2;
            }
            else
            {
                timeBetweenSteps = maxTargetDistance / Mathf.Max(worldVelocity.magnitude, Mathf.Abs(2 * Mathf.PI * rSpeed * Mathf.Deg2Rad * averageRotationRadius));
            }
        }
        else
        {
            timeBetweenSteps = minimumTimeBetweenSteps;
        }

        if (timeBetweenSteps < minimumTimeBetweenSteps)
        {
            timeBetweenSteps = minimumTimeBetweenSteps;
        }

        if (Time.time > lastStep + (timeBetweenSteps / legs.Length) && legs != null)
        {
            index = (index + 1) % legs.Length;

            if (legs[index] == null) return;

            for (int i = 0; i < legs.Length; i++)
            {
                legs[i].SetVelocity(CalculateLegVelocity(i));

            }

            legs[index].stepDuration = Mathf.Min(1f, (timeBetweenSteps / legs.Length) * stepDurationRatio);
            legs[index].worldVelocity = CalculateLegVelocity(index);

            Stepper(index);
        }

        //RayToGround();
    }

    //[SerializeField]
    //float desiredDistanceFromGround = 5f;
    //[SerializeField]
    //float currentDistanceFromGround;
    //[SerializeField]
    //Vector3 currentNormal;

    //void RayToGround()
    //{
    //    if (Physics.Raycast(body.position, -body.transform.up, out RaycastHit hit, probeMask))
    //    {
    //        Debug.DrawLine(body.position, hit.point, Color.green);
    //        currentDistanceFromGround = hit.distance;
    //        currentNormal = hit.normal.normalized;

    //    }
    //    else
    //    {
    //        Debug.DrawRay(body.position, -body.transform.up, Color.red);
    //        currentDistanceFromGround = float.PositiveInfinity;
    //    }
    //}

    //Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
    //{
    //    return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    //}

    //void AdjustVelocity()
    //{
    //    Vector3 xAxis = ProjectDirectionOnPlane(rightAxis, currentNormal);
    //    Vector3 zAxis = ProjectDirectionOnPlane(forwardAxis, currentNormal);

    //    float currentX = Vector3.Dot(worldVelocity, xAxis);
    //    float currentZ = Vector3.Dot(worldVelocity, zAxis);

    //    float acceleration = 10f;
    //    float maxSpeedChange = acceleration * Time.deltaTime;

    //    float newX = Mathf.MoveTowards(currentX, worldVelocity.x, maxSpeedChange);
    //    float newZ = Mathf.MoveTowards(currentZ, worldVelocity.z, maxSpeedChange);

    //    worldVelocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    //}

    //void AdjustRotation()
    //{
    //    float dot = Vector3.Dot(worldVelocity, currentNormal);
    //    Debug.Log(dot);

    //    if (dot > 0f)
    //    {
    //        worldVelocity = (worldVelocity - currentNormal * dot).normalized * moveSpeed;
    //    }

    //    body.rotation = Quaternion.Lerp(body.rotation, Quaternion.FromToRotation(body.transform.up, currentNormal) * body.transform.rotation, 1f);


    //    //body.rotation = Quaternion.FromToRotation(body.transform.up, currentNormal);
    //}

    void Stepper(int index)
    {
        legs[index].Step();


        if (!legs[index].isGrounded && stepTries <= maxStepTries)
        {
            stepTries++;
            Stepper(index);
        }
        else
        {
            stepTries = 0;
        }
        lastStep = Time.time;
    }

    private void FixedUpdate()
    {
        CalculateOrientation();
        //FindNormal();
        //AdjustGroundDistance();

        body.transform.Rotate(0f, rSpeed * Time.fixedDeltaTime, 0f);

        body.transform.position += (worldVelocity * Time.fixedDeltaTime);

        body.transform.rotation = Quaternion.Slerp(body.transform.rotation, Quaternion.LookRotation(Vector3.ProjectOnPlane(body.transform.forward, up), up), 22.5f * Time.fixedDeltaTime);
        if (isGrounded)
        {
            float surfDist = -(-avgSurfaceDist + desiredSufaceDist) * 0.5f;

            //if (surfDist < desiredSufaceDist)
            //{
            //    surfDist = desiredSufaceDist;
            //}
            body.transform.Translate(0, surfDist, 0, Space.Self);
        }
        else
        {
            body.transform.Translate(0, -20 * Time.fixedDeltaTime, 0, Space.World);
        }
    }

    bool sprinting = false;

    void Move()
    {
        localInput = Vector3.ClampMagnitude(transform.TransformDirection(new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"))), 1f);
        inputVelocity = Vector3.MoveTowards(inputVelocity, localInput, Time.deltaTime * 2f);

        float usedSpeed = 0f;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            usedSpeed = sprintSpeed;

            sprinting = true;
        }
        else
        {
            sprinting = false;
            usedSpeed = moveSpeed;
        }

        worldVelocity = inputVelocity * usedSpeed;

        rSpeed = Mathf.MoveTowards(rSpeed, Input.GetAxis("Turn") * rotationSpeed, rotationSensitivity * Time.deltaTime);

        //transform.Rotate(0f, rSpeed * Time.deltaTime, 0f);

        //transform.position += (worldVelocity * Time.deltaTime);
    }

    Vector3 CalculateLegVelocity(int index)
    {
        Vector3 legPoint = legs[index].restingPosition;
        Vector3 direction = legPoint - body.position;

        Vector3 rotationalPoint = ((Quaternion.AngleAxis((rSpeed * timeBetweenSteps) / 2f, transform.up) * direction) + body.position) - legPoint;
        DrawArc(body.position, direction, rSpeed / 2f, 10f, Color.black, 1f);
        return rotationalPoint + (worldVelocity * timeBetweenSteps) / 2f;
    }

    Vector3 up = Vector3.zero;
    [SerializeField]
    float avgSurfaceDist = 0f;

    [SerializeField]
    Vector3 currentNormal;

    void CalculateOrientation()
    {
        up = Vector3.zero;
        avgSurfaceDist = 0f;

        isGrounded = false;

        Vector3 point, a, b, c;
        int count = 0;

        foreach (var leg in legs)
        {
            currentNormal += leg.stepNormal;
            count++;
            if (count == 8)
            {
                count = 0;
                currentNormal /= legs.Length;
            }
        }

        for (int i = 0; i < legs.Length; i++)
        {
            point = legs[i].stepPosition;
            avgSurfaceDist += body.transform.InverseTransformPoint(point).y;
            a = (body.position - point);//.normalized;
            b = ((legs[nextLegTri[i]].stepPosition) - point);//.normalized;
            c = Vector3.Cross(a, b);


            //up += c * sensitivityCurve.Evaluate(c.magnitude) + (legs[i].stepNormal == Vector3.zero ? body.transform.forward : legs[i].stepNormal);

            isGrounded |= legs[i].isGrounded;

            //Debug.DrawRay(point, a, Color.red);

            //Debug.DrawRay(point, b, Color.green);

            Debug.DrawRay(point, up, Color.cyan);
        }
        //up /= legs.Length;
        up = currentNormal;
        avgSurfaceDist /= legs.Length;
        dist = avgSurfaceDist;
        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, up), up), 22.5f * Time.deltaTime);
        //if (isGrounded)
        //{
        //    transform.Translate(0, -(-avgSurfaceDist + desiredSufaceDist) * 0.5f, 0, Space.Self);
        //}
        //else
        //{
        //    transform.Translate(0, -20 * Time.deltaTime, 0, Space.World);
        //}
    }

    [SerializeField]
    LayerMask probeMask;

    public void DrawArc(Vector3 point, Vector3 dir, float angle, float stepSize, Color color, float duration)
    {
        if (angle < 0)
        {
            for (float i = 0; i > angle + 1; i -= stepSize)
            {
                Debug.DrawLine(point + Quaternion.AngleAxis(i, transform.up) * dir, point + Quaternion.AngleAxis(Mathf.Clamp(i - stepSize, angle, 0), transform.up) * dir, color, duration);
            }
        }
        else
        {
            for (float i = 0; i < angle - 1; i += stepSize)
            {
                Debug.DrawLine(point + Quaternion.AngleAxis(i, transform.up) * dir, point + Quaternion.AngleAxis(Mathf.Clamp(i + stepSize, 0, angle), transform.up) * dir, color, duration);
            }
        }
    }

    public void OnDrawGizmosSelected()
    {
        //Gizmos.DrawWireSphere(transform.position, averageRotationRadius);
    }
}
