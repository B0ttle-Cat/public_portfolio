using System;
using System.Collections.Generic;
using System.IO;

using BC.Base;

using UnityEngine;

using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace TFSystem.SaveIO
{
	public class SaveSlotIO : IDisposable
	{
		public SaveSlotIO(int totalCount)
		{
			this.totalCount = totalCount;
			SaveSlot = this;
		}
		public int TotalCount { get => totalCount; set => totalCount=value; }

		public static SaveSlotIO SaveSlot { get; internal set; }
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
			public bool IsEmptySlot => !string.IsNullOrWhiteSpace(saveTime);
			public bool isAutoSave;
			public int slotIndex;
			public string saveTime;
			public string saveMessage;
			public string episodeKey;
			public string chapterKey;
			public string episodeName;
			public string chapterName;

			public SlotID(bool isAutoSave, int slotIndex)
			{
				this.isAutoSave=isAutoSave;
				this.slotIndex = slotIndex;
				this.saveTime="";
				this.saveMessage = "비어 있음";
				this.episodeKey = "";
				this.chapterKey = "";
				this.episodeName = "";
				this.chapterName = "";
			}

			public string ToFormat_SlotTimeMsg(string format)
			{
				return string.Format(format, slotIndex, saveTime, saveMessage);
			}
			public string ToListString()
			{
				if(!IsEmptySlot) return "비어 있음";

				return
					$"저장 번호: {(isAutoSave ? "AutoSave_" : "")}{slotIndex:00}\n" +
					$"저장 시간: {saveTime}";
			}
			public string ToLongString()
			{
				if(!IsEmptySlot) return "비어 있음";

				List<string> endingList = new List<string>();

				return
					$"저장 번호: {(isAutoSave ? "AutoSave_" : "")}{slotIndex:00}\n" +
					$"저장 시간: {saveTime}\n" +
					$"진행 중: {episodeKey}_{episodeName} 편 | {chapterKey}_{chapterName} 장\n" +
					$"노트: {saveMessage}";
			}

			public async Awaitable SaveData(Texture2D texture, SlotData data)
			{
				if(slotIndex <0) return;

				string path = string.Format(isAutoSave ? ConstString.AutoFileSlotData : ConstString.SaveFileSlotData, slotIndex);
				string pathTexture = string.Format(isAutoSave ? ConstString.AutoFileTexture : ConstString.SaveFileTexture, slotIndex);

				saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

				ISaveLoadKeyValue saveLoadKeyValue = data;
				saveLoadKeyValue.SetData<SlotID>(this);

				string _id = JsonUtility.ToJson(this);
				string _data = data.ToJson();
				byte[] _texture = texture.EncodeToPNG();
				await Awaitable.BackgroundThreadAsync();
				File.WriteAllText(path, $"{_id}\n{_data}");
				File.WriteAllBytes(pathTexture, _texture);
				Debug.Log($"SaveDataPath: {path}");
				await Awaitable.MainThreadAsync();
			}
			public async Awaitable<SlotData> LoadData()
			{
				if(slotIndex <0) return null;
				string path = string.Format(isAutoSave ? ConstString.AutoFileSlotData : ConstString.SaveFileSlotData, slotIndex);
				if(!File.Exists(path)) return null;
				await Awaitable.BackgroundThreadAsync();
				string data="";
				using(var reader = new StreamReader(path))
				{
					// 첫 번째 줄(SlotID) 읽기
					reader.ReadLine();
					// 나머지 줄(SlotData) 읽기
					data = reader.ReadToEnd();
				}
				await Awaitable.MainThreadAsync();

				// SlotData 생성
				SlotData slotData = null;
				if(!string.IsNullOrWhiteSpace(data))
				{
					slotData = new SlotData(slotIndex);
					slotData.FromJson(data);
					ISaveLoadKeyValue saveLoadKeyValue = slotData;
				}
				return slotData;
			}
			public async Awaitable<Texture2D> LoadTexture()
			{
				if(slotIndex <0) return null;
				string path = string.Format(isAutoSave ? ConstString.AutoFileTexture : ConstString.SaveFileTexture, slotIndex);
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
				string path = string.Format(isAutoSave ? ConstString.AutoFileSlotData : ConstString.SaveFileSlotData, slotIndex);
				string pathTexture = string.Format(isAutoSave ? ConstString.AutoFileTexture : ConstString.SaveFileTexture, slotIndex);
				string _id = JsonUtility.ToJson(this);
				await Awaitable.BackgroundThreadAsync();
				if(File.Exists(path)) File.Delete(path);
				if(File.Exists(pathTexture)) File.Delete(pathTexture);
				await Awaitable.MainThreadAsync();
			}
		}

		[Serializable]
		public class SlotData : ISaveLoadKeyValue
		{
			public int saveSlot;

			[SerializeField]
			private SaveLoadKeyValue keyValue = null;

			public SlotData(int saveSlot)
			{
				this.saveSlot = saveSlot;
				keyValue = new SaveLoadKeyValue($"{ConstString.SaveSlot}_{saveSlot:00}");
			}

			SaveLoadKeyValue ISaveLoadKeyValue.KeyValue { get => keyValue; set => keyValue = value; }

			public string ToJson() => keyValue.ToJson();
			public void FromJson(string jsonData) =>
				keyValue.FromJson(jsonData);
		}
		public void LoadList(int? _totalCount = null)
		{
			TotalCount = _totalCount ?? TotalCount;
			if(TotalCount <= 0) TotalCount = 1;

			autoSlotList = new SlotID[TotalCount];
			saveSlotList = new SlotID[TotalCount];

			for(int i = 0 ; i < TotalCount ; i++)
			{
				string pathAuto = string.Format(ConstString.AutoFileSlotData, i);
				string pathSave = string.Format(ConstString.SaveFileSlotData, i);

				if(File.Exists(pathAuto))
				{
					using(var reader = new StreamReader(pathAuto))
					{
						string line = reader.ReadLine();
						if(string.IsNullOrWhiteSpace(line))
						{
							autoSlotList[i] = new SlotID(true, i);
						}
						else
						{
							autoSlotList[i] = JsonUtility.FromJson<SlotID>(line);
						}
					}
				}
				else
				{
					autoSlotList[i] = new SlotID(true, i);
				}
				if(File.Exists(pathSave))
				{
					using(var reader = new StreamReader(pathSave))
					{
						string line = reader.ReadLine();
						if(string.IsNullOrWhiteSpace(line))
						{
							saveSlotList[i] = new SlotID(false, i);
						}
						else
						{
							saveSlotList[i] = JsonUtility.FromJson<SlotID>(line);
						}
					}
				}
				else
				{
					saveSlotList[i] = new SlotID(false, i);
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
		public async Awaitable DeleteData(SlotID slotID)
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
