using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager I;
    public int point;
    public AudioSource[] allAudios;// 0 = music, 1 = put, 2 = combo, 3 = clear, 4 = fail movement
    // Start is called before the first frame update
    void Start() {
        I = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int GetPoint() {
        return point;
    }

    public void GainPoint(int value) {
        point += value;
    }

    public void PlaySound(int index) {
        allAudios[index].Play();
    }
    
    public void StopSound(int index) {
        allAudios[index].Stop();
    }
}
