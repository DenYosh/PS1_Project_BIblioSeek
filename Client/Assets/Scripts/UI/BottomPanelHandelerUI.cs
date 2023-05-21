using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class BottomPanelHandelerUI : MonoBehaviour
{
    private TMP_Text bookTitle;
    private TMP_Text bookWriter;
    private TMP_Text bookZone;
    private TMP_Text bookZoneExtra;
    private Image bookZoneImage;
    private Image bookCover;

    void Start()
    {
        Debug.Log("Start BottomPanel");
        bookTitle = gameObject.transform.Find("BookTitle").GetComponent<TMP_Text>();
        bookWriter = gameObject.transform.Find("BookWriter").GetComponent<TMP_Text>();
        bookZone = gameObject.transform.Find("BookZone").GetComponent<TMP_Text>();
        bookZoneExtra = gameObject.transform.Find("BookZoneExtra").GetComponent<TMP_Text>();
        bookZoneImage = gameObject.transform.Find("BookZoneImage").GetComponent<Image>();
        bookCover = gameObject.transform.Find("BookCover").GetComponent<Image>();
        
        Debug.Log(bookTitle, bookZone);
    }

    public void SetLevel(int newLevel)
    {
        bookWriter.gameObject.SetActive(true);
        bookZoneExtra.gameObject.SetActive(true);
        switch (newLevel)
        {
            case 2:
                bookZoneExtra.gameObject.SetActive(false);
                break; 
            case 3:
                bookWriter.gameObject.SetActive(false);
                bookZoneExtra.gameObject.SetActive(false);
                break;
        }
    }

    public void ChangeBookUI(BookInformationImage bookinf)
    {
        bookTitle.text = bookinf.bookTitle;
        bookWriter.text = bookinf.bookWriter;
        bookZone.text = bookinf.bookZone;
        bookZoneExtra.text = bookinf.bookZoneExtra;
        bookZoneImage.sprite = bookinf.bookZoneImage;
        bookCover.sprite = bookinf.bookCover;
    }
}
