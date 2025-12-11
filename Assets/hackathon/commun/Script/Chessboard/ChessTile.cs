using UnityEngine;

public class ChessTile : MonoBehaviour
{
 
    public Vector2 position;
    public Color baseColor;
    public GameObject centerPosition;
    public bool isSelectable = false;

    public void Initialize(int xpos, int ypos)
    {
        position = new Vector2(xpos, ypos);
        baseColor = GetComponent<Renderer>().material.color;
        centerPosition = transform.GetChild(0).gameObject;
    }


    public void Enable()
    {
        centerPosition.SetActive(true);
        isSelectable = true;
    }

    public void Disable()
    {
        centerPosition.SetActive(false);
        isSelectable = false;
    }

    //public void OnGUI()
    //{
    //    GUIStyle style = new GUIStyle();
    //    style.normal.textColor = Color.red;
    //    Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
    //    GUI.Label(new Rect(screenPos.x, Screen.height - screenPos.y, 100, 20), $"({position.x},{position.y})", style);
    //}

}


