using UnityEngine;

namespace BC.OdccBase
{
	public enum ActuatorEffectType : int
	{
		// Chance_Type = 0
		[InspectorName("===== 확률 관련 ========================================")]
		_Chance = 0,
		Chance_AttackHit = 1,
		Chance_CriticalHit = 2,
		Chance_BlockingPass = 3,

		[InspectorName("===== 데미지 계산 관련 ========================================")]
		____Damage_Type = 100,
		Damage_Shield_Attack = 101,
		Damage_Shield_Critical = 102,
		Damage_Shield_Blocking = 103,

		Damage_Miss = 190,

		[InspectorName("===== 획복 계산 관련 ========================================")]
		____Recovery_Type = 200,
		Recovery_Shield = 201,


		//____Buff_Type       = 500,



		//____Debuff_Type     = 600,
		____TEST_TYPE____ = 99999,
	}
}
