using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]

public class ObjetoAgarrable : MonoBehaviour
{
   
[Header("Configuracion")]
public float masaMaximaParaAgarrar = 25f;

[HideInInspector] public Rigidbody rb;
void Awake()
    {
        
        rb = GetComponent<Rigidbody>();
    }

public bool SePuedeAgarrar()
    {
        
        return rb != null && rb.mass <= masaMaximaParaAgarrar;
    }
    
}
