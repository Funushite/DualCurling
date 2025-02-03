using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bonus : MonoBehaviour
{
    private GameObject parent;
    private Stones stones;

    private void Start()
    {
        parent = this.gameObject.transform.parent.gameObject;
        stones = parent.GetComponent<Stones>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.gameObject.CompareTag("bonus"))
        {
            stones.BonusIn(this.gameObject);
            gameObject.SetActive(false);
        }
    }
}
