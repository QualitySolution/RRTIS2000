using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RRTIS2000
{
	public class ControllerItem
	{
		public int ContrillerId {get; set;}
		public int VCI_Controller { get; set; }
		public int VCI_Car { get; set; }

		public string ControllerDiscription { get; set; }
		public string ControllerName { get; set; }
	}
}
