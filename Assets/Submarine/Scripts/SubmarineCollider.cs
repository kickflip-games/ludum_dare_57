using System;
using UnityEngine;

public class SubmarineCollider : MonoBehaviour
{
    private Submarine _main;
        
    void Start()
    {
        _main = transform.root.GetComponent<Submarine>();
    }



    void OnCollisionEnter(Collision col)
    {
        
        Debug.Log("Submarine crashed!");
        _main.TakeDamage();
    }


    private void Update()
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation= Quaternion.identity;
    }
}