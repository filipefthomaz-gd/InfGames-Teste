using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SavedLevel Data", menuName = "Store Entire Level", order = 53)]
public class SavedLevelData : ScriptableObject
{
    public List<SavedLevel> savedLevels = new List<SavedLevel>();
}
