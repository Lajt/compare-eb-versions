using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using log4net;
using Loki.Bot;
using Loki.Bot.Pathfinding;
using Loki.Common;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Legacy.ObjectExplorer
{
	/// <summary>
	/// Interaction logic for Gui.xaml
	/// </summary>
	public partial class Gui : UserControl
	{
		private static readonly ILog Log = Logger.GetLoggerInstanceForType();

		public Gui()
		{
			InitializeComponent();

			ObjectExplorerSettings.Instance.LoadColumnDefinitions(LeftColDefinition, SplitterColDefinition, RightColDefinition);
		}

		private void TreeViewObjects_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			using (LokiPoe.AcquireFrame())
			{
				using (LokiPoe.Memory.TemporaryCacheState(false))
				{
					TextBoxInfoRaw.Text = "";

					var item = TreeViewObjects.SelectedItem as TreeViewItem;

					if (item == null || item.Tag == null)
						return;

					var obj = item.Tag as NetworkObject;
					if (obj != null)
					{
						if (obj.IsValid && LokiPoe.ObjectManager.GetObjectByAddress(obj.BaseAddress) != null)
							ShowObjectInformation(obj);
						return;
					}

					var skill = item.Tag as Skill;
					if (skill != null)
					{
						ShowSkillInformation(skill);
						return;
					}

					var itm = item.Tag as Item;
					if (itm != null)
					{
						ShowItemInformation(itm);
						return;
					}

					var area = item.Tag as DatWorldAreaWrapper;
					if (area != null)
					{
						ShowAreaInformation(area);
						return;
					}

					var quest = item.Tag as DatQuestWrapper;
					if (quest != null)
					{
						ShowQuestInformation(quest);
						return;
					}
				}
			}
		}

		private void ButtonRefresh_OnClick(object sender, RoutedEventArgs e)
		{
			TreeViewObjects.Items.Clear();

			using (LokiPoe.AcquireFrame())
			{
				using (LokiPoe.Memory.TemporaryCacheState(false))
				{
					if (!LokiPoe.IsInGame)
						return;

					var objs = LokiPoe.ObjectManager.Objects.OrderBy(o => o.Distance).ToList();

					var used = new Dictionary<int, bool>();

					var serverEffects = objs.OfType<ServerEffect>().ToList();
					foreach (var entry in serverEffects)
					{
						if (!used.ContainsKey(entry.Id))
							used.Add(entry.Id, true);
					}

					var effects = objs.OfType<Effect>().ToList();
					foreach (var entry in effects)
					{
						if (!used.ContainsKey(entry.Id))
							used.Add(entry.Id, true);
					}

					var monoliths = objs.OfType<Monolith>().Where(m => !m.IsMini).ToList();
					foreach (var entry in monoliths)
					{
						if (!used.ContainsKey(entry.Id))
							used.Add(entry.Id, true);
					}

					var miniMonoliths = objs.OfType<MiniMonolith>().ToList();
					foreach (var entry in miniMonoliths)
					{
						if (!used.ContainsKey(entry.Id))
							used.Add(entry.Id, true);
					}

					var breaches = objs.OfType<Breach>().ToList();
					foreach (var entry in breaches)
					{
						if (!used.ContainsKey(entry.Id))
							used.Add(entry.Id, true);
					}

					var abyss1 = objs.OfType<AbyssStartNode>().ToList();
					foreach (var entry in abyss1)
					{
						if (!used.ContainsKey(entry.Id))
							used.Add(entry.Id, true);
					}

					var abyss2 = objs.OfType<AbyssNodeMini>().ToList();
					foreach (var entry in abyss2)
					{
						if (!used.ContainsKey(entry.Id))
							used.Add(entry.Id, true);
					}

					var abyss3 = objs.OfType<AbyssNodeSmall>().ToList();
					foreach (var entry in abyss3)
					{
						if (!used.ContainsKey(entry.Id))
							used.Add(entry.Id, true);
					}

					var abyss4 = objs.OfType<AbyssFinalNodeChest>().ToList();
					foreach (var entry in abyss4)
					{
						if (!used.ContainsKey(entry.Id))
							used.Add(entry.Id, true);
					}

					var abyss5 = objs.OfType<AbyssCrackSpawner>().ToList();
					foreach (var entry in abyss5)
					{
						if (!used.ContainsKey(entry.Id))
							used.Add(entry.Id, true);
					}

					var abyss6 = objs.OfType<AbyssFinalNodeSubArea>().ToList();
					foreach (var entry in abyss6)
					{
						if (!used.ContainsKey(entry.Id))
							used.Add(entry.Id, true);
					}

					var abyss7 = objs.OfType<AbyssNodeLarge>().ToList();
					foreach (var entry in abyss7)
					{
						if (!used.ContainsKey(entry.Id))
							used.Add(entry.Id, true);
					}

					var shrines = objs.OfType<Shrine>().ToList();
					foreach (var entry in shrines)
					{
						if(!used.ContainsKey(entry.Id))
							used.Add(entry.Id, true);
					}

					var darkShrines = objs.OfType<DarkShrine>().ToList();
					foreach (var entry in darkShrines)
					{
						if (!used.ContainsKey(entry.Id))
							used.Add(entry.Id, true);
					}

					var stoneAltars = objs.OfType<StoneAltar>().ToList();
					foreach (var entry in stoneAltars)
					{
						if (!used.ContainsKey(entry.Id))
							used.Add(entry.Id, true);
					}

					var players = objs.OfType<Player>().ToList();
					foreach (var entry in players)
					{
						if (!used.ContainsKey(entry.Id))
							used.Add(entry.Id, true);
					}

					var monsters = objs.OfType<Monster>().ToList();
					foreach (var entry in monsters)
					{
						if (!used.ContainsKey(entry.Id))
							used.Add(entry.Id, true);
					}

					var npcs = objs.OfType<Npc>().ToList();
					foreach (var entry in npcs)
					{
						if (!used.ContainsKey(entry.Id))
							used.Add(entry.Id, true);
					}

					var worldItems = objs.OfType<WorldItem>().ToList();
					foreach (var entry in worldItems)
					{
						if (!used.ContainsKey(entry.Id))
							used.Add(entry.Id, true);
					}

					var portals = objs.OfType<Portal>().ToList();
					foreach (var entry in portals)
					{
						if (!used.ContainsKey(entry.Id))
							used.Add(entry.Id, true);
					}

					var chests = objs.OfType<Chest>().ToList();
					foreach (var entry in chests)
					{
						if (!used.ContainsKey(entry.Id))
							used.Add(entry.Id, true);
					}

					var areaTransitions = objs.OfType<AreaTransition>().ToList();
					foreach (var entry in areaTransitions)
					{
						if (!used.ContainsKey(entry.Id))
							used.Add(entry.Id, true);
					}

					var doors = LokiPoe.ObjectManager.Doors.OrderBy(o => o.Distance).ToList();
					foreach (var entry in doors)
					{
						if (!used.ContainsKey(entry.Id))
							used.Add(entry.Id, true);
					}

					var triggerableBlockages = objs.OfType<TriggerableBlockage>().Where(o =>
							!doors.Contains(o) && !abyss3.Contains(o) && !abyss4.Contains(o) && !abyss6.Contains(o) && !abyss7.Contains(o))
						.ToList();
					foreach (var entry in triggerableBlockages)
					{
						if (!used.ContainsKey(entry.Id))
							used.Add(entry.Id, true);
					}

					var others = objs.Where(o => !used.ContainsKey(o.Id)).ToList();

					var skills = LokiPoe.ObjectManager.Me.AvailableSkills.ToList();

					var abyss = new List<NetworkObject>();
					abyss.AddRange(abyss1);
					abyss.AddRange(abyss2);
					abyss.AddRange(abyss3);
					abyss.AddRange(abyss4);
					abyss.AddRange(abyss6);
					abyss.AddRange(abyss7);
					abyss = abyss.OrderBy(n => n.DistanceSqr).ToList();

					// Find a list of types...
					TreeViewObjects.Items.Add(FillTreeNode(new List<DatWorldAreaWrapper>
					{
						LokiPoe.CurrentWorldArea
					}, "Current Area"));
					TreeViewObjects.Items.Add("");

					var header = new TreeViewItem
					{
						Header = "Current Player"
					};

					var header2 = new TreeViewItem
					{
						Header = "Inventories"
					};
					header2.Items.Add(FillTreeNode(LokiPoe.Me.EquippedItems, "Equipped Items"));
					header2.Items.Add(FillTreeNode(LokiPoe.InstanceInfo.GetPlayerInventoryItemsBySlot(InventorySlot.Flasks), "Equipped Flasks"));
					header2.Items.Add(FillTreeNode(LokiPoe.InstanceInfo.GetPlayerInventoryItemsBySlot(InventorySlot.Main), "Inventory Items"));
					header.Items.Add(header2);
					header.Items.Add(FillTreeNode(Dat.Quests, "Quests"));
					header.Items.Add(FillTreeNode(skills, "Skills"));
					TreeViewObjects.Items.Add(header);

					TreeViewObjects.Items.Add("");

					TreeViewObjects.Items.Add(FillTreeNode(areaTransitions, "Area Transitions"));
					TreeViewObjects.Items.Add(FillTreeNode(chests, "Chests"));
					TreeViewObjects.Items.Add(FillTreeNode(worldItems, "Items [Ground]"));
					TreeViewObjects.Items.Add(FillTreeNode(monsters, "Monsters"));
					TreeViewObjects.Items.Add(FillTreeNode(npcs, "NPCs"));
					TreeViewObjects.Items.Add(FillTreeNode(players, "Players"));

					TreeViewObjects.Items.Add("");
					header = new TreeViewItem
					{
						Header = "Abyss"
					};
					header.Items.Add(FillTreeNode(abyss, "Nodes"));
					header.Items.Add(FillTreeNode(abyss5, "Spawners"));
					TreeViewObjects.Items.Add(header);
					
					TreeViewObjects.Items.Add(FillTreeNode(breaches, "Breaches"));
					TreeViewObjects.Items.Add(FillTreeNode(darkShrines, "Dark Shrines"));
					TreeViewObjects.Items.Add(FillTreeNode(doors, "Doors"));
					TreeViewObjects.Items.Add(FillTreeNode(effects, "Effects"));
					TreeViewObjects.Items.Add(FillTreeNode(miniMonoliths, "MiniMonoliths"));
					TreeViewObjects.Items.Add(FillTreeNode(monoliths, "Monoliths"));
					TreeViewObjects.Items.Add(FillTreeNode(portals, "Portals"));
					TreeViewObjects.Items.Add(FillTreeNode(serverEffects, "ServerEffects"));
					TreeViewObjects.Items.Add(FillTreeNode(shrines, "Shrines"));
					TreeViewObjects.Items.Add(FillTreeNode(stoneAltars, "Stone Altars"));
					TreeViewObjects.Items.Add(FillTreeNode(triggerableBlockages, "Triggerable Blockages"));

					TreeViewObjects.Items.Add("");

					TreeViewObjects.Items.Add(FillTreeNode(others, "<Uncategorized>"));
				}
			}
		}

		private void ButtonSave_OnClick(object sender, RoutedEventArgs e)
		{
			Directory.CreateDirectory("dump");

			File.WriteAllText(string.Format("dump\\ObjectExplorer-Raw-{0:yyyy-MM-dd_hh-mm-ss-tt}.txt", DateTime.Now),
				TextBoxInfoRaw.Text);
		}

		public void Save()
		{
			ObjectExplorerSettings.Instance.SaveColumnDefinitions(LeftColDefinition, SplitterColDefinition, RightColDefinition);
		}

		private TreeViewItem FillTreeNode(IEnumerable<DatWorldAreaWrapper> objects, string type)
		{
			var item = new TreeViewItem
			{
				Header = type
			};
			foreach (var o in objects)
			{
				try
				{
					var leaf = new TreeViewItem
					{
						Header = o.Name,
						Tag = o
					};
					item.Items.Add(leaf);
				}
				catch (Exception ex)
				{
					Log.Error("Exception adding object to viewer.", ex);
				}
			}

			return item;
		}

		private TreeViewItem FillTreeNode(IEnumerable<Skill> objects, string type)
		{
			var item = new TreeViewItem
			{
				Header = type
			};
			foreach (var o in objects)
			{
				try
				{
					if (!o.IsValid)
						continue;

					var leaf = new TreeViewItem
					{
						Header = string.Format("{0} ({1})", o.Name, o.InternalId),
						Tag = o
					};
					item.Items.Add(leaf);
				}
				catch (Exception ex)
				{
					Log.Error("Exception adding object to viewer.", ex);
				}
			}

			return item;
		}

		private TreeViewItem FillTreeNode(IEnumerable<NetworkObject> objects, string type)
		{
			var item = new TreeViewItem
			{
				Header = type
			};

			foreach (var o in objects)
			{
				try
				{
					if (!o.IsValid)
						continue;

					var leaf = new TreeViewItem
					{
						Header = o.Name,
						Tag = o
					};

					var monster = o as Monster;
					if (monster != null && monster.IsDead)
					{
						leaf.Header += " (dead)";
					}

					var chest = o as Chest;
					if (chest != null && chest.IsOpened)
					{
						leaf.Header += " (opened)";
					}

					item.Items.Add(leaf);
				}
				catch (Exception ex)
				{
					Log.Error("Exception adding object to viewer.", ex);
				}
			}

			return item;
		}

		private TreeViewItem FillTreeNode(IEnumerable<DatQuestWrapper> objects, string type)
		{
			var item = new TreeViewItem
			{
				Header = type
			};

			foreach (var o in objects)
			{
				try
				{
					var leaf = new TreeViewItem
					{
						Header = string.Format("{0} - {1}", o.Id, o.Name),
						Tag = o
					};
					item.Items.Add(leaf);
				}
				catch (Exception ex)
				{
					Log.Error("Exception adding object to viewer.", ex);
				}
			}

			return item;
		}

		private TreeViewItem FillTreeNode(IEnumerable<Item> objects, string type)
		{
			var item = new TreeViewItem
			{
				Header = type
			};

			foreach (var o in objects)
			{
				try
				{
					if (!o.IsValid)
						continue;
					var leaf = new TreeViewItem
					{
						Header = o.FullName,
						Tag = o
					};
					item.Items.Add(leaf);
				}
				catch (Exception ex)
				{
					Log.Error("Exception adding object to viewer.", ex);
				}
			}

			return item;
		}

		private void ShowObjectInformation(NetworkObject obj)
		{
			TextBoxInfoRaw.AppendText(obj.Dump());
		}

		private void ShowSkillInformation(Skill skill)
		{
			TextBoxInfoRaw.AppendText(skill.ToString());
		}

		private void ShowAreaInformation(DatWorldAreaWrapper area)
		{
			TextBoxInfoRaw.AppendText(area.ToString());

			var sb = new StringBuilder();

			// Handle updates when the bot isn't running.
			if (!BotManager.IsRunning)
			{
				ExilePather.Reload();
				//StaticLocationManager.Tick();
			}

			/*sb.AppendLine(string.Format("[Static Locations]"));
			foreach (var kvp in StaticLocationManager.StaticLocations)
			{
				sb.AppendLine(string.Format("\t{0}", kvp.Key));
				foreach (var pos in kvp.Value)
				{
					sb.AppendLine(string.Format("\t\t{0}", pos));
				}
			}*/

			/*sb.AppendLine(string.Format("[Static Waypoints]"));
			foreach (var pos in StaticLocationManager.StaticWaypointLocations)
			{
				sb.AppendLine(string.Format("\t{0}", pos));
			}*/

			sb.AppendLine(string.Format("[Mods]"));
			foreach (var mod in LokiPoe.LocalData.MapMods)
			{
				sb.AppendLine(string.Format("\t{0}: {1}", mod.Key, mod.Value));
			}

			sb.AppendLine(string.Format("[Terrain]"));
			sb.AppendLine(string.Format("\tCols: {0}", LokiPoe.TerrainData.Cols));
			sb.AppendLine(string.Format("\tRows: {0}", LokiPoe.TerrainData.Rows));

			TextBoxInfoRaw.AppendText(sb.ToString());
		}

		private void ShowQuestInformation(DatQuestWrapper quest)
		{
			var sb = new StringBuilder();
			sb.AppendLine(string.Format("[Quest]"));
			sb.AppendLine(string.Format("\tId: {0}", quest.Id));
			sb.AppendLine(string.Format("\tName: {0}", quest.Name));
			sb.AppendLine(string.Format("\tAct: {0}", quest.Act));

			var state = LokiPoe.InGameState.GetCurrentQuestState(quest);
			sb.AppendLine(string.Format("\t[State]"));
			if (state != null)
			{
				sb.AppendLine(string.Format("\t\tStateId: {0}", state.Id));
				sb.AppendLine(string.Format("\t\tQuestProgressText: {0}", state.QuestProgressText));
				sb.AppendLine(string.Format("\t\tQuestStateText: {0}", state.QuestStateText));
			}
			else
			{
				sb.AppendLine(string.Format("\t\t(null)"));
			}

			TextBoxInfoRaw.AppendText(sb.ToString());
		}

		private void ShowItemInformation(Item item)
		{
			var sb = new StringBuilder();

			sb.AppendLine(string.Format("[Network Object]"));
			sb.AppendLine(string.Format("\tBaseAddress: 0x{0:X}", item.BaseAddress.ToInt32()));
			sb.AppendLine(string.Format("\tName: {0}", item.Name));
			sb.AppendLine(string.Format("\tMetadata: {0}", item.Metadata));
			sb.AppendLine(string.Format("\tClass: {0}", item.Class)); 
			sb.AppendLine(string.Format("\tAPI Type: {0}", item.GetType()));
			sb.AppendLine();

			sb.AppendLine(string.Format("[Components]"));
			var components = item.Components.AllComponents.ToList();
			foreach (var c in components)
			{
				sb.AppendLine(string.Format("\t{0}: 0x{1:X}", c.ComponentName, c.BaseAddress.ToInt32()));
			}
			sb.AppendLine();

			foreach (var c in components)
			{
				sb.AppendLine(c.ToString());
				sb.AppendLine();
			}

			sb.AppendLine();

			sb.Append(item.Dump());

			TextBoxInfoRaw.AppendText(sb.ToString());
		}

		private void ButtonRefreshObject_OnClick(object sender, RoutedEventArgs e)
		{
			TreeViewObjects_OnSelectedItemChanged(null, null);
		}
	}
}
