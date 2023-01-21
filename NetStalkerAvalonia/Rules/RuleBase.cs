using NetStalkerAvalonia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using ReactiveUI;

namespace NetStalkerAvalonia.Rules
{
	public abstract class RuleBase : ReactiveObject, IRule, IEquatable<RuleBase>
	{
		public abstract RuleAction Action { get; }
		public RuleSourceValue SourceValue { get; protected set; }
		public bool IsRegex { get; protected set; }
		public string Target { get; protected set; }
		public int Order { get; set; }
		public bool Active { get; protected set; }

		protected RuleBase(RuleSourceValue sourceValue, bool isRegex, string target, int order, bool active)
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

			return this.Target == roleToCompare.Target;
		}

		public bool Equals(RuleBase? other)
		{
			return Equals(other);
		}

		public override int GetHashCode()
		{
			return this.Target.GetHashCode();
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