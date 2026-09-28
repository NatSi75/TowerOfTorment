using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MouseUtil
{
    public static Vector3 GetMousePositionInWorldSpace()
    {
        Camera currentCamera = Camera.main;
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(currentCamera.transform.position.z);
        Vector3 worldPos = currentCamera.ScreenToWorldPoint(mouseScreenPos);
        worldPos.z = 0f;
        return worldPos;
    }
}
