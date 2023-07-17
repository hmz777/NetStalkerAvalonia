using NetStalkerAvalonia.Core.Rules;
using ReactiveUI;
using System;

namespace NetStalkerAvalonia.Core.ViewModels.InteractionViewModels
{
	public class AddUpdateRuleModel : ReactiveObject
	{
		public Guid RuleId { get; set; }

		private RuleAction? action;
		public RuleAction? Action
		{
			get => action;
			set => this.RaiseAndSetIfChanged(ref action, value);
		}

		private RuleSourceValue? sourceValue;
		public RuleSourceValue? SourceValue
		{
			get => sourceValue;
			set => this.RaiseAndSetIfChanged(ref sourceValue, value);
		}

		private string? target;
		public string? Target
		{
			get => target;
			set => this.RaiseAndSetIfChanged(ref target, value);
		}

		private bool isRegex;
		public bool IsRegex
		{
			get => isRegex;
			set => this.RaiseAndSetIfChanged(ref isRegex, value);
		}

		private int order;
		public int Order
		{
			get => order;
			set => this.RaiseAndSetIfChanged(ref order, value);
		}

		private bool active;
		public bool Active
		{
			get => active;
			set => this.RaiseAndSetIfChanged(ref active, value);
		}

		private int upload;
		public int Upload
		{
			get => upload;
			set => this.RaiseAndSetIfChanged(ref upload, value);
		}

		private int download;
		public int Download
		{
			get => download;
			set => this.RaiseAndSetIfChanged(ref download, value);
		}
	}
}
