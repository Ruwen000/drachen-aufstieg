using UnityEngine;

public class Crosshair : MonoBehaviour
{
    void Update()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        transform.position = Camera.main.ScreenToWorldPoint(screenCenter);
    }
}
