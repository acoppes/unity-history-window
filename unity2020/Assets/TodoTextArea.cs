using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TodoTextArea : MonoBehaviour
{
    [TextArea(4, 100)]
    public string todoText;

    private void OnValidate()
    {
        gameObject.name = $"TODO: {todoText}";
    }
}
