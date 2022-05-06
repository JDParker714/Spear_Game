using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FpsText : MonoBehaviour
{
    public Text fps_text;
    public int avgFrameRate;
    public float updateRate = 1f;
    private float timer;

    private void Update()
    {
        if (Time.unscaledTime > timer)
        {
            int fps = (int)(1f / Time.unscaledDeltaTime);
            fps_text.text = "FPS: " + fps;
            timer = Time.unscaledTime + updateRate;
        }
    }
}
