using BC.Base;
using BC.OdccBase;

namespace TFContent
{
	public class StartBehaviorSetter : BehaviorSetter
	{
		public override void Setup()
		{
			agent.SetVariableValue("StartBehaviorWorld", (OdccBehaviorTarget)FindAnyObjectByType<StartBehaviorWorld>());
			agent.SetVariableValue("StartBehaviorCamera", (OdccBehaviorTarget)FindAnyObjectByType<StartBehaviorCamera>());
			agent.SetVariableValue("StartBehaviorCharacter", (OdccBehaviorTarget)FindAnyObjectByType<StartBehaviorCharacter>());
			agent.SetVariableValue("StartBehaviorCommand", (OdccBehaviorTarget)FindAnyObjectByType<StartBehaviorCommand>());
			agent.SetVariableValue("StartBehaviorScenario", (OdccBehaviorTarget)FindAnyObjectByType<StartBehaviorScenario>());

			SetVariableValue<StartBehaviorWorld>();
			SetVariableValue<StartBehaviorCamera>();
			SetVariableValue<StartBehaviorCharacter>();
			SetVariableValue<StartBehaviorCommand>();
			SetVariableValue<StartBehaviorScenario>();

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
