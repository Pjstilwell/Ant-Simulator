using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GenerateUI : MonoBehaviour
{
    public UIDocument uiDocument;

    void Start()
    {
        VisualElement root = uiDocument.rootVisualElement;

        // Create a label
        Label label = new Label("Hello from runtime!");
        label.style.fontSize = 24;
        label.style.color = Color.white;
        label.style.marginTop = 20;

        // Add to root
        root.Add(label);
    }
}
