using UnityEngine;

public class ChessTile : MonoBehaviour
{
 
    public Vector2 position;


    public void Initialize(int xpos, int ypos)
    {
        position = new Vector2(xpos, ypos);
    }

    //public void OnGUI()
    //{
    //    GUIStyle style = new GUIStyle();
    //    style.normal.textColor = Color.red;
    //    Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
    //    GUI.Label(new Rect(screenPos.x, Screen.height - screenPos.y, 100, 20), $"({position.x},{position.y})", style);
    //}

}


