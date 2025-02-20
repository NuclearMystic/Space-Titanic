using UnityEngine;
using System.IO;

public class ScreenshotCapture : MonoBehaviour
{
    public KeyCode screenshotKey = KeyCode.P;
    public string folderName = "Screenshots";
    public string fileName = "Screenshot";
    public int startingIndex = 1;
    public string fileExtension = ".png";

    private void Update()
    {
        if (Input.GetKeyDown(screenshotKey))
        {
            TakeScreenshot();
        }
    }

    private void TakeScreenshot()
    {
        // Ensure the folder exists
        string folderPath = Path.Combine(Application.dataPath, folderName);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Find the next available file name
        int index = startingIndex;
        string filePath;
        do
        {
            filePath = Path.Combine(folderPath, $"{fileName}_{index.ToString("D3")}{fileExtension}");
            index++;
        } while (File.Exists(filePath));

        // Capture and save the screenshot
        ScreenCapture.CaptureScreenshot(filePath);
        Debug.Log($"Screenshot saved to: {filePath}");
    }
}
