using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    #region movement
    [SerializeField, Range(0f, 100f)]
    float moveSpeed = 15f;
    [SerializeField, Range(0f, 100f)]
    float maxAcceleration = 10f, maxAirAcceleration = 1f;
    [SerializeField, Range(0f, 10f)]
    float jumpHeight = 2f;
    [SerializeField, Range(0, 5)]
    int maxAirJumps = 0;

    [SerializeField, Range(0f, 90f)]
    float maxGroundAngle = 25f;
    [SerializeField, Range(0f, 100f)]
    float maxSnapSpeed = 100f;
    [SerializeField, Min(0f)]
    float probeDistance = 1f;
    [SerializeField]
    LayerMask probeMask = -1;

    [SerializeField]
    Transform playerInputSpace = default;

    Vector2 playerInput;
    Vector3 velocity, desiredVelocity;
    Vector3 contactNormal, steepNormal;
    Vector3 upAxis, rightAxis, forwardAxis;

    int groundContactCount, steepContactCount;
    int stepsSinceLastGrounded, stepsSinceLastJump;
    bool desiredJump;

    bool OnGround => groundContactCount > 0;
    bool OnSteep => steepContactCount > 0;

    int jumpPhase;
    float minGroundDotProduct;

    Rigidbody body;
    #endregion

    void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
    }

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;

        OnValidate();
    }

    void Update()
    {
        playerInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        if (playerInputSpace)
        {
            rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, upAxis);
            forwardAxis = ProjectDirectionOnPlane(playerInputSpace.forward, upAxis);
        }
        else
        {
            rightAxis = ProjectDirectionOnPlane(Vector3.right, upAxis);
            forwardAxis = ProjectDirectionOnPlane(Vector3.forward, upAxis);
        }
        desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * moveSpeed;

        desiredJump |= Input.GetButtonDown("Jump");
    }

    void FixedUpdate()
    {
        Vector3 gravity = CustomGravity.GetGravity(body.position, out upAxis);
        UpdateState();

        AdjustVelocity();

        if (desiredJump)
        {
            desiredJump = false;
            Jump(gravity);
        }

        velocity += gravity * Time.deltaTime;

        body.velocity = velocity;

        ClearState();
    }

    void Jump(Vector3 gravity)
    {
        Vector3 jumpDirection;

        if (OnGround)
        {
            jumpDirection = contactNormal;
        }
        else if (OnSteep)
        {
            jumpDirection = steepNormal;
        }
        else if (jumpPhase < maxAirJumps)
        {
            jumpDirection = contactNormal;
        }
        else
        {
            return;
        }

        jumpDirection = (jumpDirection + upAxis).normalized;

        stepsSinceLastJump = 0;
        jumpPhase++;
        float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);
        float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
        if (alignedSpeed > 0f)
        {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }

        velocity += jumpDirection * jumpSpeed;

    }

    void AdjustVelocity()
    {
        Vector3 xAxis = ProjectDirectionOnPlane(rightAxis, contactNormal);
        Vector3 zAxis = ProjectDirectionOnPlane(forwardAxis, contactNormal);

        float currentX = Vector3.Dot(velocity, xAxis);
        float currentZ = Vector3.Dot(velocity, zAxis);

        float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;

        float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }

    void UpdateState()
    {
        stepsSinceLastGrounded++;
        stepsSinceLastJump++;
        velocity = body.velocity;
        if (OnGround || SnapToGround() || CheckSteepContacts())
        {
            stepsSinceLastGrounded = 0;
            jumpPhase = 0;
            if (groundContactCount > 1)
            {
                contactNormal.Normalize();
            }
        }
        else
        {
            contactNormal = upAxis;
        }
    }

    void ClearState()
    {
        groundContactCount = 0;
        contactNormal = Vector3.zero;

        steepContactCount = 0;
        steepNormal = Vector3.zero;
    }

    void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void EvaluateCollision(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            float upDot = Vector3.Dot(upAxis, normal);
            if (upDot >= minGroundDotProduct)
            {
                groundContactCount++;
                contactNormal += normal;
            }
            else if (normal.y > -0.01f)
            {
                steepContactCount += 1;
                steepNormal += normal;
            }
        }
    }

    Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
    {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }

    bool SnapToGround()
    {
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2)
        {
            return false;
        }
        float speed = velocity.magnitude;
        if (speed > maxSnapSpeed)
        {
            return false;
        }
        if (!Physics.Raycast(body.position, -upAxis, out RaycastHit hit, probeDistance, probeMask))
        {
            return false;
        }

        float upDot = Vector3.Dot(upAxis, hit.normal);

        if (upDot < minGroundDotProduct)
        {
            return false;
        }

        groundContactCount = 1;
        contactNormal = hit.normal;

        float dot = Vector3.Dot(velocity, hit.normal);

        if (dot > 0f)
        {
            velocity = (velocity - hit.normal * dot).normalized * speed;
        }
        return true;
    }

    bool CheckSteepContacts()
    {
        if (steepContactCount > 1)
        {
            steepNormal.Normalize();
            float upDot = Vector3.Dot(upAxis, steepNormal);
            if (upDot >= minGroundDotProduct)
            {
                groundContactCount = 1;
                contactNormal = steepNormal;
                return true;
            }
        }
        return false;
    }
}
