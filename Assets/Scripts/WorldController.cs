using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    public static WorldController Instance { get; private set; }
    public Vector2 WorldScale;
    public Transform Ground;
    private Renderer GroundRenderer;

    public Camera camera;
    public float ScrollSpeed = 5;
    public MovementManual ManualAnimalPrefab;
    private MovementManual ManualAnimal;
    public DiagnosticController Diagnostics;

    // Use this for initialization
    void Awake()
    {
        Instance = this;
        Ground.position = transform.position;
        Ground.localScale = new Vector3(WorldScale.x, 1, WorldScale.y);
        GroundRenderer = Ground.GetComponent<Renderer>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneController.LoadMainMenu();
        }
        else if (Input.GetKeyDown(KeyCode.D) && ManualAnimalPrefab != null)
        {
            if (ManualAnimal != null)
            {
                Destroy(ManualAnimal.gameObject);
            }
            ManualAnimal = Instantiate(ManualAnimalPrefab, new Vector3(this.transform.position.x, this.transform.position.y + 0.5f, this.transform.position.z), Quaternion.identity, this.transform);
            Diagnostics.SetSensorNet(ManualAnimal.GetComponent<SensorNet>());
        }
        else if (Input.GetButtonDown("Fire1"))
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000, LayerMask.GetMask("Animal")))
            {
                SensorNet sensNet = hit.collider.GetComponent<SensorNet>();
                Diagnostics.SetSensorNet(sensNet);
            }
            else
            {
                Diagnostics.SetSensorNet(null);
            }
        }
    }

    void LateUpdate()
    {
        // Zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        bool isGroundFullyVisible = IsFullyVisible(GroundRenderer, camera);

        if (scroll < 0 || (scroll > 0 && !isGroundFullyVisible))
        {
            float targetSize = Mathf.Max(10, camera.orthographicSize + (10f * scroll));
            camera.orthographicSize = targetSize;
        }

        // Move if on edge
        if (!isGroundFullyVisible)
        {
            Vector3 dir = Vector3.zero;
            if (Input.mousePosition.y >= (Screen.height * 0.95) && Input.mousePosition.y <= Screen.height)
            {
                dir += new Vector3(0, 0, 1);
            }
            else if (Input.mousePosition.y <= (Screen.height * 0.05) && Input.mousePosition.y >= 0)
            {
                dir += new Vector3(0, 0, -1);
            }

            if (Input.mousePosition.x >= (Screen.width * 0.95) && Input.mousePosition.x <= Screen.width)
            {
                dir += new Vector3(1, 0, 0);
            }
            else if (Input.mousePosition.x <= (Screen.width * 0.05) && Input.mousePosition.x >= 0)
            {
                dir += new Vector3(-1, 0, 0);
            }

            camera.transform.Translate(dir * Time.deltaTime * ScrollSpeed, Space.World);
        }
    }

    public static bool IsFullyVisible(Renderer renderer, Camera camera)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);

        Bounds bounds = renderer.bounds;
        Vector3 size = bounds.size;
        Vector3 min = bounds.min;

        //Calculate the 8 points on the corners of the bounding box
        List<Vector3> boundsCorners = new List<Vector3>(8) {
             min,
             min + new Vector3(0, 0, size.z),
             min + new Vector3(size.x, 0, size.z),
             min + new Vector3(size.x, 0, 0),
         };
        for (int i = 0; i < 4; i++)
            boundsCorners.Add(boundsCorners[i] + size.y * Vector3.up);

        //Check each plane on every one of the 8 bounds' corners
        for (int p = 0; p < planes.Length; p++)
        {
            for (int i = 0; i < boundsCorners.Count; i++)
            {
                if (planes[p].GetSide(boundsCorners[i]) == false)
                    return false;
            }
        }
        return true;
    }
}
