using Avalonia;
using Avalonia.Controls;
using NetStalkerAvalonia.Helpers;
using ReactiveUI;
using System.Reactive;

namespace NetStalkerAvalonia.Components
{
	public partial class LinkButton : UserControl
	{
		public static readonly StyledProperty<string> TextProperty =
						AvaloniaProperty.Register<LinkButton, string>(nameof(Text));

		public string Text
		{
			get { return GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public static readonly StyledProperty<string> LinkProperty =
						AvaloniaProperty.Register<LinkButton, string>(nameof(Link));

		public string Link
		{
			get { return GetValue(LinkProperty); }
			set { SetValue(LinkProperty, value); }
		}

		public ReactiveCommand<string, Unit> OnClicked { get; set; }

		public LinkButton()
		{
			OnClicked = ReactiveCommand.Create<string>((param) =>
			{
				if (string.IsNullOrWhiteSpace(param) == false)
				{
					Tools.OpenLink(param);
				}
			});

			InitializeComponent();
		}
	}
}