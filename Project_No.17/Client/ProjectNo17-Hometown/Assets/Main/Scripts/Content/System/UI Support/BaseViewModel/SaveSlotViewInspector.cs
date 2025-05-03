using Sirenix.OdinInspector;

using TFSystem;
using TFSystem.SaveIO;

using TMPro;

using UnityEngine;
using UnityEngine.UI;
namespace TFContent
{
	public class SaveSlotViewInspector : UIInspector
	{
		public Button closeSaveSlotButton;
		[Space]
		public RawImage selectSlotImage;
		public TMP_Text selectSlotText;
		public Texture emptySlotTexture;
		[Space]
		public Button saveGameButton;
		public Button startGameButton;
		public Button deleteSaveButton;
		[Title("ShowAutoSaveList")]
		public Button showAutoSaveList;
		[Space]
		public ScrollToggleListView<SaveSlotIO.SlotID> autoSlotListView;
		[Title("ShowGameSaveList")]
		public Button showGameSaveList;
		[Space]
		public ScrollToggleListView<SaveSlotIO.SlotID> saveSlotListView;
	}
}