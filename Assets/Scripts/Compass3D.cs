using System;
using System.Collections;
using System.Collections.Generic;
using Battlehub.UIControls;
using TMPro;
using UnityEngine;

public class Compass3D : MonoBehaviour
{
    Ray ray;
    RaycastHit hit;
    [SerializeField] Camera canvasCamera;
    Vector3 NorthDir;
    public GameObject player; // Camera
    public GameObject NorthLayer;
    public MeshRenderer clickedPart;
    [SerializeField] MeshRenderer selectedPart;
    [SerializeField] MouseClick mouseClick;

    private Vector3 targetPos, currentRotation, smoothVelocity = Vector3.zero;
    private float rotationY, rotationX, smoothTime = 0.3f;
    bool isMouseButtonDown = false;
    float mouseButtonDownTime = 0f;
    float holdDuration = 0.25f; // 1 saniye
    
    void Start()
    {
      NorthDir = Vector3.forward;

      NorthLayer.transform.rotation = Quaternion.LookRotation(NorthDir, Vector3.up);
    }


    void Update()
    {
        if (System.GC.GetTotalMemory(false) <= 2 * 1024f * 1024 * 1024)
        {
            // Bellek sınırı aşılmamışsa işlemleri sürdür
            PerformHeavyOperations();
        }
        else
        {
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        if(Input.GetMouseButtonUp(0)) isMouseButtonDown = false;

        if (isMouseButtonDown && Time.time >= mouseButtonDownTime + holdDuration) Orbit();

    }

    void PerformHeavyOperations()
    {
        NorthDir = Vector3.forward;

        NorthLayer.transform.rotation = Quaternion.LookRotation(NorthDir, Vector3.up);

        ray = canvasCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            if(hit.transform.gameObject.layer == 6 && Input.GetMouseButtonDown(0))
            {
                targetPos = mouseClick.pozisyon;
                isMouseButtonDown = true;
                mouseButtonDownTime = Time.time;
            }

            if(hit.collider.CompareTag("Ust") && Input.GetMouseButtonUp(0))
            {
                float distance = Vector3.Distance(player.transform.position, targetPos);
                player.transform.rotation = Quaternion.AngleAxis(90, Vector3.right);
                player.transform.position = new Vector3(targetPos.x, targetPos.y + distance, targetPos.z);
            }

            if(hit.collider.CompareTag("Alt") && Input.GetMouseButtonUp(0))
            {
                float distance = Vector3.Distance(player.transform.position, targetPos);
                player.transform.position = new Vector3(targetPos.x, targetPos.y - distance, targetPos.z);
                player.transform.rotation = Quaternion.AngleAxis(90, Vector3.left);
            }

            if(hit.collider.CompareTag("Sag") && Input.GetMouseButtonUp(0))
            {
                float distance = Vector3.Distance(player.transform.position, targetPos);
                player.transform.position = new Vector3(targetPos.x + distance, targetPos.y, targetPos.z);
                player.transform.rotation = Quaternion.AngleAxis(90, Vector3.down);
            }

            if(hit.collider.CompareTag("Sol") && Input.GetMouseButtonUp(0))
            { 
                float distance = Vector3.Distance(player.transform.position, targetPos);
                player.transform.position = new Vector3(targetPos.x - distance, targetPos.y, targetPos.z);
                player.transform.rotation = Quaternion.AngleAxis(90, Vector3.up);
            }

            if(hit.collider.CompareTag("On") && Input.GetMouseButtonUp(0))
            {
                float distance = Vector3.Distance(player.transform.position, targetPos);
                player.transform.position = new Vector3(targetPos.x, targetPos.y, targetPos.z - distance);
                player.transform.rotation = Quaternion.AngleAxis(0, Vector3.up);
            }

            if(hit.collider.CompareTag("Arka") && Input.GetMouseButtonUp(0))
            {
                float distance = Vector3.Distance(player.transform.position, targetPos);
                player.transform.position = new Vector3(targetPos.x, targetPos.y, targetPos.z + distance);
                player.transform.rotation = Quaternion.AngleAxis(180, Vector3.up);
            }


            if(hit.collider.CompareTag("SagArka") && Input.GetMouseButtonUp(0))
            {
                float distance = Vector3.Distance(player.transform.position, targetPos);
                player.transform.position = new Vector3(targetPos.x + distance / 1.40f, targetPos.y, targetPos.z + distance / 1.40f);
                player.transform.rotation = Quaternion.AngleAxis(135, Vector3.down);
            }

            if(hit.collider.CompareTag("SagOn") && Input.GetMouseButtonUp(0))
            {
                float distance = Vector3.Distance(player.transform.position, targetPos);
                player.transform.position = new Vector3(targetPos.x + distance / 1.40f, targetPos.y, targetPos.z - distance / 1.40f);
                player.transform.rotation = Quaternion.AngleAxis(45, Vector3.down);
            }

            if(hit.collider.CompareTag("SolArka") && Input.GetMouseButtonUp(0))
            {
                float distance = Vector3.Distance(player.transform.position, targetPos);
                player.transform.position = new Vector3(targetPos.x - distance / 1.40f, targetPos.y, targetPos.z + distance / 1.40f);
                player.transform.rotation = Quaternion.AngleAxis(135, Vector3.up);
            }

            if(hit.collider.CompareTag("SolOn") && Input.GetMouseButtonUp(0))
            {
                float distance = Vector3.Distance(player.transform.position, targetPos);
                player.transform.position = new Vector3(targetPos.x - distance / 1.40f, targetPos.y, targetPos.z - distance / 1.40f);
                player.transform.rotation = Quaternion.AngleAxis(45, Vector3.up);
            }

            if(hit.collider.CompareTag("UstSol") && Input.GetMouseButtonUp(0))
            {
                float distance = Vector3.Distance(player.transform.position, targetPos);
                player.transform.position = new Vector3(targetPos.x - distance / 1.40f, targetPos.y + distance / 1.40f, targetPos.z);
                player.transform.rotation = Quaternion.AngleAxis(90, Vector3.up);
                player.transform.rotation *= Quaternion.AngleAxis(45, Vector3.right);
            }

            if(hit.collider.CompareTag("UstSag") && Input.GetMouseButtonUp(0))
            {
                float distance = Vector3.Distance(player.transform.position, targetPos);
                player.transform.position = new Vector3(targetPos.x + distance / 1.40f, targetPos.y + distance / 1.40f, targetPos.z);
                player.transform.rotation = Quaternion.AngleAxis(90, Vector3.down);
                player.transform.rotation *= Quaternion.AngleAxis(45, Vector3.right);
            }

            if(hit.collider.CompareTag("AltSol")&& Input.GetMouseButtonUp(0))
            {
                float distance = Vector3.Distance(player.transform.position, targetPos);
                player.transform.position = new Vector3(targetPos.x - distance / 1.40f, targetPos.y - distance / 1.40f, targetPos.z);
                player.transform.rotation = Quaternion.AngleAxis(90, Vector3.up);
                player.transform.rotation *= Quaternion.AngleAxis(45, Vector3.left);
            }

            if(hit.collider.CompareTag("AltSag")&& Input.GetMouseButtonUp(0))
            {
                float distance = Vector3.Distance(player.transform.position, targetPos);
                player.transform.position = new Vector3(targetPos.x + distance / 1.40f, targetPos.y - distance / 1.40f, targetPos.z);
                player.transform.rotation = Quaternion.AngleAxis(90, Vector3.down);
                player.transform.rotation *= Quaternion.AngleAxis(45, Vector3.left);
            }

            if(hit.collider.CompareTag("AltArka")&& Input.GetMouseButtonUp(0))
            {
                float distance = Vector3.Distance(player.transform.position, targetPos);
                player.transform.position = new Vector3(targetPos.x, targetPos.y - distance / 1.40f, targetPos.z + distance / 1.40f);
                player.transform.rotation = Quaternion.AngleAxis(180, Vector3.up);
                player.transform.rotation *= Quaternion.AngleAxis(45, Vector3.left);
            }

            if(hit.collider.CompareTag("AltOn")&& Input.GetMouseButtonUp(0))
            {
                float distance = Vector3.Distance(player.transform.position, targetPos);
                player.transform.position = new Vector3(targetPos.x, targetPos.y - distance / 1.40f, targetPos.z - distance / 1.40f);
                player.transform.rotation = Quaternion.AngleAxis(0, Vector3.up);
                player.transform.rotation *= Quaternion.AngleAxis(45, Vector3.left);
            }

            if(hit.collider.CompareTag("UstArka")&& Input.GetMouseButtonUp(0))
            {
                float distance = Vector3.Distance(player.transform.position, targetPos);
                player.transform.position = new Vector3(targetPos.x, targetPos.y + distance / 1.40f, targetPos.z + distance / 1.40f);
                player.transform.rotation = Quaternion.AngleAxis(180, Vector3.up);
                player.transform.rotation *= Quaternion.AngleAxis(45, Vector3.right);
            }

            if(hit.collider.CompareTag("UstOn")&& Input.GetMouseButtonUp(0))
            {
                float distance = Vector3.Distance(player.transform.position, targetPos);
                player.transform.position = new Vector3(targetPos.x, targetPos.y + distance / 1.40f, targetPos.z - distance / 1.40f);
                player.transform.rotation = Quaternion.AngleAxis(0, Vector3.up);
                player.transform.rotation *= Quaternion.AngleAxis(45, Vector3.right); 
            }

            if(hit.collider.CompareTag("UstOnSag")&& Input.GetMouseButtonUp(0))
            {
                float distance = Vector3.Distance(player.transform.position, targetPos);
                player.transform.position = new Vector3(targetPos.x + distance / 1.725f, targetPos.y + distance / 1.725f, targetPos.z - distance / 1.725f);
                player.transform.rotation = Quaternion.AngleAxis(45, Vector3.down);
                player.transform.rotation *= Quaternion.AngleAxis(45, Vector3.right); 
            }

            if(hit.collider.CompareTag("UstOnSol")&& Input.GetMouseButtonUp(0))
            {
                float distance = Vector3.Distance(player.transform.position, targetPos);
                player.transform.position = new Vector3(targetPos.x - distance / 1.725f, targetPos.y + distance / 1.725f, targetPos.z - distance / 1.725f);
                player.transform.rotation = Quaternion.AngleAxis(45, Vector3.up);
                player.transform.rotation *= Quaternion.AngleAxis(45, Vector3.right);
            }

            if(hit.collider.CompareTag("UstArkaSag")&& Input.GetMouseButtonUp(0))
            {
                float distance = Vector3.Distance(player.transform.position, targetPos);
                player.transform.position = new Vector3(targetPos.x + distance / 1.725f, targetPos.y + distance / 1.725f, targetPos.z + distance / 1.725f);
                player.transform.rotation = Quaternion.AngleAxis(135, Vector3.down);
                player.transform.rotation *= Quaternion.AngleAxis(45, Vector3.right); 
            }

            if(hit.collider.CompareTag("UstArkaSol")&& Input.GetMouseButtonUp(0))
            {
                float distance = Vector3.Distance(player.transform.position, targetPos);
                player.transform.position = new Vector3(targetPos.x - distance / 1.725f, targetPos.y + distance / 1.725f, targetPos.z + distance / 1.725f);
                player.transform.rotation = Quaternion.AngleAxis(135, Vector3.up);
                player.transform.rotation *= Quaternion.AngleAxis(45, Vector3.right); 
            }

            if(hit.collider.CompareTag("AltOnSag")&& Input.GetMouseButtonUp(0))
            {
                float distance = Vector3.Distance(player.transform.position, targetPos);
                player.transform.position = new Vector3(targetPos.x + distance / 1.725f, targetPos.y - distance / 1.725f, targetPos.z - distance / 1.725f);
                player.transform.rotation = Quaternion.AngleAxis(45, Vector3.down);
                player.transform.rotation *= Quaternion.AngleAxis(45, Vector3.left); 
            }

            if(hit.collider.CompareTag("AltOnSol")&& Input.GetMouseButtonUp(0))
            {
                float distance = Vector3.Distance(player.transform.position, targetPos);
                player.transform.position = new Vector3(targetPos.x - distance / 1.725f, targetPos.y - distance / 1.725f, targetPos.z - distance / 1.725f);
                player.transform.rotation = Quaternion.AngleAxis(45, Vector3.up);
                player.transform.rotation *= Quaternion.AngleAxis(45, Vector3.left); 
            }

            if(hit.collider.CompareTag("AltArkaSag")&& Input.GetMouseButtonUp(0))
            {
                float distance = Vector3.Distance(player.transform.position, targetPos);
                player.transform.position = new Vector3(targetPos.x + distance / 1.725f, targetPos.y - distance / 1.725f, targetPos.z + distance / 1.725f);
                player.transform.rotation = Quaternion.AngleAxis(135, Vector3.down);
                player.transform.rotation *= Quaternion.AngleAxis(45, Vector3.left); 
            }

            if(hit.collider.CompareTag("AltArkaSol")&& Input.GetMouseButtonUp(0))
            {
                float distance = Vector3.Distance(player.transform.position, targetPos);
                player.transform.position = new Vector3(targetPos.x - distance / 1.725f, targetPos.y - distance / 1.725f, targetPos.z + distance / 1.725f);
                player.transform.rotation = Quaternion.AngleAxis(135, Vector3.up);
                player.transform.rotation *= Quaternion.AngleAxis(45, Vector3.left); 
            }
            
        }
    }
    void Orbit()
    {
        
        float distanceFromTarget = Vector3.Distance(player.transform.position, targetPos);

        float mouseX = Input.GetAxis("Mouse X") * 3f;
        float mouseY = Input.GetAxis("Mouse Y") * 3f;

        rotationY += mouseX;
        rotationX -= mouseY;

        rotationX = Mathf.Clamp(rotationX, -40, 40);

        Vector3 nextRotation = new Vector3(rotationX, rotationY);
        currentRotation = Vector3.SmoothDamp(currentRotation, nextRotation, ref smoothVelocity, smoothTime);
        player.transform.localEulerAngles = currentRotation;

        player.transform.position = targetPos - player.transform.forward * distanceFromTarget;
    }
}
