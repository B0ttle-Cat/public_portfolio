using UnityEngine;

namespace BC.OdccBase
{
    public enum AbilityType : int
    {
        [InspectorName("===== 기본값 관련 ========================================")]
        _DefaultPoint = 0,
        ShieldPoint = 01,            // 실드 내구도 (채력)	 // => 없을 경우 무력화 됨 / 평시 BatteryPoint를 사용하여 자동 회복됨.
        MaterialPoint = 02,          // 아이템 조합 물질	 // => 없을 경우 재장전 또는 아이템이 소비되는 형태(일부 스킬 포함)의 행동 불가능
        BatteryPoint = 03,           // 잔여 축전지		 // => 없을 경우 실드 회복 / 공격이 불가능해 짐
        AmmoCount = 04,              // 탄약 잔여량		 // => 없을 경우 공격 불가 및 재장전 필요
                                     // Material 과 Battery
                                     // Material => 아이템을 조합 할때/일부 스킬을 사용할 때/탄을 재장전 할 때 사용됨. 모두 소모될 경우 재쟁전 및 아이템 조함 불가능
                                     // Battery => 채력을 회복하거나 스킬 및 아이템을 사용할 떄 필요함

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
        AttackDefensePoint = 52,     // 방어력
        CriticalPoint = 53,          // 치명타 공격점수
        CriticalDefensePoint = 54,   // 치명타 방어점수
        BlockingPassPoint = 55,      // 엄패 무시 점수
        BlockingCoverPoint = 56,     // 엄패 성공 점수


        // Chance
        [InspectorName("===== 계산 확률 관련 ========================================")]
        _Chance = 100,
        AttackHitChance = 101,      // 명중률
        AttackDodgeChance = 102,    // 명중 회피율
        CriticalHitChance = 103,    // 크리티컬 확률
        CriticalDodgeChance = 104,  // 크리티컬 회피 확률
        BlockingPassChance = 105,  // 엄패 무시 확률
        BlockingCoverChance = 106,  // 엄패 성공 확률


        [InspectorName("===== 거리&반경 지정 관련 ========================================")]
        _Range = 200,
        VisibleRange = 201,          // 현제 시야 거리
        ReactionRange = 204,         // 현제 반응 거리




        [InspectorName("===== 그 외 캐릭터 관련 수티 ========================================")]
        _Other = 9999,
        CurrentEXP = 10000,          // 현제 획득한 경험치
        RequiredEXP = 10001,         // 레벨업에 필요한 경험치
    }
}
