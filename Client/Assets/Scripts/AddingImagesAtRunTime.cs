using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class AddingImagesAtRunTime : MonoBehaviour
{
    private ARTrackedImageManager trackImageManager;
    [SerializeField]
    GameObject placeblePrefab;
    [SerializeField]
    private XRReferenceImageLibrary runTimeLib;
    private GameManager gameManager;


    void Start()
    {
        System.Diagnostics.Debug.WriteLine("Start adding images");
        trackImageManager = gameObject.AddComponent<ARTrackedImageManager>();
        // gameObject.AddComponent<ARObjectTracker>().Initialize(placeblePrefab);
        gameObject.AddComponent<ARObjectTracker>();
        gameManager = GameObject.Find("XR Origin").GetComponent<GameManager>();

    }


    public void GetImage(CoverInformationImage coverInformationImage)
    {
        coverInformationImage.Image.name = coverInformationImage.IsbnNumber;
        System.Diagnostics.Debug.WriteLine(coverInformationImage.Image);
        StartCoroutine(AddImageJob(coverInformationImage.Image));

        trackImageManager.enabled = true;
        System.Diagnostics.Debug.WriteLine($"Active and Enabled: {trackImageManager.isActiveAndEnabled}");
        System.Diagnostics.Debug.WriteLine($"Support: {trackImageManager.descriptor.supportsMutableLibrary}");

    }


    public IEnumerator AddImageJob(Texture2D texture2D)
    {
        trackImageManager.referenceLibrary = trackImageManager.CreateRuntimeLibrary(runTimeLib);
        trackImageManager.requestedMaxNumberOfMovingImages = 3;

        trackImageManager.enabled = true;

        yield return null;
        System.Diagnostics.Debug.WriteLine($"Adding image: {texture2D}");

        try
        {
            if (trackImageManager.subsystem.subsystemDescriptor.supportsMutableLibrary)
            {
                MutableRuntimeReferenceImageLibrary mutableLibrary = trackImageManager.referenceLibrary as MutableRuntimeReferenceImageLibrary;

                AddReferenceImageJobState jobHandle = mutableLibrary.ScheduleAddImageWithValidationJob(
                    texture2D,
                    texture2D.name,
                    0.1f);

                while (!jobHandle.jobHandle.IsCompleted)
                {
                }
                if (jobHandle.jobHandle.IsCompleted)
                {
                    System.Diagnostics.Debug.WriteLine($"Job Completed: {texture2D.name}");
                    System.Diagnostics.Debug.WriteLine($"Job Completed current Status: {jobHandle.status}");
                    gameManager.InitializeBook(texture2D.name);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"ELSE: {trackImageManager.referenceLibrary is MutableRuntimeReferenceImageLibrary libTest}");
            }

        }
        catch (Exception err)
        {
            if (texture2D == null)
            {
                System.Diagnostics.Debug.WriteLine("texture2D is null");
            }
            System.Diagnostics.Debug.WriteLine($"Error: {err.ToString()}");
        }
    }

}
