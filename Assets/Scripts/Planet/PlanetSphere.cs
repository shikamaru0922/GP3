using System;
using System.Collections;
using UnityEngine;


    public class PlanetSphere: MonoBehaviour 
    {
        public float newGravityConstant = 0.2f; // New gravity constant for planet body
        private float currentGravityConstant;
        public float waitTime = 0.5f;
        public Planet planet;
        public PlayerController playerController;


        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerController = other.GetComponent<PlayerController>();
                playerController.standtargetAngel = gameObject.transform.position;
                // Start coroutine to change gravity after a delay
                if (gravityChangeCoroutine != null)
                {
                    StopCoroutine(gravityChangeCoroutine); // Stop any existing coroutine to reset the timer
                }
                gravityChangeCoroutine = StartCoroutine(ChangeGravityAfterDelay());
                
            }
        }

        private void OnTriggerExit(Collider other)
        {
            playerController.standtargetAngel = Vector3.zero;
        }

        private Coroutine gravityChangeCoroutine;

        private IEnumerator ChangeGravityAfterDelay()
        {
            yield return new WaitForSeconds(waitTime); // Wait for the specified time
            planet.currentGravityConstant = newGravityConstant; // Change to new gravity constant
            Debug.Log("Gravity constant changed after delay");
        }

    }