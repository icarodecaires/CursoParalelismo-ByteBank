using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteBank.View.Utils
{
	public class ByteBankProgress<T> : IProgress<T>
	{
		private readonly Action<T> _Handler;
		private readonly TaskScheduler _taskScheduler;
		public ByteBankProgress(Action<T> handler)
		{
			_taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
			_Handler = handler;
		}

		public void Report(T value)
		{
			Task.Factory.StartNew(
				() => _Handler(value),
				System.Threading.CancellationToken.None,
				TaskCreationOptions.None,
				_taskScheduler
			) ;
		}
	}
}
