using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour {
    
    private void Update() {
        if(GameObject.FindWithTag("Player").GetComponent<Transform>().position.y < -2) {
            transform.position = new Vector3(GameObject.FindWithTag("Player").GetComponent<Transform>().position.x, -2, -8);
        } else {
            transform.position = new Vector3(GameObject.FindWithTag("Player").GetComponent<Transform>().position.x, GameObject.FindWithTag("Player").GetComponent<Transform>().position.y, -8);
        }

        // if(GameObject.FindWithTag("Player").GetComponent<Transform>().position.x < 0) {
        //     transform.position = new Vector3(0, transform.position.y, -8);
        // } else {
        //     transform.position = new Vector3(GameObject.FindWithTag("Player").GetComponent<Transform>().position.x, transform.position.y, -8);
        // }
    }
}
