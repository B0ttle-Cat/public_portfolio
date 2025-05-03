
using BC.ODCC;
namespace TFContent
{
	public abstract class CommandListener : ComponentBehaviour
	{
	}
	public abstract class CommandListener<TCommandValue> : CommandListener
	{
		public abstract void CommandListenerUpdate<TCommand>(TCommand order, in TCommandValue commandValue) where TCommand : CommandController;

		public virtual void CommandListenerUpdate<TCommand>(TCommand order, in TCommandValue commandValue, params object[] paramsValue) where TCommand : CommandController { }
	}
}