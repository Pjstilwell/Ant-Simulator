using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{

    [SerializeField] GameObject menu; 
    private bool menuEnabled = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {   
        // If M is pressed toggle the menu
        if (Input.GetKeyDown(KeyCode.M))
        {
            menuEnabled = !menuEnabled;
            menu.SetActive(menuEnabled);
        }
    }
}
