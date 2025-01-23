using System;

namespace TFContent
{
	public class NumberInput : IDisposable
	{
		private UserInputSystem.NumberActions number;
		public event Action<int> started;
		public event Action<int> performed;
		public event Action<int> canceled;
		public NumberInput(UserInputSystem.NumberActions _number)
		{
			number = _number;
			number.Enable();

			number._1.started += ctx => started?.Invoke(1);
			number._1.performed += ctx => performed?.Invoke(1);
			number._1.canceled += ctx => canceled?.Invoke(1);

			number._2.started += ctx => started?.Invoke(2);
			number._2.performed += ctx => performed?.Invoke(2);
			number._2.canceled += ctx => canceled?.Invoke(2);

			number._3.started += ctx => started?.Invoke(3);
			number._3.performed += ctx => performed?.Invoke(3);
			number._3.canceled += ctx => canceled?.Invoke(3);

			number._4.started += ctx => started?.Invoke(4);
			number._4.performed += ctx => performed?.Invoke(4);
			number._4.canceled += ctx => canceled?.Invoke(4);

			number._5.started += ctx => started?.Invoke(5);
			number._5.performed += ctx => performed?.Invoke(5);
			number._5.canceled += ctx => canceled?.Invoke(5);

			number._6.started += ctx => started?.Invoke(6);
			number._6.performed += ctx => performed?.Invoke(6);
			number._6.canceled += ctx => canceled?.Invoke(6);

			number._7.started += ctx => started?.Invoke(7);
			number._7.performed += ctx => performed?.Invoke(7);
			number._7.canceled += ctx => canceled?.Invoke(7);

			number._8.started += ctx => started?.Invoke(8);
			number._8.performed += ctx => performed?.Invoke(8);
			number._8.canceled += ctx => canceled?.Invoke(8);

			number._9.started += ctx => started?.Invoke(9);
			number._9.performed += ctx => performed?.Invoke(9);
			number._9.canceled += ctx => canceled?.Invoke(9);

			number._0.started += ctx => started?.Invoke(0);
			number._0.performed += ctx => performed?.Invoke(0);
			number._0.canceled += ctx => canceled?.Invoke(0);
		}

		public void Dispose()
		{
			number.Disable();
		}
	}
}
