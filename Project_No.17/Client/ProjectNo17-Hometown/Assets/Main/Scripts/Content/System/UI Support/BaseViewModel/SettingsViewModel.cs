using Sirenix.OdinInspector;

using TFSystem;
using TFSystem.UI;

using UnityEngine;
using UnityEngine.UI;

namespace TFContent
{
	public class SettingsViewModel : UIViewModelComponent
	{
		[HideInEditorMode, SerializeField] private Button closeSettingsButton;

		protected override void AwakeUIView(ref ViewItemSetter viewItemSetter)
		{
			if(ThisContainer.TryGetComponent<SettingsViewInspector>(out var uiInspector))
			{
				SetUIInspector(uiInspector);
			}

			closeSettingsButton.onClick.AddListener(async () => await WaitOnAction(CloseSettingsButton));

			void SetUIInspector(SettingsViewInspector uiInspector)
			{
				closeSettingsButton = uiInspector.closeSettingsButton;

				uiInspector.DestroyThis();
			}
		}

		protected virtual async Awaitable CloseSettingsButton()
		{
			if(ThisContainer.TryGetObject<IUIViewController<GamePlayViewState>>(out var view))
			{
				await view.OnChangeViewState(view.LastViewStateStack);
			}
		}
	}
}
