using UnityEngine;
using System.Collections;

public class MapLabel : MonoBehaviour
{
    public string Label = "";
    public TextMesh mesh = null;
    
    public void Init(int x, int y, string label)
    {
        Label = label;
        float rx = x + 0.5f;
        float ry = y + 0.5f;

        mesh = this.gameObject.AddComponent<TextMesh>();
        mesh.text = label;
        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.fontSize = 100;
        mesh.characterSize = 0.05f;

        mesh.transform.position = new Vector3(rx, 2.1f, ry);
        mesh.transform.LookAt(new Vector3(rx, 0, ry));
        mesh.transform.parent = this.gameObject.transform;
    }
}
