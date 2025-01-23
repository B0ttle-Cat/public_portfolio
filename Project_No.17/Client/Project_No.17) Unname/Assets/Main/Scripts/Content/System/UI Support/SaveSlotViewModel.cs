using System;
using System.Collections.Generic;

using BC.Base;

using Sirenix.OdinInspector;

using TFSystem;
using TFSystem.UI;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace TFContent
{
	public abstract class SaveSlotViewModel : UIViewModelComponent
	{
		[SerializeField] protected Button closeSaveSlotButton;
		[Space]
		[SerializeField] protected RawImage selectSlotImage;
		[SerializeField] protected TMP_Text selectSlotText;
		[SerializeField] protected Texture emptySlotTexture;
		[Space]
		[SerializeField] protected Button saveGameButton;
		[SerializeField] protected Button startGameButton;
		[SerializeField] protected Button deleteSaveButton;
		[Title("ShowAutoSaveList")]
		[SerializeField] protected Button showAutoSaveList;
		[Space]
		[SerializeField] protected ScrollToggleListView<SaveSlotIO.SlotID> autoSlotListView;
		[Title("ShowGameSaveList")]
		[SerializeField] protected Button showGameSaveList;
		[Space]
		[SerializeField] protected ScrollToggleListView<SaveSlotIO.SlotID> saveSlotListView;
		[Space, ReadOnly]
		private bool selectAutoSaveList = false;
		[ReadOnly]
		private bool onClick;

		protected SaveSlotIO.SlotID currentSelectSlotID;

		protected override void AwakeUIView(ref ViewItemSetter viewItemSetter)
		{
			onClick = false;

			closeSaveSlotButton.onClick.AddListener(async () => await BlockOnClick(OnCloseSaveSlotButton()));
			saveGameButton.onClick.AddListener(async () => await BlockOnClick(OnSaveGameButton()));
			startGameButton.onClick.AddListener(async () => await BlockOnClick(OnStartGameButton()));
			deleteSaveButton.onClick.AddListener(async () => await BlockOnClick(OnDeleteSaveButton()));

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

			showAutoSaveList.onClick.AddListener(async () => await BlockOnClick(OnShowAutoSaveList()));
			showGameSaveList.onClick.AddListener(async () => await BlockOnClick(OnShowGameSaveList()));

			selectSlotImage.texture = null;
		}

		protected override async Awaitable OnShowUIView()
		{
			if(!bool.TryParse(MainSaveIO.MainSave["SelectAutoSaveList"], out selectAutoSaveList))
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


		async Awaitable BlockOnClick(Awaitable awaitable)
		{
			if(onClick) return;
			onClick = true;
			await awaitable;
			onClick = false;
		}
		void BlockOnClick(Action action)
		{
			if(onClick) return;
			onClick = true;
			action?.Invoke();
			onClick = false;
		}

		protected abstract Awaitable OnCloseSaveSlotButton();
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
			MainSaveIO.MainSave["SelectAutoSaveList"] = selectAutoSaveList.ToString();
			await SwitchListView();
		}
		protected virtual async Awaitable OnShowGameSaveList()
		{
			selectAutoSaveList = false;
			MainSaveIO.MainSave["SelectAutoSaveList"] = selectAutoSaveList.ToString();
			await SwitchListView();
		}
		protected virtual async Awaitable OnSelectSaveSlot(SaveSlotIO.SlotID slotID)
		{
			currentSelectSlotID = slotID;
			if(currentSelectSlotID.slotIndex < 0 || !currentSelectSlotID.isValid)
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
						await BlockOnClick(OnSelectSaveSlot(value));
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

	[Serializable, InlineProperty, HideLabel]
	public class ScrollToggleListView<T> : UIViewItem, UIBinding<T>, UIEvent_OnChangeValue<T>
	{
		[SerializeField]
		private ScrollRect scrollRect;
		private RectTransform content;
		private RectTransform viewport;
		private ToggleGroup toggleGroup;
		[SerializeField]
		private RectTransform scrollItem;

		[SerializeField]
		private float spacing;
		private float itemHeight;
		private int totalItemCount;


		private int firstVisibleIndex;
		private Vector2 contentPosYRange;

		private RectTransform[] showItems;
		private T[] tValues;

		public Func<int, T> onSetValue;
		public Action<RectTransform, T> onShowItem;
		public Action<RectTransform, T> onHideItem;

		public string ViewItemName { get; set; }
		public bool interaction { get; set; }
		public float Spacing { get => spacing; set => spacing=value; }
		public float ItemHeight { get => itemHeight; set => itemHeight=value; }
		public Action<T> onValueChanged { get; set; }
		public int TotalItemCount { get => totalItemCount; set => totalItemCount=value; }

		public void SetupView()
		{
			scrollRect.onValueChanged.RemoveAllListeners();
			scrollRect.onValueChanged.AddListener(ChangeScrollRect);

			content = scrollRect.content;
			viewport = scrollRect.viewport;
			float viewportY = viewport.rect.height;
			if(viewportY < 1) return;

			toggleGroup = content.GetComponent<ToggleGroup>();

			var showItemList = new List<RectTransform>();
			tValues = new T[totalItemCount];

			itemHeight = scrollItem.rect.height;
			scrollItem.gameObject.SetActive(false);

			for(int i = 0 ; i < totalItemCount ; i++)
			{
				var newItem = GameObject.Instantiate<RectTransform>(scrollItem, content);
				Toggle newToggle = newItem.GetComponentInChildren<Toggle>();
				if(newToggle != null) newToggle.group = toggleGroup;

				float posY = itemHeight * i + spacing * (i - 1);
				newItem.anchoredPosition.Set(0, posY);

				showItemList.Add(newItem);
				if(posY >= viewportY)
				{
					break;
				}
			}
			int oldShowItemLength = showItems ==null ? 0 : showItems.Length;
			for(int i = 0 ; i < oldShowItemLength ; i++)
			{
				GameObject.Destroy(showItems[i].gameObject);
			}
			showItems = showItemList.ToArray();

			float totalSize = itemHeight * totalItemCount + spacing * (totalItemCount - 1);
			Vector2 newSize = content.sizeDelta;
			newSize.y = totalSize; // 높이 변경
			content.sizeDelta = newSize;

			contentPosYRange = new Vector2(0, Mathf.Max(0f, totalSize - viewport.sizeDelta.y));

			firstVisibleIndex = -1;
			scrollRect.normalizedPosition = Vector2.up;
		}
		private void ChangeScrollRect(Vector2 pos)
		{
			ChangeScrollRect();
		}
		private void ChangeScrollRect()
		{
			if(showItems == null || showItems.Length == 0) return;

			float scrollPosY = content.anchoredPosition.y;

			// 2. 아이템 하나의 높이와 간격 계산
			float totalItemHeight = itemHeight + spacing;

			// 3. 현재 뷰포트에 보이는 첫 번째 아이템의 인덱스 계산
			int _firstVisibleIndex = Mathf.FloorToInt(scrollPosY / totalItemHeight);

			_firstVisibleIndex = Mathf.Clamp(_firstVisibleIndex, 0, totalItemCount - 1);

			if(firstVisibleIndex == _firstVisibleIndex) return;
			firstVisibleIndex = _firstVisibleIndex;

			for(int i = 0 ; i < showItems.Length ; i++)
			{
				int dataIndex = firstVisibleIndex + i;
				if(dataIndex < 0 || dataIndex >= totalItemCount)
				{
					// 보이지 않는 항목은 숨기기
					onHideItem?.Invoke(showItems[i], default);
					showItems[i].gameObject.SetActive(false);
				}
				else
				{
					// 보이는 항목은 위치 및 데이터 업데이트
					float newY = -dataIndex * totalItemHeight;
					showItems[i].anchoredPosition = new Vector2(0, newY);
					showItems[i].gameObject.SetActive(true);

					if(onShowItem != null)
					{
						onShowItem(showItems[i], tValues[dataIndex]);
					}
				}
			}
		}

		public void SetupValue()
		{
			if(onSetValue == null) return;
			if(tValues == null) return;

			for(int i = 0 ; i < totalItemCount ; i++)
			{
				tValues[i] = onSetValue.Invoke(i);
			}
			ChangeScrollRect();
		}

		public T GetValue()
		{
			return default;
		}

		public void SetValue(T setValue, bool _interaction = true)
		{

		}

		public async Awaitable OnShowView()
		{
			scrollRect.gameObject.SetActive(true);
			await Awaitable.EndOfFrameAsync();
			if(showItems == null || showItems.Length == 0)
			{
				SetupView();
				SetupValue();
			}
			else
			{
				firstVisibleIndex = -1;
				SetupValue();
			}
		}
		public async Awaitable OnHideView()
		{
			scrollRect.gameObject.SetActive(false);
			showItems = null;
			await Awaitable.EndOfFrameAsync();
		}

		public void Deinit()
		{
			int oldShowItemLength = showItems ==null ? 0 : showItems.Length;
			for(int i = 0 ; i < oldShowItemLength ; i++)
			{
				GameObject.Destroy(showItems[i].gameObject);
			}
			showItems = null;
			tValues = null;
		}
	}
}
