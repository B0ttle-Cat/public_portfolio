using System;

using Sirenix.OdinInspector;

using TFSystem.SaveLoad;

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


	public TestTest test;
	[Button]
	public void SaveLoadTest()
	{
		test.a = 50;
		ISaveLoadJson testTest = test;
		string save = testTest.ToJson();
		Debug.Log(save);
		test.a = 100;
		test = testTest.FromJson<TestTest>(save);
	}
	[Serializable]
	public struct TestTest : ISaveLoadJson
	{
		public int a;
	}
}
