using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    float mSpeed = 10f;
    Vector3 inputVelocity, localInput, worldVelocity;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        localInput = Vector3.ClampMagnitude(transform.TransformDirection(new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"))), 1f);
        inputVelocity = Vector3.MoveTowards(inputVelocity, localInput, Time.deltaTime * 2f);
        worldVelocity = inputVelocity * mSpeed;

        transform.position += (worldVelocity * Time.deltaTime);


    }
}
