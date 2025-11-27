using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JuiceController : MonoBehaviour
{
    float startHeight = 1f;
    float height = 1f;
    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.transform.localScale = new Vector3(this.gameObject.transform.localScale.x,startHeight,this.gameObject.transform.localScale.z);
    }

    // Update is called once per frame
    void Update()
    {
        height += 0.1f * Time.deltaTime;
        this.gameObject.transform.localScale = new Vector3(this.gameObject.transform.localScale.x,height,this.gameObject.transform.localScale.z);
    }
}
