using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraAspect : BaseMono
{
    public float targetAspect = 1200f / 800f;

    int prevWidth;
    int prevHeight;

    void Start()
    {
        prevWidth = Screen.width;
        prevHeight = Screen.height;

        UpdateAspect();
    }

    void Update()
    {
        if (Screen.width != prevWidth ||
            Screen.height != prevHeight)
        {
            prevWidth = Screen.width;
            prevHeight = Screen.height;

            UpdateAspect();
        }
    }

    void UpdateAspect()
    {
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Camera cam = GetComponent<Camera>();

        if (scaleHeight < 1.0f)
        {
            Rect rect = cam.rect;

            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;

            cam.rect = rect;
        }
        else
        {
            float scaleWidth = 1.0f / scaleHeight;

            Rect rect = cam.rect;

            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;

            cam.rect = rect;
        }
    }
}