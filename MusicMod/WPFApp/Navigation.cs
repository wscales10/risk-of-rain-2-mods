﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WPFApp.Controls;

namespace WPFApp
{
	internal abstract class Navigation
	{
		protected Navigation(IEnumerable<ControlBase> controls)
		{
			Controls = new ReadOnlyCollection<ControlBase>(controls.ToList());
		}

		public ReadOnlyCollection<ControlBase> Controls { get; }

		public abstract Navigation Reverse { get; }
	}

	internal class AddNavigation : Navigation
	{
		internal AddNavigation(IEnumerable<ControlBase> controls) : base(controls) { }
		internal AddNavigation(params ControlBase[] controls) : base(controls) { }

		public override Navigation Reverse => new RemoveNavigation(Controls);
	}

	internal class RemoveNavigation : Navigation
	{
		internal RemoveNavigation(IEnumerable<ControlBase> controls) : base(controls) { }

		internal RemoveNavigation(params ControlBase[] controls) : base(controls) { }

		public override Navigation Reverse => new AddNavigation(Controls);
	}
}
