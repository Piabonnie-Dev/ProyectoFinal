using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNoise : MonoBehaviour
{
    [Header("Ruido")]
    public bool estaHaciendoRuido = false;
    public float radioRuidoActual = 0f;

    [Header("Valores")]
    public float radioRuidoCaminar = 6f;
    public float radioRuidsoCorrer = 10f;
    public float radioRuidoQuieto = 0f;

    [Header("Referencias")]
    public CharacterController controller;


    void Start()
    {
        if(controller == null)
        controller = GetComponent<CharacterController>();


    }
    void Update()
    {
        ActualizarRuido();
    }
    void ActualizarRuido()
    {
        float velocidad = 0f;

        if(controller != null)
        velocidad = new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude;
   if(velocidad > 0.1f)
        {
            estaHaciendoRuido = true;

            if(velocidad > 4.5f)
            radioRuidoActual = radioRuidsoCorrer;
            else
            radioRuidoActual = radioRuidoCaminar;

        }
        else
        {
            estaHaciendoRuido = false;
            radioRuidoActual = radioRuidoQuieto;
        }
   
    }
}
