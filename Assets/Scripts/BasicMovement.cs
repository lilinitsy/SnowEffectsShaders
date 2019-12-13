using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMovement : MonoBehaviour
{
	public float speed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey("w"))
		{
			transform.position += transform.TransformDirection(Vector3.forward) * Time.deltaTime * speed;
		}

		else if(Input.GetKey("s"))
		{
			transform.position -= transform.TransformDirection(Vector3.forward) * Time.deltaTime * speed;
		}

		if(Input.GetKey("a"))
		{
			transform.position -= transform.TransformDirection(Vector3.right) * Time.deltaTime * speed;
		}

		else if(Input.GetKey("d"))
		{
			transform.position += transform.TransformDirection(Vector3.right) * Time.deltaTime * speed;
		}
    }
}
