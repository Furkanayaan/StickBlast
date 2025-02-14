using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    public static UIManager I;
    public TMP_InputField inputFieldWidth;
    public TMP_InputField inputFieldHeight;
    public Button button;
    public Button failRestartButton;
    public GameObject failUI;
    public static int storedValueWidth = 0;
    public static int storedValueHeigth = 0;
    public TMP_Text pointText;
    public TMP_Text comboText;
    public Button soundButton;
    public TMP_Text comboPointText;
    public GameObject comboParent;
    public GameObject pointParent;
    private bool _isStopMovement = false;

    void Start() {
        I = this;
        //inputFieldWidth.text = storedValueWidth.ToString();
        
        //inputFieldHeight.text = storedValueHeigth.ToString();
        
        //Assigning the function that will execute when the button is clicked.
        button.onClick.AddListener(OnButtonClick);
        failRestartButton.onClick.AddListener(OnButtonClick);
    }

    private void Update() {
        pointText.text = GameManager.I.GetPoint().ToString();
    }

    public void ComboUIControl(int comboValue) {
        comboText.text = comboValue.ToString();
        comboParent.SetActive(true);
    }

    public IEnumerator DelayPointAnim(int value, Vector3 pos) {
        yield return new WaitForSeconds(1f);
        comboPointText.text = "+" + value;
        pointParent.transform.position = pos;
        pointParent.SetActive(true);
    }

    void OnButtonClick() {
        //Assigning the value from the input field to a static value.
        if (int.TryParse(inputFieldWidth.text, out int newValueWidth)) {
            storedValueWidth = newValueWidth;
        }
        
        if (int.TryParse(inputFieldHeight.text, out int newValueHeight)) {
            storedValueHeigth = newValueHeight;
        }
        //Rebuilding the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public IEnumerator ActiveFailUI() {
        yield return new WaitForSeconds(1f);
        failUI.SetActive(true);
        button.gameObject.SetActive(false);
        _isStopMovement = true;
    }

    public bool IsStopMovement() {
        return _isStopMovement;
    }
    
    public void SoundControl() {
        if (soundButton.transform.GetChild(0).gameObject.activeSelf) {
            soundButton.transform.GetChild(0).gameObject.SetActive(false);
            soundButton.transform.GetChild(1).gameObject.SetActive(true);
            GameManager.I.StopSound(0);
        }
        else if (soundButton.transform.GetChild(1).gameObject.activeSelf) {
            soundButton.transform.GetChild(1).gameObject.SetActive(false);
            soundButton.transform.GetChild(0).gameObject.SetActive(true);
            GameManager.I.PlaySound(0);
        }
    }
}
