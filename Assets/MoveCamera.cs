using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{

    [SerializeField] float speed = 3f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            transform.Translate(Vector3.left * speed);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            transform.Translate(Vector3.right * speed);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                transform.Translate(Vector3.back * speed);
            } else {
                transform.Translate(Vector3.up * speed);
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                transform.Translate(Vector3.forward * speed);
            } else {
                transform.Translate(Vector3.down * speed);
            }
        }
    }
}
