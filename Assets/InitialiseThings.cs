using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialiseThings : MonoBehaviour
{

    [SerializeField] GameObject food;
    public float gridSize = 1f; // Size of your grid squares
    public Camera mainCamera;  // Assign your main camera in the inspector
    public GameObject placeNest; // A visual object representing the cursor
    public GameObject nest;

    private Vector3 snappedPosition;

    // Start is called before the first frame update
    void Start()
    {
        GameObject newFood = Instantiate(food, new Vector3(-5, 0.1f, 0), Quaternion.identity);
        Cursor.visible = false;
        placeNest.SetActive(true);

    }

    // Update is called once per frame
    void Update()
    {
        // Cast a ray from the mouse position into the world
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Define a plane on XZ (y = 0)
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);

            // Snap to grid in XZ plane
            snappedPosition = new Vector3(
                Mathf.Round(hitPoint.x / gridSize) * gridSize,
                0.1f,
                Mathf.Round(hitPoint.z / gridSize) * gridSize
            );

            placeNest.transform.position = snappedPosition;
        }
    }

    private void OnMouseDown()
    {
        GameObject newNest = Instantiate(nest);
        newNest.SetActive(true);
        newNest.transform.position = snappedPosition;
    }
}
