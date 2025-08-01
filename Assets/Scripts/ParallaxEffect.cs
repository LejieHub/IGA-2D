using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    public Camera camera;               
    public Transform followTarget;      

    Vector2 startingPosition;   
    float startingZ;            

    Vector2 camMoveSinceStart => (Vector2)camera.transform.position - startingPosition;

    float zDistanceFromTarget =>transform.position.z - followTarget.position.z;

    float clippingPlane => (camera.transform.position.z + (zDistanceFromTarget > 0 ? camera.farClipPlane : camera.nearClipPlane));

    float parallaxFactor => Mathf.Abs(zDistanceFromTarget) / (clippingPlane);

    void Start()
    {
        startingPosition = transform.position;
        startingZ = transform.position.z;
    }

    void Update()
    {
        Vector2 newPosition = startingPosition + camMoveSinceStart * parallaxFactor;
        transform.position = new Vector3(newPosition.x, newPosition.y, startingZ);
    }
}
