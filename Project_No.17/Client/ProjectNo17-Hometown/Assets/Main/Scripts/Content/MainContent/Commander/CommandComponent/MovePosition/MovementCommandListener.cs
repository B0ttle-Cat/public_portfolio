using Pathfinding;

using UnityEngine;

namespace TFContent
{
	public class MovementCommandListener : CommandListener<(NNInfo nnInfo, Vector3 look)>
	{
		private IMovePositionListener iMovePosition;

		public override void CommandListenerUpdate<TCommand>(TCommand order, in (NNInfo nnInfo, Vector3 look) commandValue)
		{
			if(order == null) return;
			if(iMovePosition == null && !ThisContainer.TryGetComponent<IMovePositionListener>(out iMovePosition)) return;

			NNInfo nnInfo = commandValue.nnInfo;
			Vector3 look = commandValue.look;
			iMovePosition.OnMovePosition(nnInfo.position, look);
		}
	}
}