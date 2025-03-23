using BC.Base;
using BC.OdccBase;

namespace TFContent
{
	public class InitBehaviorSetter : BehaviorSetter
	{
		public override void Setup()
		{
			SetVariableValue<InitBehaviorWorld>();
			SetVariableValue<InitBehaviorCamera>();
			SetVariableValue<InitBehaviorCharacter>();
			SetVariableValue<InitBehaviorCommand>();
			SetVariableValue<InitBehaviorScenario>();

			void SetVariableValue<T>() where T : OdccBehaviorTarget
			{
				if(!agent.SetVariableValue(typeof(T).Name, (OdccBehaviorTarget)FindAnyObjectByType<T>()))
				{
					Debug.LogError($"{typeof(T).Name} Is Fail Setup");
				}
			}
		}
	}
}
