using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using UnityEngine;

using Object = UnityEngine.Object;

namespace TFSystem
{
	public class SaveSlotIO : IDisposable
	{
		internal static string _AutoFileSlotData => Path.Combine(SaveGameFileIO._SaveFilePath, "Auto_{0:00}.json");
		internal static string _SaveFileSlotData => Path.Combine(SaveGameFileIO._SaveFilePath, "Save_{0:00}.json");
		internal static string _AutoFileTexture => Path.Combine(SaveGameFileIO._SaveFilePath, "Auto_{0:00}.png");
		internal static string _SaveFileTexture => Path.Combine(SaveGameFileIO._SaveFilePath, "Save_{0:00}.png");

		public const string CurrentChapter = nameof(CurrentChapter);
		public int TotalCount { get => totalCount; set => totalCount=value; }

		public static SaveSlotIO SaveSlot;
		private int totalCount;

		[SerializeField]
		private SlotID[] autoSlotList = null;
		[SerializeField]
		private SlotID[] saveSlotList = null;


		public SlotID this[bool auto, int index] {
			get {
				if(index < 0 || index >= TotalCount)
					index = Mathf.Clamp(index, 0, TotalCount - 1);
				SlotID[] list = auto ? autoSlotList : saveSlotList;
				return list[index];
			}
			set {
				if(index < 0 || index >= TotalCount)
					index = Mathf.Clamp(index, 0, TotalCount - 1);
				SlotID[] list = auto ? autoSlotList : saveSlotList;
				list[index] = value;
			}
		}

		[Serializable]
		public struct SlotID
		{
			public bool isValid;
			public bool isAutoSave;
			public int slotIndex;
			public string saveTime;
			public string saveMessage;

			public int endingCycle;
			public bool endingCheckA;
			public bool endingCheckB;
			public bool endingCheckC;
			public bool endingCheckD;
			public bool endingCheckE;

			public SlotID(bool isAutoSave, int slotIndex)
			{
				this.isValid=false;
				this.isAutoSave=isAutoSave;
				this.slotIndex = slotIndex;
				this.saveTime="";
				this.saveMessage = "비어 있음";
				this.endingCycle=0;
				this.endingCheckA=false;
				this.endingCheckB=false;
				this.endingCheckC=false;
				this.endingCheckD=false;
				this.endingCheckE=false;
			}

			public string ToFormat_SlotTimeMsg(string format)
			{
				return string.Format(format, slotIndex, saveTime, saveMessage);
			}
			public string ToListString()
			{
				if(!isValid) return "비어 있음";

				return
					$"저장 번호: {(isAutoSave ? "AutoSave_" : "")}{slotIndex:00}\n" +
					$"저장 시간: {saveTime}\n\n" +
					$"진행 중: {saveMessage}";
			}
			public string ToLongString()
			{
				if(!isValid) return "비어 있음";

				List<string> endingList = new List<string>();
				if(endingCheckA) { endingList.Add("Ending A"); }
				if(endingCheckB) { endingList.Add("Ending B"); }
				if(endingCheckC) { endingList.Add("Ending C"); }
				if(endingCheckD) { endingList.Add("Ending D"); }
				if(endingCheckE) { endingList.Add("Ending E"); }

				return
					$"저장 번호: {(isAutoSave ? "AutoSave_" : "")}{slotIndex:00}\n" +
					$"저장 시간: {saveTime}\n" +
					$"진행 중: {saveMessage}\n\n" +
					(endingCycle > 0 ? $"회차: {endingCycle} 회\n" : "") +
					((endingList.Count > 0) ? $"획득 엔딩: {string.Join(" | ", endingList)}\n" : "")
					;
			}

			public async Awaitable SaveData(Texture2D texture, SlotData data)
			{
				if(slotIndex <0) return;
				string path = string.Format(isAutoSave ? _AutoFileSlotData : _SaveFileSlotData, slotIndex);
				string pathTexture = string.Format(isAutoSave ? _AutoFileTexture : _SaveFileTexture, slotIndex);

				string _id = JsonUtility.ToJson(this);
				string _data = JsonUtility.ToJson(data);
				byte[] _texture = texture.EncodeToPNG();
				await Awaitable.BackgroundThreadAsync();
				File.WriteAllText(path, $"{_id}\n{_data}");
				File.WriteAllBytes(pathTexture, _texture);
				await Awaitable.MainThreadAsync();

				saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
				saveMessage = data.GetData<string>(CurrentChapter);
			}
			public async Awaitable<SlotData> LoadData()
			{
				if(slotIndex <0) return null;
				string path = string.Format(isAutoSave ? _AutoFileSlotData : _SaveFileSlotData, slotIndex);
				if(!File.Exists(path)) return null;
				await Awaitable.BackgroundThreadAsync();
				string data="";
				using(var reader = new StreamReader(path))
				{
					// 첫 번째 줄(SlotID) 읽고 버리기
					reader.ReadLine();
					data = reader.ReadToEnd();
				}
				await Awaitable.MainThreadAsync();

				SlotData slotData = null;
				if(string.IsNullOrWhiteSpace(data))
				{
					slotData = new SlotData();
				}
				slotData = JsonUtility.FromJson<SlotData>(data);
				slotData.saveSlot = slotIndex;
				return slotData;
			}
			public async Awaitable<Texture2D> LoadTexture()
			{
				if(slotIndex <0) return null;
				string path = string.Format(isAutoSave ? _AutoFileTexture : _SaveFileTexture, slotIndex);
				if(!File.Exists(path)) return null;
				await Awaitable.BackgroundThreadAsync();
				byte[] _texture = File.ReadAllBytes(path);
				await Awaitable.MainThreadAsync();
				Texture2D texture = new Texture2D(2, 2);
				texture.LoadImage(_texture);
				return texture;
			}
			internal async Awaitable DeleteData()
			{
				if(slotIndex <0) return;
				string path = string.Format(isAutoSave ? _AutoFileSlotData : _SaveFileSlotData, slotIndex);
				string pathTexture = string.Format(isAutoSave ? _AutoFileTexture : _SaveFileTexture, slotIndex);
				string _id = JsonUtility.ToJson(this);
				await Awaitable.BackgroundThreadAsync();
				if(File.Exists(path)) File.Delete(path);
				if(File.Exists(pathTexture)) File.Delete(pathTexture);
				await Awaitable.MainThreadAsync();
			}
		}

		[Serializable]
		public class SlotData
		{
			public int saveSlot;

			[SerializeField]
			private SaveLoadKeyValue KeyValue = new SaveLoadKeyValue();

			public void SetData<T>(string key, T value) => KeyValue.SetData<T>(key, value);
			public bool HasData(string key) => KeyValue.HasData(key);
			public bool DeleteData(string key) => KeyValue.DeleteData(key);
			public T GetData<T>(string key, T defaultValue = default) => KeyValue.GetData(key, defaultValue);
			public bool TryGetData<T>(string key, out T tValue) => KeyValue.TryGetData(key, out tValue);
			public bool TryNotGetData<T>(string key, out T tValue) => KeyValue.TryNotGetData(key, out tValue);
			public T PopData<T>(string key, T defaultValue = default) => KeyValue.PopData(key, defaultValue);
			public bool TryPopData<T>(string key, out T tValue) => KeyValue.TryPopData(key, out tValue);
			public bool TryNotPopData<T>(string key, out T tValue) => KeyValue.TryNotPopData(key, out tValue);

			public void SetData<T>(T value) => SetData(typeof(T).FullName, value);
			public bool HasData<T>() => HasData(typeof(T).FullName);
			public bool DeleteData<T>() => DeleteData(typeof(T).FullName);
			public T GetData<T>(T defaultValue = default) => GetData<T>(typeof(T).FullName, defaultValue);
			public bool TryGetData<T>(out T tValue) => TryGetData(typeof(T).FullName, out tValue);
			public bool TryNotGetData<T>(out T tValue) => TryNotGetData(typeof(T).FullName, out tValue);
			public T PopData<T>(T defaultValue = default) => PopData<T>(typeof(T).FullName, defaultValue);
			public bool TryPopData<T>(out T tValue) => TryPopData(typeof(T).FullName, out tValue);
			public bool TryNotPopData<T>(out T tValue) => TryNotPopData(typeof(T).FullName, out tValue);
		}
		public void LoadList(int? _totalCount = null)
		{
			TotalCount = _totalCount ?? TotalCount;
			if(TotalCount <= 0) TotalCount = 1;

			autoSlotList = new SlotID[TotalCount];
			saveSlotList = new SlotID[TotalCount];

			for(int i = 0 ; i < TotalCount ; i++)
			{
				string pathAuto = string.Format(_AutoFileSlotData, i);
				string pathSave = string.Format(_SaveFileSlotData, i);

				if(File.Exists(pathAuto))
				{
					using(var reader = new StreamReader(pathAuto))
					{
						string line = reader.ReadLine();
						if(string.IsNullOrWhiteSpace(line))
						{
							autoSlotList[i] = new SlotID(true, i);
							autoSlotList[i].isValid = false;
						}
						else
						{
							autoSlotList[i] = JsonUtility.FromJson<SlotID>(line);
							autoSlotList[i].isValid = true;
						}
					}
				}
				else
				{
					autoSlotList[i] = new SlotID(true, i);
					autoSlotList[i].isValid = false;
				}
				if(File.Exists(pathSave))
				{
					using(var reader = new StreamReader(pathSave))
					{
						string line = reader.ReadLine();
						if(string.IsNullOrWhiteSpace(line))
						{
							saveSlotList[i] = new SlotID(false, i);
							autoSlotList[i].isValid = false;
						}
						else
						{
							saveSlotList[i] = JsonUtility.FromJson<SlotID>(line);
							autoSlotList[i].isValid = true;
						}
					}
				}
				else
				{
					saveSlotList[i] = new SlotID(false, i);
					autoSlotList[i].isValid = false;
				}
			}
		}

		public async Awaitable SaveData(SlotID slotID, Texture2D texture, SlotData data)
		{
			await slotID.SaveData(texture, data);
			LoadList();
		}
		public async Awaitable SaveData(SlotID slotID, Camera camera, SlotData data)
		{
			if(camera == null) return;

			int imageWidth = Screen.width/4;   // 화면 너비
			int imageHeight = Screen.height/4; // 화면 높이

			RenderTexture renderTexture = new RenderTexture(imageWidth, imageHeight, 24);
			camera.targetTexture = renderTexture;

			RenderTexture.active = renderTexture;
			await Awaitable.EndOfFrameAsync();
			camera.Render();

			Texture2D screenshot = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);
			screenshot.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
			screenshot.Apply();

			await SaveData(slotID, screenshot, data);

			camera.targetTexture = null;
			RenderTexture.active = null;
			Object.Destroy(renderTexture);
			Object.Destroy(screenshot);
		}
		public async Awaitable<SlotData> LoadData(SlotID slotID)
		{
			return await slotID.LoadData();
		}
		public async Awaitable<Texture2D> LoadTexture(SlotID slotID)
		{
			return await slotID.LoadTexture();
		}
		public async Task DeleteData(SlotID slotID)
		{
			await slotID.DeleteData();
			saveSlotList[slotID.slotIndex] = new SlotID(slotID.isAutoSave, slotID.slotIndex) { saveTime = "", saveMessage = "비어 있음" };
			LoadList();
		}

		public void Dispose()
		{
			autoSlotList = null;
			saveSlotList = null;
		}

	}
}
