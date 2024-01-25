using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class StreetViewLoader : MonoBehaviour
{
    private string location = "49.1433689,-8.6103176";
    private string fov = "90";
    private string pitch1 = "90";
    private string pitch2 = "-90";
    private string heading_piso = "-180";
    private string heading_cielo = "-90";


    private string heading1 = "0";
    private string heading2 = "90";
    private string heading3 = "180";
    private string heading4 = "270";
    private string size2 = "13312x6656";
    private string apiKey2 = "PUT YOUR GOOGLE MAP API API KEY";
    public Renderer[] imagen_Map;
    public Renderer[] Cielo_Tierra_Map;
    public Renderer Google_Equi;

    private Texture2D[] downloadedTextures = new Texture2D[6];
    private Texture2D panoramaTexture;
    public InputField iField;
    public string searchQuery = "Torres del paine Chile"; 
   
    public UnityEngine.UI.Image myImg;

    public Texture2D texture2convert = null;

    void Start()
    {
        iField.text = "Valle de la Muerte";
        StartCoroutine(DownloadStreetView2());
    }


    public void boton_apretado_pa_crear()
    {     
        StartCoroutine(DownloadStreetView2());
    }


    IEnumerator DownloadStreetView2()
    {
        searchQuery = iField.text;
        iField.text = "";
        print(searchQuery);


        // build the request URL with the search query and API key
        string searchURL = $"https://maps.googleapis.com/maps/api/geocode/json?address={searchQuery}&key={apiKey2}";

        string[] headings = {heading1, heading2, heading3, heading4};
        string[] heading_piso_cielo = {heading_piso, heading_cielo};
        string[] pitchs = {pitch1, pitch2};

        for (int i = 0; i < 4; i++)
        {

            UnityWebRequest searchRequest = UnityWebRequest.Get(searchURL);
            yield return searchRequest.SendWebRequest();

            // check for any errors
            if (searchRequest.result == UnityWebRequest.Result.ConnectionError || searchRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error searching for location: {searchRequest.error}");
                yield break;
            }

            // get the latitude and longitude from the search response
            string jsonResult = searchRequest.downloadHandler.text;
            float lat = JsonUtility.FromJson<GeoCodeResult>(jsonResult).results[0].geometry.location.lat;
            float lng = JsonUtility.FromJson<GeoCodeResult>(jsonResult).results[0].geometry.location.lng;


            //string url = "https://maps.googleapis.com/maps/api/streetview?location=" + location +
            //   "&fov=" + fov + "&heading=" + headings[i] + "&size=" + size2 + "&key=" + apiKey2;
            string location2 = $"{lat}, {lng}";
            string url = "https://maps.googleapis.com/maps/api/streetview?location=" + location2 +
                "&fov=" + fov + "&heading=" + headings[i] + "&size=" + size2 + "&key=" + apiKey2;  

            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                // Do something with the downloaded texture, like apply it to a material or save it as a file
                imagen_Map[i].material.mainTexture = texture;
                downloadedTextures[i] = texture;
            }
            else
            {
                Debug.LogError("StreetViewDownloader: Download error: " + request.error);
            }
        }

        for (int i = 0; i < 2; i++)
        {
            UnityWebRequest searchRequest = UnityWebRequest.Get(searchURL);
            yield return searchRequest.SendWebRequest();

            // check for any errors
            if (searchRequest.result == UnityWebRequest.Result.ConnectionError || searchRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error searching for location: {searchRequest.error}");
                yield break;
            }

            string jsonResult = searchRequest.downloadHandler.text;
            float lat = JsonUtility.FromJson<GeoCodeResult>(jsonResult).results[0].geometry.location.lat;
            float lng = JsonUtility.FromJson<GeoCodeResult>(jsonResult).results[0].geometry.location.lng;
            string location2 = $"{lat}, {lng}";
            

            string url = "https://maps.googleapis.com/maps/api/streetview?location=" + location2 +
                "&fov=" + fov  + "&heading=" + heading_piso_cielo[i] + "&pitch=" + pitchs[i] + "&size=" + size2 + "&key=" + apiKey2;

            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                // Do something with the downloaded texture
                Cielo_Tierra_Map[i].material.mainTexture = texture;
                downloadedTextures[i + 4] = texture;
            }
            else
            {
                Debug.LogError("StreetViewDownloader: Download error: " + request.error);
            }
        }

        /// Calculate the width and height of each texture
        int textureWidth = downloadedTextures[0].width;
        int textureHeight = downloadedTextures[0].height;

        // Calculate the width and height of the panorama
        int panoramaWidth = textureWidth * 4;
        int panoramaHeight = textureHeight * 3;

        // Create a new texture for the panorama
        Texture2D panoramaTexture = new Texture2D(panoramaWidth, panoramaHeight, TextureFormat.RGBA32, false);

        // Loop over each texture and map its pixels to the panorama texture
        for (int i = 0; i < 6; i++)
        {
            // Calculate the position of the current texture in the panorama
            int x = i<4?i:1;
            int y = i < 4 ? 1 : (i==4?2:0);

            // Get the pixels from the current texture
            Color[] pixelsOut = downloadedTextures[i].GetPixels();

            /////
            
            if (i == 5)
            {
                Color[] pixels = (Color[])pixelsOut.Clone();

                for (int tx = 0; tx < downloadedTextures[i].width; ++tx)
                {
                    for (int ty = 0; ty < downloadedTextures[i].height; ++ty)
                    {
                        pixelsOut[(downloadedTextures[i].width - 1 - tx) + (downloadedTextures[i].height - 1 - ty) * downloadedTextures[i].width] = pixels[tx + ty * downloadedTextures[i].width];
                    }
                }
            }

            // Map the pixels onto the appropriate region of the panorama
            panoramaTexture.SetPixels(x * textureWidth, y * textureHeight, textureWidth, textureHeight, pixelsOut);
        }

        // Apply the changes to the panorama texture
        panoramaTexture.Apply();

        //myPrefab.renderer.material = texture;
        byte[] textureBytes = panoramaTexture.EncodeToPNG();


        string fileName = "EEEQQQQIIII.png";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        File.WriteAllBytes(filePath, textureBytes);
        Debug.Log("Se guardo cube MAP Google!");
        Google_Equi.material.mainTexture = panoramaTexture;

        
        myImg.sprite = Sprite.Create(panoramaTexture, new Rect(0, 0, panoramaTexture.width, panoramaTexture.height), new Vector2(0.5f, 0.5f));

        // Get the face index for a given equirectangular coordinate
        int GetCubemapFace(Vector3 direction)
        {
            float absX = Mathf.Abs(direction.x);
            float absY = Mathf.Abs(direction.y);
            float absZ = Mathf.Abs(direction.z);

            if (absX > absY && absX > absZ)
            {
                if (direction.x > 0)
                {
                    return 0; // right
                }
                else
                {
                    return 1; // left
                }
            }
            else if (absY > absX && absY > absZ)
            {
                if (direction.y > 0)
                {
                    return 2; // up
                }
                else
                {
                    return 3; // down
                }
            }
            else
            {
                if (direction.z > 0)
                {
                    return 4; // front
                }
                else
                {
                    return 5; // back
                }
            }
        }
    }

    private class GeoCodeResult
    {
        public Result[] results;

        [System.Serializable]
        public class Result
        {
            public Geometry geometry;

            [System.Serializable]
            public class Geometry
            {
                public Location location;

                [System.Serializable]
                public class Location
                {
                    public float lat;
                    public float lng;
                }
            }
        }
    }
}

