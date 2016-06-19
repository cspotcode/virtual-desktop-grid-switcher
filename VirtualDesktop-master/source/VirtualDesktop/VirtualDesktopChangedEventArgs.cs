using System;

namespace WindowsDesktop
{
	/// <summary>
	/// Provides data for the <see cref="VirtualDesktop.CurrentChanged"/> event.
	/// </summary>
	public class VirtualDesktopChangedEventArgs : EventArgs
	{
        public VirtualDesktop OldDesktop { get; private set; }
        public VirtualDesktop NewDesktop { get; private set;  }

		public VirtualDesktopChangedEventArgs(VirtualDesktop oldDesktop, VirtualDesktop newDesktop)
		{
			this.OldDesktop = oldDesktop;
			this.NewDesktop = newDesktop;
		}
	}
}
