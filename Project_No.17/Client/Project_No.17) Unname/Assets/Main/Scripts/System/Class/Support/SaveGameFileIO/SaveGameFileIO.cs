using System;
using System.IO;

using UnityEngine;

namespace TFSystem
{
	public static class SaveGameFileIO
	{
		internal static string _SaveFilePath = Path.Combine(Application.persistentDataPath, "SaveFile");
		internal static string _SaveFileMain => Path.Combine(_SaveFilePath, "Main.json");
		internal static string _SaveFileSlotList => Path.Combine(_SaveFilePath, "Slot.json");

		public static void Init()
		{
			if(!Directory.Exists(_SaveFilePath))
			{
				Directory.CreateDirectory(_SaveFilePath);
			}

			MainSaveIO.MainSave = new MainSaveIO();
			MainSaveIO.MainSave.Load();
			SaveSlotIO.SaveSlot = new SaveSlotIO();
			SaveSlotIO.SaveSlot.LoadList(20);
		}
		public static void Save()
		{
			if(!Directory.Exists(_SaveFilePath))
			{
				Directory.CreateDirectory(_SaveFilePath);
			}
			MainSaveIO.MainSave.Save();

			MainSaveIO.MainSave.Dispose();
			SaveSlotIO.SaveSlot.Dispose();
		}

		[Serializable]
		public class Data
		{
			public string key;
			public string data;
		}
	}
}
