
using BC.ODCC;
namespace TFContent
{
	public abstract class CommandListener : ComponentBehaviour
	{
		public abstract void CommandUpdate();
	}
	public abstract class CommandListener<TCommand> : CommandListener
	{
		public abstract void OnCommanding(in TCommand commandValue, in bool inQueue = false);

		public virtual void OnCommanding(in TCommand commandValue, params object[] paramsValue) { }
	}
}