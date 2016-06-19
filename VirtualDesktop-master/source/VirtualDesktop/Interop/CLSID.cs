using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable InconsistentNaming

namespace WindowsDesktop.Interop
{
	public static class CLSID
	{
		public static Guid ImmersiveShell = new Guid("c2f03a33-21f5-47fa-b4bb-156362a2f239");

		public static Guid VirtualDesktopManager = new Guid("aa509086-5ca9-4c25-8f95-589d3c07b48a");

		public static Guid VirtualDesktopAPIUnknown = new Guid("c5e0cdca-7b6e-41b2-9fc4-d93975cc467b");

		public static Guid VirtualDesktopNotificationService = new Guid("a501fdec-4a09-464c-ae4e-1b9c21b84918");
	}
}
