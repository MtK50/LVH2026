using UnityEngine;

public class ChessTile : MonoBehaviour
{
    #region Public Fields
    public Vector2 position;
    public Color baseColor;
    public GameObject centerPosition;
    public bool isSelectable = false;
    #endregion

    #region Initialization
    public void Initialize(int xpos, int ypos)
    {
        position = new Vector2(xpos, ypos);
        baseColor = GetComponent<Renderer>().material.color;
        centerPosition = transform.GetChild(0).gameObject;
        
        // Start disabled
        Disable();
    }
    #endregion

    #region Selection State
    public void Enable()
    {
        if (centerPosition != null)
        {
            centerPosition.SetActive(true);
        }
        isSelectable = true;
    }

    public void Disable()
    {
        if (centerPosition != null)
        {
            centerPosition.SetActive(false);
        }
        isSelectable = false;
    }
    #endregion


}


