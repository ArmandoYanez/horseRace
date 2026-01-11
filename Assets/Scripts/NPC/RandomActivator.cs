using UnityEngine;
using System.Collections.Generic;

public class RandomActivator : MonoBehaviour
{
    // Lista de objetos que arrastrarás desde el Inspector de Unity
    public List<GameObject> objetos;

    /// <summary>
    /// Desactiva todos los objetos y activa solo uno al azar.
    /// </summary>
    public void ActiveNpc()
    {
        if (objetos == null || objetos.Count == 0)
        {
            Debug.LogWarning("La lista de objetos está vacía.");
            return;
        }

        // 1. Primero desactivamos todos los objetos de la lista
        foreach (GameObject obj in objetos)
        {
            if (obj != null) obj.SetActive(false);
        }

        // 2. Elegimos un índice al azar entre 0 y el total de la lista
        int indiceAleatorio = Random.Range(0, objetos.Count);

        // 3. Activamos solo el objeto seleccionado
        if (objetos[indiceAleatorio] != null)
        {
            objetos[indiceAleatorio].SetActive(true);
            objetos[indiceAleatorio].GetComponent<AudioSource>().pitch = Random.Range(0.8f, 1.2f);
            objetos[indiceAleatorio].GetComponent<AudioSource>().Play();
            Debug.Log("Se activó el objeto: " + objetos[indiceAleatorio].name);
        }
    }
    
}