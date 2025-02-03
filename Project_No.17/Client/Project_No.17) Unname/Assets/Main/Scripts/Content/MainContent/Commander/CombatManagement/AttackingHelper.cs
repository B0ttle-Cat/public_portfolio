using BC.ODCC;

using UnityEngine;
namespace TFContent
{
	public class AttackingHelper : ComponentBehaviour//, IOdccUpdate
	{
		private AttackingTarget attackingTarget;

		private Collider[] selectCollider;
		protected override void BaseEnable()
		{
			selectCollider = GetComponentsInChildren<Collider>();
			int length = selectCollider.Length;
			for(int i = 0 ; i < length ; i++)
			{
				selectCollider[i].gameObject.layer = LayerMask.NameToLayer("AttackingTarget");
				selectCollider[i].isTrigger = true;
			}
		}
		protected override void BaseDisable()
		{
			selectCollider = null;
		}
	}
}