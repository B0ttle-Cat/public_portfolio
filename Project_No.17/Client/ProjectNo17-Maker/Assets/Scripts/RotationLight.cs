using UnityEngine;

[ExecuteInEditMode]
public class RotationLight : MonoBehaviour
{
	public Vector3 axis;
	public float speed;

#if UNITY_EDITOR
	[UnityEditor.InitializeOnLoadMethod]
	public static void InitializeOnLoadMethod()
	{
		RotationLight rotationLight = FindAnyObjectByType<RotationLight>();
		if(rotationLight == null) return;
		UnityEditor.EditorApplication.update += () => {
			UpdateRotationLight(rotationLight);
		};
	}

	// Update is called once per frame
	static void UpdateRotationLight(RotationLight rotationLight)
	{
		if(rotationLight == null) return;
		var eulerAngles = rotationLight.transform.rotation.eulerAngles;
		eulerAngles += rotationLight.speed * rotationLight.axis;
		rotationLight.transform.rotation = Quaternion.Euler(eulerAngles);
	}
#else
	void Update()
	{
		var eulerAngles = transform.rotation.eulerAngles;
		eulerAngles += speed * axis;
		transform.rotation = Quaternion.Euler(eulerAngles);
	}
#endif
}