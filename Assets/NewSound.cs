using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class NewSound : MonoBehaviour {

    public GameObject soundSphere;
    private AudioSource sound;
    public UnityEngine.UI.Text title;
    public int i = 0;
    //static FileInfo[] soundFiles;
    public GameObject soundSpheres;

    public List<AudioClip> sounds = new List<AudioClip>();

    void Awake() {
        //soundFiles = (new DirectoryInfo("Assets/Sounds/")).GetFiles("*.wav");
        sound = soundSphere.GetComponent<AudioSource>();
    }

    // Use this for initialization
    void Start() {
        Sync();
    }

    // Update is called once per frame
    void Update() {

    }

    void Sync() {
        title.text = sounds[i].name;//soundFiles[i].Name;
        //string link = "file://" + soundFiles[i].FullName;
        //Debug.Log(link);
        //WWW www = new WWW(link);
        AudioClip clip = sounds[i];//www.GetAudioClip(true);
        if (clip) {
            sound.clip = clip;
            Debug.Log(sound.clip.length);
        } else {
            Debug.Log("Failed to load clip");
        }
    }

    public void Next() {
        if (i + 1 < sounds.Count) { i++; }
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

    public GameObject roost;
    public void spawnNewSphere() {
        GameObject newSphere = Object.Instantiate(soundSphere);
        Destroy(newSphere.GetComponent<FixedJoint>());
        newSphere.transform.parent = roost.transform;
        newSphere.transform.localPosition = new Vector3(0, 1.25f, 0);
        newSphere.transform.localRotation = new Quaternion(0, 0, 0, 0);
        soundSphere = newSphere;
        sound = soundSphere.GetComponent<AudioSource>();
    }
}
