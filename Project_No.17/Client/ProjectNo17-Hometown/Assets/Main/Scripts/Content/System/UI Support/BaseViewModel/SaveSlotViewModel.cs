using BC.Base;

using Sirenix.OdinInspector;

using TFSystem;
using TFSystem.SaveIO;
using TFSystem.UI;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace TFContent
{
	public abstract class SaveSlotViewModel : UIViewModelComponent
	{
		[HideInEditorMode,SerializeField] protected Button closeSaveSlotButton;
		[Space]
		[HideInEditorMode,SerializeField] protected RawImage selectSlotImage;
		[HideInEditorMode,SerializeField] protected TMP_Text selectSlotText;
		[HideInEditorMode,SerializeField] protected Texture emptySlotTexture;
		[Space]
		[HideInEditorMode,SerializeField] protected Button saveGameButton;
		[HideInEditorMode,SerializeField] protected Button startGameButton;
		[HideInEditorMode,SerializeField] protected Button deleteSaveButton;
		[Title("ShowAutoSaveList")]
		[HideInEditorMode,SerializeField] protected Button showAutoSaveList;
		[Space]
		[HideInEditorMode,SerializeField] protected ScrollToggleListView<SaveSlotIO.SlotID> autoSlotListView;
		[Title("ShowGameSaveList")]
		[HideInEditorMode,SerializeField] protected Button showGameSaveList;
		[Space]
		[HideInEditorMode,SerializeField] protected ScrollToggleListView<SaveSlotIO.SlotID> saveSlotListView;

		[Space, ReadOnly]
		private bool selectAutoSaveList = false;
		[SerializeField, ReadOnly]
		protected SaveSlotIO.SlotID currentSelectSlotID;

		protected override void AwakeUIView(ref ViewItemSetter viewItemSetter)
		{
			if(ThisContainer.TryGetComponent<SaveSlotViewInspector>(out var uiInspector))
			{
				SetUIInspector(uiInspector);
			}


			closeSaveSlotButton.onClick.AddListener(async () => await WaitOnAction(OnCloseSaveSlotButton));
			saveGameButton.onClick.AddListener(async () => await WaitOnAction(OnSaveGameButton));
			startGameButton.onClick.AddListener(async () => await WaitOnAction(OnStartGameButton));
			deleteSaveButton.onClick.AddListener(async () => await WaitOnAction(OnDeleteSaveButton));

			autoSlotListView.onSetValue = (index) => SaveSlotIO.SaveSlot[true, index];
			autoSlotListView.onShowItem = OnShowItem;
			autoSlotListView.onHideItem = OnHideItem;
			autoSlotListView.TotalItemCount = SaveSlotIO.SaveSlot.TotalCount;
			viewItemSetter.Add(autoSlotListView, nameof(autoSlotListView));

			saveSlotListView.onSetValue = (index) => SaveSlotIO.SaveSlot[false, index];
			saveSlotListView.onShowItem = OnShowItem;
			saveSlotListView.onHideItem = OnHideItem;
			saveSlotListView.TotalItemCount = SaveSlotIO.SaveSlot.TotalCount;
			viewItemSetter.Add(saveSlotListView, nameof(saveSlotListView));

			showAutoSaveList.onClick.AddListener(async () => await WaitOnAction(OnShowAutoSaveList));
			showGameSaveList.onClick.AddListener(async () => await WaitOnAction(OnShowGameSaveList));

			selectSlotImage.texture = null;

			void SetUIInspector(SaveSlotViewInspector uiInspector)
			{
				closeSaveSlotButton = uiInspector.closeSaveSlotButton;
				selectSlotImage =     uiInspector.selectSlotImage;
				selectSlotText =      uiInspector.selectSlotText;
				emptySlotTexture =    uiInspector.emptySlotTexture;
				saveGameButton =      uiInspector.saveGameButton;
				startGameButton =     uiInspector.startGameButton;
				deleteSaveButton =    uiInspector.deleteSaveButton;
				showAutoSaveList =    uiInspector.showAutoSaveList;
				autoSlotListView =    uiInspector.autoSlotListView;
				showGameSaveList =    uiInspector.showGameSaveList;
				saveSlotListView =    uiInspector.saveSlotListView;

				uiInspector.DestroyThis();
			}
		}
		protected override async Awaitable OnShowUIView()
		{
			if(!SaveMainIO.SaveKeyValue.TryGetData(ConstString.SelectAutoSaveList, out selectAutoSaveList))
			{
				selectAutoSaveList = false;
			}

			SaveSlotIO.SaveSlot.LoadList();
			await OnSelectSaveSlot(new SaveSlotIO.SlotID(false, -1));

			await AwaitableUtility.ParallelWaitAll(
				base.OnShowUIView(),
				SwitchListView()
			);
		}

		protected override async Awaitable OnHideUIView()
		{
			await base.OnHideUIView();

			autoSlotListView.Deinit();
			saveSlotListView.Deinit();
		}

		async Awaitable SwitchListView()
		{
			await Awaitable.EndOfFrameAsync();
			if(selectAutoSaveList)
			{
				await AwaitableUtility.ParallelWaitAll(
				autoSlotListView.OnShowView(),
				saveSlotListView.OnHideView());
			}
			else
			{
				await AwaitableUtility.ParallelWaitAll(
				autoSlotListView.OnHideView(),
				saveSlotListView.OnShowView());
			}
		}

		protected virtual async Awaitable OnCloseSaveSlotButton()
		{
			if(ThisContainer.TryGetObject<IUIViewController<GamePlayViewState>>(out var view))
			{
				await view.OnChangeViewState(view.LastViewStateStack);
			}
		}
		protected abstract Awaitable OnSaveGameButton();
		protected abstract Awaitable OnStartGameButton();
		protected virtual async Awaitable OnDeleteSaveButton()
		{
			if(currentSelectSlotID.slotIndex < 0) return;
			await SaveSlotIO.SaveSlot.DeleteData(currentSelectSlotID);

			await SwitchListView();
			await OnSelectSaveSlot(new SaveSlotIO.SlotID(false, -1));
		}
		protected virtual async Awaitable OnShowAutoSaveList()
		{
			selectAutoSaveList = true;
			SaveMainIO.SaveKeyValue.SetData(ConstString.SelectAutoSaveList, selectAutoSaveList);
			await SwitchListView();
		}
		protected virtual async Awaitable OnShowGameSaveList()
		{
			selectAutoSaveList = false;
			SaveMainIO.SaveKeyValue.SetData(ConstString.SelectAutoSaveList, selectAutoSaveList);
			await SwitchListView();
		}
		protected virtual async Awaitable OnSelectSaveSlot(SaveSlotIO.SlotID slotID)
		{
			currentSelectSlotID = slotID;
			if(currentSelectSlotID.slotIndex < 0)
			{
				if(selectSlotImage.texture != null && selectSlotImage.texture != emptySlotTexture)
				{
					Destroy(selectSlotImage.texture);
				}
				selectSlotImage.texture = emptySlotTexture;
				selectSlotText.text = "저장 파일을 선택하세요.";

				saveGameButton.interactable = false;
				startGameButton.interactable = false;
				deleteSaveButton.interactable = false;
				return;
			}
			else
			{
				var texture = await slotID.LoadTexture();
				if(selectSlotImage.texture != null && selectSlotImage.texture != emptySlotTexture)
				{
					Destroy(selectSlotImage.texture);
				}
				selectSlotImage.texture = texture != null ? texture : emptySlotTexture;
				selectSlotText.text = slotID.ToLongString();

				saveGameButton.interactable = !currentSelectSlotID.isAutoSave;
				startGameButton.interactable = true;
				deleteSaveButton.interactable = true;
			}
		}
		private void OnShowItem(RectTransform rectTransform, SaveSlotIO.SlotID value)
		{
			Toggle toggle = rectTransform.GetComponentInChildren<Toggle>(true);
			if(toggle != null)
			{
				toggle.onValueChanged.RemoveAllListeners();
				toggle.onValueChanged.AddListener(async (isOn) => {
					if(isOn)
					{
						await WaitOnAction(OnSelectSaveSlot(value));
					}
				});
			}

			TMP_Text text = rectTransform.GetComponentInChildren<TMP_Text>(true);
			if(text != null)
			{
				text.text = value.ToListString();
			}
		}
		private void OnHideItem(RectTransform rectTransform, SaveSlotIO.SlotID value)
		{
			Toggle toggle = rectTransform.GetComponentInChildren<Toggle>(true);
			if(toggle != null)
			{
				toggle.onValueChanged.RemoveAllListeners();
			}
			TMP_Text text = rectTransform.GetComponentInChildren<TMP_Text>(true);
			if(text != null)
			{
				text.text = "비어 있음";
			}
		}
	}
}
