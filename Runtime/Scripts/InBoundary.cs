using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using VaroniaBackOffice;

// Enum�ration pour d�finir o� se trouve l'objet par rapport aux limites
public enum Where
{
    Unknown = 0,
    OutLimit = 1,
    InLimit = 2
}

public class InBoundary : MonoBehaviour
{
    [Header("Settings")]
    public LayerMask layerMask; // Couches utilis�es pour les Raycasts
    public bool autoStart = true; // D�marrer automatiquement le suivi
    public bool lockRotation = false; // Verrouiller la rotation de l'objet
    public bool isPlayer = false; // Est-ce que cet objet est un joueur ?
    public bool waitInit = false; // Attendre une initialisation avant de commencer

    [Header("Near Distances")]
    public float nearLimitDistance = 0.8f; // Distance pour �tre consid�r� "proche" de la limite int�rieure
    public float nearOutLimitDistance = 0.8f; // Distance pour �tre "proche" de la limite ext�rieure

    [Header("Status")]
    public Where currentStatus = Where.Unknown; // Etat actuel par rapport aux limites
    public bool isNear = false; // Proche de la limite int�rieure
    public bool isNearOut = false; // Proche de la limite ext�rieure

    private GameObject debugVaronia; // Objet utilis� pour envoyer des messages de debug

    private void OnEnable()
    {
        if (autoStart)
            StartCoroutine(UpdateBoundaryStatus()); // D�marre la mise � jour automatique
    }

    private void OnDisable()
    {
        StopAllCoroutines(); // Arr�te toutes les coroutines au d�sactivation
    }

    // Coroutine principale pour mettre � jour r�guli�rement la position par rapport aux limites
    private IEnumerator UpdateBoundaryStatus()
    {
        if (isPlayer)
            StartCoroutine(SearchDebugObject()); // Lance la recherche du debug object si c'est un joueur

        while (true)
        {
            if (waitInit)
            {
                yield return new WaitForSeconds(0.4f); // Attendre un peu avant la premi�re v�rification
                waitInit = false;
            }

            CheckBoundary(); // V�rifie la position actuelle
            yield return new WaitForFixedUpdate(); // Attend la prochaine frame physique
        }
    }

    // Coroutine pour rechercher l'objet de debug en permanence
    private IEnumerator SearchDebugObject()
    {
        nearLimitDistance = 1f; // Force une distance standard au d�marrage

        while (true)
        {
            if (debugVaronia == null)
                debugVaronia = GameObject.Find("[DEBUG]"); // Recherche l'objet de debug par son nom

            yield return new WaitForSeconds(5f); // R�essaie toutes les 5 secondes
        }
    }

    // V�rifie la position de l'objet par rapport aux limites d�finies
    private void CheckBoundary()
    {
     
        if (lockRotation)
            transform.rotation = Quaternion.identity; // R�initialise la rotation si besoin

        // D�finition des directions � tester pour les raycasts
        Vector3[] directions =
        {
            transform.forward * 1.2f,
            transform.forward + Vector3.right,
            transform.forward + Vector3.left,
            -transform.forward * 1.2f,
            -transform.forward + Vector3.right,
            -transform.forward + Vector3.left
        };

        // Variables temporaires pour stocker les r�sultats des raycasts
        bool foundIn = false;
        bool foundOut = false;
        bool foundNearIn = false;
        bool foundNearOut = false;

        // Lancement de plusieurs raycasts dans les directions sp�cifi�es
        foreach (var dir in directions)
        {
            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, 30f, layerMask))
            {
                if (hit.collider.CompareTag("OUT"))
                {
                    foundOut = true;
                    Debug.DrawRay(transform.position, dir * 2f, Color.red, 1f); // Dessine un rayon rouge pour OUT

                    if (Vector3.Distance(hit.point, transform.position) < nearOutLimitDistance)
                        foundNearOut = true; // L'objet est proche d'une limite ext�rieure
                }
                else if (hit.collider.CompareTag("IN"))
                {
                    foundIn = true;
                    Debug.DrawRay(transform.position, dir * 2f, Color.green, 1f); // Dessine un rayon vert pour IN

                    if (Vector3.Distance(hit.point, transform.position) < nearLimitDistance)
                        foundNearIn = true; // L'objet est proche d'une limite int�rieure
                }
            }
        }

        // Mise � jour des �tats de proximit�
        isNear = foundNearIn;
        isNearOut = foundNearOut;

        // D�duction du statut global en fonction des raycasts
        if (foundIn && foundOut)
            currentStatus = Where.OutLimit;
        else if (foundIn)
            currentStatus = Where.InLimit;
        else if (foundOut)
            currentStatus = Where.OutLimit;
        else
            currentStatus = Where.Unknown;

        // Envoi des informations � l'objet de debug si c'est un joueur
        if (isPlayer && debugVaronia != null && VaroniaGlobal.VG != null)
        {
            debugVaronia.SendMessage("Up_Player_B", (int)currentStatus);
            debugVaronia.SendMessage("Up_Player_B_Near", isNear);
        }

    
    }
}
