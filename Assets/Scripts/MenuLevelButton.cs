using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuLevelButton : MonoBehaviour
{
    public int level;
    public GameObject blockedImage;
    public string scene;
    private Button button;
    private GameObject blocked;
    private int status = 0;
    // Generates a gridSize x gridSize grid centered on the screen, and adds two points to be connected
    void LevelLocked()
    {
      blocked.SetActive(true);
      button.enabled = false;
    }

    void LevelUnlocked()
    {
      blocked.SetActive(false);
      button.enabled = true;
    }

    void Start()
    {
      GetComponentInChildren<Text>().text = level.ToString();
      gameObject.name = "Button Level" + level;

      button = GetComponent<Button>();
      button.onClick.AddListener(LoadLevel);

      blocked = Instantiate(blockedImage, transform);
      string playerPrefsKey = "";

      if(scene == "LevelScene")
        playerPrefsKey = "Level "+(level-1).ToString();

      else
        playerPrefsKey = "Level_2 "+(level-1).ToString();

      if(PlayerPrefs.HasKey(playerPrefsKey))
        status = PlayerPrefs.GetInt(playerPrefsKey);

      if(status == 1 || level == 1)
        LevelUnlocked();

      else
        LevelLocked();
    }

    IEnumerator LoadYourAsyncScene(string scene)
    {
       // The Application loads the Scene in the background as the current Scene runs.
       // This is particularly good for creating loading screens.
       // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
       // a sceneBuildIndex of 1 as shown in Build Settings.

       AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene);

       // Wait until the asynchronous scene fully loads
       while (!asyncLoad.isDone)
       {
           yield return null;
       }
   }

    public void LoadLevel()
    {
        //Vibration.Vibrate();
        GridGenerator.currentLevel = level;
        StartCoroutine(LoadYourAsyncScene(scene));
    }
}
