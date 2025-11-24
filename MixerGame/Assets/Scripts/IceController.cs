using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class IceController : MonoBehaviour
{
    public GameObject iceSprite_big;
    public GameObject iceSprite_mid;
    public GameObject iceSprite_sml;
    // Start is called before the first frame update
    void Start()
    {   
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Blade")
        {
            float3 position = this.transform.position;
            if(this.transform.localScale.x >= 1)
            {
                Destroy(this.gameObject);
                Instantiate(iceSprite_mid,new Vector3(position.x,position.y,position.z),Quaternion.identity);
                Instantiate(iceSprite_mid,new Vector3(position.x,position.y,position.z),Quaternion.identity);
            }
            else if(this.transform.localScale.x >= 0.5)
            {
                Destroy(this.gameObject);
                Instantiate(iceSprite_sml,new Vector3(position.x,position.y,position.z),Quaternion.identity);
                Instantiate(iceSprite_sml,new Vector3(position.x,position.y,position.z),Quaternion.identity);
            }
        }
    }
}
