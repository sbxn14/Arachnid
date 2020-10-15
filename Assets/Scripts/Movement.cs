using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField]
    float moveSpeed = 10f;
    [SerializeField]
    float rotationSensitivity = 90f;
    [SerializeField]
    float rotationSpeed = 90f;

    Vector3 inputVelocity, localInput, worldVelocity;

    [SerializeField]
    float timeBetweenSteps = 0.25f;
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

    void Start()
    {
        for (int i = 0; i < legs.Length; i++)
        {
            averageRotationRadius += legs[i].restPosition.z;
        }
        averageRotationRadius /= legs.Length;
    }

    // Update is called once per frame
    void Update()
    {
        CalculateOrientation();
        Move();

        if (isGrounded)
        {
            timeBetweenSteps = maxTargetDistance / Mathf.Max(worldVelocity.magnitude, Mathf.Abs(2 * Mathf.PI * rSpeed * Mathf.Deg2Rad * averageRotationRadius));
            
        }
        else
        {
            timeBetweenSteps = 0.25f;
        }

        if(timeBetweenSteps < 0.25f)
        {
            timeBetweenSteps = 0.25f;
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
            legs[index].Step();
            lastStep = Time.time;
        }
    }

    void Move()
    {
        localInput = Vector3.ClampMagnitude(transform.TransformDirection(new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"))), 1f);
        inputVelocity = Vector3.MoveTowards(inputVelocity, localInput, Time.deltaTime * 2f);
        worldVelocity = inputVelocity * moveSpeed;

        rSpeed = Mathf.MoveTowards(rSpeed, Input.GetAxis("Turn") * rotationSpeed, rotationSensitivity * Time.deltaTime);

        transform.Rotate(0f, rSpeed * Time.deltaTime, 0f);

        transform.position += (worldVelocity * Time.deltaTime);
    }

    Vector3 CalculateLegVelocity(int index)
    {
        Vector3 legPoint = legs[index].restingPosition;
        Vector3 direction = legPoint - transform.position;

        Vector3 rotationalPoint = ((Quaternion.AngleAxis((rSpeed * timeBetweenSteps) / 2f, transform.up) * direction) + transform.position) - legPoint;
        DrawArc(transform.position, direction, rSpeed / 2f, 10f, Color.black, 1f);
        return rotationalPoint + (worldVelocity * timeBetweenSteps) / 2f;
    }

    void CalculateOrientation()
    {
        Vector3 up = Vector3.zero;
        float avgSurfaceDist = 0f;

        isGrounded = false;

        Vector3 point, a, b, c;

        for (int i = 0; i < legs.Length; i++)
        {
            point = legs[i].stepPosition;
            avgSurfaceDist += transform.InverseTransformPoint(point).y;
            a = (transform.position - point).normalized;
            b = ((legs[nextLegTri[i]].stepPosition) - point).normalized;
            c = Vector3.Cross(a, b);

            up += c * sensitivityCurve.Evaluate(c.magnitude) + (legs[i].stepNormal == Vector3.zero ? transform.forward : legs[i].stepNormal);
            isGrounded |= legs[i].isGrounded;
            Debug.DrawRay(point, a, Color.red);

            Debug.DrawRay(point, b, Color.green);

            Debug.DrawRay(point, c, Color.blue);
        }
        up /= legs.Length;
        avgSurfaceDist /= legs.Length;
        dist = avgSurfaceDist;
        Debug.DrawRay(transform.position, up, Color.magenta);

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, up), up), 22.5f * Time.deltaTime);
        if (isGrounded)
        {
            transform.Translate(0, -(-avgSurfaceDist + desiredSufaceDist) * 0.5f, 0, Space.Self);
        }
        else
        {
            transform.Translate(0, -20 * Time.deltaTime, 0, Space.World);
        }

    }

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
        Gizmos.DrawWireSphere(transform.position, averageRotationRadius);
    }
}
