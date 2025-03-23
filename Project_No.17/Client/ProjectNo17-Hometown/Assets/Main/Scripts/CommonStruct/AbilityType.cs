using UnityEngine;

namespace BC.OdccBase
{
	public enum AbilityType : int
	{
		[InspectorName("===== 기본값 관련 ========================================")]
		_DefaultPoint = 0,
		ShieldPoint = 01,            // 실드 내구도 (채력)	 // => 없을 경우 무력화 됨
		MaterialPoint = 02,          // 아이템 조합 물질	 // => 없을 경우 재장전 또는 아이템이 소비되는 형태의 행동 불가능
		BatteryPoint = 03,           // 잔여 축전지		 // => 없을 경우 실드 회복 / 아이템 조합 / 공격이 불가능해 짐
		AmmoCount = 04,              // 탄약 잔여량		 // => 없을 경우 공격 불가 및 재장전 필요

		[InspectorName("===== 기본값(최대) 관련 ========================================")]
		_DefaultMaxPoint = 10,
		ShieldMaxPoint = 11,         // 최대 실드 내구도
		MaterialMaxPoint = 12,       // 최대 아이템 조합 물질
		BatteryMaxPoint = 13,        // 최대 전지 에너지
		AmmoMaxCount = 14,           // 최대 탄약 보유량

		[InspectorName("===== 회복력 관련 ========================================")]
		_Recovery = 20,
		MaterialRecovery = 21,       // 물질 회복 속도
		BatteryRecovery = 22,        // 전력 회복 속도

		[InspectorName("===== 효율 관련 ========================================")]
		_Efficiency = 30,
		MaterialEfficiency = 31,     // 물질 변환 효율 // => 탄 재장전 및 소비품 제작 에 사용됨 
		BatteryEfficiency = 32,      // 전력 변환 효율 // => 실드 회복 속도 / 물질 제작 및 회복 / 공격에 사용됨


		// Point
		[InspectorName("===== 계산 점수 관련 ========================================")]
		_Point = 50,
		AttackPoint = 51,            // 공격력
		DefensePoint = 52,           // 방어력
		CriticalPoint = 53,          // 치명타점수
		BlockingPoint = 54,          // 공격차단점수
		SplashMinHitPoint = 55,      // 범위공격 최소 수치
		SplashMaxHitPoint = 56,      // 범위공격 최대 수치


		// Chance
		[InspectorName("===== 계산 확률 관련 ========================================")]
		_Chance = 100,
		AttackChance = 101,             // 명중률
		DodgeChance = 102,           // 회피율
		CriticalChance = 103,        // 크리티컬 확률
		BlockingChance = 104,        // 공격 차단 확률
		SplashMinDodgeChance= 105,   // 범위공격 회피 수치
		SplashMaxDodgeChance= 106,   // 범위공격 회피 수치


		[InspectorName("===== 거리&반경 지정 관련 ========================================")]
		_Range = 200,
		VisibleRange = 201,          // 현제 시야 거리
		VisibleMinRange = 202,       // 시야 최소 거리
		VisibleMaxRange = 203,       // 시야 최데 거리
		ReactionRange = 204,         // 현제 반응 거리
		ReactionMinRange = 205,      // 반응 최소 거리
		ReactionMaxRange = 206,      // 반응 최대 거리
		SplashRange = 207,           // 범위형 효과 적용 반경
		SplashMinEffectRange = 208,  // 범위형 반경내 최대 효과 적용이 끝나는 범위
		SplashMaxEffectRange = 209,  // 범위형 반경내 최소 효과 적용이 시작되는 범위
	}
}
