using System.IO;

using UnityEngine;

namespace BC.Base
{
	public static partial class ConstString //.TFSystem
	{
		#region ContentLoad
		// 게임 시작시 사용할 저장 슬롯 호출 키
		public const string CurrentSlotID = nameof(CurrentSlotID);
		// 게임 시작시 사용할 에피소드 호출 키
		public const string CurrentEpisodeName = nameof(CurrentEpisodeName);

		// 새 게임 시작시 실행되는 에피소드 이름
		public const string EpisodeName_NewGame = "Ep_00";
		#endregion

		#region MainSaveIO
		public const string MainSave = nameof(MainSave);
		// 게임 계속 하기에 사용할 저장 슬롯 호출 키
		public const string MainContentKey_ContinueGamePlay = nameof(MainContentKey_ContinueGamePlay);
		// 무한 모드 컨텐츠 Open 상태키
		public const string MainContentOpenState_NeverEndingPlay = nameof(MainContentOpenState_NeverEndingPlay);
		// 조합키로 사용. 해당 컨텐츠가 열리지 않음.
		public const string MainContentState_Close = "Close";
		// 조합키로 사용. 해당 컨텐츠가 이번에 처음 열린 것.
		public const string MainContentState_FirstOpen = "FirstOpen";
		// 조합키로 사용. 해당 컨텐츠가 열려 있던 것.
		public const string MainContentState_Open = "Open";

		// 최근에 저장 슬롯 리스트(뷰)가 자동 저장 리스트인지 확인하기 위한 키
		public const string SelectAutoSaveList = nameof(SelectAutoSaveList);
		#endregion
		#region SaveSlotIO
		public const string SaveSlot = nameof(SaveSlot);
		public static string SaveFilePath = Path.Combine(Application.persistentDataPath, "SaveFile");
		public static string SaveFileMain = Path.Combine(SaveFilePath, "Main.json");

		public static string AutoFileSlotData = Path.Combine(SaveFilePath, "AutoFile_{0:00}.json");
		public static string SaveFileSlotData = Path.Combine(SaveFilePath, "SaveFile_{0:00}.json");
		public static string AutoFileTexture = Path.Combine(SaveFilePath, "AutoFile_{0:00}.png");
		public static string SaveFileTexture = Path.Combine(SaveFilePath, "SaveFile_{0:00}.png");
		#endregion


		public static class ContentString
		{
			public const string 공용변환물질 = "공용 변환 물질";
			public const string 공용보유전력 = "공용 보유 전력";

			public const string StatusBarStringFormat = "{0}<size=50%>/{1}";
		}
	}
}
