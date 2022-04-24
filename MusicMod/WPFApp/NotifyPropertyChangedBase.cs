using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WPFApp
{
	public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public void SubscribeTo(INotifyPropertyChanged obj) => obj.PropertyChanged += (s, e) => NotifyPropertyChanged(e);

		protected void NotifyPropertyChanged([CallerMemberName] string info = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

		protected void NotifyPropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);
	}
}