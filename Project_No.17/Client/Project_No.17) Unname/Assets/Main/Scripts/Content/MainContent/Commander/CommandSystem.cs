using System;
using System.Collections.Generic;
using System.Linq;

using BC.ODCC;

using Sirenix.OdinInspector;

using UnityEngine;
namespace TFContent
{
	public class CommandSystem : ObjectBehaviour, ISelectorCommandListener
	{
		[Flags]
		public enum CommandControllerFlag
		{
			None = 0,
			UnitControl = 1<<0,
		}
		[Serializable]
		public struct CommandControllerStateGroup
		{
			public CommandControllerFlag enableFlag;
			public List<CommandController> commandControllers;
		}

		[SerializeField]
		private QuerySystem queryAllCommandable;
		[SerializeField]
		private OdccQueryCollector collectorAllCommandable;
		[Space]
		[SerializeField, InlineButton("UpdateCommandControllerFlag", "Update")]
		private  CommandControllerFlag commandFlag;
		[Space]
		public List<CommandControllerStateGroup> commandStateGroups;

		public CommandControllerFlag CommandFlag {
			get => commandFlag; set {
				commandFlag=value;
				UpdateCommandControllerFlag(commandFlag);
			}
		}

		protected override void BaseValidate(in bool isPrefab = false)
		{
			if(isPrefab) return;
			UpdateCommandControllerFlag(CommandFlag);
		}
		protected override void BaseAwake()
		{
			queryAllCommandable = QuerySystemBuilder.CreateQuery().WithAll<CharacterObject, SelectingTarget, UnitCommandable>().Build(ThisScene);
			collectorAllCommandable = OdccQueryCollector.CreateQueryCollector(queryAllCommandable, ThisScene);
			collectorAllCommandable.CreateChangeListEvent(SettingCommandNumber);
		}
		private void SettingCommandNumber(ObjectBehaviour change, bool isAdd)
		{
			if(!isAdd) return;
			if(!change.ThisContainer.TryGetComponent<UserCommandConnector>(out var commandConnector)) return;

			commandConnector.commandNumber = 0;

			List<int> allCommandNumber = collectorAllCommandable.GetQueryItems().Select(i=>i.GetComponent<UserCommandConnector>().commandNumber).ToList();
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

		protected override void BaseDestroy()
		{
			queryAllCommandable = null;
			collectorAllCommandable = null;
		}

		public List<ObjectBehaviour> GetSelectList()
		{
			List<ObjectBehaviour> allCommandable = collectorAllCommandable.GetQueryItems();
			return allCommandable.Where(i => i.ThisContainer.TryGetComponent<SelectingHandle>(out _)).ToList();
		}
		public List<ObjectBehaviour> GetUnselectList()
		{
			List<ObjectBehaviour> allCommandable = collectorAllCommandable.GetQueryItems();
			return allCommandable.Where(i => !i.ThisContainer.TryGetComponent<SelectingHandle>(out _)).ToList();
		}

		public void GetSelectTargetAndState(int number, out ObjectBehaviour selectingTarget, out bool currentSelectState)
		{
			currentSelectState = false;
			selectingTarget = null;

			List<ObjectBehaviour> allCommandable = collectorAllCommandable.GetQueryItems();
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

		public void UpdateCommandControllerFlag(CommandControllerFlag commandFlag)
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
				item.enabled = true;
			}
		}
	}
}