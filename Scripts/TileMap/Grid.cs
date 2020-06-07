using UnityEngine;
using System.Collections;

public class Grid : MonoBehaviour {

    public float widht = 32.0f;
    public float height = 32.0f;

    public Color color = Color.white;

    public int zOrder = 0;

    public Transform tileprefab;

    public TileSet tileSet;

    void OnDrawGizmos()
    {
        if (widht < 1) widht = 1;
        if (height < 1) height = 1;

        Vector3 pos = Camera.current.transform.position;
        Gizmos.color = this.color;

        for (float y = pos.y - 800.0f; y < pos.y + 800.0f; y += this.height)
        {
            Gizmos.DrawLine(new Vector3(-100000.0f, Mathf.Floor(y / height) * height, 0.0f), 
                new Vector3(100000.0f, Mathf.Floor(y / height) * height, 0.0f));
        }
        for (float x = pos.x - 800.0f; x < pos.x + 800.0f; x += this.widht)
        {
            Gizmos.DrawLine(new Vector3(Mathf.Floor(x / widht) * widht ,- 100000.0f, 0.0f), 
                new Vector3(Mathf.Floor(x / widht) * widht,100000.0f, 0.0f));
        }
    }
}
