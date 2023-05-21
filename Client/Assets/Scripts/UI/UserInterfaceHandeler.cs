using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInterfaceHandeler : MonoBehaviour
{
    private GameObject BackgroundImage;
    private GameObject fullscreenCanvas;
    private GameObject bottomCanvas;
    private GameObject endScreenCanvas;

    void Start()
    {
        System.Diagnostics.Debug.WriteLine("Starting UI");
        Debug.Log("Starting UI");

        BackgroundImage = GameObject.Find("BackgroundImage");
        System.Diagnostics.Debug.WriteLine($"BackgroundImage: {BackgroundImage}");
        Debug.Log($"BackgroundImage: {BackgroundImage}");

        fullscreenCanvas = GameObject.Find("PanelFullScreen");
        System.Diagnostics.Debug.WriteLine($"fullscreenCanvas: {fullscreenCanvas}");
        Debug.Log($"fullscreenCanvas: {fullscreenCanvas}");

        bottomCanvas = GameObject.Find("PanelBottom");
        System.Diagnostics.Debug.WriteLine($"bottomCanvas: {bottomCanvas}");
        Debug.Log($"bottomCanvas: {bottomCanvas}");

        endScreenCanvas = GameObject.Find("PanelEndScreen");
        System.Diagnostics.Debug.WriteLine($"PanelEndScreen: {endScreenCanvas}");
        Debug.Log($"PanelEndScreen: {endScreenCanvas}");

    }

    public void StartUI()
    {
        bottomCanvas.gameObject.SetActive(false);
        endScreenCanvas.gameObject.SetActive(false);
        BackgroundImage.gameObject.SetActive(true);
        fullscreenCanvas.gameObject.SetActive(true);

    }

    public void SwitchUIModeBottom()
    {
        endScreenCanvas.gameObject.SetActive(false);
        bottomCanvas.gameObject.SetActive(true);
        fullscreenCanvas.gameObject.SetActive(false);
        BackgroundImage.gameObject.SetActive(false);
    }

    public void EndGame()
    {
        fullscreenCanvas.gameObject.SetActive(false);
        BackgroundImage.gameObject.SetActive(false);
        bottomCanvas.gameObject.SetActive(false);
        endScreenCanvas.gameObject.SetActive(true);

    }
}
