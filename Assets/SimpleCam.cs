using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCam : MonoBehaviour
{
    [SerializeField]
    Transform target;

    Vector3 pos;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        pos = target.position;
        pos.x = transform.position.x;
        pos.y = transform.position.y;

        transform.position = Vector3.Lerp(transform.position, pos, 1);
    }
}
