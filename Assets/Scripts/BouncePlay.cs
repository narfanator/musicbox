using UnityEngine;
using System.Collections;

public class BouncePlay : MonoBehaviour {

    public AudioSource source;

    // Use this for initialization
    void Start() {
        Debug.Log(Physics.gravity);
    }

    // Update is called once per frame
    void Update() {

    }

    void OnCollisionEnter(Collision collision) {
        //Use our source to play our sound
        source.PlayOneShot(source.clip, source.volume);

        /* Calculate the "bounce" force needed to reach our sound cube
        vy = (h - 0.5(g)t^2) / t
        At the top (2m) we want to play at 40 bpm, which is 1.5 sec between each start-of-play
        At the bottom (0.5m) we want to play at 120 bmp, which is 

        Plug that in -
        2 - 0.5*(-9.8)(1.5)^2 / 1.5 = 9.35

        So, let's take that, and make it proportional to actual height
        */
        float vy = 9.35f * (source.gameObject.transform.position.y / 2);
        gameObject.GetComponent<Rigidbody>().velocity = new Vector3(0, vy, 0);
    }
}
