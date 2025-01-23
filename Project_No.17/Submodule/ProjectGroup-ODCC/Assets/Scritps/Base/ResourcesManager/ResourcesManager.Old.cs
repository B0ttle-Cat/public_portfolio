//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Threading.Tasks;

//using Sirenix.OdinInspector;
//using Sirenix.Utilities;

//using UnityEngine;
//using UnityEngine.Networking;

//using Object = UnityEngine.Object;
//namespace BC.Base
//{
//	public partial class ResourcesManager//.Old
//	{
//		// �ε� ���� ����
//		public enum LoadState
//		{
//			None = 0,
//			Loading = 1,
//			Success = 2,
//			Error = 3,
//		}

//		[Serializable]
//		private class AssetBundleCash
//		{
//			public string BundleName;

//			public AssetBundle AssetBundle;

//			public LoadState LoadState = LoadState.None;

//			// �ε�� ���� ������Ʈ ĳ�� ����Ʈ
//			public List<AssetObjectCash> AssetObjectCashList;

//			public Action<float> OnProgress;
//			public Action<AssetBundle> OnResult;

//			// ���� ���� �񵿱� �ٿ�ε�
//			public IEnumerator DownloadAsync(Action<float> _progress, Action<AssetBundle> _result)
//			{
//				// ���� ������Ʈ ĳ�� ����Ʈ�� �ʱ�ȭ�մϴ�.
//				AssetObjectCashList ??= new List<AssetObjectCash>();

//				// ���� �̸��� ����ְų� null�� ��� ���� ���·� ó���ϰ� ����� ��ȯ�մϴ�.
//				if(string.IsNullOrWhiteSpace(BundleName))
//				{
//					LoadState = LoadState.Error;
//					Debug.LogError("Bundle name is empty or null.");
//					_result?.Invoke(null);
//					yield break;
//				}

//				// ���α׷��� �� ��� �ݹ��� ����մϴ�.
//				OnProgress += _progress;
//				OnResult += _result;

//				// �̹� �ε� ���� ���, �ߺ� �ε��� �����ϱ� ���� �ߴ��մϴ�.
//				if(LoadState == LoadState.Loading)
//				{
//					yield break;
//				}

//				if(AssetBundle == null)
//				{
//					Debug.Log($"Already Loaded Check");
//					var loadedList =  AssetBundle.GetAllLoadedAssetBundles();
//					loadedList.ForEach(loaded => {
//						if(loaded.name == BundleName)
//						{
//							Debug.Log($"Is Already Loaded LoadAssetBundle");
//							AssetBundle = loaded;
//						}
//					});
//				}

//				// ������ �ε���� ���� ������ ��� ó���մϴ�.
//				if(AssetBundle == null)
//				{
//					LoadState = LoadState.Loading;
//					// ���� ���� ��ο� URL ��θ� �����մϴ�.
//					string fullLocalPath = Path.Combine(ConstString.BundleLocalPath, BundleName);
//					string fullUrlPath = Path.Combine(ConstString.BundleURLPath, BundleName);

//					// ���� ������ �������� ������ ������ �ٿ�ε��մϴ�.
//					if(!File.Exists(fullLocalPath))
//					{
//						Debug.Log($"Down LoadAssetBundle URL: {fullUrlPath}");
//						// UnityWebRequest�� �����ϰ� �ش� URL�κ��� ���� �����͸� �ٿ�ε��մϴ�.
//						using(UnityWebRequest www = UnityWebRequest.Get(fullUrlPath))
//						{
//							try
//							{
//								www.SendWebRequest();
//							}
//							catch(Exception ex)
//							{
//								Debug.LogException(ex);
//								LoadState = LoadState.Error;
//								OnResult?.Invoke(null);
//								yield break;
//							}

//							// �ٿ�ε尡 �Ϸ�� ������ ���� ��Ȳ�� �����մϴ�.
//							while(!www.isDone)
//							{
//								OnProgress?.Invoke(www.downloadProgress * 0.5f);
//								yield return null;
//							}

//							// �ٿ�ε尡 ������ ��� ���� �޽����� ����ϰ� ����� ��ȯ�մϴ�.
//							if(www.result != UnityWebRequest.Result.Success)
//							{
//								Debug.LogError($"Failed to download LoadAssetBundle: {www.error}");
//								LoadState = LoadState.Error;
//								OnResult?.Invoke(null);
//								yield break;
//							}

//							// �ٿ�ε��� ���� �����͸� ���� ���Ͽ� �����մϴ�.
//							try
//							{
//								File.WriteAllBytes(fullLocalPath, www.downloadHandler.data);
//								Debug.Log("LoadAssetBundle downloaded and saved.");
//							}
//							catch(Exception ex)
//							{
//								LoadState = LoadState.Error;
//								Debug.LogException(ex);
//								OnResult?.Invoke(null);
//								yield break;
//							}
//						}
//					}

//					// ���� ���Ͽ��� ������ �ε��մϴ�.
//					AssetBundleCreateRequest async = null;
//					try
//					{
//						Debug.Log($"ResourcesInstantiate LoadAssetBundle from Path: {fullLocalPath}");
//						async = AssetBundle.LoadFromFileAsync(fullLocalPath);
//					}
//					catch(Exception ex)
//					{
//						async = null;
//						Debug.LogException(ex);
//					}

//					// �ε��� �Ϸ�� ������ ���� ��Ȳ�� �����մϴ�.
//					if(async != null)
//					{
//						while(!async.isDone)
//						{
//							OnProgress?.Invoke(async.progress * 0.5f + 0.5f);
//							yield return null;
//						}
//						AssetBundle = async.assetBundle;
//						Debug.Log("LoadAssetBundle loaded successfully.");
//					}
//				}

//				// �ε� ���¸� ������Ʈ�ϰ� ���� ����� �ݹ��մϴ�.
//				LoadState = AssetBundle == null ? LoadState.Error : LoadState.Success;
//				Debug.Log($"LoadAssetBundle Result : {LoadState}");
//				OnProgress?.Invoke(1f);
//				OnResult?.Invoke(AssetBundle);
//			}

//			public void DownloadNow(Action<float> _progress, Action<AssetBundle> _result)
//			{
//				// ���� ������Ʈ ĳ�� ����Ʈ�� �ʱ�ȭ�մϴ�.
//				AssetObjectCashList ??= new List<AssetObjectCash>();

//				// ���� �̸��� ����ְų� null�� ��� ���� ���·� ó���ϰ� ����� ��ȯ�մϴ�.
//				if(string.IsNullOrWhiteSpace(BundleName))
//				{
//					LoadState = LoadState.Error;
//					Debug.LogError("Bundle name is empty or null.");
//					_result?.Invoke(null);
//					return;
//				}

//				// ���α׷��� �� ��� �ݹ��� ����մϴ�.
//				OnProgress += _progress;
//				OnResult += _result;

//				// �̹� �ε� ���� ���, �ߺ� �ε��� �����ϱ� ���� �ߴ��մϴ�.
//				if(LoadState == LoadState.Loading)
//				{
//					return;
//				}

//				if(AssetBundle == null)
//				{
//					Debug.Log($"Already Loaded Check");
//					var loadedList =  AssetBundle.GetAllLoadedAssetBundles();
//					loadedList.ForEach(loaded => {
//						if(loaded.name == BundleName)
//						{
//							Debug.Log($"Is Already Loaded LoadAssetBundle");
//							AssetBundle = loaded;
//						}
//					});
//				}

//				// ������ �ε���� ���� ������ ��� ó���մϴ�.
//				if(AssetBundle == null)
//				{
//					LoadState = LoadState.Loading;
//					// ���� ���� ��ο� URL ��θ� �����մϴ�.
//					string fullLocalPath = Path.Combine(ConstString.BundleLocalPath, BundleName);
//					string fullUrlPath = Path.Combine(ConstString.BundleURLPath, BundleName);

//					// ���� ������ �������� ������ ������ �ٿ�ε��մϴ�.
//					if(!File.Exists(fullLocalPath))
//					{
//						Debug.Log($"Down LoadAssetBundle URL: {fullUrlPath}");
//						bool? result = null;
//						Task.Run(async () => {
//							try
//							{
//								using var www = UnityWebRequest.Get(fullUrlPath);

//								www.SetRequestHeader("Content-Type", "application/json");

//								var operation = www.SendWebRequest();

//								while(!operation.isDone)
//									await Task.Yield();

//								if(www.result == UnityWebRequest.Result.Success)
//								{
//									File.WriteAllBytes(fullLocalPath, www.downloadHandler.data);
//									result = true;

//								}
//								else
//								{
//									Debug.LogError($"Failed: {www.error}");
//								}
//							}
//							catch(Exception ex)
//							{
//								Debug.LogException(ex);
//							}
//							finally
//							{
//								if(!result.HasValue)
//								{
//									result = false;
//								}
//							}
//						});
//						while(!result.HasValue) ;

//						if(!result.Value)
//						{
//							return;
//						}
//					}

//					// ���� ���Ͽ��� ������ �ε��մϴ�.
//					AssetBundle = AssetBundle.LoadFromFile(fullLocalPath);
//					Debug.Log("LoadAssetBundle loaded successfully.");
//				}

//				// �ε� ���¸� ������Ʈ�ϰ� ���� ����� �ݹ��մϴ�.
//				LoadState = AssetBundle == null ? LoadState.Error : LoadState.Success;
//				Debug.Log($"LoadAssetBundle Result : {LoadState}");
//				OnProgress?.Invoke(1f);
//				OnResult?.Invoke(AssetBundle);
//			}
//			public void Unload()
//			{
//				foreach(var assetObjectCash in AssetObjectCashList)
//				{
//					assetObjectCash.Clear();
//				}
//				AssetObjectCashList.Clear();
//				if(AssetBundle != null)
//				{
//					AssetBundle.Unload(true);
//					Object.DestroyImmediate(AssetBundle, true);
//					AssetBundle = null;
//				}
//				LoadState = LoadState.None;
//			}
//			public void Clear()
//			{
//				Unload();
//				BundleName = null;
//			}
//		}

//		[Serializable]
//		private class AssetObjectCash
//		{
//			public string AssetName;
//			public Object AssetObject;
//			public LoadState LoadState = LoadState.None;

//			public Action<float> OnProgress;
//			public Action<Object> OnResult;

//			// ���� ������Ʈ �񵿱� �ٿ�ε�
//			public IEnumerator DownloadAsync(AssetBundle assetBundle, Action<float> _progress, Action<Object> _result)
//			{
//				// ���� ������Ʈ �̸��� ����ְų� null�� ��� ���� ���·� ó���ϰ� ����� ��ȯ�մϴ�.
//				if(string.IsNullOrWhiteSpace(AssetName))
//				{
//					LoadState = LoadState.Error;
//					Debug.LogError("None name is empty or null.");
//					_result?.Invoke(null);
//					yield break;
//				}

//				// ���α׷��� �� ��� �ݹ��� ����մϴ�.
//				OnProgress += _progress;
//				OnResult += _result;

//				// �̹� �ε� ���� ���, �ߺ� �ε��� �����ϱ� ���� �ߴ��մϴ�.
//				if(LoadState == LoadState.Loading)
//				{
//					yield break;
//				}

//				// ���� ������Ʈ�� �ε���� ���� ������ ��� ó���մϴ�.
//				if(AssetObject == null)
//				{
//					// �ε带 ���� AssetBundle�� ��ȿ���� ���� ��� ���� ���·� ó���ϰ� ����� ��ȯ�մϴ�.
//					if(assetBundle == null)
//					{
//						LoadState = LoadState.Error;
//						Debug.LogError("LoadAssetBundle is null.");
//						_result?.Invoke(null);
//						yield break;
//					}

//					LoadState = LoadState.Loading;

//					// ������ �񵿱�� �ε��մϴ�.
//					Debug.Log($"ResourcesInstantiate None: {AssetName}");
//					AssetBundleRequest async = null;
//					async = assetBundle.LoadAssetAsync<Object>(AssetName);
//					while(!async.isDone)
//					{
//						_progress?.Invoke(async.progress);
//						yield return null;
//					}

//					// �ε尡 �Ϸ�Ǿ��� ��� AssetObject�� �Ҵ��մϴ�.
//					AssetObject = async.asset;
//					Debug.Log("BuiltinObject loaded successfully.");
//				}

//				// �ε� ���¸� ������Ʈ�ϰ� ���� ����� �ݹ��մϴ�.
//				LoadState = AssetObject == null ? LoadState.Error : LoadState.Success;
//				Debug.Log($"BuiltinObject Result : {LoadState}");
//				OnProgress?.Invoke(1f);
//				OnResult?.Invoke(AssetObject);
//			}
//			public void DownloadNow(AssetBundle assetBundle, Action<float> _progress, Action<Object> _result)
//			{
//				// ���� ������Ʈ �̸��� ����ְų� null�� ��� ���� ���·� ó���ϰ� ����� ��ȯ�մϴ�.
//				if(string.IsNullOrWhiteSpace(AssetName))
//				{
//					LoadState = LoadState.Error;
//					Debug.LogError("None name is empty or null.");
//					_result?.Invoke(null);
//					return;
//				}

//				// ���α׷��� �� ��� �ݹ��� ����մϴ�.
//				OnProgress += _progress;
//				OnResult += _result;

//				// �̹� �ε� ���� ���, �ߺ� �ε��� �����ϱ� ���� �ߴ��մϴ�.
//				if(LoadState == LoadState.Loading)
//				{
//					return;
//				}

//				// ���� ������Ʈ�� �ε���� ���� ������ ��� ó���մϴ�.
//				if(AssetObject == null)
//				{
//					// �ε带 ���� AssetBundle�� ��ȿ���� ���� ��� ���� ���·� ó���ϰ� ����� ��ȯ�մϴ�.
//					if(assetBundle == null)
//					{
//						LoadState = LoadState.Error;
//						Debug.LogError("LoadAssetBundle is null.");
//						_result?.Invoke(null);
//						return;
//					}

//					LoadState = LoadState.Loading;

//					// �ε尡 �Ϸ�Ǿ��� ��� AssetObject�� �Ҵ��մϴ�.
//					AssetObject = assetBundle.LoadAsset<Object>(AssetName);
//					Debug.Log("BuiltinObject loaded successfully.");
//				}

//				// �ε� ���¸� ������Ʈ�ϰ� ���� ����� �ݹ��մϴ�.
//				LoadState = AssetObject == null ? LoadState.Error : LoadState.Success;
//				Debug.Log($"BuiltinObject Result : {LoadState}");
//				OnProgress?.Invoke(1f);
//				OnResult?.Invoke(AssetObject);
//			}
//			public void Unload()
//			{
//				if(AssetObject != null)
//				{
//					AssetObject = null;
//				}
//				LoadState = LoadState.None;
//			}
//			public void Clear()
//			{
//				Unload();
//				AssetName = null;
//			}
//		}

//		[Serializable]
//		private class BuiltinObjectCash
//		{
//			public string BuiltinPath;
//			public Object BuiltinObject;
//			public LoadState LoadState = LoadState.None;

//			public Action<float> OnProgress;
//			public Action<Object> OnResult;

//			public float? AutoUnloadTime;

//			public IEnumerator DownloadAsync(Action<float> _progress, Action<Object> _result)
//			{
//				// ������Ʈ �̸��� ����ְų� null�� ��� ���� ���·� ó���ϰ� ����� ��ȯ�մϴ�.
//				if(string.IsNullOrWhiteSpace(BuiltinPath))
//				{
//					LoadState = LoadState.Error;
//					Debug.LogError("None name is empty or null.");
//					_result?.Invoke(null);
//					yield break;
//				}

//				// ���α׷��� �� ��� �ݹ��� ����մϴ�.
//				OnProgress += _progress;
//				OnResult += _result;

//				// �̹� �ε� ���� ���, �ߺ� �ε��� �����ϱ� ���� �ߴ��մϴ�.
//				if(LoadState == LoadState.Loading)
//				{
//					yield break;
//				}

//				// ���� ������Ʈ�� �ε���� ���� ������ ��� ó���մϴ�.
//				if(BuiltinObject == null)
//				{
//					LoadState = LoadState.Loading;
//					// �񵿱������� ������ �ε��մϴ�.
//					ResourceRequest resourceRequest = Resources.LoadAsync(BuiltinPath);

//					// �ε��� �Ϸ�� ������ ����մϴ�.
//					while(!resourceRequest.isDone)
//					{
//						_progress?.Invoke(resourceRequest.progress);
//						yield return null;
//					}

//					// �ε��� �Ϸ�Ǹ� �ε�� ������ �����ɴϴ�.
//					BuiltinObject = resourceRequest.asset;
//				}

//				// �ε� ���¸� ������Ʈ�ϰ� ���� ����� �ݹ��մϴ�.
//				LoadState = BuiltinObject == null ? LoadState.Error : LoadState.Success;
//				Debug.Log($"BuiltinObject Result : {LoadState}");
//				OnProgress?.Invoke(1f);
//				OnResult?.Invoke(BuiltinObject);
//			}
//			public void DownloadNow(Action<float> _progress, Action<Object> _result)
//			{
//				// ������Ʈ �̸��� ����ְų� null�� ��� ���� ���·� ó���ϰ� ����� ��ȯ�մϴ�.
//				if(string.IsNullOrWhiteSpace(BuiltinPath))
//				{
//					LoadState = LoadState.Error;
//					Debug.LogError("None name is empty or null.");
//					_result?.Invoke(null);
//					return;
//				}

//				// ���α׷��� �� ��� �ݹ��� ����մϴ�.
//				OnProgress += _progress;
//				OnResult += _result;

//				// �̹� �ε� ���� ���, �ߺ� �ε��� �����ϱ� ���� �ߴ��մϴ�.
//				if(LoadState == LoadState.Loading)
//				{
//					return;
//				}

//				// ���� ������Ʈ�� �ε���� ���� ������ ��� ó���մϴ�.
//				if(BuiltinObject == null)
//				{
//					LoadState = LoadState.Loading;
//					// �ε��� �Ϸ�Ǹ� �ε�� ������ �����ɴϴ�.
//					BuiltinObject = Resources.Load(BuiltinPath); ;
//				}

//				// �ε� ���¸� ������Ʈ�ϰ� ���� ����� �ݹ��մϴ�.
//				LoadState = BuiltinObject == null ? LoadState.Error : LoadState.Success;
//				Debug.Log($"BuiltinObject Result : {LoadState}");
//				OnProgress?.Invoke(1f);
//				OnResult?.Invoke(BuiltinObject);
//			}
//			public void Unload()
//			{
//				if(BuiltinObject != null)
//				{
//					try
//					{
//						Object.DestroyImmediate(BuiltinObject, true);
//					}
//					catch { }
//					BuiltinObject = null;
//				}
//				LoadState = LoadState.None;
//			}
//			public void Clear()
//			{
//				Unload();
//				BuiltinPath = null;
//			}
//		}

//		[ShowInInspector,ReadOnly]
//		private static List<AssetBundleCash> assetBundleCashList;

//		[ShowInInspector,ReadOnly]
//		private static List<BuiltinObjectCash> builtinObjectCashList;
//		// �̱��� �ʱ�ȭ
//		protected override void CreatedSingleton()
//		{
//			assetBundleCashList = new List<AssetBundleCash>();
//			builtinObjectCashList = new List<BuiltinObjectCash>();
//		}

//		// �̱��� ����
//		protected override void DestroySingleton()
//		{
//			if(assetBundleCashList != null)
//			{
//				foreach(var assetBundleCash in assetBundleCashList)
//				{
//					assetBundleCash.Clear();
//				}
//				assetBundleCashList.Clear();
//				assetBundleCashList = null;
//			}
//			if(builtinObjectCashList != null)
//			{
//				foreach(var builtinObjectCash in builtinObjectCashList)
//				{
//					builtinObjectCash.Clear();
//				}
//				builtinObjectCashList.Clear();
//				builtinObjectCashList = null;
//			}
//		}

//		private static void LoadAssetBundle(MonoBehaviour target, string bundleName, Action<float> progress, Action<AssetBundle> result)
//		{
//			Instance(Instance => {
//				if(target == null) target = Instance;
//				bool downAsync = true;
//#if UNITY_EDITOR
//				if(!UnityEditor.EditorApplication.isPlaying)
//				{
//					downAsync = false;
//				}
//#endif
//				if(downAsync)
//				{
//					Instance.StartCoroutine(Instance.DownloadBundleAsync(bundleName, value => {
//						if(target != null) progress?.Invoke(value);
//					},
//					LoadAssetBundleCash => {
//						if(target != null)
//						{
//							result?.Invoke(LoadAssetBundleCash.AssetBundle);
//						}
//					}));
//				}
//				else
//				{
//					Instance.DownloadBundleNow(bundleName, value => {
//						if(target != null) progress?.Invoke(value);
//					},
//					LoadAssetBundleCash => {
//						if(target != null)
//						{
//							result?.Invoke(LoadAssetBundleCash.AssetBundle);
//						}
//					});
//				}
//			});
//		}
//		private static void LoadAssetObject<T>(MonoBehaviour target, string bundleName, string assetName, Action<float> progress, Action<T> result) where T : Object
//		{
//			Instance(Instance => {
//				if(target == null) target = Instance;
//				bool downAsync = true;
//#if UNITY_EDITOR
//				if(!UnityEditor.EditorApplication.isPlaying)
//				{
//					downAsync = false;
//				}
//#endif
//				if(downAsync)
//				{
//					Instance.StartCoroutine(Instance.DownloadBundleAssetAsync(bundleName, assetName,
//					value => {
//						if(target != null) progress?.Invoke(value);
//					},
//					(LoadAssetBundleCash, LoadAssetObjectCash) => {
//						if(target != null)
//						{
//							if(LoadAssetObjectCash.AssetObject is T tAsset)
//							{
//								result?.Invoke(tAsset);
//							}
//							else
//							{
//								Debug.LogError($"Type({typeof(T).Name}) Mismatch");
//								result?.Invoke(null);
//							}
//						}
//					}));
//				}
//				else
//				{
//					Instance.DownloadBundleAssetNow(bundleName, assetName,
//					value => {
//						if(target != null) progress?.Invoke(value);
//					},
//					(LoadAssetBundleCash, LoadAssetObjectCash) => {
//						if(target != null)
//						{
//							if(LoadAssetObjectCash.AssetObject is T tAsset)
//							{
//								result?.Invoke(tAsset);
//							}
//							else
//							{
//								Debug.LogError($"Type({typeof(T).Name}) Mismatch");
//								result?.Invoke(null);
//							}
//						}
//					});
//				}
//			});
//		}
//		private void DownloadBundleNow(string bundleName, Action<float> progress, Action<AssetBundleCash> _result)
//		{
//			if(string.IsNullOrWhiteSpace(bundleName))
//			{
//				_result?.Invoke(null);
//				return;
//			}

//			AssetBundleCash assetBundleCash = assetBundleCashList.Find(assetBundle => assetBundle.BundleName == bundleName);

//			if(assetBundleCash == null)
//			{
//				assetBundleCash = new AssetBundleCash() {
//					BundleName = bundleName,
//					AssetBundle = null,
//					OnResult = null,
//					OnProgress = null,
//					LoadState = LoadState.None,
//					AssetObjectCashList = new List<AssetObjectCash>(),
//				};
//				assetBundleCashList.Add(assetBundleCash);
//			}

//			AssetBundle assetBundle = null;

//			assetBundleCash.DownloadNow(progress, loadBundle => assetBundle = loadBundle);

//			if(assetBundle == null)
//			{
//				_result?.Invoke(null);
//				return;
//			}
//			progress?.Invoke(1f);
//			_result?.Invoke(assetBundleCash);
//		}
//		private IEnumerator DownloadBundleAsync(string bundleName, Action<float> progress, Action<AssetBundleCash> _result)
//		{
//			if(string.IsNullOrWhiteSpace(bundleName))
//			{
//				_result?.Invoke(null);
//				yield break;
//			}

//			AssetBundleCash assetBundleCash = assetBundleCashList.Find(assetBundle => assetBundle.BundleName == bundleName);

//			if(assetBundleCash == null)
//			{
//				assetBundleCash = new AssetBundleCash() {
//					BundleName = bundleName,
//					AssetBundle = null,
//					OnResult = null,
//					OnProgress = null,
//					LoadState = LoadState.None,
//					AssetObjectCashList = new List<AssetObjectCash>(),
//				};
//				assetBundleCashList.Add(assetBundleCash);
//			}

//			AssetBundle assetBundle = null;

//			yield return assetBundleCash.DownloadAsync(progress, loadBundle => assetBundle = loadBundle);

//			if(assetBundle == null)
//			{
//				_result?.Invoke(null);
//				yield break;
//			}
//			progress?.Invoke(1f);
//			_result?.Invoke(assetBundleCash);
//		}
//		private void DownloadBundleAssetNow(string bundleName, string assetName, Action<float> progress, Action<AssetBundleCash, AssetObjectCash> _result)
//		{
//			if(string.IsNullOrWhiteSpace(bundleName) || string.IsNullOrWhiteSpace(assetName))
//			{
//				_result?.Invoke(null, null);
//				return;
//			}

//			float progressValue = 0f;
//			AssetBundleCash assetBundleCash = null;
//			DownloadBundleNow(bundleName,
//				  value => progress?.Invoke(value * 0.5f + progressValue),
//				  loadCash => assetBundleCash = loadCash);

//			if(assetBundleCash == null || assetBundleCash.AssetBundle == null)
//			{
//				_result?.Invoke(null, null);
//				return;
//			}

//			List<AssetObjectCash> objectCashList = assetBundleCash.AssetObjectCashList;
//			AssetObjectCash assetObjectCash = objectCashList.Find(cash => cash.AssetName == assetName);

//			if(assetObjectCash == null)
//			{
//				assetObjectCash = new AssetObjectCash() {
//					AssetName = assetName,
//					AssetObject = null,
//					OnResult = null,
//					OnProgress = null,
//					LoadState = LoadState.None,
//				};
//				assetBundleCash.AssetObjectCashList.Add(assetObjectCash);
//			}

//			progressValue = 0.5f;
//			Object asset = null;
//			assetObjectCash.DownloadNow(assetBundleCash.AssetBundle,
//				value => progress?.Invoke(value * 0.5f + progressValue),
//				loadAsset => asset = loadAsset);

//			if(asset == null)
//			{
//				_result?.Invoke(null, null);
//				return;
//			}
//			_result?.Invoke(assetBundleCash, assetObjectCash);
//		}
//		private IEnumerator DownloadBundleAssetAsync(string bundleName, string assetName, Action<float> progress, Action<AssetBundleCash, AssetObjectCash> _result)
//		{
//			if(string.IsNullOrWhiteSpace(bundleName) || string.IsNullOrWhiteSpace(assetName))
//			{
//				_result?.Invoke(null, null);
//				yield break;
//			}

//			float progressValue = 0f;
//			AssetBundleCash assetBundleCash = null;
//			yield return DownloadBundleAsync(bundleName,
//				  value => progress?.Invoke(value * 0.5f + progressValue),
//				  loadCash => assetBundleCash = loadCash);

//			if(assetBundleCash == null || assetBundleCash.AssetBundle == null)
//			{
//				_result?.Invoke(null, null);
//				yield break;
//			}

//			List<AssetObjectCash> objectCashList = assetBundleCash.AssetObjectCashList;
//			AssetObjectCash assetObjectCash = objectCashList.Find(cash => cash.AssetName == assetName);

//			if(assetObjectCash == null)
//			{
//				assetObjectCash = new AssetObjectCash() {
//					AssetName = assetName,
//					AssetObject = null,
//					OnResult = null,
//					OnProgress = null,
//					LoadState = LoadState.None,
//				};
//				assetBundleCash.AssetObjectCashList.Add(assetObjectCash);
//			}

//			progressValue = 0.5f;
//			Object asset = null;
//			yield return assetObjectCash.DownloadAsync(assetBundleCash.AssetBundle,
//				value => progress?.Invoke(value * 0.5f + progressValue),
//				loadAsset => asset = loadAsset);

//			if(asset == null)
//			{
//				_result?.Invoke(null, null);
//				yield break;
//			}
//			_result?.Invoke(assetBundleCash, assetObjectCash);
//		}
//		public static void UnloadAssetBundle(string bundleName)
//		{
//			Instance(Instance => {
//				if(bundleName.IsNullOrWhitespace()) return;

//				AssetBundleCash assetBundleCash = assetBundleCashList.Find(assetBundle => assetBundle.BundleName == bundleName);

//				if(assetBundleCash == null) return;

//				assetBundleCash.Unload();
//			});
//		}
//		public static void UnloadAssetObject(string bundleName, string assetName)
//		{
//			Instance(Instance => {
//				if(bundleName.IsNullOrWhitespace() || assetName.IsNullOrWhitespace()) return;

//				AssetBundleCash assetBundleCash = assetBundleCashList.Find(assetBundle => assetBundle.BundleName == bundleName);

//				if(assetBundleCash == null) return;

//				List<AssetObjectCash> list = assetBundleCash.AssetObjectCashList;

//				if(list.IsNullOrEmpty()) return;

//				var assetObjectCash = list.Find(assetObject => assetObject.AssetName == assetName);

//				if(assetObjectCash == null) return;

//				assetObjectCash.Unload();
//				list.Remove(assetObjectCash);
//				list.RemoveTo(item => item.LoadState == LoadState.Error);
//				if(list.Count == 0)
//				{
//					assetBundleCash.Unload();
//					assetBundleCashList.Remove(assetBundleCash);
//				}
//			});
//		}

//		private static void LoadBuiltinAsset<T>(MonoBehaviour target, string builtinPath, Action<float> progress, Action<T> result) where T : Object
//		{
//			Instance(Instance => {
//				if(target == null) target = Instance;
//				bool downAsync = true;
//#if UNITY_EDITOR
//				if(!UnityEditor.EditorApplication.isPlaying)
//				{
//					downAsync = false;
//				}
//#endif
//				if(downAsync)
//				{
//					Instance.StartCoroutine(Instance.DownloadBuiltinAssetAsync(builtinPath,
//					value => {
//						if(target != null) progress?.Invoke(value);
//					},
//					(BuiltinObjectCash) => {
//						if(target != null)
//						{
//							if(BuiltinObjectCash.BuiltinObject is T tAsset)
//							{
//								result?.Invoke(tAsset);
//							}
//							else
//							{
//								Debug.LogError($"Type({typeof(T).Name}) Mismatch");
//								result?.Invoke(null);
//							}
//						}
//					}));
//				}
//				else
//				{
//					Instance.DownloadBuiltinAssetNow(builtinPath,
//					value => {
//						if(target != null) progress?.Invoke(value);
//					},
//					(BuiltinObjectCash) => {
//						if(target != null)
//						{
//							if(BuiltinObjectCash.BuiltinObject is T tAsset)
//							{
//								result?.Invoke(tAsset);
//							}
//							else
//							{
//								Debug.LogError($"Type({typeof(T).Name}) Mismatch");
//								result?.Invoke(null);
//							}
//						}
//					});
//				}
//			});
//		}
//		private void DownloadBuiltinAssetNow(string builtinPath, Action<float> progress, Action<BuiltinObjectCash> _result)
//		{
//			if(builtinPath.IsNullOrWhitespace())
//			{
//				_result?.Invoke(null);
//				return;
//			}

//			BuiltinObjectCash builtinObjectCash = builtinObjectCashList.Find(builtin => builtin.BuiltinPath == builtinPath);
//			if(builtinObjectCash == null)
//			{
//				builtinObjectCash = new BuiltinObjectCash() {
//					BuiltinPath = builtinPath,
//					BuiltinObject = null,
//					OnResult = null,
//					OnProgress = null,
//					AutoUnloadTime = null,
//					LoadState = LoadState.None,
//				};
//				builtinObjectCashList.Add(builtinObjectCash);
//			}

//			Object builtin = null;
//			builtinObjectCash.DownloadNow(
//				  value => progress?.Invoke(value),
//				loadbuiltin => builtin = loadbuiltin);

//			if(builtin == null)
//			{
//				_result?.Invoke(null);
//				return;
//			}

//			_result?.Invoke(builtinObjectCash);
//		}
//		private IEnumerator DownloadBuiltinAssetAsync(string builtinPath, Action<float> progress, Action<BuiltinObjectCash> _result)
//		{
//			if(builtinPath.IsNullOrWhitespace())
//			{
//				_result?.Invoke(null);
//				yield break;
//			}

//			BuiltinObjectCash builtinObjectCash = builtinObjectCashList.Find(builtin => builtin.BuiltinPath == builtinPath);
//			if(builtinObjectCash == null)
//			{
//				builtinObjectCash = new BuiltinObjectCash() {
//					BuiltinPath = builtinPath,
//					BuiltinObject = null,
//					OnResult = null,
//					OnProgress = null,
//					AutoUnloadTime = null,
//					LoadState = LoadState.None,
//				};
//				builtinObjectCashList.Add(builtinObjectCash);
//			}

//			Object builtin = null;
//			yield return builtinObjectCash.DownloadAsync(
//				  value => progress?.Invoke(value),
//				loadbuiltin => builtin = loadbuiltin);

//			if(builtin == null)
//			{
//				_result?.Invoke(null);
//				yield break;
//			}

//			_result?.Invoke(builtinObjectCash);
//		}
//		public static void UnloadBuiltin(string builtinPath)
//		{
//			Instance(Instance => {
//				if(builtinPath.IsNullOrWhitespace()) return;

//				BuiltinObjectCash builtinObjectCash = builtinObjectCashList.Find(builtin => builtin.BuiltinPath == builtinPath);

//				if(builtinObjectCash == null) return;

//				builtinObjectCash.Unload();
//				builtinObjectCashList.Remove(builtinObjectCash);
//			});
//		}



//		public static void _AsyncInstantiate<T>(ResourcesKey loadKey, MonoBehaviour mono, Action<float> progress, Action<T> result) where T : Object
//		{
//			switch(loadKey.LoadType)
//			{
//				case eResourcesLoadType.ResourcesLoad:
//					LoadBuiltinAsset<GameObject>(mono, loadKey.FullPath,
//						value => progress?.Invoke(value),
//						loadAsset => {
//							loadAsset.SetActive(false);
//							result?.Invoke(GameObject.Instantiate(loadAsset) as T);
//						}
//						);
//					break;
//				case eResourcesLoadType.AssetBundle:
//					LoadAssetObject<T>(mono, loadKey.BundleName, loadKey.FullPath,
//						value => progress?.Invoke(value),
//						loadAsset => {
//							if(loadAsset is GameObject gameObj)
//							{
//								gameObj.SetActive(false);
//							}
//							result?.Invoke(GameObject.Instantiate(loadAsset));
//						}
//						);
//					break;
//				default:
//					result?.Invoke(null);
//					break;
//			}
//		}
//		public static void _Unload(ResourcesKey loadKey)
//		{
//			switch(loadKey.LoadType)
//			{
//				case eResourcesLoadType.ResourcesLoad:
//					UnloadBuiltin(loadKey.FullPath);
//					break;
//				case eResourcesLoadType.AssetBundle:
//					UnloadAssetObject(loadKey.BundleName, loadKey.FullPath);
//					break;
//			}
//		}
//	}
//}
