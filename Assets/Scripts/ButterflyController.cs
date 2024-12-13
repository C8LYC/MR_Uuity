using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButterflyController : MonoBehaviour {

    public bool isPresent;
    public float timeToArrived;
    public bool isBlown;
    public float timeToBeBlown;
    public float timeToHurtDandelion;
    public float remainingTime;


    public Vector3 blowDirection = new Vector3(0, 1, 2);
    public float blowForce = 1;
    public bool setStartPosition;
    public Vector3 startPosition;
    public bool setTargetPosition;
    public Vector3 targetPosition;
    public bool reset;

    private float moveTime;
    private float blownTime;
    public Animator butterflyAnimator;
    public Rigidbody butterflyRigidbody;
    public GameObject butterflyGameObject;
    private bool isEntered;

    // Start is called before the first frame update
    void Start() {
        isPresent = false;
        setStartPosition = false;
        setTargetPosition = false;
        isEntered = false;
        isBlown = false;
        reset = false;
        timeToArrived = 1.04f;
        moveTime = 0.0f;
        timeToBeBlown = 1.0f;
        blownTime = 0.0f;

        timeToHurtDandelion = 5f;
        remainingTime = timeToHurtDandelion;

        //butterflyGameObject = gameObject.transform.Find("Butterfly").gameObject;
        butterflyAnimator = butterflyGameObject.GetComponent<Animator>();
        butterflyRigidbody = butterflyGameObject.GetComponent<Rigidbody>();
        
        butterflyGameObject.SetActive(false);
    }

    void FixedUpdate() {
        if (isBlown) {
            if (blownTime <= timeToBeBlown) {
                butterflyAnimator.SetBool("blown", true);
                butterflyRigidbody.AddForce(blowDirection * blowForce);
                blownTime += Time.deltaTime;
            }
            if (blownTime > timeToBeBlown) {
                butterflyGameObject.SetActive(false);
            }
        } else {
            blownTime = 0.0f;
            resetPool(butterflyRigidbody);
            butterflyAnimator.SetBool("blown", false);
        }
    }

    // Update is called once per frame
    void Update() {
        if (setStartPosition) {
            setStartPositionWithNowPosition();
            setStartPosition = false;
        }
        if (setTargetPosition) {
            setTargetPositionWithNowPosition();
            setTargetPosition = false;
        }
        if (reset) {
            resetButterfly();
            reset = false;
        }
        if (isPresent) {
            remainingTime -= Time.deltaTime;
        }
        if (remainingTime < 0f) {
            gameObject.GetComponent<LerpMaterialColors>().enabled = true;
        }

        if (isPresent && isBlown != true) {
            butterflyGameObject.SetActive(true);
            if (isEntered) {
                butterflyAnimator.SetBool("flyin", false);
            }
            if (isEntered != true) {
                butterflyAnimator.SetBool("flyin", true);
                isEntered = true;
            }
            moveTime += Time.deltaTime/timeToArrived;
            butterflyGameObject.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, moveTime);
        } else {
            moveTime = 0.0f;
            isEntered = false;
        }
    }

    void setStartPositionWithNowPosition() {
        startPosition = butterflyGameObject.transform.localPosition;
    }

    void setTargetPositionWithNowPosition() {
        targetPosition = butterflyGameObject.transform.localPosition;
    }

    void resetButterfly() {
        isPresent = false;
        isEntered = false;
        isBlown = false;
        moveTime = 0.0f;
        blownTime = 0.0f;
        butterflyGameObject.SetActive(false);
    }

    public void resetPool(Rigidbody rg) {
        rg.velocity = Vector3.zero;
        rg.angularVelocity = Vector3.zero;
    }
}
