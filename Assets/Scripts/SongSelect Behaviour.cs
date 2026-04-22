using System.Windows.Forms;
using UnityEngine;
using UnityEngine.UI;

public class SongSelectBehaviour : MonoBehaviour
{
    public UnityEngine.UI.Button button;
    private string folderPath;
    private string folderName;

    public MapInfoDisplay SongInfoDisplay;

    public GameObject SongIconPrefab;

    public void Initialise(string path, MapInfoDisplay infoDisplay)
    {

        folderPath = path;
        folderName = System.IO.Path.GetFileName(folderPath);
        button=SongIconPrefab.GetComponent<UnityEngine.UI.Button>();
        button.onClick.AddListener(onClick);
        Debug.Log("Initialised SongSelectBehaviour with path: " + folderPath);
        SongInfoDisplay = infoDisplay;

    }
    void onClick()
    {
        Debug.Log("Clicked on song: " + folderName);
        if (SongInfoDisplay != null)
        {
            SongInfoDisplay.DisplayInfo(folderName,folderPath);
        }
    }
}
