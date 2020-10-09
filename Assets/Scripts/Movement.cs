using System.Collections;
using System.Collections.Generic;
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
    float rSpeed = 0f;

    [SerializeField]
    ProcedurallyAnimatedLeg[] legs;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        localInput = Vector3.ClampMagnitude(transform.TransformDirection(new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"))), 1f);
        inputVelocity = Vector3.MoveTowards(inputVelocity, localInput, Time.deltaTime * 2f);
        worldVelocity = inputVelocity * moveSpeed;

        transform.position += (worldVelocity * Time.deltaTime);

        rSpeed = Mathf.MoveTowards(rSpeed, Input.GetAxis("Turn") * rotationSpeed, rotationSensitivity * Time.deltaTime);

        transform.Rotate(0f, rSpeed * Time.deltaTime, 0f);

        for (int i = 0; i < legs.Length; i++)
        {
            //legs[i].SetVelocity(CalculateLegVelocity(i));
        }
    }

    Vector3 CalculateLegVelocity(int index)
    {
        Vector3 legPoint = legs[index].restPosition;
        Vector3 direction = legPoint - transform.position;

        //Vector3 rotationalPoint = ((Quaternion.AngleAxis((rSpeed * timeBetweenSteps) / 2f, transform.up) * direction) + transform.position) - legPoint;

        //return rotationalPoint + (worldVelocity * timeBetweenSteps) / 2f;
        return worldVelocity;
    }
}
