using WPFApp.Controls.GridManagers;

namespace WPFApp.Controls.Rows
{
	internal interface IRowControl
	{
		IRowManager RowManager { get; }
	}
}