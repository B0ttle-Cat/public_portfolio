using System;
using System.Collections.Generic;
using System.IO;

using BC.Base;

using UnityEngine;

namespace TFSystem
{
	[Serializable]
	public class MainSaveIO : IDisposable
	{
		internal static string _SaveFileMain => Path.Combine(SaveGameFileIO._SaveFilePath, "Main.json");

		public const string ContinueSlot = nameof(ContinueSlot);
		public const string ContentState_NeverEndingPlay = nameof(ContentState_NeverEndingPlay);
		public const string ContentState_Close = "Close";
		public const string ContentState_FirstOpen = "FirstOpen";
		public const string ContentState_Open = "Open";

		public static MainSaveIO MainSave;

		[SerializeField]
		private List<SaveGameFileIO.Data> KeyValue = new List<SaveGameFileIO.Data>();
		public string this[string key] {
			get {
				for(int i = 0 ; i<KeyValue.Count ; i++)
				{
					SaveGameFileIO.Data item = KeyValue[i];
					if(item.key == key)
					{
						return item.data;
					}
				}
				return null;
			}
			set {
				bool valueIsNull = value.IsNullOrWhiteSpace();
				for(int i = 0 ; i<KeyValue.Count ; i++)
				{
					SaveGameFileIO.Data item = KeyValue[i];
					if(item.key == key)
					{
						if(valueIsNull)
						{
							KeyValue.RemoveAt(i);
						}
						else
						{
							item.data = value;
						}
						return;
					}
				}
				if(!valueIsNull)
					KeyValue.Add(new SaveGameFileIO.Data() { key = key, data = value });
			}
		}

		internal void Save()
		{
			File.WriteAllText(_SaveFileMain, JsonUtility.ToJson(MainSaveIO.MainSave));
		}

		internal void Load()
		{
			if(File.Exists(_SaveFileMain))
			{
				var load = JsonUtility.FromJson<MainSaveIO>(File.ReadAllText(_SaveFileMain));
				KeyValue = new List<SaveGameFileIO.Data>();
				KeyValue.AddRange(load.KeyValue);
				load.Dispose();
				return;
			}
			else
			{
				File.WriteAllText(_SaveFileMain, JsonUtility.ToJson(MainSaveIO.MainSave));
			}
		}

		public void Dispose()
		{
			if(KeyValue != null) KeyValue.Clear();
			KeyValue = null;
		}
	}
}
