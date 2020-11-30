using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [SerializeField]
    public SavedLevelData savedLevel;


    // Start is called before the first frame update
    void Start()
    {
      Instantiate(savedLevel.savedLevels[GridGenerator.currentLevel - 1].level);
    }


  }
