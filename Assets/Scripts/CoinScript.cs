using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinScript : MonoBehaviour {


    float angle = 1f;

    [SerializeField]
    float rotationSpeed;

    bool wasCollected = false;

    public int Value;

    public void Collect()
    {
        wasCollected = true;
    }

	// Update is called once per frame
	void Update ()
    {
        if (wasCollected)
        {
            AudioScript.Instance.PlaySound(0);

            ScoreManager.instance.IncreaseScore(Value);

            DestroyImmediate(gameObject);
            return;
        }

        // Rotate coin
        transform.Rotate(Vector3.forward, angle * rotationSpeed);
	}
}
