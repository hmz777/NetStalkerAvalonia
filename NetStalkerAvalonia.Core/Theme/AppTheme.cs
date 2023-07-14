namespace NetStalkerAvalonia.Core.Theme
{
	public class AppTheme
	{
		private static AppTheme? _instance;

		public AppTheme() { }

		public static AppTheme Instance
		{
			get
			{
				_instance ??= new AppTheme();
				return _instance;
			}
		}

		public string WindowWidth => "1000";
		public string WindowMinWidth => "850";
		public string WindowHeight => "500";
		public string WindowMinHeight => "500";
		public string NavBorderColor => "#2C2C2C";
		public string NavBackground => "#1E1E1E";
		public string NavElementBackground => "#2C2C2C";
		public string NavElementBorder => "#1F1F1F";
		public string NavElementText => "#ffffff";
		public string WindowBackground => "#1E1E1E";
		public string TitleBarBackColor => "#000000";
		public string TitleBarButtonBackColor => "#000000";
		public string TitleBarButtonFrontColor => "#FFFFFF";
		public string TitleBarButtonsHoverColor => "#1F1F1F";
		public string PageTitleColor => "white";
		public string AccentColor => "#449C82";
		public string PageTitleSize => "25";
		public string ListRowSelectedBackColor => "Teal";
		public string ListColumnHeaderBackColor => "#2C2C2C";
		public string PagePadding => "15";
		public string DialogButtonWidth => "60";
		public string DialogButtonHeight => "35";
	}
}