using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StickRetrackt : MonoBehaviour
{
    private void OnTriggerEnter(Collider collidesWith)
    {
        if ((collidesWith.gameObject.layer == 8 || collidesWith.gameObject.tag == "DeadlyWeapon") || (collidesWith.gameObject.tag == "Helmet" && collidesWith.transform.parent != transform.parent))
        {
            Animator anim = GetComponent<Animator>();
            anim.SetBool("attacking", false);
            GetComponent<Collider>().enabled = false;
        }
    }
}