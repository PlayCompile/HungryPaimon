using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class throwObjects : MonoBehaviour
{
    public List<GameObject> throwThese = new List<GameObject>();
    public Transform destination;
    public GameObject throwParent;
    public float throwSpeed = 3f;
    public float throwArc = 2f;
    private Vector3 originalPos;
    public AnimationCurve positionCurveX;
    public AnimationCurve heightCurve;
    public float throwDuration = 1.0f;
    public Text txtDebug;
    public Animator animPaimon;
    private bool shouldEat = true;
    public bool queued = false;

    private void Start()
    {
        originalPos = transform.position;
    }

    void Update()
    {
        if (throwThese.Count > 0)
        {
            audioManager.instance.PlaySound("0");

            GameObject getObject = throwThese[0];
            getObject.SetActive(true);
            getObject.GetComponent<RawImage>().enabled = true;

            // Store the world position before changing the parent
            Vector3 originalPosition = getObject.transform.position;

            // Instantiate the object
            GameObject newClone = Instantiate(getObject, originalPosition, getObject.transform.rotation);

            // Set the parent
            newClone.transform.SetParent(throwParent.transform, false);

            // Restore the original world position
            newClone.transform.position = originalPosition;

            // Adjust the scale if needed
            newClone.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

            StartCoroutine(ThrowObject(newClone, destination.transform.position));

            // Remove the object from the list
            throwThese.RemoveAt(0);

            if (throwThese.Count == 0) { queued = false; }
        }
    }

    IEnumerator ThrowObject(GameObject theObject, Vector3 destination)
    {
        shouldEat = true;
        animPaimon.Play("paimonFloat");
        Vector3 startPosition = theObject.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < throwDuration)
        {
            // Normalized time [0, 1]
            float normalizedTime = elapsedTime / throwDuration;

            // Use the curve to get the new positions
            float newX = Mathf.Lerp(startPosition.x, destination.x, positionCurveX.Evaluate(normalizedTime));
            float newY = Mathf.Lerp(startPosition.y, destination.y, heightCurve.Evaluate(normalizedTime));
            float newZ = Mathf.Lerp(startPosition.z, destination.z, positionCurveX.Evaluate(normalizedTime));

            theObject.transform.position = new Vector3(newX, newY, newZ);

            elapsedTime += Time.deltaTime;
            yield return null;

            Vector2 getDistance = theObject.transform.position - destination;
            if (getDistance.x < 0.05f)
            {
                if (shouldEat)
                {
                    shouldEat = false;
                    animPaimon.Play("paimonFloat");
                    animPaimon.Play("paimonEat");
                }
                yield return new WaitForSeconds(0.05f);
                theObject.GetComponent<RawImage>().enabled = false;
            }
            txtDebug.text = getDistance.ToString();
        }

        // Ensure the object reaches the final destination
        theObject.transform.position = destination;
        Destroy(theObject);
    }
}