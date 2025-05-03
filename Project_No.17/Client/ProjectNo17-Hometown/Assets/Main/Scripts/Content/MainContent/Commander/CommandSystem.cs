using System;
using System.Collections.Generic;
using System.Linq;

using BC.ODCC;

using Sirenix.OdinInspector;

using TFSystem.SaveLoad;

using UnityEngine;
namespace TFContent
{
	public partial class CommandSystem : ObjectBehaviour
	{
		public enum CommandControllerType
		{
			None = 0,
			DefaultCommand = 1,
			CombatCommand = 2,
		}
		[Serializable]
		public struct CommandControllerStateGroup
		{
			public CommandControllerType enableFlag;
			public List<CommandController> commandControllers;
		}

		[SerializeField]
		private OdccQuerySystem queryAllCommandable;
		[SerializeField]
		private OdccQueryCollector collectorAllCommandable;
		[Space]
		[SerializeField, InlineButton("UpdateCommandControllerType", "Update")]
		private  CommandControllerType commandType;
		[Space]
		public List<CommandControllerStateGroup> commandStateGroups;
		public CommandControllerType CommandType {
			get => commandType; set {
				commandType=value;
				UpdateCommandControllerType(commandType);
			}
		}

		protected override void BaseValidate(in bool isPrefab = false)
		{
			if(isPrefab) return;
			UpdateCommandControllerType(CommandType);
		}
		protected override void BaseAwake()
		{
			queryAllCommandable = OdccQueryBuilder.CreateQuery().WithAll<CharacterObject, SelectingTarget, UnitCommandComponent>().Build(ThisScene);
			collectorAllCommandable = OdccQueryCollector.CreateQueryCollector(queryAllCommandable, ThisScene);
			collectorAllCommandable.CreateChangedListEvent(SettingCommandNumber);
		}
		private void SettingCommandNumber(ObjectBehaviour change, bool isAdd)
		{
			if(!isAdd) return;
			if(!change.ThisContainer.TryGetComponent<UserCommandConnector>(out var commandConnector)) return;

			commandConnector.commandNumber = 0;

			List<int> allCommandNumber = collectorAllCommandable.GetQueryItemList().Select(i=>i.GetComponent<UserCommandConnector>().commandNumber).ToList();
			allCommandNumber.Sort();
			int length = allCommandNumber.Count;
			int i = 0;
			int number = 1;
			for(; i < length ; i++)
			{
				if(allCommandNumber[i] == 0)
				{
					continue;
				}
				else if(allCommandNumber[i] == number)
				{
					number++;
				}
				else
				{
					break;
				}
			}
			commandConnector.commandNumber = number;
		}
		private void UpdateCommandControllerType(CommandControllerType commandFlag)
		{
			int length = commandStateGroups.Count;
			HashSet<CommandController> enableList = new HashSet<CommandController>();
			HashSet<CommandController> disableList = new HashSet<CommandController>();

			for(int i = 0 ; i < length ; i++)
			{
				var group = commandStateGroups[i];
				var list = group.commandControllers;
				int length2 = list.Count;
				if(commandFlag.HasFlag(group.enableFlag))
				{
					for(int ii = 0 ; ii < length2 ; ii++)
					{
						enableList.Add(list[ii]);
					}
				}
				else
				{
					for(int ii = 0 ; ii < length2 ; ii++)
					{
						disableList.Add(list[ii]);
					}
				}
			}

			foreach(var item in disableList)
			{
				if(item == null) continue;
				if(enableList.Contains(item))
				{
					continue;
				}
				else
				{
					item.enabled = false;
				}
			}
			foreach(var item in enableList)
			{
				if(item == null) continue;
				item.enabled = true;
			}
		}
		protected override void BaseDestroy()
		{
			queryAllCommandable = null;
			collectorAllCommandable = null;
		}
	}

	public partial class CommandSystem : ISelectorCommandListener
	{
		List<ObjectBehaviour> ISelectorCommandListener.GetSelectList()
		{
			List<ObjectBehaviour> allCommandable = collectorAllCommandable.GetQueryItemList();
			return allCommandable.Where(i => i.ThisContainer.TryGetComponent<SelectingHandle>(out _)).ToList();
		}
		List<ObjectBehaviour> ISelectorCommandListener.GetUnselectList()
		{
			List<ObjectBehaviour> allCommandable = collectorAllCommandable.GetQueryItemList();
			return allCommandable.Where(i => !i.ThisContainer.TryGetComponent<SelectingHandle>(out _)).ToList();
		}
		bool ISelectorCommandListener.CanMultiSelect()
		{
			return commandType == CommandControllerType.DefaultCommand;
		}
		void ISelectorCommandListener.GetSelectTargetAndState(int number, out ObjectBehaviour selectingTarget, out bool currentSelectState)
		{
			currentSelectState = false;
			selectingTarget = null;

			List<ObjectBehaviour> allCommandable = collectorAllCommandable.GetQueryItemList();
			int length = allCommandable.Count;
			for(int i = 0 ; i < length ; i++)
			{
				if(allCommandable[i].TryGetComponent<UserCommandConnector>(out var commandConnector))
				{
					if(commandConnector.commandNumber == number)
					{
						selectingTarget = allCommandable[i];
						break;
					}
				}
			}

			currentSelectState = selectingTarget != null && selectingTarget.TryGetComponent<SelectingHandle>(out _);
		}

	}
	public partial class CommandSystem : ISaveLoadTarget
	{
		void ISaveLoadData.OnSave(ref SaveLoadData data)
		{
			data.SaveValue(nameof(commandType), commandType);
			ISaveLoadTarget.SaveThisDataObject(ThisContainer, ref data);
		}
		void ISaveLoadData.OnLoad(in SaveLoadData data)
		{
			ISaveLoadTarget.LoadThisDataObject(ThisContainer, in data);
			if(data.LoadValue(nameof(commandType), out var _commandType, CommandControllerType.DefaultCommand))
			{
				CommandType = _commandType;
			}
		}
	}
}