using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private AddingImagesAtRunTime runtimeManager;
    private ARObjectTracker ARTracker;
    private UserInterfaceHandeler userInterFaceHandeler;
    private MainMenuHandeler mainMenuHandeler;
    private BottomPanelHandelerUI bottomPanel;
    private EndPanel endPanel;
    private NetworkHandeler networkHandeler;
    private List<String> previousBooks = new();

    private int booksFound = 0;
    private DateTime startTime;
    private void Start()
    {
        runtimeManager = GetComponent<AddingImagesAtRunTime>();

        ARTracker = GetComponent<ARObjectTracker>();

        GameObject UserInterface = GameObject.Find("UserInterface");
        userInterFaceHandeler = UserInterface.GetComponent<UserInterfaceHandeler>();

        GameObject BottomPanel = GameObject.Find("PanelBottom");
        bottomPanel = BottomPanel.GetComponent<BottomPanelHandelerUI>();

        GameObject MainMenu = GameObject.Find("MainMenuHandeler");
        mainMenuHandeler = MainMenu.GetComponent<MainMenuHandeler>();
        
        GameObject EndPanel = GameObject.Find("PanelEndScreen");
        endPanel = EndPanel.GetComponent<EndPanel>();

        Debug.Log($"BottomPanel 2: {bottomPanel}");

        networkHandeler = GetComponent<NetworkHandeler>();
        Debug.Log(networkHandeler);

        userInterFaceHandeler.StartUI();
    }

    public void InitializeBook(string bookName)
    {
        ARTracker.SetCurrentSearchingBook(bookName);
    }

    public void StartGame(int level)
    {
        Debug.Log($"Start game");
        System.Diagnostics.Debug.WriteLine($"Start game");
        startTime = DateTime.Now;
        userInterFaceHandeler.SwitchUIModeBottom();
        Debug.Log($"Switched UI mode");
        System.Diagnostics.Debug.WriteLine($"Switched UI mode");
        Debug.Log($"BottomPanel: {bottomPanel}");
        bottomPanel.SetLevel(level);
        Debug.Log($"Set level: {level}");
        System.Diagnostics.Debug.WriteLine($"Set level: {level}");
        networkHandeler.GetBookCoverImage((coverInformationImage) =>
        {
            System.Diagnostics.Debug.WriteLine($"Netwerk completed GetBookCoverImage: {coverInformationImage}");
            System.Diagnostics.Debug.WriteLine($"Added book to previousBooks: {coverInformationImage.IsbnNumber}");
            this.previousBooks.Add(coverInformationImage.IsbnNumber);
            System.Diagnostics.Debug.WriteLine($"Updated previous Books: {this.previousBooks}");
            runtimeManager.GetImage(coverInformationImage);
            System.Diagnostics.Debug.WriteLine($"Netwerk completed GetImage: {coverInformationImage}");
            
            networkHandeler.GetUIInformation(coverInformationImage, (bookInformationImage) =>
            {
                System.Diagnostics.Debug.WriteLine($"Netwerk completed GetUIInformation: {bookInformationImage}");
                Debug.Log("Change UI");
                bottomPanel.ChangeBookUI(bookInformationImage);
            }
            );
        });
    }

    public void BookFound()
    {
        if (booksFound >= 10)
        {
            Debug.Log($"Books found: {booksFound}");
            System.Diagnostics.Debug.WriteLine($"Books found: {booksFound}");
            TimeSpan elapsedTime = DateTime.Now - startTime;
            endPanel.EditTime(elapsedTime);
            userInterFaceHandeler.EndGame();
            previousBooks.Clear();
            return;
        }
        networkHandeler.GetBookCoverImage((coverInformationImage) =>
        {

            System.Diagnostics.Debug.WriteLine($"Netwerk completed GetBookCoverImage: {coverInformationImage}");

            if (!this.previousBooks.Contains(coverInformationImage.IsbnNumber))
            {
                System.Diagnostics.Debug.WriteLine($"Added book to previousBooks no double book: {coverInformationImage.IsbnNumber}");
                this.previousBooks.Add(coverInformationImage.IsbnNumber);
                System.Diagnostics.Debug.WriteLine($"Updated previous Books: {this.previousBooks}");

                runtimeManager.GetImage(coverInformationImage);
                System.Diagnostics.Debug.WriteLine($"Netwerk completed GetImage: {coverInformationImage}");

                networkHandeler.GetUIInformation(coverInformationImage, (bookInformationImage) =>
                {
                    System.Diagnostics.Debug.WriteLine($"Netwerk completed GetUIInformation: {bookInformationImage}");
                    booksFound++;
                    bottomPanel.ChangeBookUI(bookInformationImage);
                }
                );
            } else
            {
                Debug.Log($"Double book");
                System.Diagnostics.Debug.WriteLine($"Double book");
                this.BookFound();
            }
            
        });
    }

    public void ResetGame()
    {
        booksFound = 0;
        mainMenuHandeler.ResetMainMenu();
        userInterFaceHandeler.StartUI();
    }

}
