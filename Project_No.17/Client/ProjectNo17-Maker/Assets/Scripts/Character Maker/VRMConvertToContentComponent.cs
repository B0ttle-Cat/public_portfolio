#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using BC.CMC;

using MagicaCloth2;

using Sirenix.OdinInspector;

using UniHumanoid;

using Unity.VisualScripting;

using UnityEditor;

using UnityEngine;

using UniVRM10;

using static BC.CMC.CharacterModelController;

using Path = System.IO.Path;

public partial class VRMConvertToContentComponent : MonoBehaviour
{
	[SerializeField]
	[InlineButton("FindVrmModel","Find Vrm Model")]
	public GameObject VrmModelTarget;

	[SerializeField]
	private bool resetTransform = true;

	[SerializeField]
	private RuntimeAnimatorController runtimeAnimatorController;

	public void FindVrmModel()
	{
		Vrm10Instance vrm10Instance = null;
		if(VrmModelTarget != null)
		{
			vrm10Instance = VrmModelTarget.GetComponentInChildren<Vrm10Instance>(true);
		}
		if(vrm10Instance == null)
		{
			vrm10Instance =  gameObject.GetComponentInChildren<Vrm10Instance>();
		}
		if(vrm10Instance != null)
		{
			VrmModelTarget = vrm10Instance.gameObject;
		}
	}
	[Button]
	public void ConvertVrmToUnity()
	{
		FindVrmModel();
		if(VrmModelTarget == null) return;

		if(resetTransform)
		{
			VrmModelTarget.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
			VrmModelTarget.transform.localScale = Vector3.one;
		}

		Humanoid humanoid = VrmModelTarget.GetComponent<Humanoid>();
		Vrm10Instance vrmInstance = VrmModelTarget.GetComponent<Vrm10Instance>();
		if(humanoid == null) return;
		if(vrmInstance == null) return;
		CharacterModelController characterModelController = VrmModelTarget.GetComponent<CharacterModelController>() ?? VrmModelTarget.AddComponent<CharacterModelController>();
		characterModelController.InitVRMCharacter();

		ConvertAnimator();

		ConvertUnityToonShader();

		ConvertLookAt(humanoid, vrmInstance, characterModelController);

		ConvertExpression(humanoid, vrmInstance, characterModelController);

		ConvertMagicaCloth(humanoid, vrmInstance, characterModelController);

	}
}

public partial class VRMConvertToContentComponent // ConvertAnimator
{
	void ConvertAnimator()
	{
		if(VrmModelTarget.TryGetComponent<Animator>(out var animator))
		{
			animator.runtimeAnimatorController = runtimeAnimatorController;
		}
	}
}
public partial class VRMConvertToContentComponent // ConvertUnityToonShader
{
	[SerializeField]
	[LabelText("convertToonShader(전환이 불안정함. 비활성 추천)"), LabelWidth(300)]
	private bool convertToonShader = false;

	void ConvertUnityToonShader()
	{
		if(!convertToonShader) return;
		Shader targetShader = Shader.Find("Toon");
		if(targetShader == null)
		{
			Debug.LogError("Toon Shader를 찾을 수 없습니다. Unity Toon Shader 기 설치되어 있는지 확인하고, 해당 쉐이더 이름을 확인하세요.");
			return;
		}

		Color _2nd_ShadeColor = new Color(150f/255f,150f/255f,150f/255f);

		HashSet<Material> convertPath = new HashSet<Material>();

		var skinnedMeshRenderers = VrmModelTarget.GetComponentsInChildren<SkinnedMeshRenderer>(true);
		int length1 = skinnedMeshRenderers.Length;
		int length2 = 0;
		for(int i = 0 ; i < length1 ; i++)
		{
			var materials = skinnedMeshRenderers[i].sharedMaterials;
			length2 = materials.Length;
			for(int ii = 0 ; ii < length2 ; ii++)
			{
				materials[ii] = Convert(materials[ii]);
			}
			skinnedMeshRenderers[i].sharedMaterials = materials;
		}
		var meshRenderers= VrmModelTarget.GetComponentsInChildren<MeshRenderer>(true);
		length1 = meshRenderers.Length;
		for(int i = 0 ; i < length1 ; i++)
		{
			var materials = meshRenderers[i].sharedMaterials;
			length2 = materials.Length;
			for(int ii = 0 ; ii < length2 ; ii++)
			{
				materials[ii] = Convert(materials[ii]);
			}
			meshRenderers[i].sharedMaterials = materials;
		}
		Material Convert(Material originalMat)
		{
			if(!convertPath.Add(originalMat)) return originalMat;
			if(originalMat.shader.name != "VRM10/MToon10" && originalMat.shader.name != "VRM10/Universal Render Pipeline/MToon10")
			{
				Debug.LogWarning($"선택된 머티리얼은 MToon10 Shader가 아닙니다. ({originalMat.shader.name})");
				return originalMat;
			}
			string originalPath = AssetDatabase.GetAssetPath(originalMat);
			string directory = Path.GetDirectoryName(originalPath);
			string originalName = Path.GetFileNameWithoutExtension(originalPath);
			string newPath = Path.Combine(directory, $"Toon_{originalMat.name}.mat").Replace("\\", "/");
			// 새로운 머티리얼 생성 및 속성 복사
			Material newMat = new Material(targetShader);


			NewShaderaFloat("_ClippingMode", 2f);
			NewShaderaFloat("_IsBaseMapAlphaAsClippingMask", 1f);
			/////Color & Texture//////////////////////////////
			ConvertTexture("_MainTex", "_MainTex");
			ConvertColor("_Color", "_Color");
			ConvertTexture("_ShadeTex", "_1st_ShadeMap");
			NewShaderToggle("_Use_BaseAs1st", false);
			ConvertColor("_Color", "_1st_ShadeColor");
			ConvertTexture("_ShadeTex", "_2nd_ShadeMap");
			NewShaderToggle("_Use_1stAs2nd", true);
			NewShaderColor("_2nd_ShadeColor", _2nd_ShadeColor);
			/////Color & Texture Step//////////////////////////////
			NewShaderaFloat("_BaseColor_Step", 0.5f);
			NewShaderaFloat("_BaseShade_Feather", 0.05f);
			NewShaderaFloat("_ShadeColor_Step", 0.05f);
			NewShaderaFloat("_1st2nd_Shades_Feather", 0.05f);
			/////NormalMap//////////////////////////////
			ConvertTexture("_BumpMap", "_NormalMap");
			ConvertFloat("_BumpScale", "_BumpScale");
			///HighColor_Tex////////////////////////////////////
			ConvertTexture("_EmissionMap", "_HighColor_Tex");
			ConvertColor("_EmissionColor", "_HighColor");
			ConvertColorWhitePower("_EmissionColor", "_HighColor_Power");
			NewShaderaFloat("_Is_BlendAddToHiColor", 1f);
			///MatcapMask////////////////////////////////////
			ConvertTexture("_MatcapTex", "_MatCap_Sampler");
			ConvertColor("_MatcapColor", "_MatCapColor");
			NewShaderaFloat("_MatCap", 1f);
			///Emissive////////////////////////////////////
			ConvertTexture("_EmissionMap", "_Emissive_Tex");
			ConvertColor("_EmissionColor", "_Emissive_Color");
			///Outline////////////////////////////////////
			ConvertTexture("_OutlineWidthTex", "_OutlineTex");
			ConvertColor("_OutlineColor", "_Outline_Color");
			NewShaderaFloat("_Outline_Width", 1f);
			NewShaderaFloat("_OUTLINE", 0f);
			/////////////////////////////////
			void ConvertTexture(string original, string newName)
			{
				if(originalMat.HasProperty(original) && newMat.HasProperty(newName))
					newMat.SetTexture(newName, originalMat.GetTexture(original));
			}
			void ConvertColor(string original, string newName)
			{
				if(originalMat.HasProperty(original) && newMat.HasProperty(newName))
					newMat.SetColor(newName, originalMat.GetColor(original));
			}
			void ConvertColorWhitePower(string original, string newName)
			{
				if(originalMat.HasProperty(original) && newMat.HasProperty(newName))
				{
					Color color = originalMat.GetColor(original);
					float white = ((color.r + color.g + color.b) / 3f) * color.a;
					newMat.SetFloat(newName, white);
				}
			}
			void ConvertFloat(string original, string newName)
			{
				if(originalMat.HasProperty(original) && newMat.HasProperty(newName))
					newMat.SetFloat(newName, originalMat.GetFloat(original));
			}
			void NewShaderToggle(string newName, bool toggle)
			{
				if(newMat.HasProperty(newName))
				{
					newMat.SetFloat(newName, toggle ? 1.0f : 0.0f);
				}
			}
			void NewShaderColor(string newName, Color color)
			{
				if(newMat.HasProperty(newName))
				{
					newMat.SetColor(newName, color);
				}
			}
			void NewShaderaFloat(string newName, float value)
			{
				if(newMat.HasProperty(newName))
				{
					newMat.SetFloat(newName, value);
				}
			}
			// 저장
			AssetDatabase.CreateAsset(newMat, newPath);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			return newMat;
		}
	}
}
public partial class VRMConvertToContentComponent // ConvertLookAt
{
	void ConvertLookAt(Humanoid humanoid, Vrm10Instance vrmInstance, CharacterModelController characterModelController)
	{
		var headBone = humanoid.GetBoneTransform(HumanBodyBones.Head);
		var leftEyeBone = humanoid.GetBoneTransform(HumanBodyBones.LeftEye);
		var rightEyeBone = humanoid.GetBoneTransform(HumanBodyBones.RightEye);
		characterModelController.SetLookTarget(headBone,
		vrmInstance.Vrm.LookAt.OffsetFromHead,
		vrmInstance.Vrm.LookAt.HorizontalOuter.CurveYRangeDegree,
		vrmInstance.Vrm.LookAt.HorizontalInner.CurveYRangeDegree,
		vrmInstance.Vrm.LookAt.VerticalDown.CurveYRangeDegree,
		vrmInstance.Vrm.LookAt.VerticalUp.CurveYRangeDegree,
		leftEyeBone, rightEyeBone, vrmInstance.LookAtTarget);
	}
}
public partial class VRMConvertToContentComponent // ConvertExpression
{
	void ConvertExpression(Humanoid humanoid, Vrm10Instance vrmInstance, CharacterModelController characterModelController)
	{
		Transform face = VrmModelTarget.transform.Find("Face");
		if(face != null && face.gameObject.TryGetComponent<SkinnedMeshRenderer>(out SkinnedMeshRenderer faceMesh) && faceMesh.sharedMesh != null)
		{
			List<FaceBlendShapes> blendShapesList = new List<FaceBlendShapes>();
			int blendShapeCount = faceMesh.sharedMesh.blendShapeCount;
			for(int i = 0 ; i < blendShapeCount ; i++)
			{
				blendShapesList.Add(new FaceBlendShapes() {
					index = i,
					name = faceMesh.sharedMesh.GetBlendShapeName(i),
					weight = faceMesh.GetBlendShapeWeight(i),
				});
			}

			List<ExpressionItem> expressionList = new List<ExpressionItem>();
			var clips = vrmInstance.Vrm.Expression.Clips;
			foreach(var clip in clips)
			{
				if(clip.Clip == null) continue;
				FaceBlendShapes[] faceBlendShapeList = clip.Clip.MorphTargetBindings
								.Where(x=>x.RelativePath == "Face")
								.Select(x => new FaceBlendShapes() {
									index = x.Index,
									weight = x.Weight,
									name = faceMesh.sharedMesh.GetBlendShapeName(x.Index),
								})
							.ToArray();
				if(faceBlendShapeList.Length == 0) continue;

				ExpressionItem expressionItem = new ExpressionItem(){
					clip = clip.Clip,
				};
				expressionList.Add(expressionItem);
			}

			characterModelController.SetExpressionList(faceMesh, blendShapesList, expressionList);
		}
	}
}
public partial class VRMConvertToContentComponent // ConvertMagicaCloth
{
	[SerializeField]
	private bool convertMagicaCloth = true;
	[SerializeField]
	private MagicaClothConvertMappingData magicaClothConvertMappingData;

	const string prefix = "MC2_Preset";
	class PresetInfo
	{
		public string presetPath;
		public string presetName;
		public TextAsset text;
	}
	public struct SpringConvert
	{
		public Vrm10InstanceSpringBone.Spring spring;
		public VRM10SpringBoneJoint[] joints;
		public VRM10SpringBoneColliderGroup[] colliderGroups;
	}
	private string MagicaCapsuleColliderName(string name) => $"Magica Capsule Collider ({name})";
	private string MagicaPlaneColliderName(string name) => $"Magica Plane Collider ({name})";
	void ConvertMagicaCloth(Humanoid humanoid, Vrm10Instance vrmInstance, CharacterModelController characterModelController)
	{
		if(!convertMagicaCloth) return;

		Vrm10InstanceSpringBone springBone = vrmInstance.SpringBone;
		List<Vrm10InstanceSpringBone.Spring> springs = springBone.Springs;
		List<VRM10SpringBoneColliderGroup> colliderGroups = springBone.ColliderGroups;

		if(springs.Count == 0) return;
		if(colliderGroups.Count == 0) return;

		List<MagicaCloth> magicaClothMapping = new List<MagicaCloth>();
		HashSet<MagicaCloth> magicaClothHash = new HashSet<MagicaCloth>();

		MappingMagicaCloth();
		void MappingMagicaCloth()
		{
			int length = springs == null ? 0 : springs.Count;
			for(int i = 0 ; i < length ; i++)
			{
				var cloth = _MappingMagicaCloth(springs[i]);
				magicaClothMapping.Add(cloth);
				if(cloth != null)
				{
					magicaClothHash.Add(cloth);
				}
			}
			MagicaCloth _MappingMagicaCloth(Vrm10InstanceSpringBone.Spring spring)
			{
				if(spring == null) return null;
				if(spring.Joints == null || spring.Joints.Count ==0) return null;
				List<MagicaCloth> list = new List<MagicaCloth>();

				string vrmName = spring.Name;
				List<MagicaClothConvertMappingData.BoneMappingData> mappingDatas = magicaClothConvertMappingData.boneMappingDatas.Where(x => x.vrmName == vrmName).ToList();
				if(vrmName == "Hair")
				{
					if(IsFrontHairCheck())
						mappingDatas.RemoveAll(i => i.mc2Name != "FrontHair");
					else if(IsShortHairCheck())
						mappingDatas.RemoveAll(i => i.mc2Name != "ShortHair");
					else if(IsLongHairCheck())
						mappingDatas.RemoveAll(i => i.mc2Name != "LongHair");
					else
						mappingDatas.RemoveAll(i => i.mc2Name != "VeryLongHair");
				}

				if(mappingDatas == null || mappingDatas.Count == 0) return null;
				return FindMagicaCloth(mappingDatas[0]);

				bool IsFrontHairCheck()
				{
					VRM10SpringBoneJoint lastJoin = spring.Joints.Count == 1 ? spring.Joints[^1] : spring.Joints[^2];
					Transform neck = humanoid.Neck;
					Vector3 lastJoinPos = lastJoin.transform.position;
					Vector3 neckJoinPos = neck.position;
					Vector3 neckForward = neck.forward;
					neckForward.y = 0;
					Vector3 lastJoinDir = (lastJoinPos - neckJoinPos);
					lastJoinDir.y = 0;
					lastJoinDir = lastJoinDir.normalized;

					return Vector3.Dot(neckForward, lastJoinDir) > 0f;
				}
				bool IsShortHairCheck()
				{
					VRM10SpringBoneJoint lastJoin = spring.Joints.Count == 1 ? spring.Joints[^1] : spring.Joints[^2];
					Transform heightCheck = humanoid.LeftShoulder;
					Vector3 lastJoinPos = lastJoin.transform.position;
					Vector3 heightCheckJoinPos = heightCheck.position;
					return lastJoinPos.y > heightCheckJoinPos.y;
				}
				bool IsLongHairCheck()
				{
					VRM10SpringBoneJoint lastJoin = spring.Joints.Count == 1 ? spring.Joints[^1] : spring.Joints[^2];
					Transform shoulder = humanoid.Spine;
					Vector3 lastJoinPos = lastJoin.transform.position;
					Vector3 shoulderJoinPos = shoulder.position;
					return lastJoinPos.y > shoulderJoinPos.y;
				}
				MagicaCloth FindMagicaCloth(MagicaClothConvertMappingData.BoneMappingData mappingData)
				{
					string colthName = $"Magica Cloth ({mappingData.mc2Name})";
					var findCloth = VrmModelTarget.transform.Find(colthName);
					if(findCloth == null)
					{
						findCloth = new GameObject(colthName).transform;
						findCloth.parent = VrmModelTarget.transform;
						findCloth.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
						findCloth.localScale = Vector3.one;
					}
					if(!findCloth.TryGetComponent<MagicaCloth>(out var findMagicaCloth)) findMagicaCloth = findCloth.AddComponent<MagicaCloth>();

					findMagicaCloth.SerializeData.clothType = (ClothProcess.ClothType)mappingData.mc2BoneType;
					var bones = findMagicaCloth.SerializeData.rootBones;

					var vrmRootJoint = spring.Joints[0].transform;
					if(!bones.Contains(vrmRootJoint)) bones.Add(vrmRootJoint);
					return findMagicaCloth;
				}
			}
		}

		MagicaClothSetup();
		void MagicaClothSetup()
		{
			var clothPreset = LoadMagicaClothPreset();
			foreach(var cloth in magicaClothHash)
			{
				_MagicaClothSetup(clothPreset, cloth);
			}

			Dictionary<string, PresetInfo> LoadMagicaClothPreset()
			{
				var guidArray = AssetDatabase.FindAssets($"{prefix} t:{nameof(TextAsset)}");
				if(guidArray == null) return null;

				var dict = new Dictionary<string, PresetInfo>();
				foreach(var guid in guidArray)
				{
					var filePath = AssetDatabase.GUIDToAssetPath(guid);

					if(filePath.EndsWith(".json") == false)
						continue;

					var text = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
					if(text)
					{
						var info = new PresetInfo();
						info.presetPath = filePath;
						var fname = Path.GetFileNameWithoutExtension(filePath);
						fname = fname.Replace(prefix, "");
						if(fname.StartsWith("_")) fname = fname.Remove(0, 1);
						info.presetName = fname;
						info.text = text;

						dict[filePath] = info;
					}
				}
				return dict;
			}
			void _MagicaClothSetup(Dictionary<string, PresetInfo> clothPreset, MagicaCloth magicaCloth)
			{
				string name = magicaCloth.transform.name;
				Match match = Regex.Match(name, @"\(([^)]+)\)");
				if(!match.Success) return;
				name = match.Groups[1].Value;
				var findMapping = magicaClothConvertMappingData.boneMappingDatas.FirstOrDefault(x => x.mc2Name == name);
				string presetPath = findMapping.mc2PresetPath;
				if(string.IsNullOrWhiteSpace(presetPath)) return;

				if(clothPreset.TryGetValue(presetPath, out var presetInfos) && presetInfos.text != null)
				{
					ClothSerializeData sdata = magicaCloth.SerializeData;

					if(sdata.ImportJson(presetInfos.text.text))
						Develop.Log("Completed.");
					else
						Develop.LogError("Preset load error!");
				}
			}
		}

		MappingMagicaJoinSize();
		void MappingMagicaJoinSize()
		{
			foreach(var cloth in magicaClothHash)
			{
				var rootBones = cloth.SerializeData.rootBones;
				foreach(var root in rootBones)
				{
					VRM10SpringBoneJoint rootBoneJoints = root.GetComponent<VRM10SpringBoneJoint>();
					var findJoint = springBone.FindJoint(rootBoneJoints);
					if(!findJoint.HasValue) continue;

					var spring = findJoint.Value.Item1;
					int length = spring.Joints.Count - 1;// -1 : 마지막 조인트 제거
					if(length <= 0) continue;

					float[] jointRadius = new float[length+1];
					jointRadius[^1] = 0f;
					for(int i = 0 ; i < length ; i++)
					{
						jointRadius[i] = spring.Joints[i].m_jointRadius;
					}
					float maxRadius = Mathf.Max(jointRadius);
					if(maxRadius <= 0) continue;

					for(int i = 0 ; i < length ; i++)
					{
						jointRadius[i] /= maxRadius;
					}
					var radiusInf0 = cloth.SerializeData.radius;
					radiusInf0.value = maxRadius;
					radiusInf0.useCurve = true;
					float fLength = length;
					radiusInf0.curve = new AnimationCurve() {
						keys = jointRadius.Select((v, i) => new Keyframe((float)i/fLength, v)).ToArray()
					};
				}
			}
		}

		ConvertCollider();
		void ConvertCollider()
		{
			Init();
			void Init()
			{
				foreach(var cloth in magicaClothHash)
				{
					List<ColliderComponent> colliderList = cloth.SerializeData.colliderCollisionConstraint.colliderList;
					colliderList.Clear();
				}

				ColliderComponent[] deleteAllColliderComponent = VrmModelTarget.GetComponentsInChildren<ColliderComponent>(true);
				foreach(var item in deleteAllColliderComponent)
				{
					DestroyImmediate(item);
				}
			}

			var colliderComponentGroup = _ConvertAllCollider();
			Dictionary<string, List<ColliderComponent>> _ConvertAllCollider()
			{
				Dictionary<string, List<ColliderComponent>> colliderComponentGroup = new Dictionary<string, List<ColliderComponent>>();
				int groupLength = colliderGroups == null ? 0 : colliderGroups.Count;
				for(int g = 0 ; g < groupLength ; g++)
				{
					List<VRM10SpringBoneCollider> colliders = colliderGroups[g].Colliders;
					List<ColliderComponent> mc2ColliderList = _ConvertCollider(colliders);
					if(colliderComponentGroup.TryGetValue(colliderGroups[g].Name, out var list))
					{
						list.AddRange(mc2ColliderList);
					}
					else
					{
						colliderComponentGroup.Add(colliderGroups[g].Name, mc2ColliderList);
					}
				}
				return colliderComponentGroup;
			}

			ConnectSpineToChest();
			void ConnectSpineToChest()
			{
				if(springs == null || springs.Count == 0) return;
				Transform spine = humanoid.Spine;
				Transform chest = humanoid.UpperChest;

				if(spine == null || chest == null) return;

				if(!colliderComponentGroup.TryGetValue(spine.name, out List<ColliderComponent> spineColliders)) return;
				if(!colliderComponentGroup.TryGetValue(chest.name, out List<ColliderComponent> chestColliders)) return;

				(ColliderComponent spine, ColliderComponent chest, float distance) find = FindClosestPair(spineColliders, chestColliders);
				if(find.spine.GetColliderType() != ColliderManager.ColliderType.Sphere) return;
				if(find.chest.GetColliderType() != ColliderManager.ColliderType.Sphere) return;

				MagicaCapsuleCollider newSpine = ConverSphereToCapsuleCollider(
					spine.InverseTransformPoint(GetWorldCenter(find.spine)),
					spine.InverseTransformPoint(GetWorldCenter(find.chest)),
					find.spine.GetSize().x,
					find.chest.GetSize().x,
					spine);
				spineColliders.Remove(find.spine);
				chestColliders.Remove(find.chest);
				DestroyImmediate(find.spine);
				DestroyImmediate(find.chest);

				if(chestColliders.Count == 2)
				{
					var chestA = chestColliders[0];
					var chestB = chestColliders[1];
					MagicaCapsuleCollider newUpperChest =  ConverSphereToCapsuleCollider(
						chestA.center,
						chestB.center,
						chestA.GetSize().x,
						chestB.GetSize().x,
						chest);
					chestColliders.Remove(chestA);
					chestColliders.Remove(chestB);
					DestroyImmediate(chestA);
					DestroyImmediate(chestB);
					chestColliders.Add(newUpperChest);
				}
				spineColliders.Add(newSpine);
				chestColliders.Add(newSpine);

				static (ColliderComponent spine, ColliderComponent chest, float distance) FindClosestPair(List<ColliderComponent> spineColliders, List<ColliderComponent> chestColliders)
				{
					ColliderComponent bestSpine = null;
					ColliderComponent bestChest = null;
					float minDistance = float.MaxValue;

					foreach(var spine in spineColliders)
					{
						Vector3 spineCenter = GetWorldCenter(spine);

						foreach(var chest in chestColliders)
						{
							Vector3 chestCenter = GetWorldCenter(chest);

							float distance = Vector3.Distance(spineCenter, chestCenter);
							if(distance < minDistance)
							{
								minDistance = distance;
								bestSpine = spine;
								bestChest = chest;
							}
						}
					}

					return (bestSpine, bestChest, minDistance);
				}
				static Vector3 GetWorldCenter(ColliderComponent collider)
				{
					return collider.transform.position + collider.transform.rotation * collider.center;
				}
			}

			MagicaPlaneCollider planeCollider = CreateMagicaPlaneCollider();

			ConnectSpringCollider();
			void ConnectSpringCollider()
			{
				int length = springs == null ? 0 : springs.Count;
				for(int i = 0 ; i < length ; i++)
				{
					if(springs[i] == null || magicaClothMapping[i] == null) continue;
					var serializeData = magicaClothMapping[i].SerializeData;
					List<ColliderComponent> colliderList = serializeData.colliderCollisionConstraint.colliderList;
					if(planeCollider != null && !colliderList.Contains(planeCollider)) colliderList.Add(planeCollider);

					List<VRM10SpringBoneColliderGroup> colliderGroups = springs[i].ColliderGroups;
					int gLength = colliderGroups == null ? 0 : colliderGroups.Count;
					for(int g = 0 ; g < gLength ; g++)
					{
						string groupKey = colliderGroups[g].Name;
						if(colliderComponentGroup.TryGetValue(groupKey, out var colliderComponents))
						{
							int colliderLength = colliderComponents == null ? 0 : colliderComponents.Count;
							for(int c = 0 ; c < colliderLength ; c++)
							{
								var collider = colliderComponents[c];
								if(collider == null || colliderList.Contains(collider)) continue;
								colliderList.Add(collider);
							}
						}
					}
				}
			}

			List<ColliderComponent> _ConvertCollider(List<VRM10SpringBoneCollider> colliders)
			{
				if(colliders == null || colliders.Count == 0) return null;
				SortCollidersAlongDirection(colliders);
				PreMergeNearbyColliders(colliders);
				List<ColliderComponent> mc2ColliderList = new List<ColliderComponent>();

				int length = colliders.Count;
				bool isCanSphereToCapsule = true;
				for(int i = 0 ; i < length ; i++)
				{
					if(colliders[i].ColliderType != VRM10SpringBoneColliderTypes.Sphere)
						isCanSphereToCapsule = false;
				}

				if(isCanSphereToCapsule && length >= 2)
				{
					var capsuleCollider = ConverSphereListToCapsuleCollider(colliders);
					if(capsuleCollider != null)
					{
						mc2ColliderList.Add(capsuleCollider);
						return mc2ColliderList;
					}
				}

				for(int i = 0 ; i < length ; i++)
				{
					ColliderComponent collider = colliders[i].ColliderType switch {
						VRM10SpringBoneColliderTypes.Sphere => ConverSphereCollider(colliders[i]),
						VRM10SpringBoneColliderTypes.Capsule => ConverCapsuleCollider(colliders[i]),
						_ => null
					};
					if(collider == null) continue;
					mc2ColliderList.Add(collider);
				}
				return mc2ColliderList;
			}
			void PreMergeNearbyColliders(List<VRM10SpringBoneCollider> colliders, float mergeThreshold = 0.001f)
			{
				for(int i = 0 ; i < colliders.Count ; i++)
				{
					for(int j = i + 1 ; j < colliders.Count ; j++)
					{
						float centerDistance = Vector3.Distance(colliders[i].Offset, colliders[j].Offset);
						if(centerDistance < mergeThreshold)
						{
							// 1. 중심은 평균
							Vector3 mergedCenter = (colliders[i].Offset + colliders[j].Offset) * 0.5f;

							// 2. 반지름은 두 콜라이더를 모두 감싸는 최소 반지름
							float r1 = colliders[i].Radius;
							float r2 = colliders[j].Radius;
							float mergedRadius = (centerDistance + r1 + r2) * 0.5f;

							colliders[i].Offset = mergedCenter;
							colliders[i].Radius = mergedRadius;

							colliders.RemoveAt(j);
							j--;
						}
					}
				}
			}
			bool ArePointsCollinear(List<VRM10SpringBoneCollider> colliders, float mergeThreshold = 0.001f)
			{
				if(colliders.Count <= 2)
					return true;

				Vector3 origin = colliders[0].Offset;
				Vector3 dir = (colliders[1].Offset - origin).normalized;

				for(int i = 2 ; i < colliders.Count ; i++)
				{
					Vector3 toPoint = colliders[i].Offset - origin;
					float projectedLength = Vector3.Dot(toPoint, dir);
					Vector3 projectedPoint = origin + dir * projectedLength;
					float distanceFromLine = (colliders[i].Offset - projectedPoint).magnitude;

					if(distanceFromLine > mergeThreshold)
						return false;
				}
				return true;
			}
			void SortCollidersAlongDirection(List<VRM10SpringBoneCollider> colliders)
			{
				if(colliders == null || colliders.Count < 2) return;
				Vector3 origin = colliders[0].Offset;
				Vector3 dir = (colliders[1].Offset - origin).normalized;

				colliders.Sort((a, b) => {
					float da = Vector3.Dot(a.Offset - origin, dir);
					float db = Vector3.Dot(b.Offset - origin, dir);
					return da.CompareTo(db);
				});
			}
			void CalculateSplitRadius(List<VRM10SpringBoneCollider> colliders, out float startRadius, out float endRadius)
			{
				int count = colliders.Count;
				int mid = count / 2; // 중앙 인덱스

				// 시작 그룹: 0 ~ mid
				float startSum = 0f;
				for(int i = 0 ; i <= mid ; i++)
				{
					startSum += colliders[i].Radius;
				}
				startRadius = startSum / (mid + 1);

				// 끝 그룹: mid ~ 끝
				float endSum = 0f;
				for(int i = mid ; i < count ; i++)
				{
					endSum += colliders[i].Radius;
				}
				endRadius = endSum / (count - mid);
			}
			MagicaCapsuleCollider.Direction GetCapsuleDirection(Vector3 dir)
			{
				dir = dir.normalized;
				dir = new Vector3(Mathf.Abs(dir.x), Mathf.Abs(dir.y), Mathf.Abs(dir.z));

				if(dir.x > dir.y && dir.x > dir.z)
					return MagicaCapsuleCollider.Direction.X; // X축
				else if(dir.y > dir.z)
					return MagicaCapsuleCollider.Direction.Y; // Y축
				else
					return MagicaCapsuleCollider.Direction.Z; // Z축
			}

			MagicaPlaneCollider CreateMagicaPlaneCollider()
			{
				string name = MagicaPlaneColliderName(VrmModelTarget.name);
				Transform planeTr = VrmModelTarget.transform.Find(name);
				if(planeTr == null)
				{
					planeTr = new GameObject(name).transform;
					planeTr.parent = VrmModelTarget.transform;
				}
				if(!planeTr.gameObject.TryGetComponent<MagicaPlaneCollider>(out var _planeCollider)) _planeCollider = planeTr.gameObject.AddComponent<MagicaPlaneCollider>();

				planeTr.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
				planeTr.localScale = Vector3.one;
				_planeCollider.center = Vector3.zero;
				return _planeCollider;
			}
			MagicaCapsuleCollider ConverSphereListToCapsuleCollider(List<VRM10SpringBoneCollider> colliders, float mergeThreshold = 0.001f)
			{
				if(!ArePointsCollinear(colliders, mergeThreshold)) return null;
				VRM10SpringBoneCollider startC = colliders[0];
				VRM10SpringBoneCollider endedC = colliders[^1];
				CalculateSplitRadius(colliders, out float startRadius, out float endRadius);
				return ConverSphereToCapsuleCollider(startC.Offset, endedC.Offset, startRadius, endRadius, startC.gameObject.transform, mergeThreshold);
			}
			MagicaCapsuleCollider ConverSphereToCapsuleCollider(Vector3 startPoint, Vector3 endedPoint, float startRadius, float endRadius, Transform parent, float mergeThreshold = 0.001f)
			{
				Vector3 center = (startPoint + endedPoint) * 0.5f;
				Vector3 dir = endedPoint - startPoint;
				if(Mathf.Abs(startRadius-endRadius) < mergeThreshold)
				{
					endRadius = startRadius = (startRadius + endRadius) * 0.5f;
				}
				float length = dir.magnitude + startRadius + endRadius;

				string name = MagicaCapsuleColliderName(parent.name);
				Transform thisTr = parent.Find(name);
				if(thisTr == null)
				{
					thisTr = new GameObject(name).transform;
					thisTr.parent = parent;
					thisTr.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
					thisTr.localScale = Vector3.one;
				}
				MagicaCapsuleCollider magicaCollider = thisTr.gameObject.AddComponent<MagicaCapsuleCollider>();
				magicaCollider.direction = GetCapsuleDirection(dir);
				magicaCollider.reverseDirection = true;
				magicaCollider.alignedOnCenter = true;
				magicaCollider.center = center;
				magicaCollider.SetSize(startRadius, endRadius, length);
				return magicaCollider;
			}
			MagicaSphereCollider ConverSphereCollider(VRM10SpringBoneCollider springBoneCollider)
			{
				Transform parent = springBoneCollider.gameObject.transform;
				string name = MagicaCapsuleColliderName(parent.name);
				Transform thisTr = parent.Find(name);
				if(thisTr == null)
				{
					thisTr = new GameObject(name).transform;
					thisTr.parent = parent;
					thisTr.localPosition = Vector3.zero;
					thisTr.localRotation = Quaternion.identity;
					thisTr.localScale = Vector3.one;
				}
				MagicaSphereCollider magicaCollider = thisTr.gameObject.AddComponent<MagicaSphereCollider>();
				magicaCollider.center = springBoneCollider.Offset;
				magicaCollider.SetSize(springBoneCollider.Radius);
				return magicaCollider;
			}
			MagicaCapsuleCollider ConverCapsuleCollider(VRM10SpringBoneCollider springBoneCollider)
			{
				Vector3 start = springBoneCollider.Offset;
				Vector3 ended = start + springBoneCollider.Tail;
				Vector3 center = (start + ended) * 0.5f;
				Vector3 dir = ended - start;
				float radius = springBoneCollider.Radius;
				float length = dir.magnitude + radius + radius;

				Transform parent = springBoneCollider.gameObject.transform;
				string name = MagicaCapsuleColliderName(parent.name);
				Transform thisTr = parent.Find(name);
				if(thisTr == null)
				{
					thisTr = new GameObject(name).transform;
					thisTr.parent = parent;
					thisTr.localPosition = Vector3.zero;
					thisTr.localRotation = Quaternion.identity;
					thisTr.localScale = Vector3.one;
				}
				MagicaCapsuleCollider magicaCollider = thisTr.gameObject.AddComponent<MagicaCapsuleCollider>();
				magicaCollider.direction = GetCapsuleDirection(dir);
				magicaCollider.reverseDirection = true;
				magicaCollider.alignedOnCenter = true;
				magicaCollider.center = center;
				magicaCollider.SetSize(radius, radius, length);
				return magicaCollider;
			}
		}

		foreach(var cloth in magicaClothHash) LoadPresetFinish(cloth);
		static void LoadPresetFinish(MagicaCloth cloth)
		{
			if(EditorApplication.isPlaying)
			{
				cloth.SetParameterChange();
			}
			else
			{
				EditorUtility.SetDirty(cloth);
			}
		}

		RemoveOldCollider();
		void RemoveOldCollider()
		{
			springBone.ColliderGroups.Clear();
			springBone.Springs.Clear();

			var allColliderGroup = VrmModelTarget.GetComponentsInChildren<VRM10SpringBoneColliderGroup>(true);
			int length = allColliderGroup.Length;
			for(int i = 0 ; i < length ; i++)
			{
				if(allColliderGroup[i] == null) continue;
				GameObject.DestroyImmediate(allColliderGroup[i]);
			}

			var allSpringCollider = VrmModelTarget.GetComponentsInChildren<VRM10SpringBoneCollider>(true);
			length = allSpringCollider.Length;
			for(int i = 0 ; i < length ; i++)
			{
				if(allSpringCollider[i] == null) continue;
				GameObject.DestroyImmediate(allSpringCollider[i]);
			}

			var allJoint = VrmModelTarget.GetComponentsInChildren<VRM10SpringBoneJoint>(true);
			length = allJoint.Length;
			for(int i = 0 ; i < length ; i++)
			{
				if(allJoint[i] == null) continue;
				GameObject.DestroyImmediate(allJoint[i]);
			}
		}
	}
}
#endif