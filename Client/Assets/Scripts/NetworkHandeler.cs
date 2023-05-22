using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkHandeler : MonoBehaviour
{
    private readonly string _APIUrl = "http://yoshi.blackcreek.nl:3000/";

    TaskCompletionSource<CoverInformationImage> tcsCover;
    public async void GetBookCoverImage(System.Action<CoverInformationImage> callback)
    {
        Debug.Log($"GetBookCoverImage:");
        System.Diagnostics.Debug.WriteLine($"GetBookCoverImage:");
        tcsCover = new TaskCompletionSource<CoverInformationImage>();
        StartCoroutine(StartGettingBookCoverInformation());
        Task<CoverInformationImage> task = tcsCover.Task;
        CoverInformationImage result = await task;
        Debug.Log($"GetBookCoverImage: {result}");
        System.Diagnostics.Debug.WriteLine($"GetBookCoverImage: {result}");
        tcsCover = null;
        callback?.Invoke(result);
    }

    private IEnumerator StartGettingBookCoverInformation()
    {
        Debug.Log($"StartGettingBookCoverInformation:");
        System.Diagnostics.Debug.WriteLine($"StartGettingBookCoverInformation:");
        int completedFunctions = 0;
        yield return StartCoroutine(GetCoverInformation(_APIUrl + "cover", (CoverInformation) =>
        {
            if (CoverInformation == null)
            {
                Debug.Log($"ERROR: GetCoverInformation");
                System.Diagnostics.Debug.WriteLine($"ERROR: GetCoverInformation");
                return;
            }
            Debug.Log($"StartGettingBookCoverInformation: CallBack 1");
            System.Diagnostics.Debug.WriteLine($"StartGettingBookCoverInformation: Callback 1");
            completedFunctions++;

            StartCoroutine(GetImageFromAPI(CoverInformation.bookCover, (downloadedTexture) =>
            {
                if (downloadedTexture == null)
                {
                    Debug.Log($"ERROR: GetImageFromAPI");
                    System.Diagnostics.Debug.WriteLine($"ERROR: GetImageFromAPI");
                    return;
                }
                Debug.Log($"StartGettingBookCoverInformation: CallBack 2");
                System.Diagnostics.Debug.WriteLine($"StartGettingBookCoverInformation: Callback 2");
                CoverInformationImage bookCoverImage = new CoverInformationImage(CoverInformation.Isbn, downloadedTexture, CoverInformation.bookId);
                tcsCover.SetResult(bookCoverImage);
                completedFunctions++;
            }));
        }));

        yield return new WaitUntil(() => completedFunctions == 2);

    }

    private IEnumerator GetImageFromAPI(string url, System.Action<Texture2D> callback)
    {
        Debug.Log($"GetImageFromAPI: {url}");
        System.Diagnostics.Debug.WriteLine($"GetImageFromAPI: {url}");
        UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url);
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            Texture2D tex = DownloadHandlerTexture.GetContent(webRequest);
            Debug.Log($"Image: {tex} | URL: {url}");
            System.Diagnostics.Debug.WriteLine($"Image: {tex} | URL: {url}");
            callback?.Invoke(tex);
        }
        else
        {
            Debug.Log($"Failed to download image: {webRequest.error}");
            System.Diagnostics.Debug.WriteLine($"Failed to download image: {webRequest.error}");
            callback?.Invoke(null);
        }

    }
    private IEnumerator GetCoverInformation(string url, System.Action<CoverInformation> callback)
    {
        Debug.Log($"GetCoverInformation: {url}");
        System.Diagnostics.Debug.WriteLine($"GetCoverInformation: {url}");
        UnityWebRequest webRequest = UnityWebRequest.Get(url);
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            CoverInformation newInformation = JsonUtility.FromJson<CoverInformation>(webRequest.downloadHandler.text);
            Debug.Log($"Webresult: {webRequest.downloadHandler.text}");
            System.Diagnostics.Debug.WriteLine($"Webresult: {webRequest.downloadHandler.text}");
            callback?.Invoke(newInformation);
        }
        else
        {
            Debug.Log($"Failed to get JSON data: {webRequest.error} + {webRequest.responseCode}  + {webRequest.result}");
            callback?.Invoke(null);
        }
    }






    TaskCompletionSource<BookInformationImage> tcsBookInf;
    public async void GetUIInformation(CoverInformationImage coverinfo, System.Action<BookInformationImage> callback)
    {
        Debug.Log($"GetUIInformation");
        System.Diagnostics.Debug.WriteLine($"GetUIInformation");
        tcsBookInf = new TaskCompletionSource<BookInformationImage>();
        StartCoroutine(StartGettingBookInformation(coverinfo));

        Task<BookInformationImage> task = tcsBookInf.Task;
        BookInformationImage result = await task;
        Debug.Log($"GetUIInformation: {result}");
        System.Diagnostics.Debug.WriteLine($"GetUIInformation: {result}");
        tcsBookInf = null;
        callback?.Invoke(result);
    }

    private IEnumerator StartGettingBookInformation(CoverInformationImage coverinfo)
    {
        Debug.Log($"StartGettingBookInformation");
        System.Diagnostics.Debug.WriteLine($"StartGettingBookInformation");
        int completedFunctions = 0;
        string newUrl = _APIUrl + "getInfo?Id=" + coverinfo.bookId;
       // string newUrl = _APIUrl + "testget";
        //TODO: Check API URL
        yield return StartCoroutine(GetBookInformation(newUrl, (bookInformation) =>
        {
            if (bookInformation == null) 
            {
                Debug.Log($"ERROR: GetBookInformation");
                System.Diagnostics.Debug.WriteLine($"ERROR: GetBookInformation");
                return;
            }
            BookInformation BookInformation = bookInformation;
            Debug.Log($"StartGettingBookInformation: callback 1");
            System.Diagnostics.Debug.WriteLine($"StartGettingBookInformation: callback 1");
            completedFunctions++;

            string newUrl = _APIUrl + BookInformation.bookZoneImage;
            StartCoroutine(GetImageFromAPI(newUrl, (downloadedTexture) =>
            {
                if (downloadedTexture == null) 
                {
                    Debug.Log($"ERROR: GetImageFromAPI");
                    System.Diagnostics.Debug.WriteLine($"ERROR: GetImageFromAPI");
                    return; 
                }
                Debug.Log($"StartGettingBookInformation: callback 2");
                System.Diagnostics.Debug.WriteLine($"StartGettingBookInformation: callback 2");
                Sprite bookZoneImage = Sprite.Create(downloadedTexture, new Rect(0.0f, 0.0f, downloadedTexture.width, downloadedTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
                Sprite bookCoverSprite = Sprite.Create(coverinfo.Image, new Rect(0.0f, 0.0f, coverinfo.Image.width, coverinfo.Image.height), new Vector2(0.5f, 0.5f), 100.0f);
                BookInformationImage bookInformationImage = new BookInformationImage(
                    BookInformation.bookTitle,
                    BookInformation.bookWriter,
                    BookInformation.bookZone,
                    BookInformation.bookZoneExtra,
                    bookZoneImage,
                    bookCoverSprite);
                tcsBookInf.SetResult(bookInformationImage);
                completedFunctions++;
            }));
        }));

        yield return new WaitUntil(() => completedFunctions == 2);

    }

    private IEnumerator GetBookInformation(string url, System.Action<BookInformation> callback)
    {
        Debug.Log($"GetBookInformation | URL: {url}");
        System.Diagnostics.Debug.WriteLine($"GetBookInformation | URL: {url}");
        UnityWebRequest webRequest = UnityWebRequest.Get(url);
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            System.Diagnostics.Debug.WriteLine($"Bookinformation: {webRequest.downloadHandler.text}");
            Debug.Log($"Bookinformation: {webRequest.downloadHandler.text}");
            BookInformation bookInformation = JsonUtility.FromJson<BookInformation>(webRequest.downloadHandler.text);
            callback?.Invoke(bookInformation);
        }
        else
        {
            Debug.Log($"Failed to get JSON data: {webRequest.error} + {webRequest.responseCode}  + {webRequest.result}");
            callback?.Invoke(null);
        }
    }
}
