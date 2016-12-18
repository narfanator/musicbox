using UnityEngine;
using System.Collections;

public class Bounce : MonoBehaviour {

    private Rigidbody rigidBody;

    // Use this for initialization
    void Start() {
        rigidBody = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update() {
        //Check vs zero vector to reduce log spam. TODO: Actually do something in that case, and maybe within epsilon?
        if (GetComponent<FixedJoint>() == null && rigidBody.velocity != Vector3.zero) {
            //transform.forward = rigidBody.velocity;
        }
    }

    void OnCollisionEnter(Collision collision) {
       // rigidBody.velocity = Vector3.Reflect(rigidBody.velocity, collision.contacts[0].normal);
    }
}
