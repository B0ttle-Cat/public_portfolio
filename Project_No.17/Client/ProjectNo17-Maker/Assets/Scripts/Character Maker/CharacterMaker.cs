using System;

using BC.OdccBase;

using Sirenix.OdinInspector;

using UnityEngine;

using UniVRM10;

public class CharacterMaker : MonoBehaviour
{
	[SerializeField,ReadOnly]
	private GameObject characterTemplate;

	[SerializeField]
	private Vrm10Instance modelPrefab;

	[Button]
	public void CreateNewCharacter()
	{
		if(characterTemplate == null) return;
		if(modelPrefab == null) return;

		GameObject mackingTarget = Instantiate(characterTemplate, modelPrefab, "Model");
		if(mackingTarget == null) return;

		VRMConvertToUnity(mackingTarget);
	}

	private GameObject Instantiate(GameObject characterTemplate, Vrm10Instance modelPrefab, string modelParentName)
	{
		string modelName = modelPrefab.name;
		GameObject mackingTarget = Instantiate(characterTemplate, Vector3.zero, Quaternion.identity);
		Transform modelParent =  mackingTarget.transform.Find(modelParentName);
		Vrm10Instance modelTarget = Instantiate(modelPrefab, Vector3.zero, Quaternion.identity, modelParent);
		mackingTarget.name = $"Character_{modelName}";
		modelTarget.name = modelName;
		return mackingTarget;
	}

	private void VRMConvertToUnity(GameObject mackingTarget)
	{
		if(TryGetComponent<VRMConvertToContentComponent>(out var convert))
		{
			convert.VrmModelTarget = mackingTarget.gameObject;
			convert.FindVrmModel();
			convert.ConvertVrmToUnity();
		}
	}

	[Button("Open Ability Table Editor")]
	private void OpenAbilityTableEditor()
	{
		CreateAbilityTableEditorWindow.ShowWindow(this);
	}

	internal void ApplyAbilityTable(AbilityTable abilityTable)
	{
		throw new NotImplementedException();
	}
}
