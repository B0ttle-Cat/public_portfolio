using System;
using System.IO;

using BC.Base;

using UnityEngine;

namespace TFSystem.SaveIO
{
	[Serializable]
	public class SaveMainIO : ISaveLoadKeyValue, IDisposable
	{
		public SaveMainIO()
		{
			keyValue = new SaveLoadKeyValue(ConstString.MainSave);
			SaveMain = this;
			SaveKeyValue = keyValue;
		}

		public static SaveMainIO SaveMain { get; internal set; }
		public static SaveLoadKeyValue SaveKeyValue { get; private set; }

		[SerializeField]
		private SaveLoadKeyValue keyValue;
		public SaveLoadKeyValue KeyValue { get => keyValue; set => keyValue = value; }

		public void Save()
		{
			File.WriteAllText(ConstString.SaveFileMain, JsonUtility.ToJson(SaveMainIO.SaveMain));
		}
		public async Awaitable SaveAsync()
		{
			await File.WriteAllTextAsync(ConstString.SaveFileMain, JsonUtility.ToJson(SaveMainIO.SaveMain));
		}

		public void Load()
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
		public async Awaitable LoadAsync()
		{
			if(File.Exists(ConstString.SaveFileMain))
			{
				keyValue ??= new SaveLoadKeyValue(ConstString.MainSave);
				string readAll = await File.ReadAllTextAsync(ConstString.SaveFileMain);
				keyValue.FromJson(readAll);
				return;
			}
			else
			{
				await File.WriteAllTextAsync(ConstString.SaveFileMain, keyValue.ToJson());
			}
		}
		public void Dispose()
		{
			keyValue?.Dispose();
			keyValue = null;
		}
	}
}
