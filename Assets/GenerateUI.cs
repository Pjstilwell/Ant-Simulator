using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GenerateUI : MonoBehaviour
{
    //Object that handles creating objects. Will need to access to generate buttons on button click.
    [SerializeField] InitialiseThings initialiseThings;
    public UIDocument uiDocument;

    void OnEnable()
    {
        VisualElement root = uiDocument.rootVisualElement;

        // Create a label
        Label title = new Label("Ants!");
        title.style.fontSize = 16;
        title.style.color = Color.black;
        title.style.marginTop = 1;
        title.style.marginLeft = 1;

        //Create Add Nest Button
        Button addNest = new Button();
        addNest.text = "Add Nest";
        addNest.style.marginTop = 1;
        addNest.style.marginLeft = 1;
        addNest.style.width = 40;
        addNest.style.height = 8;
        addNest.style.minHeight = 0;
        addNest.style.fontSize = 6;
        addNest.clicked += () => handleButtonClick("addNest");

        //Create Add Food Button
        Button addFood = new Button();
        addFood.text = "Add Food";
        addFood.style.marginTop = 1;
        addFood.style.marginLeft = 1;
        addFood.style.width = 40;
        addFood.style.height = 8;
        addFood.style.minHeight = 0;
        addFood.style.fontSize = 6;
        addFood.clicked += () => handleButtonClick("addFood");

        // Add to root
        root.Add(title);
        root.Add(addNest);
        root.Add(addFood);
    }

    private void handleButtonClick(string buttonClicked)
    {
        switch (buttonClicked)
        {
            case "addNest":
                initialiseThings.handleChangeState(Constants.MENU_STATE_ADD_NEST);
                gameObject.SetActive(false);
                break;
            case "addFood":
                initialiseThings.handleChangeState(Constants.MENU_STATE_ADD_NEST);
                gameObject.SetActive(false);
                break;
            default:
                return;
        }
        // Debug.Log(buttonClicked);
    }
}
