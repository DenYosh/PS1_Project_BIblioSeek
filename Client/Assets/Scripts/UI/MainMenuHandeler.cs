using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuHandeler : MonoBehaviour, IDragHandler, IEndDragHandler
{
    private int currentSlide = 0;
    private List<GameObject> listRound = new();
    private List<GameObject> listText = new();
    private Button ButtonStart;
    private GameObject levelSelect;
    private GameObject slideSelect;
    private GameManager gameManager;

    void Start()
    {
        gameManager = GameObject.Find("XR Origin").GetComponent<GameManager>();
        Debug.Log($"GameManager: {gameManager}");
        Debug.Log($"GameManager: {GameObject.Find("XR Origin")}");
        SetupChildElements();
        SetupSlideMenu();
        StartButtons();
    }


    void SetupChildElements()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.name.ToString().Contains("SlideMenu"))
            {
                Debug.Log($"SlideMenu: {child.gameObject} name: {child.name}");
                System.Diagnostics.Debug.WriteLine($"SlideMenu: {child.gameObject} name: {child.name}");
                slideSelect = child.gameObject;
            }
            else if (child.name.ToString().Contains("InfoText"))
            {
                Debug.Log($"InfoText: {child.gameObject} name: {child.name}");
                System.Diagnostics.Debug.WriteLine($"InfoText: {child.gameObject} name: {child.name}");
                listText.Add(child.gameObject);
                if (child.name != "InfoTextFirst")
                {
                    child.gameObject.SetActive(false);
                }
            }
            else if (child.name.ToString().Contains("LevelSelect"))
            {
                levelSelect = child.gameObject;
                levelSelect.gameObject.SetActive(false);
            }
        }
    }

    void SetupSlideMenu()
    {
        Debug.Log(slideSelect.transform.childCount);
        for (int i = 0; i < slideSelect.transform.childCount; i++)
        {
            Transform child = slideSelect.transform.GetChild(i);
            if (child.name.ToString().Contains("Slide"))
            {
                Debug.Log($"ChildSlide: {child.gameObject} name: {child.name}");
                System.Diagnostics.Debug.WriteLine($"ChildSlide: {child.gameObject} name: {child.name}");
                listRound.Add(child.gameObject);
            }
            else if (child.name.ToString().Contains("InfoText"))
            {
                Debug.Log($"ChildText: {child.gameObject} name: {child.name}");
                System.Diagnostics.Debug.WriteLine($"ChildText: {child.gameObject} name: {child.name}");
                listText.Add(child.gameObject);
                if (child.name != "InfoTextFirst")
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }

    void StartButtons()
    {
        Button nextButton = slideSelect.transform.Find("NextButton").GetComponent<Button>();
        System.Diagnostics.Debug.WriteLine($"Button Next Slide: {nextButton}");
        Debug.Log($"Button Next Slide: {nextButton}");
        nextButton.onClick.AddListener(TaskOnClickNext);

        Button ButtonPrevious = slideSelect.transform.Find("PreviousButton").GetComponent<Button>();
        System.Diagnostics.Debug.WriteLine($"Button Prev Slide: {ButtonPrevious}");
        Debug.Log($"Button Prev Slide: {ButtonPrevious}");
        ButtonPrevious.onClick.AddListener(TaskOnClickPrev);

        ButtonStart = slideSelect.transform.Find("StartButton").GetComponent<Button>();
        System.Diagnostics.Debug.WriteLine($"Button Prev Slide: {ButtonStart}");
        Debug.Log($"Button Prev Slide: {ButtonStart}");
        ButtonStart.onClick.AddListener(StartLevelSelect);
        ButtonStart.gameObject.SetActive(false);
    }

    void OpenSlide(int newSlide)
    {
        GameObject newElement = listRound.ElementAt(newSlide);
        newElement.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        newElement.transform.position = new Vector3(newElement.transform.position.x, newElement.transform.position.y - 10, newElement.transform.position.z);

        GameObject oldElement = listRound.ElementAt(currentSlide);
        oldElement.transform.localScale = new Vector3(1f, 1f, 1f);
        oldElement.transform.position = new Vector3(oldElement.transform.position.x, newElement.transform.position.y + 10, oldElement.transform.position.z);

        ButtonStart.gameObject.SetActive(false);
        listText.ElementAt(currentSlide).SetActive(false);
        listText.ElementAt(newSlide).SetActive(true);

        if (newSlide == 2)
        {
            ButtonStart.gameObject.SetActive(true);
        }

        currentSlide = newSlide;
    }
    void TaskOnClickNext()
    {
        Debug.Log($"CurrentSlide: {currentSlide}  Pressed: next");
        System.Diagnostics.Debug.WriteLine($"CurrentSlide: {currentSlide}  Pressed: next");

        switch (currentSlide)
        {
            case 0:
                OpenSlide(1);
                break;
            case 1:
                OpenSlide(2);
                break;
            case 2:
                OpenSlide(0);
                break;
        }
    }

    void TaskOnClickPrev()
    {
        Debug.Log($"CurrentSlide: {currentSlide} Pressed: prev");
        System.Diagnostics.Debug.WriteLine($"CurrentSlide: {currentSlide} Pressed: prev");
        switch (currentSlide)
        {
            case 0:
                OpenSlide(2);
                break;
            case 1:
                OpenSlide(0);
                break;
            case 2:
                OpenSlide(1);
                break;
        }
    }
    private float difference;
    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.dragging)
        {
            difference = eventData.pressPosition.x - eventData.position.x;

        }
        Debug.Log($"Start {difference}");

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"End {difference}");
        System.Diagnostics.Debug.WriteLine($"End {difference}");

        if (difference > 350 & currentSlide != 2)
        {
            OpenSlide(currentSlide + 1);
        }
        else if (difference < -350 & currentSlide != 0)
        {
            OpenSlide(currentSlide - 1);
        }

    }

    void StartLevelSelect()
    {
        Debug.Log("Start Level Select");
        System.Diagnostics.Debug.WriteLine("Start Level Select");
        levelSelect.SetActive(true);
        slideSelect.SetActive(false);
        SetupLevelSelect();
    }

    public void ResetMainMenu() 
    {
        OpenSlide(0);
        levelSelect.SetActive(false);
        slideSelect.SetActive(true);
    }

    void SetupLevelSelect()
    {
        Debug.Log($"LevelSelect: {levelSelect.transform.childCount}");
        System.Diagnostics.Debug.WriteLine($"LevelSelect: {levelSelect.transform.childCount}");

        for (int i = 0; i < levelSelect.transform.childCount; i++)
        {
            if (levelSelect.transform.Find("Level" + i + "Button"))
            {
                Button levelButton = levelSelect.transform.Find("Level" + i + "Button").GetComponent<Button>();
                System.Diagnostics.Debug.WriteLine($"Button level {i}: {levelButton}");
                Debug.Log($"Button level 1: {levelButton}");
                switch (i)
                {
                    case 1:
                        levelButton.onClick.AddListener(() =>
                        {
                            SelectLevel(1);
                        });
                        break;
                    case 2:
                        levelButton.onClick.AddListener(() =>
                        {
                            SelectLevel(2);
                        });
                        break;
                    case 3:
                        levelButton.onClick.AddListener(() =>
                        {
                            SelectLevel(3);
                        });
                        break;
                }
            }

        }
    }

    void SelectLevel(int level)
    {
        Debug.Log(gameManager);
        gameManager.StartGame(level);
    }
}
