using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetChildColor : MonoBehaviour
{
    public SpriteRenderer child;

    public void SetColor(Color newColor)
    {
        child.color = newColor;
    }
}
