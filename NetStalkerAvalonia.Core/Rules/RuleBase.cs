using NetStalkerAvalonia.Core.Models;
using NetStalkerAvalonia.Core.Rules.Implementations;
using ReactiveUI;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace NetStalkerAvalonia.Core.Rules
{
	[JsonDerivedType(typeof(BlockRule), typeDiscriminator: "block")]
	[JsonDerivedType(typeof(RedirectRule), typeDiscriminator: "redirect")]
	[JsonDerivedType(typeof(LimitRule), typeDiscriminator: "limit")]
	public abstract class RuleBase : ReactiveObject, IEquatable<RuleBase>
	{
		public Guid RuleId { get; protected set; }

		[JsonIgnore]
		public abstract RuleAction Action { get; }

		private RuleSourceValue sourceValue;
		public RuleSourceValue SourceValue
		{
			get => sourceValue;
			protected set => this.RaiseAndSetIfChanged(ref sourceValue, value);
		}

		private bool isRegex;

		public bool IsRegex
		{
			get => isRegex;
			protected set => this.RaiseAndSetIfChanged(ref isRegex, value);
		}

		private string target;
		public string Target
		{
			get => target;
			protected set => this.RaiseAndSetIfChanged(ref target, value);
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
			protected set => this.RaiseAndSetIfChanged(ref active, value);
		}

		[JsonConstructor]

		public RuleBase(RuleSourceValue sourceValue, bool isRegex, string target, int order, bool active, Guid ruleId = default)
		{
			if (order <= 0)
			{
				throw new ArgumentException("Rule order can't be zero or lower");
			}

			SourceValue = sourceValue;
			IsRegex = isRegex;
			Target = target ?? throw new ArgumentNullException(nameof(target));
			Order = order;
			Active = active;

			RuleId = ruleId == default ? Guid.NewGuid() : ruleId;
		}

		public void Activate() => Active = true;
		public void Deactivate() => Active = false;

		public bool Match(Device device)
		{
			ArgumentNullException.ThrowIfNull(device, nameof(device));

			if (Active == false)
			{
				return false;
			}

			try
			{
				switch (SourceValue)
				{
					case RuleSourceValue.IPAddress:
						{
							if (IsRegex)
							{
								return Regex.IsMatch(device.Ip.ToString(), Target);
							}

							var ipAddress = IPAddress.Parse(Target);
							return device.Ip.Equals(ipAddress);
						}
					case RuleSourceValue.MacAddress:
						{
							if (IsRegex)
							{
								return Regex.IsMatch(device.Mac.ToString(), Target);
							}

							var macAddress = PhysicalAddress.Parse(Target);
							return device.Mac.Equals(macAddress);
						}
					case RuleSourceValue.Name:
						{
							if (IsRegex)
							{
								return Regex.IsMatch(device.Name!, Target);
							}

							return device.Name?.Equals(Target, StringComparison.InvariantCultureIgnoreCase) ?? false;
						}
					case RuleSourceValue.Vendor:
						{
							if (IsRegex)
							{
								return Regex.IsMatch(device.Vendor!, Target);
							}

							return device.Vendor?.Equals(Target, StringComparison.InvariantCultureIgnoreCase) ?? false;
						}
					default:
						break;
				}
			}
			catch // If matching failed for any reason, we return false
			{ }

			return false;
		}

		public abstract void Apply(Device device);

		#region Overrides

		public override bool Equals(object? obj)
		{
			if (obj is null)
				return false;

			if (obj is not RuleBase roleToCompare)
				return false;

			return this.RuleId == roleToCompare.RuleId;
		}

		public bool Equals(RuleBase? other)
		{
			return this.Equals(obj: other);
		}

		public override int GetHashCode()
		{
			return this.RuleId.GetHashCode();
		}

		public static bool operator ==(RuleBase a, RuleBase b)
		{
			if (a is null && b is null)
				return true;

			if (a is null || b is null)
				return false;

			return a.Equals(b);
		}

		public static bool operator !=(RuleBase a, RuleBase b)
		{
			return !(a == b);
		}

		public override string ToString()
		{
			return $"{SourceValue} > {Action} > {Target}";
		}

		#endregion
	}
}