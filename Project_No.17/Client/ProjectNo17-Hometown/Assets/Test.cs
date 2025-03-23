using UnityEngine;

public sealed class Test : MonoBehaviour
{
	private float runtime;
	public float scale;
	public float speed;
	public void Update()
	{
		runtime += Time.deltaTime * speed;
		Vector3 pos = new Vector3(Mathf.Sin(runtime),0,Mathf.Cos(runtime)) * scale;
		transform.position = pos;
	}
}
