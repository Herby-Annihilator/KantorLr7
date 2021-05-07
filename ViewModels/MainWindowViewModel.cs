using KantorLr7.Infrastructure.Commands;
using KantorLr7.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows.Markup;

namespace KantorLr7.ViewModels
{
	[MarkupExtensionReturnType(typeof(MainWindowViewModel))]
	public class MainWindowViewModel : ViewModel
	{
		#region Properties
		private string _title = "Title";
		public string Title { get => _title; set => Set(ref _title, value); }
		#endregion

		#region Commands

		#endregion
	}
}
