using UnityEngine;
using System.Collections;

public delegate void ClickHandler(GameObject obj, Vector3 clickPos);

public class ClickableObject : MonoBehaviour
{
    public ClickHandler clickHandler = null;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnMouseDown()
    {
        if (clickHandler != null)
        {

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
                clickHandler(this.gameObject, hit.point);
        }
    }
}
