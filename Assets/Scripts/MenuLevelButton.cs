using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuLevelButton : MonoBehaviour
{
    public int level;
    public GameObject blockedImage;
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
      if(PlayerPrefs.HasKey("Level "+(level-1).ToString()))
      {
        status = PlayerPrefs.GetInt("Level "+(level-1).ToString());
      }

      if(status == 1 || level == 1)
        LevelUnlocked();

      else
        LevelLocked();
    }

    public void LoadLevel()
    {
        Vibration.Vibrate();
        GridGenerator.currentLevel = level;
        SceneManager.LoadScene("TestScene");
    }
}
