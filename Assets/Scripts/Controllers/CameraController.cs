using UnityEngine;

public class CameraController : MonoBehaviour
{
	public Transform Target = null;
	public float Smoothing = 5f;

    [SerializeField]
	private Vector3 offset = new Vector3(0f, 13f, -7f);
	void Start ()
	{
        if (Target == null)
            return;
        transform.position = Target.position + offset;
        transform.LookAt(Target);
    }
	void FixedUpdate()
	{
        if (Target == null)
            return;
		Vector3 targetCamPos = Target.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetCamPos, Smoothing * Time.deltaTime);
    }
}
