using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARObjectTracker : MonoBehaviour
{
    //private GameObject placeablePrefab;
    private ARTrackedImageManager trackedImageManager;
    private GameManager gameManager;
    private string currentSearchingBook;
    private bool found = false;
    private bool isReady = false;


    //public void Initialize(GameObject PlaceablePrefab)
    //{
    //    placeablePrefab = PlaceablePrefab;
    //}

    private void Start()
    {
        gameManager = GameObject.Find("XR Origin").GetComponent<GameManager>();
        //Instantiate(placeablePrefab);
        //placeablePrefab.SetActive(false);
        trackedImageManager = GetComponent<ARTrackedImageManager>();
        trackedImageManager.trackedImagesChanged += ImageChanged;
        System.Diagnostics.Debug.WriteLine("Start ARObjectTracker");
        isReady = true;
    }

    public bool getReady()
    {
        return isReady;
    }

    public void SetCurrentSearchingBook(string searchingBook)
    {
        Debug.Log($"SetCurrentSearchinBook: {searchingBook}");
        System.Diagnostics.Debug.WriteLine($"SetCurrentSearchinBook: {searchingBook}");
        currentSearchingBook = searchingBook;
        found = false;
    }

    private void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += ImageChanged;
    }

    private void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= ImageChanged;
    }

    private void ImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            //System.Diagnostics.Debug.WriteLine("Added");
            //System.Diagnostics.Debug.WriteLine(trackedImage.referenceImage.name);
            //System.Diagnostics.Debug.WriteLine("End Added");

            UpdateImage(trackedImage);
        }
        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {


                UpdateImage(trackedImage);

                if (currentSearchingBook == trackedImage.referenceImage.name & !found)
                {
                    System.Diagnostics.Debug.WriteLine("Updated");
                    System.Diagnostics.Debug.WriteLine(trackedImage.referenceImage.name);
                    System.Diagnostics.Debug.WriteLine("End Updated");
                    found = true;
                    gameManager.BookFound();
                }

            }
            else
            {
               // placeablePrefab.SetActive(false);
            }


        }
        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            //System.Diagnostics.Debug.WriteLine("Removed");
            //System.Diagnostics.Debug.WriteLine(trackedImage.referenceImage.name);
            //System.Diagnostics.Debug.WriteLine("End Removed");

            //placeablePrefab.SetActive(false);
        }
    }

    private void UpdateImage(ARTrackedImage trackedImage)
    {
        var minLocalScalar = Mathf.Min(trackedImage.size.x, trackedImage.size.y) / 3;

        //placeablePrefab.transform.localScale = new Vector3(minLocalScalar, minLocalScalar, minLocalScalar);

       // placeablePrefab.transform.position = trackedImage.transform.position;
       // placeablePrefab.transform.rotation = Quaternion.identity;
       // placeablePrefab.SetActive(true);
    }
}
