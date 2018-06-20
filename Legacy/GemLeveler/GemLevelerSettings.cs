using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Loki;
using Loki.Common;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;
using Newtonsoft.Json;

namespace Legacy.GemLeveler
{
	/// <summary>Settings for the GemLeveler plugin. </summary>
	public class GemLevelerSettings : JsonSettings
	{
		private static GemLevelerSettings _instance;

		/// <summary>The current instance for this class. </summary>
		public static GemLevelerSettings Instance => _instance ?? (_instance = new GemLevelerSettings());

		/// <summary>The default ctor. Will use the settings path "GemLeveler".</summary>
		public GemLevelerSettings()
			: base(GetSettingsFilePath(Configuration.Instance.Name, string.Format("{0}.json", "GemLeveler")))
		{
			if (_globalNameIgnoreList == null)
			{
				_globalNameIgnoreList = new ObservableCollection<string>();
			}
			if (_skillGemsToLevelList == null)
			{
				_skillGemsToLevelList = new ObservableCollection<string>();
			}
		}

		private bool _debugStatements;
		private ObservableCollection<string> _globalNameIgnoreList;
		private ObservableCollection<string> _skillGemsToLevelList;

		/// <summary>
		/// Should the plugin log debug statements?
		/// </summary>
		[DefaultValue(false)]
		public bool DebugStatements
		{
			get
			{
				return _debugStatements;
			}
			set
			{
				if (value.Equals(_debugStatements))
				{
					return;
				}
				_debugStatements = value;
				NotifyPropertyChanged(() => DebugStatements);
			}
		}

		public void RefreshSkillGemsList()
		{
			var userSkillGems = UserSkillGems;

			for (var x = 0; x < SkillGemsToLevelList.Count;)
			{
				var entry = userSkillGems.FirstOrDefault(sge => sge.SerializationString == SkillGemsToLevelList[x]);
				if (entry == null)
				{
					SkillGemsToLevelList.RemoveAt(x);
				}
				else
				{
					++x;
				}
			}

			NotifyPropertyChanged(() => UserSkillGems);
			NotifyPropertyChanged(() => AllSkillGems);
			NotifyPropertyChanged(() => SkillGemsToLevelList);
		}

		/// <summary>
		/// A list of skillgem names to ignore from leveling.
		/// </summary>
		public ObservableCollection<string> GlobalNameIgnoreList
		{
			get
			{
				return _globalNameIgnoreList;
			}
			set
			{
				if (value.Equals(_globalNameIgnoreList))
				{
					return;
				}
				_globalNameIgnoreList = value;
				NotifyPropertyChanged(() => GlobalNameIgnoreList);
			}
		}

		/// <summary>
		/// The list of skillgem entries to level in string format.
		/// </summary>
		public ObservableCollection<string> SkillGemsToLevelList
		{
			get
			{
				return _skillGemsToLevelList;
			}
			set
			{
				if (value.Equals(_skillGemsToLevelList))
				{
					return;
				}
				_skillGemsToLevelList = value;
				NotifyPropertyChanged(() => SkillGemsToLevelList);
			}
		}

		/// <summary>
		/// The string representation of UserSkillGems.
		/// </summary>
		[JsonIgnore]
		public ObservableCollection<string> AllSkillGems
		{
			get
			{
				return new ObservableCollection<string>(UserSkillGems.Select(sge => sge.SerializationString).ToList());
			}
		}

		/// <summary>
		/// A list of SkillGemEntry for the user's skillgems.
		/// </summary>
		[JsonIgnore]
		public ObservableCollection<SkillGemEntry> UserSkillGems
		{
			get
			{
				using (LokiPoe.AcquireFrame())
				{
					var skillGemEntries = new ObservableCollection<SkillGemEntry>();

					if (!LokiPoe.IsInGame)
						return skillGemEntries;

					foreach (var inv in UsableInventories)
					{
						foreach (var item in inv.Items)
						{
							if (item == null)
								continue;

							if (item.Components.SocketsComponent == null)
							{
								continue;
							}

							for (var idx = 0; idx < item.SocketedGems.Length; idx++)
							{
								var gem = item.SocketedGems[idx];
								if (gem == null)
								{
									continue;
								}

								skillGemEntries.Add(new SkillGemEntry(gem.Name, inv.PageSlot, idx));
							}
						}
					}
					return skillGemEntries;
				}
			}
		}

		public void UpdateGlobalNameIgnoreList()
		{
			NotifyPropertyChanged(() => GlobalNameIgnoreList);
		}

		public class SkillGemEntry
		{
			public string Name;
			public InventorySlot InventorySlot;
			public int SocketIndex;

			public string SerializationString { get; private set; }

			public SkillGemEntry(string name, InventorySlot slot, int socketIndex)
			{
				Name = name;
				InventorySlot = slot;
				SocketIndex = socketIndex;
				SerializationString = string.Format("{0} [{1}: {2}]", Name, InventorySlot, SocketIndex);
			}

			public Item InventoryItem
			{
				get
				{
					return UsableInventories.Where(ui => ui.PageSlot == InventorySlot)
						.Select(ui => ui.Items.FirstOrDefault())
						.FirstOrDefault();
				}
			}

			public Item SkillGem
			{
				get
				{
					var item = InventoryItem;
					if (item == null || item.Components.SocketsComponent == null)
						return null;

					var sg = item.SocketedGems[SocketIndex];
					if (sg == null)
						return null;

					if (sg.Name != Name)
						return null;

					return sg;
				}
			}
		}

		private static IEnumerable<Inventory> UsableInventories => new[]
		{
			LokiPoe.InstanceInfo.GetPlayerInventoryBySlot(InventorySlot.LeftHand),
			LokiPoe.InstanceInfo.GetPlayerInventoryBySlot(InventorySlot.RightHand),
			LokiPoe.InstanceInfo.GetPlayerInventoryBySlot(InventorySlot.OffLeftHand),
			LokiPoe.InstanceInfo.GetPlayerInventoryBySlot(InventorySlot.OffRightHand),
			LokiPoe.InstanceInfo.GetPlayerInventoryBySlot(InventorySlot.Head),
			LokiPoe.InstanceInfo.GetPlayerInventoryBySlot(InventorySlot.Chest),
			LokiPoe.InstanceInfo.GetPlayerInventoryBySlot(InventorySlot.Gloves),
			LokiPoe.InstanceInfo.GetPlayerInventoryBySlot(InventorySlot.Boots),
			LokiPoe.InstanceInfo.GetPlayerInventoryBySlot(InventorySlot.LeftRing),
			LokiPoe.InstanceInfo.GetPlayerInventoryBySlot(InventorySlot.RightRing),
			LokiPoe.InstanceInfo.GetPlayerInventoryBySlot(InventorySlot.Neck)
		};
	}
}