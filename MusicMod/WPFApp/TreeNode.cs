using System.Collections.Generic;
using System.Linq;
using System;
using Utils;

namespace WPFApp
{
	public class TreeNode : NotifyPropertyChangedBase
	{
		private readonly Func<TreeNode> parentGetter;

		public TreeNode(string label, ITreeItem value)
		{
			Label = label;
			Value = value ?? throw new ArgumentNullException(nameof(value));
		}

		private TreeNode(string label, ITreeItem value, Func<TreeNode> parentGetter) : this(label, value) => this.parentGetter = parentGetter;

		public string Label { get; }

		public ITreeItem Value { get; }

		public TreeNode Parent => parentGetter?.Invoke();

		public IEnumerable<TreeNode> Children => Value.Children.Where(p => p.Item2 is not null).Select(pair => Then(pair.Item1, pair.Item2));

		public TreeNode Then(string label, ITreeItem value) => new(label, value, () => this);
	}
}