using System.Collections.Generic;

using BC.AIProvider;
using BC.ODCC;

using UnityEngine;
namespace TFContent
{
	public class UnitCommandData : DataObject
	{
		public enum UnitCommandStateType
		{
			None = -1,
			Idle = 0, Move, Combat, Neutralize
		}
		public UnitCommandData() : base()
		{
			unitCommandState = UnitCommandStateType.Idle;
			inRangeReactionUnit = new List<ObjectBehaviour>();
			hasMoveTarget = false;
		}

		public UnitCommandStateType unitCommandState;

		[SerializeField]
		private bool isAlive;
		public bool IsAlive { get => isAlive; set => isAlive = value; }
		public bool IsDead { get => !isAlive; set => isAlive = !value; }

		public bool hasMoveTarget;
		public Vector3 moveTargetPosition;
		public Vector3 moveTargetLook;
		public bool moveIsTeleport;

		public List<ObjectBehaviour> inRangeReactionUnit;

		public float resetAutoActuatorTimeMax = 3f;
		public float resetAutoActuatorTimeUpdate = -1f;
		public bool isEnterCombat;

		public ActuatorModule autoSelectActuator;
		public UnitPairsTarget autoSelectPair;

		public ActuatorModule manualSelectActuator;
		public UnitPairsTarget manualSelectPair;
		public bool HasManualActuator => manualSelectActuator != null;
		public bool HasAutoActuator => autoSelectActuator != null;
		public ActuatorModule SelectActuator => HasManualActuator ? manualSelectActuator : autoSelectActuator;
		public UnitPairsTarget SelectPair => HasManualActuator ? manualSelectPair : autoSelectPair;
		public bool HasSelectActuator => HasManualActuator || HasAutoActuator;

		public void OnMovePosition(in Vector3 position, in Vector3 look)
		{
			moveTargetPosition = position;
			moveTargetLook = look;
			moveIsTeleport = false;
			hasMoveTarget = true;
		}
		public void OnSetPosition(in Vector3 position, in Vector3 look)
		{
			moveTargetPosition = position;
			moveTargetLook = look;
			moveIsTeleport = true;
			hasMoveTarget = true;
		}
		public void OnInReactionRange(ObjectBehaviour targetUnit)
		{
			inRangeReactionUnit.Add(targetUnit);
		}

		public void OnOutReactionRange(ObjectBehaviour targetUnit)
		{
			inRangeReactionUnit.Remove(targetUnit);
		}

		protected override void Disposing()
		{
			OnClearMove();
			OnClearSelectActuator();
			inRangeReactionUnit.Clear();
		}
		public void OnClearMove()
		{
			moveTargetPosition = Vector3.zero;
			moveTargetLook = Vector3.zero;
			moveIsTeleport = false;
			hasMoveTarget = false;
		}
		public void OnClearSelectActuator()
		{
			autoSelectActuator = null;
			manualSelectActuator = null;
		}
	}
}