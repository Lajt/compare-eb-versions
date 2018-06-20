using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using Loki;
using Loki.Bot;
using Loki.Common;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;
using Newtonsoft.Json;

namespace Legacy.ItemFilterEditor
{
	/// <summary>
	/// The default configurable item evaluator.
	/// </summary>
	public class ConfigurableItemEvaluator : IItemEvaluator
	{
		private static readonly ILog Log = Logger.GetLoggerInstanceForType();

		private static ConfigurableItemEvaluator _instance;

		public static ConfigurableItemEvaluator Instance
		{
			get
			{
				if (_instance == null)
				{
					if (File.Exists(DefaultPath))
					{
						_instance = new ConfigurableItemEvaluator();
						_instance.Load(DefaultPath);
					}
					else
					{
						_instance = CreateDefaultItemEvaluator();
						_instance.Save(DefaultPath);
					}
				}
				return _instance;
			}
		}

        /// <summary>
        /// The name of this item evaluator.
        /// </summary>
        public string Name => "ConfigurableItemEvaluator";

		private ObservableCollection<Category> _categories = new ObservableCollection<Category>();

		/// <summary> </summary>
		public ObservableCollection<Category> Categories
		{
			get
			{
				return _categories;
			}
			set
			{
				_categories = value;
			}
		}

		/// <summary>
		/// Attempts to match an item based on the specified evaluation type.
		/// </summary>
		/// <param name="item">The item to match.</param>
		/// <param name="type">The type of filter to match.</param>
		/// <param name="matchedFilter">The filtered matched if the function returns true.</param>
		/// <returns>true if the item matches and false if not.</returns>
		public bool Match(Item item, EvaluationType type_)
		{
			var type = (MyEvaluationType) ((int)(type_));

			// Read once, as we may have tons of filters.
			var cachedItemName = item.Name;
			var cachedItemType = item.Metadata;
			var cachedItemLevel = item.ItemLevel;
			var cachedItemRarity = item.Rarity;
			var cachedItemQuality = item.Quality;
			var cachedItemSize = item.Size;
			var cachedItemIded = item.IsIdentified;
			var cachedItemStats = item.Stats;
			var cachedItemImplicitStats = item.ImplicitStats;
			var cachedItemExplicitStats = item.ExplicitStats;
			var cachedHighestImplicitValue = item.HighestImplicitValue;
			var cachedIsElderItem = item.IsElderItem;
			var cachedIsShaperItem = item.IsShaperItem;
			var cachedIsElderOrShaper = cachedIsElderItem || cachedIsShaperItem;

			var cachedItemSockets = new[]
				{
					SocketColor.None, SocketColor.None, SocketColor.None, SocketColor.None, SocketColor.None, SocketColor.None
				};
			var cachedItemLinks = new byte[6];

			if (item.Components.SocketsComponent != null)
			{
				cachedItemSockets = item.SocketColors;
				cachedItemLinks = item.SocketLinks;
			}

			var cachedSockets = 0;
			for (var x = 0; x < 6; ++x)
			{
				if (cachedItemSockets[x] != SocketColor.None)
				{
					cachedSockets++;
				}
			}

			// New feature to limit picking up SoW/Portal based on inventory count. This is just a hardcoded feature and ideally would be
			// more flexible in the next item filter version.
			if (type == MyEvaluationType.PickUp && (ItemFilterEditorSettings.Instance.LimitWisdomPickup || ItemFilterEditorSettings.Instance.LimitPortalPickup))
			{
				if (item.HasMetadataFlags(MetadataFlags.CurrencyIdentification)) // Scroll of Wisdom
				{
					var mainInv = LokiPoe.InstanceInfo.GetPlayerInventoryBySlot(InventorySlot.Main);
					var sowCount = mainInv.GetTotalItemQuantityByMetadataFlags(MetadataFlags.CurrencyIdentification);
					if (sowCount + item.StackCount > ItemFilterEditorSettings.Instance.WisdomPickupLimit)
					{
						if (ItemFilterEditorSettings.Instance.DebugPickupLimit)
						{
							Log.Debug($"[ConfigurableItemEvaluator.Match] Not looting [{item.WorldItemId}] {item.FullName} because we already have {sowCount} / {ItemFilterEditorSettings.Instance.WisdomPickupLimit} in the inventory.");
						}
						return false;
					}
				}
				else if (item.HasMetadataFlags(MetadataFlags.CurrencyPortal)) // Portal Scroll
				{
					var mainInv = LokiPoe.InstanceInfo.GetPlayerInventoryBySlot(InventorySlot.Main);
					var portalCount = mainInv.GetTotalItemQuantityByMetadataFlags(MetadataFlags.CurrencyPortal);
					if (portalCount + item.StackCount > ItemFilterEditorSettings.Instance.PortalPickupLimit)
					{
						if (ItemFilterEditorSettings.Instance.DebugPickupLimit)
						{
							Log.Debug($"[ConfigurableItemEvaluator.Match] Not looting [{item.WorldItemId}] {item.FullName} because we already have {portalCount} / {ItemFilterEditorSettings.Instance.PortalPickupLimit} in the inventory.");
						}
						return false;
					}
				}
			}

			// Pfft, who needs fancy logic when it'll be an O(n) operation no matter what. Might as well keep things simple and easy to read.
			foreach (Category category in _categories)
			{
				// Ignore evaluations other than the requested.
				if (category.Type != type)
				{
					continue;
				}

				foreach (var filter in category.Filters)
				{
					// Filter is disabled, ignore it.
					if (!filter.Enabled)
					{
						continue;
					}

					var isEmptyFilter = true;

					if (filter.Types != null && filter.Types.Count > 0)
					{
						isEmptyFilter = false;
						var matches = false;

						var typeMatchAnyRatherThanAll = true;
						if (filter.TypeMatchAnyRatherThanAll.HasValue)
						{
							typeMatchAnyRatherThanAll = filter.TypeMatchAnyRatherThanAll.Value;
						}

						if (typeMatchAnyRatherThanAll)
						{
							foreach (string t in filter.Types)
							{
								if (filter.TypeRegex == true)
								{
									Match match = Regex.Match(cachedItemType, t, RegexOptions.IgnoreCase);
									if (match.Success)
									{
										matches = true;
										break;
									}
								}
								else
								{
									if (cachedItemType.Contains(t))
									{
										matches = true;
										break;
									}
								}
							}
						}
						else
						{
							matches = true;
							foreach (string t in filter.Types)
							{
								if (filter.TypeRegex == true)
								{
									Match match = Regex.Match(cachedItemType, t, RegexOptions.IgnoreCase);
									if (!match.Success)
									{
										matches = false;
										break;
									}
								}
								else
								{
									if (cachedItemType.Contains(t))
									{
										matches = false;
										break;
									}
								}
							}
						}

						if (!matches)
						{
							continue;
						}
					}

					if (filter.Names != null && filter.Names.Count > 0)
					{
						isEmptyFilter = false;
						var matches = false;

						var nameMatchAnyRatherThanAll = true;
						if (filter.NameMatchAnyRatherThanAll.HasValue)
						{
							nameMatchAnyRatherThanAll = filter.NameMatchAnyRatherThanAll.Value;
						}

						if (nameMatchAnyRatherThanAll)
						{
							foreach (string t in filter.Names)
							{
								if (filter.NameRegex == true)
								{
									Match match = Regex.Match(cachedItemName, t, RegexOptions.IgnoreCase);
									if (match.Success)
									{
										matches = true;
										break;
									}
								}
								else
								{
									if (cachedItemName.Contains(t))
									{
										matches = true;
										break;
									}
								}
							}
						}
						else
						{
							matches = true;
							foreach (string t in filter.Names)
							{
								if (filter.NameRegex == true)
								{
									Match match = Regex.Match(cachedItemName, t, RegexOptions.IgnoreCase);
									if (!match.Success)
									{
										matches = false;
										break;
									}
								}
								else
								{
									if (cachedItemName.Contains(t))
									{
										matches = false;
										break;
									}
								}
							}
						}

						if (!matches)
						{
							continue;
						}
					}

					if (filter.HasToBeShaper.HasValue)
					{
						isEmptyFilter = false;
						if (filter.HasToBeShaper == true)
						{
							if (!cachedIsShaperItem)
							{
								continue;
							}
						}
						else if (filter.HasToBeShaper == false)
						{
							if (cachedIsShaperItem)
							{
								continue;
							}
						}
					}

					if (filter.HasToBeElder.HasValue)
					{
						isEmptyFilter = false;
						if (filter.HasToBeElder == true)
						{
							if (!cachedIsElderItem)
							{
								continue;
							}
						}
						else if (filter.HasToBeElder == false)
						{
							if (cachedIsElderItem)
							{
								continue;
							}
						}
					}

					if (filter.HasToBeElderOrShaper.HasValue)
					{
						isEmptyFilter = false;
						if (filter.HasToBeElderOrShaper == true)
						{
							if (!cachedIsElderOrShaper)
							{
								continue;
							}
						}
						else if (filter.HasToBeElderOrShaper == false)
						{
							if (cachedIsElderOrShaper)
							{
								continue;
							}
						}
					}

					if (filter.HasToBeIdentified.HasValue)
					{
						isEmptyFilter = false;
						if (filter.HasToBeIdentified == true)
						{
							if (!cachedItemIded)
							{
								continue;
							}
						}
						else if (filter.HasToBeIdentified == false)
						{
							if (cachedItemIded)
							{
								continue;
							}
						}
					}

					if (filter.HasToBeUnidentified.HasValue)
					{
						isEmptyFilter = false;
						if (filter.HasToBeUnidentified == true)
						{
							if (cachedItemIded)
							{
								continue;
							}
						}
						else if (filter.HasToBeUnidentified == false)
						{
							if (!cachedItemIded)
							{
								continue;
							}
						}
					}

					if (filter.MinWidth.HasValue)
					{
						isEmptyFilter = false;
						if (cachedItemSize.X < filter.MinWidth)
						{
							continue;
						}
					}

					if (filter.MaxWidth.HasValue)
					{
						isEmptyFilter = false;
						if (cachedItemSize.X > filter.MaxWidth)
						{
							continue;
						}
					}

					if (filter.MinHeight.HasValue)
					{
						isEmptyFilter = false;
						if (cachedItemSize.Y < filter.MinHeight)
						{
							continue;
						}
					}

					if (filter.MaxHeight.HasValue)
					{
						isEmptyFilter = false;
						if (cachedItemSize.Y > filter.MaxHeight)
						{
							continue;
						}
					}

					if (filter.ItemLevelMin.HasValue)
					{
						isEmptyFilter = false;
						if (cachedItemLevel < filter.ItemLevelMin)
						{
							continue;
						}
					}

					if (filter.ItemLevelMax.HasValue)
					{
						isEmptyFilter = false;
						if (cachedItemLevel > filter.ItemLevelMax)
						{
							continue;
						}
					}

					if (filter.HighestImplicitValue.HasValue)
					{
						isEmptyFilter = false;
						if (cachedHighestImplicitValue < filter.HighestImplicitValue)
						{
							continue;
						}
					}

					if (filter.Rarities != null)
					{
						if (filter.Rarities.Count > 0)
							isEmptyFilter = false;
						if (filter.Rarities.Count != 0 && !filter.Rarities.Contains(cachedItemRarity))
						{
							continue;
						}
					}

					if (filter.MinQuality.HasValue)
					{
						isEmptyFilter = false;
						if (cachedItemQuality < filter.MinQuality)
						{
							continue;
						}
					}

					if (filter.MaxQuality.HasValue)
					{
						isEmptyFilter = false;
						if (cachedItemQuality > filter.MaxQuality)
						{
							continue;
						}
					}

					if (filter.MinSockets.HasValue)
					{
						isEmptyFilter = false;
						if (cachedSockets < filter.MinSockets)
						{
							continue;
						}
					}

					if (filter.MaxSockets.HasValue)
					{
						isEmptyFilter = false;
						if (cachedSockets > filter.MaxSockets)
						{
							continue;
						}
					}

					if (filter.MinLinks.HasValue)
					{
						isEmptyFilter = false;
						bool matches = false;
						for (int x = 0; x < 6; ++x)
						{
							if (cachedItemLinks[x] >= filter.MinLinks)
							{
								matches = true;
								break;
							}
						}
						if (!matches)
						{
							continue;
						}
					}

					if (filter.MaxLinks.HasValue)
					{
						isEmptyFilter = false;
						bool matches = true;
						for (int x = 0; x < 6; ++x)
						{
							if (cachedItemLinks[x] > filter.MaxLinks)
							{
								matches = false;
								break;
							}
						}
						if (!matches)
						{
							continue;
						}
					}

					// Save the most complex/time-consuming logic for last, which it's not as bad now that
					// it uses new logic, but still!
					if (filter.SocketColors != null)
					{
						isEmptyFilter = false;
						if (filter.SocketColors.Count > 0)
						{
							bool matched = false;
							// ReSharper disable once LoopCanBeConvertedToQuery
							foreach (string socketColor in filter.SocketColors)
							{
								if (LokiPoe.MatchesSocketColors(socketColor, cachedItemSockets, cachedItemLinks))
								{
									matched = true;
									break;
								}
							}
							if (!matched)
							{
								continue;
							}
						}
					}

					// Yay, affix matching via stats.
					if (filter.Affixes != null && filter.Affixes.Count > 0)
					{
						isEmptyFilter = false;

						var groupOptionals = new Dictionary<int, int>();
						var groupOptionalChecks = new Dictionary<int, bool>();

						var doesNotMatch = false;
						foreach (var affix in filter.Affixes)
						{
							StatCategory c;
							StatTypeGGG t;
							int value;
							StatValueOperation operation;
							StatRequirement requirement;
							int group;

							if (
								!ParseStatFilter(affix, out c, out t, out value, out operation, out group,
									out requirement))
							{
								Log.ErrorFormat(
									"[Match] One of your item filters is invalid. Please correct it before starting the bot again.");
								doesNotMatch = true;
								break;
							}

							// If we have a group, mark the # so we can check group matches at the end.
							if (requirement == StatRequirement.Optional)
							{
								if (!groupOptionalChecks.ContainsKey(group))
									groupOptionalChecks.Add(group, true);

								if (!groupOptionals.ContainsKey(group))
									groupOptionals.Add(group, 0);
							}

							if (c == StatCategory.Total)
							{
								StatLogicHelper(cachedItemStats, t, value, operation, requirement, group,
									ref groupOptionals, ref doesNotMatch);

								if (doesNotMatch)
									break;
							}
							else if (c == StatCategory.Implicit)
							{
								StatLogicHelper(cachedItemImplicitStats, t, value, operation, requirement, group,
									ref groupOptionals, ref doesNotMatch);

								if (doesNotMatch)
									break;
							}
							else if (c == StatCategory.Explicit)
							{
								StatLogicHelper(cachedItemExplicitStats, t, value, operation, requirement, group,
									ref groupOptionals, ref doesNotMatch);

								if (doesNotMatch)
									break;
							}
						}

						// If a required filter failed, this filter cannot match.
						if (doesNotMatch)
							continue;

						// Finally, optional group matching checks.
						// ReSharper disable once LoopCanBeConvertedToQuery
						foreach (var kvp in groupOptionalChecks)
						{
							if (groupOptionals.ContainsKey(kvp.Key))
							{
								// We have to match at least one optional per group.
								if (groupOptionals[kvp.Key] == 0)
								{
									doesNotMatch = true;
									break;
								}
							}
						}

						if (doesNotMatch)
							continue;
					}

					// If we get here, all of the filter matched. However, don't
					// match empty filters, ever.
					if (!isEmptyFilter)
					{
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>Returns the default item filter path for the current configuration.</summary>
		public static string DefaultPath => JsonSettings.GetSettingsFilePath(Configuration.Instance.Name,
			string.Format("{0}.json", "StaticItemFilter"));

		// We can't use a static instance, since we will have multiple sets of filters as well as 
		// our current default item code is setup in Registry. ItemEvaluator static ctor runs before Registry,
		// so an empty filter is created, then Registry doesn't create one, and everyone loses their mind because
		// the bot doesn't loot anything.

		private void StatLogicHelper(Dictionary<StatTypeGGG, int> src, StatTypeGGG t, int value, StatValueOperation operation,
			StatRequirement requirement, int group, ref Dictionary<int, int> groupOptionals, ref bool doesNotMatch)
		{
			int v;
			if (src.TryGetValue(t, out v))
			{
				// We don't care about the value, only the presence of the stat.
				if (operation == StatValueOperation.None)
				{
					// If we have an optional filter and match, we should add an optional group match so the filter
					// can pass based on 1 optional value for the group
					if (requirement == StatRequirement.Optional)
					{
						groupOptionals[group]++;
					}
					return;
				}

				if (operation == StatValueOperation.E)
				{
					if (value == v)
					{
						// If we have an optional filter and match, we should add an optional group match so the filter
						// can pass based on 1 optional value for the group
						if (requirement == StatRequirement.Optional)
						{
							groupOptionals[group]++;
						}
						return;
					}

					// If the value does not match, and the filter requires it, we cannot match.
					if (requirement == StatRequirement.Required)
					{
						doesNotMatch = true;
						// ReSharper disable once RedundantJumpStatement
						return;
					}
				}
				else if (operation == StatValueOperation.NE)
				{
					if (value != v)
					{
						// If we have an optional filter and match, we should add an optional group match so the filter
						// can pass based on 1 optional value for the group
						if (requirement == StatRequirement.Optional)
						{
							groupOptionals[group]++;
						}
						return;
					}

					// If the value does not match, and the filter requires it, we cannot match.
					if (requirement == StatRequirement.Required)
					{
						doesNotMatch = true;
						// ReSharper disable once RedundantJumpStatement
						return;
					}
				}
				else if (operation == StatValueOperation.LT)
				{
					if (v < value)
					{
						// If we have an optional filter and match, we should add an optional group match so the filter
						// can pass based on 1 optional value for the group
						if (requirement == StatRequirement.Optional)
						{
							groupOptionals[group]++;
						}
						return;
					}

					// If the value does not match, and the filter requires it, we cannot match.
					if (requirement == StatRequirement.Required)
					{
						doesNotMatch = true;
						// ReSharper disable once RedundantJumpStatement
						return;
					}
				}
				else if (operation == StatValueOperation.LTE)
				{
					if (v <= value)
					{
						// If we have an optional filter and match, we should add an optional group match so the filter
						// can pass based on 1 optional value for the group
						if (requirement == StatRequirement.Optional)
						{
							groupOptionals[group]++;
						}
						return;
					}

					// If the value does not match, and the filter requires it, we cannot match.
					if (requirement == StatRequirement.Required)
					{
						doesNotMatch = true;
						// ReSharper disable once RedundantJumpStatement
						return;
					}
				}
				else if (operation == StatValueOperation.GT)
				{
					if (v > value)
					{
						// If we have an optional filter and match, we should add an optional group match so the filter
						// can pass based on 1 optional value for the group
						if (requirement == StatRequirement.Optional)
						{
							groupOptionals[group]++;
						}
						return;
					}

					// If the value does not match, and the filter requires it, we cannot match.
					if (requirement == StatRequirement.Required)
					{
						doesNotMatch = true;
						// ReSharper disable once RedundantJumpStatement
						return;
					}
				}
				else if (operation == StatValueOperation.GTE)
				{
					if (v >= value)
					{
						// If we have an optional filter and match, we should add an optional group match so the filter
						// can pass based on 1 optional value for the group
						if (requirement == StatRequirement.Optional)
						{
							groupOptionals[group]++;
						}
						return;
					}

					// If the value does not match, and the filter requires it, we cannot match.
					if (requirement == StatRequirement.Required)
					{
						doesNotMatch = true;
						// ReSharper disable once RedundantJumpStatement
						return;
					}
				}
			}
			else
			{
				// If the stat does not exist, and the filter requires it, we cannot match.
				if (requirement == StatRequirement.Required)
				{
					doesNotMatch = true;
					// ReSharper disable once RedundantJumpStatement
					return;
				}
			}
		}

		/// <summary>
		/// Verifies the affix filters to make sure there are no syntax errors.
		/// </summary>
		/// <returns></returns>
		public bool VerifyFilters()
		{
			// Pfft, who needs fancy logic when it'll be an O(n) operation no matter what. Might as well keep things simple and easy to read.
			foreach (var category in _categories)
			{
				foreach (var filter in category.Filters)
				{
					// Yay, affix matching via stats.
					if (filter.Affixes != null && filter.Affixes.Count > 0)
					{
						foreach (var affix in filter.Affixes)
						{
							StatCategory c;
							StatTypeGGG t;
							int value;
							StatValueOperation operation;
							StatRequirement requirement;
							int group;

							if (
								!ParseStatFilter(affix, out c, out t, out value, out operation, out group,
									out requirement))
							{
								return false;
							}
						}
					}
				}
			}
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filter">The filter to parse.</param>
		/// <param name="category"></param>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <param name="operation"></param>
		/// <param name="group"></param>
		/// <param name="requirement"></param>
		/// <returns></returns>
		public static bool ParseStatFilter(string filter, out StatCategory category, out StatTypeGGG type, out int value,
			out StatValueOperation operation, out int group, out StatRequirement requirement)
		{
			category = StatCategory.None;
			type = StatTypeGGG.Level;
			value = 0;
			operation = StatValueOperation.None;
			requirement = StatRequirement.Optional;
			group = -1;

			var parts = filter.Split(new[]
				{
					' '
				}, StringSplitOptions.RemoveEmptyEntries).ToList();

			if (!parts.Any())
			{
				Log.ErrorFormat("[ParseStatFilter] The filter \"{0}\" is missing required data.", filter);
				return false;
			}

			var p0 = parts[0];
			parts.RemoveAt(0);
			if (p0 == "ExplicitStat")
			{
				category = StatCategory.Explicit;
			}
			else if (p0 == "ImplicitStat")
			{
				category = StatCategory.Implicit;
			}
			else if (p0 == "TotalStat")
			{
				category = StatCategory.Total;
			}
			else
			{
				Log.ErrorFormat(
					"[ParseStatFilter] \"{0}\" is not valid. Valid options are [ExplicitStat, ImplicitStat, TotalStat]",
					p0);
				return false;
			}

			if (!parts.Any())
			{
				Log.ErrorFormat("[ParseStatFilter] The filter \"{0}\" is missing required data.", filter);
				return false;
			}

			var p1 = parts[0];
			parts.RemoveAt(0);

			var stats = LokiPoe.StatNameToType;

			if (!stats.TryGetValue(p1, out type))
			{
				Log.ErrorFormat("[ParseStatFilter] \"{0}\" is not a valid stat.", p1);
				return false;
			}

			// Nothing else has to be here.
			if (!parts.Any())
			{
				return true;
			}

			var p2 = parts[0];
			parts.RemoveAt(0);
			if (p2 == "Required")
			{
				requirement = StatRequirement.Required;

				// Nothing else has to be here.
				if (!parts.Any())
				{
					return true;
				}

				Log.ErrorFormat("[ParseStatFilter] The filter \"{0}\" contains extra data.", filter);

				return false;
			}

			if (p2 == "Optional")
			{
				requirement = StatRequirement.Optional;

				// Nothing else has to be here.
				if (!parts.Any())
				{
					return true;
				}

				var s3 = parts[0];
				parts.RemoveAt(0);
				if (s3 != "Group")
				{
					Log.ErrorFormat("[ParseStatFilter] \"{0}\" is not valid. Valid options are [Group]", s3);
					return false;
				}

				if (!parts.Any())
				{
					Log.ErrorFormat("[ParseStatFilter] The filter \"{0}\" is missing required data.", filter);
					return false;
				}

				var s4 = parts[0];
				parts.RemoveAt(0);
				if (!Int32.TryParse(s4, out group))
				{
					Log.ErrorFormat("[ParseStatFilter] \"{0}\" is not a valid integer.", s4);
					return false;
				}

				if (!parts.Any())
					return true;

				Log.ErrorFormat("[ParseStatFilter] The filter \"{0}\" contains extra data.", filter);

				return false;
			}

			if (p2 != "Value")
			{
				Log.ErrorFormat(
					"[ParseStatFilter] \"{0}\" is not valid. Valid options are [Required, Optional, Value]", p2);
				return false;
			}

			if (!parts.Any())
			{
				Log.ErrorFormat("[ParseStatFilter] The filter \"{0}\" is missing required data.", filter);
				return false;
			}

			var p3 = parts[0];
			parts.RemoveAt(0);
			if (p3 == "<")
			{
				operation = StatValueOperation.LT;
			}
			else if (p3 == ">")
			{
				operation = StatValueOperation.GT;
			}
			else if (p3 == "=")
			{
				operation = StatValueOperation.E;
			}
			else if (p3 == "==")
			{
				operation = StatValueOperation.E;
			}
			else if (p3 == "<=")
			{
				operation = StatValueOperation.LTE;
			}
			else if (p3 == ">=")
			{
				operation = StatValueOperation.GTE;
			}
			else if (p3 == "!=")
			{
				operation = StatValueOperation.NE;
			}
			else if (p3 == "<>")
			{
				operation = StatValueOperation.NE;
			}

			if (!parts.Any())
			{
				Log.ErrorFormat("[ParseStatFilter] The filter \"{0}\" is missing required data.", filter);
				return false;
			}

			var p4 = parts[0];
			parts.RemoveAt(0);

			if (!Int32.TryParse(p4, out value))
			{
				Log.ErrorFormat("[ParseStatFilter] \"{0}\" is not a valid integer.", p4);
				return false;
			}

			// Nothing else has to be here.
			if (!parts.Any())
			{
				return true;
			}

			var p5 = parts[0];
			parts.RemoveAt(0);
			if (p5 == "Required")
			{
				requirement = StatRequirement.Required;

				// Nothing else has to be here.
				if (!parts.Any())
				{
					return true;
				}

				Log.ErrorFormat("[ParseStatFilter] The filter \"{0}\" contains extra data.", filter);

				return false;
			}

			if (p5 == "Optional")
			{
				requirement = StatRequirement.Optional;

				// Nothing else has to be here.
				if (!parts.Any())
				{
					return true;
				}

				var s6 = parts[0];
				parts.RemoveAt(0);
				if (s6 != "Group")
				{
					Log.ErrorFormat("[ParseStatFilter] \"{0}\" is not valid. Valid options are [Group]", s6);
					return false;
				}

				if (!parts.Any())
				{
					Log.ErrorFormat("[ParseStatFilter] The filter \"{0}\" is missing required data.", filter);
					return false;
				}

				var s7 = parts[0];
				parts.RemoveAt(0);
				if (!Int32.TryParse(s7, out group))
				{
					Log.ErrorFormat("[ParseStatFilter] \"{0}\" is not a valid integer.", s7);
					return false;
				}

				if (!parts.Any())
					return true;

				Log.ErrorFormat("[ParseStatFilter] The filter \"{0}\" contains extra data.", filter);

				return false;
			}

			Log.ErrorFormat("[ParseStatFilter] The filter \"{0}\" contains extra data.", filter);

			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="category"></param>
		public void AddCategory(Category category)
		{
			_categories.Add(category);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filename"></param>
		public void Save(string filename)
		{
			try
			{
				var dir = Path.GetDirectoryName(filename);

				if (!String.IsNullOrEmpty(dir))
				{
					if (!Directory.Exists(dir))
					{
						Directory.CreateDirectory(dir);
					}
				}

				var settings = new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore,
					MissingMemberHandling = MissingMemberHandling.Ignore
				};

				File.WriteAllText(filename, JsonConvert.SerializeObject(_categories, Formatting.Indented, settings));

				ItemEvaluator.Refresh();
			}
			catch (Exception ex)
			{
				Log.Error("[Save] Error while saving the item filter.", ex);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filename"></param>
		public void Load(string filename)
		{
			if (!File.Exists(filename))
			{
				// Save a default copy of the file.
				// Even if it has nothing in it, we don't want to throw exceptions here.
				Log.Info("Tried to load the item evaluation rules, but could not find the file (" + filename +
						 "). Have you set up your rules yet?");

				Save(filename);
			}

			var settings = new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				DefaultValueHandling = DefaultValueHandling.Populate
			};

			_categories = JsonConvert.DeserializeObject<ObservableCollection<Category>>(File.ReadAllText(filename),
				settings);
		}

		/// <summary>
		/// Creates an item evalutor with default presets.
		/// </summary>
		/// <returns></returns>
		public static ConfigurableItemEvaluator CreateDefaultItemEvaluator()
		{
			var itemEvaluator = new ConfigurableItemEvaluator();

			// ExcludeFromChaosRecipe
			{
				var excludeCategory = new Category
				{
					Description = "Exclude From ChaosRecipe Plugin",
					Type = MyEvaluationType.ExcludeFromChaosRecipe
				};

				{
					var itemFilter = new Filter
					{
						Description = "Elder/Shaper Items",
						HasToBeElderOrShaper = true,
						Enabled = false
					};
					excludeCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Description = "Stygian Vise",
						Types = new ObservableCollection<string>
							{
								"Metadata/Items/Belts/BeltAbyss",
							},
						Enabled = false
					};
					excludeCategory.Filters.Add(itemFilter);
				}

				itemEvaluator.AddCategory(excludeCategory);
			}

			// Save
			{
				var dnsCategory = new Category
				{
					Description = "Save filters",
					Type = MyEvaluationType.Save
				};

				{
					var itemFilter = new Filter
					{
						Description = "Currency, uniques, and skill gems",
						SocketColors = new ObservableCollection<string>(),
						Rarities = new List<Rarity>
							{
								Rarity.Currency,
								Rarity.Unique,
								Rarity.Gem
							}
					};
					dnsCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Description = "5+ linked items",
						MinLinks = 5
					};
					dnsCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Description = "Maps and fragments",
						Types = new ObservableCollection<string>
							{
								"/Maps/",
								"/MapFragments/"
							}
					};
					dnsCategory.Filters.Add(itemFilter);
				}

				itemEvaluator.AddCategory(dnsCategory);
			}

			// Pick
			{
				var pickCategory = new Category
				{
					Description = "Pickup filters",
					Type = MyEvaluationType.PickUp
				};

				{
					var itemFilter = new Filter
					{
						Description = "Currency",
						NameMatchAnyRatherThanAll = true,
						Rarities = new List<Rarity>
							{
								Rarity.Currency
							},
						Types = new ObservableCollection<string>
							{
								"/Currency/"
							}
					};
					pickCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Description = "Elder / Shaper Items",
						HasToBeElderOrShaper = true
					};
					pickCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Description = "All rares",
						Rarities = new List<Rarity>
							{
								Rarity.Rare
							}
					};
					pickCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Enabled = false,
						NameMatchAnyRatherThanAll = true,
						Description = "Item types for valuable uniques",
						Names = new ObservableCollection<string>(),
						Rarities = new List<Rarity>
							{
								Rarity.Normal
							}
					};
					itemFilter.Names.Add("Occultist's Vestment");
					itemFilter.Names.Add("Glorious Plate");
					itemFilter.Names.Add("Siege Axe");
					itemFilter.Names.Add("Spine Bow");
					pickCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Description = "Fishing rods",
						Types = new ObservableCollection<string>
							{
								"/FishingRods/"
							}
					};
					pickCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Description = "Incursion Items",
						Types = new ObservableCollection<string>
							{
								"/Incursion/"
							}
					};
					pickCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Description = "Divination Cards",
						Types = new ObservableCollection<string>
							{
								"/DivinationCards/"
							}
					};
					pickCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Description = "Leaguestones",
						Types = new ObservableCollection<string>
							{
								"/Leaguestones/"
							}
					};
					pickCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Description = "Jewels",
						Types = new ObservableCollection<string>
							{
								"/Jewels/"
							}
					};
					pickCategory.Filters.Add(itemFilter);
				}

                {
                    var itemFilter = new Filter
                    {
                        Description = "Labyrinth",
                        Types = new ObservableCollection<string>
                            {
                                "/Labyrinth/"
                            }
                    };
                    pickCategory.Filters.Add(itemFilter);
                }

                {
                    var itemFilter = new Filter
					{
						Description = "Maps",
						Types = new ObservableCollection<string>
							{
								"/Maps/"
							}
					};
					pickCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Description = "Map fragments",
						Types = new ObservableCollection<string>
							{
								"/MapFragments/"
							}
					};
					pickCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Description = "Quest and uniques",
						Rarities = new List<Rarity>
							{
								Rarity.Unique,
								Rarity.Quest
							}
					};
					pickCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Description = "All skill gems",
						NameMatchAnyRatherThanAll = true,
						Rarities = new List<Rarity>
							{
								Rarity.Gem
							},
						Types = new ObservableCollection<string>
							{
								"/Gems/"
							}
					};
					pickCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Description = "Items worth a Chromatic Orb",
						SocketColors = new ObservableCollection<string>
							{
								"R-G-B"
							}
					};
					pickCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Description = "5 and 6-linked items",
						MinLinks = 5
					};
					pickCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Description = "6-socket items",
						MinSockets = 6
					};
					pickCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Enabled = false,
						Description = "Rare accessories",
						Rarities = new List<Rarity>
							{
								Rarity.Rare
							},
						Types = new ObservableCollection<string>
							{
								"/Rings/",
								"/Amulets/"
							},
						TypeMatchAnyRatherThanAll = true
					};
					pickCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Enabled = false,
						Description =
							"Items worth an Armourer's Scrap, Blacksmith Whetstone, or Glassblower's Bauble.",
						MinQuality = 20,
						MaxQuality = 20,
						Rarities = new List<Rarity>
							{
								Rarity.Normal
							},
						Types =
							new ObservableCollection<string>
								{
									"Metadata/Items/Weapons/",
									"Metadata/Items/Armours/",
									"Metadata/Items/Flasks/"
								},
						TypeMatchAnyRatherThanAll = true
					};
					pickCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Description = "Any quality flask",
						Enabled = false,
						Types = new ObservableCollection<string>
							{
								"/Flasks/"
							},
						MinQuality = 1
					};
					pickCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Description = "Utility flasks",
						Enabled = false,
						Types = new ObservableCollection<string>
							{
								"/FlaskUtility"
							}
					};
					pickCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Description = "Talismans",
						Types = new ObservableCollection<string>
							{
								"/Talismans/"
							}
					};
					pickCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Enabled = false,
						Description = "All 2x2 4 linked objects.",
						MinLinks = 4,
						MaxWidth = 2,
						MaxHeight = 2
					};
					pickCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Enabled = false,
						Description = "All magic items",
						Rarities = new List<Rarity>
							{
								Rarity.Magic
							}
					};
					pickCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Enabled = false,
						Description = "Perfect White Coral Rings",
						Rarities = new List<Rarity>
							{
								Rarity.Normal
							},
						Names = new ObservableCollection<string>
							{
								"Coral Ring"
							},
						Affixes = new ObservableCollection<string>
							{
								"ImplicitStat BaseMaximumLife Value == 30"
							}
					};
					pickCategory.Filters.Add(itemFilter);
				}

				itemEvaluator.AddCategory(pickCategory);
			}

			// Id
			{
				var idCategory = new Category
				{
					Description = "Id filters",
					Type = MyEvaluationType.Id
				};

				// NOTE: We don't have to set HasToBeUnidentified because bot logic checks it internally, 
				// as we can't id an already identified item.

				{
					var itemFilter = new Filter
					{
						Enabled = false,
						Description = "Id all rares under ilvl 60.",
						Rarities = new List<Rarity>
							{
								Rarity.Rare
							},
						ItemLevelMax = 59
					};
					idCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Enabled = false,
						Description = "Id all magic items.",
						Rarities = new List<Rarity>
							{
								Rarity.Magic
							}
					};
					idCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Enabled = false,
						Description = "Id all rares except accessories.",
						Rarities = new List<Rarity>
							{
								Rarity.Rare
							},
						Types = new ObservableCollection<string>
							{
								"/Rings/",
								"/Amulets/"
							},
						TypeMatchAnyRatherThanAll = false
					};
					idCategory.Filters.Add(itemFilter);
				}

				itemEvaluator.AddCategory(idCategory);
			}

			// Sell
			{
				var sellCategory = new Category
				{
					Description = "Vendor filters",
					Type = MyEvaluationType.Sell
				};

				{
					var itemFilter = new Filter
					{
						Description = "Unvaluable chromatic items",
						SocketColors = new ObservableCollection<string>
							{
								"R-G-B"
							},
						Rarities = new List<Rarity>
							{
								Rarity.Normal,
								Rarity.Magic
							},
						MaxLinks = 4,
						MaxSockets = 5
					};
					sellCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Enabled = false,
						Description = "All identified rares",
						Rarities = new List<Rarity>
							{
								Rarity.Rare
							},
						HasToBeIdentified = true
					};
					sellCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Enabled = false,
						Description = "All identified rares except accessories.",
						Rarities = new List<Rarity>
							{
								Rarity.Rare
							},
						HasToBeIdentified = true,
						TypeMatchAnyRatherThanAll = false,
						Types = new ObservableCollection<string>
							{
								"/Amulets/",
								"/Rings/"
							}
					};
					sellCategory.Filters.Add(itemFilter);
				}

				{
					var itemFilter = new Filter
					{
						Enabled = false,
						Description = "All identified magics.",
						Rarities = new List<Rarity>
							{
								Rarity.Magic
							},
						HasToBeIdentified = true
					};
					sellCategory.Filters.Add(itemFilter);
				}

				itemEvaluator.AddCategory(sellCategory);
			}

			// Buy
			{
				var buyCategory = new Category
				{
					Description = "Buy filters (not implemented)",
					Type = MyEvaluationType.Buy
				};

				itemEvaluator.AddCategory(buyCategory);
			}

			return itemEvaluator;
		}
	}
}
