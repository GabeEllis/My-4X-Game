using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitView : MonoBehaviour
{
    void Stat()
    {
        finalPosition = this.transform.position + Vector3.right;
    }

    Vector3 originalPosition;
    Vector3 finalPosition;

    Vector3 currentVelocity;
    float smoothTime = 0.5f;

    public void OnUnitMoved(Hex originalHex, Hex finalHex) 
    {
        // Animate the unit moving from originalHex to the finalHex.
        this.transform.position = originalHex.PositionFromCamera();
        finalPosition = finalHex.PositionFromCamera();
        currentVelocity = Vector3.zero;

        if (Vector3.Distance(this.transform.position, finalPosition) > 2)
        {
            // This unit moved more than one hex, not an expect result.
            this.transform.position = finalPosition;
        }
    }

    void Update() 
    {
        this.transform.position = Vector3.SmoothDamp(this.transform.position, finalPosition, ref currentVelocity, smoothTime);
    }
}
