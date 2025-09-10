// © XIV-Tools.
// Licensed under the MIT license.

#pragma warning disable IDE0130
namespace XivToolsWpf;
#pragma warning restore IDE0130

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

public static class Dispatch
{
	public static SwitchToUiAwaitable MainThread() => default;

	public static SwitchFromUiAwaitable NonUiThread() => default;

	public struct SwitchToUiAwaitable : INotifyCompletion
	{
		public readonly bool IsCompleted => Application.Current?.Dispatcher.CheckAccess() == true;

		public readonly SwitchToUiAwaitable GetAwaiter() => this;

		public readonly void GetResult() { }

		public readonly void OnCompleted(Action continuation)
		{
			Application.Current?.Dispatcher.BeginInvoke(continuation);
		}
	}

	public struct SwitchFromUiAwaitable : INotifyCompletion
	{
		public readonly bool IsCompleted => Application.Current?.Dispatcher.CheckAccess() == false;

		public readonly SwitchFromUiAwaitable GetAwaiter() => this;

		public readonly void GetResult() { }

		public readonly void OnCompleted(Action continuation)
		{
			Task.Run(continuation);
		}
	}
}
