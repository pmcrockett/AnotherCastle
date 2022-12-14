using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VectorGraphics;

public class HealthDisplay : MonoBehaviour
{
    public SVGImage icon;
    public Health healthSource;
    // Start is called before the first frame update
    void Start()
    {
        Redraw();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Redraw() {
        int heartDelta = 0;
        foreach (Transform x in transform) {
            heartDelta++;
        }
        heartDelta = healthSource.health - heartDelta;
        Debug.Log("Heart delta is " + heartDelta);
        if (heartDelta > 0) {
            for (int i = 0; i < heartDelta; i++) {
                Instantiate(icon, transform).gameObject.name = "Heart";
            }
        } else if (heartDelta < 0) {
            int idx = 0;
            foreach (Transform x in transform) {
                Destroy(x.gameObject);
                Debug.Log("Heart destroyed");
                idx--;
                if (idx <= heartDelta) break;
            }
        }
    }
}
