using System;
using System.IO;

using BC.Base;

namespace TFSystem.SaveIO
{
	public static class SaveGameFileIO
	{
		public static void Init()
		{
			if(!Directory.Exists(ConstString.SaveFilePath))
			{
				Directory.CreateDirectory(ConstString.SaveFilePath);
			}


			if(SaveMainIO.SaveMain == null)
			{
				SaveMainIO main = new SaveMainIO();
				main.Load();
			}
			if(SaveSlotIO.SaveSlot == null)
			{
				SaveSlotIO slot = new SaveSlotIO(20);
				slot.LoadList();
			}
		}
		public static void Save()
		{
			if(!Directory.Exists(ConstString.SaveFilePath))
			{
				Directory.CreateDirectory(ConstString.SaveFilePath);
			}
			SaveMainIO.SaveMain?.Save();

			SaveMainIO.SaveMain?.Dispose();
			SaveSlotIO.SaveSlot?.Dispose();
		}

		[Serializable]
		public class Data
		{
			public string key;
			public string data;
		}

		public static void Dispose()
		{
			SaveMainIO.SaveMain?.Dispose();
			SaveSlotIO.SaveSlot?.Dispose();
			SaveMainIO.SaveMain = null;
			SaveSlotIO.SaveSlot = null;
		}
	}
}
