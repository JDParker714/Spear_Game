using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.3F;
    private Vector3 velocity = Vector3.zero;
    private Vector3 fake_pos;

    public float zoom_out_size = 18;
    private float zoom_in_size;
    public Vector3 zoom_out_pos;

    private bool zoomed_in = true;

    private void Awake()
    {
        zoom_in_size = GetComponent<Camera>().orthographicSize;
        zoomed_in = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z)) zoomed_in = !zoomed_in;
    }

    public void Move_To_Position(Vector3 pos)
    {
        velocity = Vector3.zero;
        transform.position = new Vector3(pos.x, pos.y, transform.position.z);
    }

    void LateUpdate()
    {

        if (target==null) target = WorldManager.Instance.Get_Player_T();

        Vector3 targetPosition = transform.position;
        if (target!=null) targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
        float targetOrthoSize = zoom_in_size;

        if (!zoomed_in)
        {
            TilemapCollider2D tilemap = FindObjectOfType<TilemapCollider2D>();
            if (tilemap != null)
            {
                Vector3 center_pos = tilemap.bounds.center;
                targetPosition = new Vector3(center_pos.x, center_pos.y, transform.position.z);
            }
            else
            {
                targetPosition = new Vector3(zoom_out_pos.x, zoom_out_pos.y, transform.position.z);
            }
            targetOrthoSize = zoom_out_size;
        }

        fake_pos = Vector3.SmoothDamp(fake_pos, targetPosition, ref velocity, smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);

        //transform.position = new Vector3(temp_pos.x, temp_pos.y, transform.position.z);
        transform.position = fake_pos;
        GetComponent<Camera>().orthographicSize = Mathf.MoveTowards(GetComponent<Camera>().orthographicSize, targetOrthoSize, 24f * Time.unscaledDeltaTime);
    }
}
