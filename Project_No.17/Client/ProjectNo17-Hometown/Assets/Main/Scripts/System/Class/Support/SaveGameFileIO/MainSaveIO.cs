using System;
using System.IO;

using BC.Base;

using UnityEngine;

namespace TFSystem.SaveIO
{
	[Serializable]
	public class MainSaveIO : ISaveLoadKeyValue, IDisposable
	{
		public MainSaveIO()
		{
			keyValue = new SaveLoadKeyValue(ConstString.MainSave);
			MainSave = this;
			SaveKeyValue = keyValue;
		}

		public static MainSaveIO MainSave { get; private set; }
		public static SaveLoadKeyValue SaveKeyValue { get; private set; }

		[SerializeField]
		private SaveLoadKeyValue keyValue;
		public SaveLoadKeyValue KeyValue { get => keyValue; set => keyValue = value; }

		internal void Save()
		{
			File.WriteAllText(ConstString.SaveFileMain, JsonUtility.ToJson(MainSaveIO.MainSave));
		}

		internal void Load()
		{
			if(File.Exists(ConstString.SaveFileMain))
			{
				keyValue ??= new SaveLoadKeyValue(ConstString.MainSave);
				keyValue.FromJson(File.ReadAllText(ConstString.SaveFileMain));
				return;
			}
			else
			{
				File.WriteAllText(ConstString.SaveFileMain, keyValue.ToJson());
			}
		}

		public void Dispose()
		{
			keyValue?.Dispose();
			keyValue = null;
		}
	}
}
