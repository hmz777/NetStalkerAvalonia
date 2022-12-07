using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.ViewModels.RoutedViewModels;
using ReactiveUI;
using System.Diagnostics;

namespace NetStalkerAvalonia.Views.RoutedViews
{
	public partial class AboutView : ReactiveUserControl<AboutViewModel>
	{
		public AboutView()
		{
			this.WhenActivated(disposables =>
			{
				var linksContainer = this.Find<StackPanel>("links");

				foreach (var control in linksContainer.Children)
				{
					if (control.GetType() == typeof(StackPanel))
					{
						var stackPanel = control as StackPanel;

						foreach (var innerControl in stackPanel.Children)
						{
							if (innerControl.GetType() == typeof(TextBlock))
							{
								var textBlock = innerControl as TextBlock;
								textBlock.PointerPressed += TextBlock_PointerPressed;
							}
						}
					}
				}

				var social = this.Find<StackPanel>("sociallinks");

				foreach (var control in social.Children)
				{
					control.PointerPressed += Control_PointerPressed;
				}

			});

			InitializeComponent();
		}

		private void Control_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
		{
			var control = sender as Control;

			if (e.GetCurrentPoint(control).Properties.IsLeftButtonPressed)
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = control!.Tag!.ToString(),
					UseShellExecute = true
				});
			}
		}

		private void TextBlock_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
		{
			var textBlock = sender as TextBlock;

			if (e.GetCurrentPoint(textBlock).Properties.IsLeftButtonPressed)
			{
				System.Diagnostics.Process.Start(new ProcessStartInfo
				{
					FileName = textBlock.Text,
					UseShellExecute = true
				});
			}
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}