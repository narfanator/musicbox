using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class NewSound : MonoBehaviour {

    public GameObject soundSphere;
    private AudioSource sound;
    public UnityEngine.UI.Text title;
    public int i = 0;
    static FileInfo[] soundFiles;

    void Awake() {
        soundFiles = (new DirectoryInfo("Assets/Sounds/")).GetFiles("*.wav");
        sound = soundSphere.GetComponent<AudioSource>();
        Debug.Log(soundFiles.Length);
    }

    // Use this for initialization
    void Start() {
        Sync();
    }

    // Update is called once per frame
    void Update() {

    }

    void Sync() {
        title.text = soundFiles[i].Name;
        sound.clip = Resources.Load<AudioClip>("Sounds/" + soundFiles[i]);
    }

    public void Next() {
        if (i + 1 < soundFiles.Length) { i++; }
        Sync();
    }

    public void Previous() {
        if (i - 1 >= 0) { i--; }
        Sync();
    }

    public void Sanity(string foo) {
        Debug.Log("sane: " + foo);
    }

    public void OnTriggerEnter(Collider other) {
        Debug.Log("enter");
        sound.PlayOneShot(sound.clip);
    }

    void OnCollisionEnter(Collision collision) {
        Debug.Log("collision enter");
        sound.PlayOneShot(sound.clip);
    }
}
