using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    Ray ray;
    RaycastHit hit;
    private float speed = 20.0f;
    private float horizontalInput, verticalInput, xEksen = 0.0f, yEksen = 0.0f;
    private float hassasiyet = 2.0f, sensitivity = 3f; // Fare hassasiyeti
    private float rotationY, rotationX, distanceFromTarget, smoothTime = 0.3f;
    private Vector3 currentRotation , smoothVelocity = Vector3.zero;
    private Camera mainCamera;
    [SerializeField] MoveButtons moveButtons;
    [SerializeField] MouseClick mouseClick;
    [SerializeField] ClippingController clippingController;
    private Vector3 mousePos;
    [SerializeField] GameObject pivot;
    private bool notHit, timerStart, canOrbit, mouseButtonUp, shiftPressed;
    private float timer;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if(!mouseClick.isOverUI && Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButton(2) && !clippingController.isOverTopLayer)
        { 
            shiftPressed = true;
            canOrbit = true;
            Orbit();
            pivot.SetActive(true);
        }

        if(!mouseClick.isOverUI && Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButton(0) && !clippingController.isOverTopLayer)
        {
            shiftPressed = true;
            Pan();
        }

        if(!mouseClick.isOverUI && Input.GetMouseButtonDown(0) && moveButtons.pan && !clippingController.isOverTopLayer && !shiftPressed)
        {
            mouseButtonUp = true;
        }

        /*if(!mouseClick.isOverUI && Input.GetMouseButtonDown(2) && !clippingController.isOverTopLayer)
        {
            mouseButtonUp = true;
        }*/

        if(!mouseClick.isOverUI && Input.GetMouseButtonDown(0) && moveButtons.orbit && !clippingController.isOverTopLayer && !shiftPressed)
        { 
            mouseButtonUp = true;
            canOrbit = true;
            pivot.SetActive(true);
        }

        if (!mouseClick.isOverUI && Input.GetMouseButtonDown(0) && moveButtons.lookAround && !clippingController.isOverTopLayer && !shiftPressed)
        {
            mouseButtonUp = true;
        }

        if(Input.GetMouseButtonUp(0))
        { 
            mouseButtonUp = false;
            canOrbit = false;
            pivot.SetActive(false);
        }
        if(Input.GetMouseButtonUp(2))
        { 
            canOrbit = false;
            pivot.SetActive(false);
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            shiftPressed = false;
            canOrbit = false;
            if (pivot != null)
            {
                pivot.SetActive(false);
            }
        }


        if(mouseButtonUp)
        {
            if(moveButtons.pan)
            {
                Pan();
            }
            if(moveButtons.orbit)
            {
                Orbit();
            }
            if(moveButtons.lookAround)
            {
                LookAround();
            }
        }

        if(!mouseClick.isOverUI && !canOrbit)
        {
            Zoom(Input.GetAxis("Mouse ScrollWheel"), mainCamera.orthographic);
        }

        if(timerStart)
        {
            timer -= Time.deltaTime;
            if(timer <= 0 && !canOrbit)
            {
                pivot.SetActive(false);
                timerStart = false;
            }
        }
        
    }

    void Orbit()
    {

        distanceFromTarget = Vector3.Distance(transform.position, pivot.transform.position);

        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        rotationY += mouseX;
        rotationX -= mouseY;

        rotationX = Mathf.Clamp(rotationX, -40, 40);

        Vector3 nextRotation = new Vector3(rotationX, rotationY);
        currentRotation = Vector3.SmoothDamp(currentRotation, nextRotation, ref smoothVelocity, smoothTime);
        transform.eulerAngles = currentRotation;

        transform.position = pivot.transform.position - transform.forward * distanceFromTarget;
    }

    void LookAround()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontalInput, 0, verticalInput) * speed * Time.deltaTime;
        transform.Translate(moveDirection, Space.Self);

        float fareX = Input.GetAxis("Mouse X");
        float fareY = Input.GetAxis("Mouse Y");

        xEksen += fareX * hassasiyet;
        yEksen -= fareY * hassasiyet;

        yEksen = Mathf.Clamp(yEksen, -90f, 90f);

        transform.eulerAngles = new Vector3(yEksen, xEksen, 0.0f);
        
    }

    void Zoom(float zoomDiff, bool orthographic)
    {
        if(!orthographic)
        {
            ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if(zoomDiff > 0)
            {
                distanceFromTarget = Vector3.Distance(transform.position, pivot.transform.position);

                if (Physics.Raycast(ray, out hit) && !timerStart && !canOrbit)
                {
                    notHit = false;
                    pivot.transform.position = hit.point;  
                }
                else notHit = true;

                if(notHit && !timerStart)
                {
                    pivot.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    mousePos = Input.mousePosition;
                    mousePos.z = 6f;
                    pivot.transform.position = mainCamera.ScreenToWorldPoint(mousePos);
                }
            
                if(distanceFromTarget > 1.2f)
                {
                    pivot.transform.localScale = new Vector3(distanceFromTarget / 20, distanceFromTarget / 20, distanceFromTarget / 20);
                    Ray mouseRay = mainCamera.ScreenPointToRay(Input.mousePosition);
                    transform.position = Vector3.MoveTowards(transform.position, pivot.transform.position, 1f);
                }

                pivot.SetActive(true);
                timer = 1f;
                timerStart = true; 
            }

            if(zoomDiff < 0)
            {
                distanceFromTarget = Vector3.Distance(transform.position, pivot.transform.position);

                if (Physics.Raycast(ray, out hit) && !timerStart && !canOrbit)
                {
                    notHit = false;
                    pivot.transform.position = hit.point;  
                }
                else notHit = true;

                if(notHit && !timerStart)
                {
                    pivot.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    mousePos = Input.mousePosition;
                    mousePos.z = 4f;
                    pivot.transform.position = mainCamera.ScreenToWorldPoint(mousePos);
                }

                pivot.transform.localScale = new Vector3(distanceFromTarget / 20, distanceFromTarget / 20, distanceFromTarget / 20);
                Ray mouseRay = mainCamera.ScreenPointToRay(Input.mousePosition);
                transform.position = Vector3.MoveTowards(transform.position, pivot.transform.position, -1f);

                pivot.SetActive(true);
                timer = 1f;
                timerStart = true; 
            }
        }
        else
        {
            if(zoomDiff > 0 && mainCamera.orthographicSize > 1)
            {
                mainCamera.orthographicSize -= zoomDiff * 10.0f;
            }
            if(zoomDiff < 0)
            {
                mainCamera.orthographicSize -= zoomDiff * 10.0f;
            }
        } 
    }

    void Pan()
    {
        float fareX = Input.GetAxis("Mouse X");
        float fareY = Input.GetAxis("Mouse Y");

        Vector3 moveDirection = new Vector3(-fareX, -fareY, 0) * speed * Time.deltaTime;

        transform.transform.Translate(moveDirection, Space.Self);
    }
}
