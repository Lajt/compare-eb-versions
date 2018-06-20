using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using log4net;
using Loki.Common;
using Loki.Game;
using Loki.Game.GameData;
using System.Diagnostics;

namespace Legacy.DumpTab
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
		}

		private void ButtonDumpActiveSkills_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var entry in Dat.ActiveSkills)
				{
					sb.Append(entry.ToString());
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\ActiveSkills-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("ActiveSkills saved!");
		}

		private void ButtonDumpBackendErrors_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("[Index] [Id] [Text]");
			sb.AppendLine();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var type in Dat.BackendErrors)
				{
					sb.AppendFormat("[{0}] [{1}] [{2}]", type.Index, type.Id, type.Text);
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\BackendErrors-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("BackendErrors saved!");
		}

		private void ButtonDumpBaseItemTypes_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("Index,Id,Name,Class");
			sb.AppendLine();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var entry in Dat.BaseItemTypes)
				{
					sb.AppendFormat("{0},", entry.Index);
					sb.AppendFormat("{0},", entry.Metadata);
					sb.AppendFormat("\"{0}\",", entry.Name);
					sb.AppendFormat("{0}", entry.ItemClass);
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\BaseItemTypes-" + LokiPoe.SupportedClientVersion + ".csv", sb.ToString());
			Log.DebugFormat("BaseItemTypes saved!");
		}

		private void ButtonDumpBuffDefinitions_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("[Index] [Id] [Name - Desc]");
			sb.AppendLine();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var buff in Dat.BuffDefinitions)
				{
					sb.AppendFormat("[{0}] [{1}] [{2} - {3}]", buff.Index, buff.Id, buff.Name, buff.Desc.Replace('\t', ' ').Replace('\n', ' '));
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\BuffDefinitions-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("BuffDefinitions saved!");
		}

		private void ButtonDumpChests_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("[Metadata] [Name]");
			sb.AppendLine();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var chest in Dat.Chests)
				{
					sb.AppendFormat("[{0}] [{1}]", chest.Metadata, chest.Name);
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\Chests-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("Chests saved!");
		}

		private void ButtonDumpClientStrings_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("Index,Key,Value");
			sb.AppendLine();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var entry in Dat.ClientStrings)
				{
					sb.AppendFormat("{0},", entry.Index);
					sb.AppendFormat("\"{0}\",", entry.Key);
					sb.AppendFormat("\"{0}\"", entry.Value);
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\ClientStrings-" + LokiPoe.SupportedClientVersion + ".csv", sb.ToString());
			Log.DebugFormat("ClientStrings saved!");
		}

		private void ButtonDumpGrantedEffects_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var entry in Dat.GrantedEffects)
				{
					sb.Append(entry.ToString());
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\GrantedEffects-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("GrantedEffects saved!");
		}

		private void ButtonDumpGrantedEffectsPerLevel_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var entry in Dat.GrantedEffectsPerLevel)
				{
					sb.Append(entry.ToString());
					sb.AppendLine();
				}
			}
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\GrantedEffectsPerLevel-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("GrantedEffectsPerLevel saved!");
		}

		private void ButtonDumpMinimapIcons_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("Index,Name,_04,_08,_0C,_0D,_0E");
			sb.AppendLine();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var entry in Dat.MinimapIcons)
				{
					sb.AppendFormat("{0},", entry.Index);
					sb.AppendFormat("{0},", entry.Name);
					sb.AppendFormat("{0},", entry._04);
					sb.AppendFormat("{0},", entry._08);
					sb.AppendFormat("{0},", entry._0C);
					sb.AppendFormat("{0},", entry._0D);
					sb.AppendFormat("{0}", entry._0E);
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\MinimapIcons-" + LokiPoe.SupportedClientVersion + ".csv", sb.ToString());
			Log.DebugFormat("MinimapIcons saved!");
		}

		private void ButtonDumpMods_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("Index");
			sb.AppendLine();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var entry in Dat.Mods)
				{
					sb.AppendFormat("[{0}] {1}", entry.Index, entry.ToString());
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\Mods-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("Mods saved!");
		}

		private void ButtonDumpMonsterVarieties_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("[Metadata] [Name]");
			sb.AppendLine();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var monster in Dat.MonsterVarieties)
				{
					sb.AppendFormat("[{0}] [{1}]", monster.Metadata, monster.Name);
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\MonsterVarieties-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("MonsterVarieties saved!");
		}

		private void ButtonDumpNpcs_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("[Metadata] [Name]");
			sb.AppendLine();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var npc in Dat.Npcs)
				{
					sb.AppendFormat("[{0}] [{1}]", npc.Metadata, npc.Name);
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\Npcs-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("Npcs saved!");
		}

		private void ButtonDumpNpcTalk_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("Index,NpcName,Text");
			sb.AppendLine();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var entry in Dat.NpcTalk)
				{
					sb.AppendFormat("{0},", entry.Index);
					sb.AppendFormat("\"{0}\",", entry.NpcName);
					sb.AppendFormat("\"{0}\"", entry.Text);
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\NpcTalk-" + LokiPoe.SupportedClientVersion + ".csv", sb.ToString());
			Log.DebugFormat("NpcTalk saved!");
		}

		private void ButtonDumpPassiveSkills_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("[Name] [Id] [stat1: value1 | stat2: value2 | stat3: value3 | stat4: value4]");
			sb.AppendLine();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var passive in Dat.PassiveSkills)
				{
					sb.AppendFormat("{0} [{1}] [", passive.Name, passive.Id);
					foreach (var kvp in passive.Stats)
					{
						sb.AppendFormat("{0}: {1} |", kvp.Key.Id, kvp.Value);
					}
					sb.AppendFormat("]");
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\PassiveSkills-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("PassiveSkills saved!");
		}

		private void ButtonDumpProphecies_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("[Index] [Id] [ProphecyId] [Name] [Description] [FlavourText]");
			sb.AppendLine();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var prophecy in Dat.Prophecies)
				{
					sb.AppendFormat("[{0}] [{1}] [{2}|{3}] [{4}] [{5}] [{6}]", prophecy.Index, prophecy.Id,
						prophecy.ProphecyId, prophecy.SealCost,
						prophecy.Name, prophecy.Description, prophecy.FlavourText);
					sb.AppendLine();
					foreach (var datClientStringWrapper in prophecy.ClientStrings)
					{
						sb.AppendFormat("\t{0}: {1}", datClientStringWrapper.Key, datClientStringWrapper.Value);
						sb.AppendLine();
					}
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\Prophecies-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("Prophecies saved!");
		}

		private void ButtonDumpQuests_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("Index,Id,Act,Name");
			sb.AppendLine();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var entry in Dat.Quests)
				{
					sb.AppendFormat("{0},", entry.Index);
					sb.AppendFormat("{0},", entry.Id);
					sb.AppendFormat("{0},", entry.Act);
					sb.AppendFormat("{0}", entry.Name);
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\Quests-" + LokiPoe.SupportedClientVersion + ".csv", sb.ToString());
			Log.DebugFormat("Quests saved!");
		}

		private void ButtonDumpQuestStates_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("[Index] [StateId] [QuestProgressText] [QuestStateText]");
			sb.AppendLine();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var state in Dat.QuestStates)
				{
					sb.AppendFormat("[{0}] [{1}] [{2}] [{3}]", state.Index, state.Id, state.QuestProgressText, state.QuestStateText);
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\QuestStates-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("QuestStates saved!");
		}

		private void ButtonDumpStats_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("[Index] [ApiId] [Id] [Name]");
			sb.AppendLine();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var stat in Dat.Stats)
				{
					sb.AppendFormat("[{0}] [{1}] [{2}] [{3}]", stat.Index, stat.ApiId, stat.Id, stat.Name);
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\Stats-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("Stats saved!");
		}

		private void ButtonDumpWorldAreas_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("[Id | Index] [Name] [Connections] [CorruptedAreas]");
			sb.AppendLine();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var worldArea in Dat.WorldAreas)
				{
					sb.AppendFormat("[{0} | {4}] [{1}] (Level {2}) | IsMap: {3}", worldArea.Id, worldArea.Name, worldArea.MonsterLevel,
						worldArea.IsMap, worldArea.Index);
					if (worldArea.Connections.Count > 0)
					{
						sb.AppendFormat(" Connections: [");
						foreach (var connection in worldArea.Connections)
						{
							sb.AppendFormat("[{0} - {1}]", connection.Id, connection.Name);
						}
						sb.AppendFormat("]");
					}
					if (worldArea.CorruptedAreas.Count > 0)
					{
						sb.AppendFormat(" CorruptedAreas: [");
						foreach (var area in worldArea.CorruptedAreas)
						{
							sb.AppendFormat("[{0} - {1}]", area.Id, area.Name);
						}
						sb.AppendFormat("]");
					}
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\WorldAreas-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("WorldAreas saved!");

			/*
				var outDir = Path.Combine("dump-beta", LokiPoe.SupportedClientVersion);
				Directory.CreateDirectory(outDir);
				using (LokiPoe.AcquireFrame())
				{
					foreach (var worldArea in Dat.WorldAreas)
					{
						var sb = new StringBuilder();
						sb.Append(worldArea.ToString());
						File.WriteAllText(Path.Combine(outDir, worldArea.Id) + ".txt", sb.ToString());
					}
				}
			 */
		}

		private void ButtonGenerateActiveSkills_OnClick(object sender, RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("namespace Loki.Game.GameData");
			sb.AppendLine();
			sb.AppendFormat("{{");
			sb.AppendLine();
			sb.AppendFormat("    // ReSharper disable UnusedMember.Global");
			sb.AppendLine();
			sb.AppendFormat("    #pragma warning disable 1591");
			sb.AppendLine();
			sb.AppendLine();
			if (LokiPoe.ClientVersion == LokiPoe.PoeVersion.Official ||
				LokiPoe.ClientVersion == LokiPoe.PoeVersion.OfficialSteam)
				sb.AppendFormat("    public enum ActiveSkillsEnum");
			else
				throw new NotImplementedException();
			sb.AppendLine();
			sb.AppendFormat("    {{");
			sb.AppendLine();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var entry in Dat.ActiveSkills)
				{
					sb.AppendFormat("        // {0}: {1}", entry.InternalName, entry.DisplayName);
					sb.AppendLine();
					sb.AppendFormat("        {0},", entry.InternalId);
					sb.AppendLine();
					sb.AppendLine();
				}
			}
			sb.AppendFormat("    }}");
			sb.AppendLine();
			sb.AppendFormat("}}");
			sb.AppendLine();
			Directory.CreateDirectory("dump");
			if (LokiPoe.ClientVersion == LokiPoe.PoeVersion.Official ||
				LokiPoe.ClientVersion == LokiPoe.PoeVersion.OfficialSteam)
				File.WriteAllText("dump\\ActiveSkills.cs", sb.ToString());
			else
				throw new NotImplementedException();

			Log.DebugFormat("ActiveSkills saved!");
		}

		private void ButtonGenerateBackendErrors_OnClick(object sender, RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("namespace Loki.Game.GameData");
			sb.AppendLine();
			sb.AppendFormat("{{");
			sb.AppendLine();
			sb.AppendFormat("    // ReSharper disable UnusedMember.Global");
			sb.AppendLine();
			sb.AppendFormat("    #pragma warning disable 1591");
			sb.AppendLine();
			sb.AppendLine();
			if (LokiPoe.ClientVersion == LokiPoe.PoeVersion.Official ||
				LokiPoe.ClientVersion == LokiPoe.PoeVersion.OfficialSteam)
				sb.AppendFormat("    public enum BackendErrorEnum : ushort");
			else
				throw new NotImplementedException();
			sb.AppendLine();
			sb.AppendFormat("    {{");
			sb.AppendLine();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var type in Dat.BackendErrors)
				{
					sb.AppendFormat("        // {0}", type.Text.Replace("\n", " ").Replace("\r", " "));
					sb.AppendLine();
					sb.AppendFormat("        {1} = {0},", type.Index, type.Id);
					sb.AppendLine();
					sb.AppendLine();
				}
			}
			sb.AppendFormat("    }}");
			sb.AppendLine();
			sb.AppendFormat("}}");
			sb.AppendLine();
			Directory.CreateDirectory("dump");
			if (LokiPoe.ClientVersion == LokiPoe.PoeVersion.Official ||
				LokiPoe.ClientVersion == LokiPoe.PoeVersion.OfficialSteam)
				File.WriteAllText("dump\\BackendErrors.cs", sb.ToString());
			else
				throw new NotImplementedException();

			Log.DebugFormat("BackendErrors saved!");
		}

		private void ButtonGenerateBaseItemTypes_OnClick(object sender, RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("// ReSharper disable InconsistentNaming");
			sb.AppendLine();
			sb.AppendFormat("namespace Loki.Game.GameData");
			sb.AppendLine();
			sb.AppendFormat("{{");
			sb.AppendLine();
			sb.AppendFormat("    // ReSharper disable UnusedMember.Global");
			sb.AppendLine();
			sb.AppendFormat("    #pragma warning disable 1591");
			sb.AppendLine();
			sb.AppendLine();
			if (LokiPoe.ClientVersion == LokiPoe.PoeVersion.Official ||
				LokiPoe.ClientVersion == LokiPoe.PoeVersion.OfficialSteam)
				sb.AppendFormat("    public enum BaseItemTypeEnum");
			else
				throw new NotImplementedException();
			sb.AppendLine();
			sb.AppendFormat("    {{");
			sb.AppendLine();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var entry in Dat.BaseItemTypes)
				{
					sb.AppendFormat("        /// <summary>{0}</summary>", entry.Name);
					sb.AppendLine();
					sb.AppendFormat("        {0},", LokiPoe.CleanifyMetadataString(entry.Metadata));
					sb.AppendLine();
					sb.AppendLine();
				}
			}
			sb.AppendFormat("    }}");
			sb.AppendLine();
			sb.AppendFormat("}}");
			sb.AppendLine();
			Directory.CreateDirectory("dump");
			if (LokiPoe.ClientVersion == LokiPoe.PoeVersion.Official ||
				LokiPoe.ClientVersion == LokiPoe.PoeVersion.OfficialSteam)
				File.WriteAllText("dump\\BaseItemTypes.cs", sb.ToString());
			else
				throw new NotImplementedException();

			Log.DebugFormat("BaseItemTypes saved!");
		}

		private void ButtonGenerateBaseItemTypesComposite_OnClick(object sender, RoutedEventArgs e)
		{
			if (!(LokiPoe.ClientVersion == LokiPoe.PoeVersion.Official ||
				LokiPoe.ClientVersion == LokiPoe.PoeVersion.OfficialSteam))
				throw new NotImplementedException();

			var sb = new StringBuilder();
			var tokens = new HashSet<string>();
			var groupLookups = new Dictionary<string, HashSet<string>>();
			using (LokiPoe.AcquireFrame())
			{
				var numAlpha = new Regex("(?<Alpha>[a-zA-Z]*)(?<Numeric>[0-9_]*)");

				foreach (var entry in Dat.BaseItemTypes)
				{
					string[] subparts;

					var group = new HashSet<string>();
					var metadata = entry.Metadata;
					var parts = metadata.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

					/*var needsSubs = !(entry.ItemClass.Equals("Microtransaction") || entry.ItemClass.Equals("DivinationCard") ||
									entry.ItemClass.Equals("LabyrinthTrinket") || entry.ItemClass.Equals("HideoutDoodad"));*/

					// Special processing due to inconsistencies.
					if (metadata.Contains("Metadata/Items/Currency/CurrencyEssence"))
					{
						tokens.Add("Essence");
						group.Add("Essence");
					}

					string part;
					for (var i = 2; i < parts.Length; i++)
					{
						part = parts[i].Replace("-", "");
						tokens.Add(part);
						group.Add(part);

						if (part.Any(char.IsDigit))
						{
							var match = numAlpha.Match(part);

							var alpha = match.Groups["Alpha"].Value;
							tokens.Add(alpha);
							group.Add(alpha);

							var num = match.Groups["Numeric"].Value;
							num = "_" + num;
							tokens.Add(num);
							group.Add(num);
						}

						// Special processing due to inconsistencies.
						if (i == parts.Length - 1)
						{
							if (part.StartsWith("CurrencyBreachUpgrade"))
							{
								tokens.Add("BreachUpgrade");
								group.Add("BreachUpgrade");
							}
							else if (part.StartsWith("CurrencyBreach") && part.EndsWith("Shard"))
							{
								tokens.Add("BreachShard");
								group.Add("BreachShard");
							}
						}

						/*if (!needsSubs)
							continue;

						subparts = SplitCamelCase(part);
						foreach (var subpart in subparts)
						{
							if (string.IsNullOrEmpty(subpart))
								continue;

							tokens.Add(subpart);
							group.Add(subpart);
						}*/
					}

					//part = entry.ItemClass.Replace(" ", "");
					//tokens.Add(part);
					//group.Add(part);

					/*subparts = SplitCamelCase(part);
					foreach (var subpart in subparts)
					{
						if(string.IsNullOrEmpty(subpart))
							continue;

						tokens.Add(subpart);
						group.Add(subpart);
					}*/

					groupLookups.Add(entry.Metadata, group);
				}
			}

			string Token_MetadataFlags = "MetadataFlags";
			string Token_MetadataFlagLookup = "MetadataFlagLookup";

			sb.AppendLine("using System;");
			sb.AppendLine("using System.Collections;");
			sb.AppendLine("using System.Collections.Generic;");

			sb.AppendLine("namespace Loki.Game.GameData");
			sb.AppendLine("{");
			sb.AppendLine($"\tpublic enum {Token_MetadataFlags}");
			sb.AppendLine("\t{");
			foreach (var token in tokens)
			{
				sb.AppendLine("\t\t" + token + ",");
			}
			sb.AppendLine("\t}");
			sb.AppendLine("");

			sb.AppendLine("\tpublic static class BITC");
			sb.AppendLine("\t{");
			sb.AppendLine(
				$"\t\tinternal static Dictionary<string, HashSet<{Token_MetadataFlags}>> {Token_MetadataFlagLookup} = new Dictionary<string, HashSet<{Token_MetadataFlags}>>();");
			sb.AppendLine("");

			sb.AppendLine($"\t\tpublic static HashSet<{Token_MetadataFlags}> GetFlagsForMetadata(string metadata)");
			sb.AppendLine("\t\t{");
			sb.AppendLine($"\t\t\tHashSet<{Token_MetadataFlags}> tokens;");
			sb.AppendLine($"\t\t\tif (!{Token_MetadataFlagLookup}.TryGetValue(metadata, out tokens))");
			sb.AppendLine($"\t\t\t\treturn new HashSet<{Token_MetadataFlags}>();");
			sb.AppendLine($"\t\t\treturn new HashSet<{Token_MetadataFlags}>(tokens);");
			sb.AppendLine("\t\t}");
			sb.AppendLine("");

			sb.AppendLine($"\t\tpublic static bool HasMetadataFlag(string metadata, {Token_MetadataFlags} token)");
			sb.AppendLine("\t\t{");
			sb.AppendLine($"\t\t\tHashSet<{Token_MetadataFlags}> tokens;");
			sb.AppendLine($"\t\t\tif(!{Token_MetadataFlagLookup}.TryGetValue(metadata, out tokens))");
			sb.AppendLine("\t\t\t\treturn false;");
			sb.AppendLine("\t\t\treturn tokens.Contains(token);");
			sb.AppendLine("\t\t}");
			sb.AppendLine("");

			sb.AppendLine("\t\tinternal static void Initialize()");
			sb.AppendLine("\t\t{");
			sb.AppendLine($"\t\t\tHashSet<{Token_MetadataFlags}> tokens;");
			foreach (var lookup in groupLookups)
			{
				sb.AppendLine($"\t\t\ttokens = new HashSet<{Token_MetadataFlags}>();");
				foreach (var entry in lookup.Value)
				{
					sb.AppendLine($"\t\t\ttokens.Add({Token_MetadataFlags}." + entry + ");");
				}
				sb.AppendLine($"\t\t\t{Token_MetadataFlagLookup}.Add(\"" + lookup.Key + "\", tokens);");
				sb.AppendLine();
			}
			sb.AppendLine("\t\t}");
			sb.AppendLine("\t}");
			sb.AppendLine("}");

			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\BaseItemTypesComposite.cs", sb.ToString());

			Log.DebugFormat("BaseItemTypesComposite saved!");
		}

		private void ButtonGenerateBuffDefinitions_OnClick(object sender, RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("namespace Loki.Game.GameData");
			sb.AppendLine();
			sb.AppendFormat("{{");
			sb.AppendLine();
			sb.AppendFormat("    // ReSharper disable UnusedMember.Global");
			sb.AppendLine();
			sb.AppendFormat("    #pragma warning disable 1591");
			sb.AppendLine();
			sb.AppendLine();
			if (LokiPoe.ClientVersion == LokiPoe.PoeVersion.Official ||
				LokiPoe.ClientVersion == LokiPoe.PoeVersion.OfficialSteam)
				sb.AppendFormat("    public enum BuffDefinitionsEnum");
			else
				throw new NotImplementedException();
			sb.AppendLine();
			sb.AppendFormat("    {{");
			sb.AppendLine();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var entry in Dat.BuffDefinitions)
				{
					sb.AppendFormat("        // {0}: {1}", entry.Name, entry.Desc.Replace('\t', ' ').Replace('\n', ' '));
					sb.AppendLine();
					sb.AppendFormat("        {0},", entry.Id.Replace(" ", "__"));
					sb.AppendLine();
					sb.AppendLine();
				}
			}
			sb.AppendFormat("    }}");
			sb.AppendLine();
			sb.AppendFormat("}}");
			sb.AppendLine();
			Directory.CreateDirectory("dump");
			if (LokiPoe.ClientVersion == LokiPoe.PoeVersion.Official ||
				LokiPoe.ClientVersion == LokiPoe.PoeVersion.OfficialSteam)
				File.WriteAllText("dump\\BuffDefinitions.cs", sb.ToString());
			else
				throw new NotImplementedException();

			Log.DebugFormat("BuffDefinitions saved!");
		}

		private void ButtonGenerateClientStrings_OnClick(object sender, RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("namespace Loki.Game.GameData");
			sb.AppendLine();
			sb.AppendFormat("{{");
			sb.AppendLine();
			sb.AppendFormat("    // ReSharper disable UnusedMember.Global");
			sb.AppendLine();
			sb.AppendFormat("    #pragma warning disable 1591");
			sb.AppendLine();
			sb.AppendLine();
			if (LokiPoe.ClientVersion == LokiPoe.PoeVersion.Official ||
				LokiPoe.ClientVersion == LokiPoe.PoeVersion.OfficialSteam)
				sb.AppendFormat("    public enum ClientStringsEnum");
			else
				throw new NotImplementedException();
			sb.AppendLine();
			sb.AppendFormat("    {{");
			sb.AppendLine();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var type in Dat.ClientStrings)
				{
					sb.AppendFormat("        // {0}", type.Value.Replace("\n", " ").Replace("\r", " "));
					sb.AppendLine();
					sb.AppendFormat("        {0},", type.Key);
					sb.AppendLine();
					sb.AppendLine();
				}
			}
			sb.AppendFormat("    }}");
			sb.AppendLine();
			sb.AppendFormat("}}");
			sb.AppendLine();
			Directory.CreateDirectory("dump");
			if (LokiPoe.ClientVersion == LokiPoe.PoeVersion.Official ||
				LokiPoe.ClientVersion == LokiPoe.PoeVersion.OfficialSteam)
				File.WriteAllText("dump\\ClientStrings.cs", sb.ToString());
			else
				throw new NotImplementedException();

			Log.DebugFormat("ClientStrings saved!");
		}

		private void ButtonGenerateStatType_OnClick(object sender, RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("namespace Loki.Game.GameData");
			sb.AppendLine();
			sb.AppendFormat("{{");
			sb.AppendLine();
			sb.AppendFormat("    // ReSharper disable UnusedMember.Global");
			sb.AppendLine();
			sb.AppendFormat("    #pragma warning disable 1591");
			sb.AppendLine();
			sb.AppendLine();
			if (LokiPoe.ClientVersion == LokiPoe.PoeVersion.Official ||
				LokiPoe.ClientVersion == LokiPoe.PoeVersion.OfficialSteam)
				sb.AppendFormat("    public enum StatTypeGGG");
			else
				throw new NotImplementedException();
			sb.AppendLine();
			sb.AppendFormat("    {{");
			sb.AppendLine();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var stat in Dat.Stats)
				{
					sb.AppendFormat("        {1} = {0},", stat.Index, stat.ApiId);
					sb.AppendLine();
				}
			}
			sb.AppendFormat("    }}");
			sb.AppendLine();
			sb.AppendFormat("}}");
			sb.AppendLine();
			Directory.CreateDirectory("dump");
			if (LokiPoe.ClientVersion == LokiPoe.PoeVersion.Official ||
				LokiPoe.ClientVersion == LokiPoe.PoeVersion.OfficialSteam)
				File.WriteAllText("dump\\StatTypeGGG.cs", sb.ToString());
			else
				throw new NotImplementedException();

			Log.DebugFormat("StatTypeGGG saved!");
		}

		private void ButtonDumpQuestRewards_OnClick(object sender, RoutedEventArgs e)
		{
			using (LokiPoe.AcquireFrame())
			{
				Dat.DumpQuestRewards();
			}

			Log.DebugFormat("QuestRewards saved!");
		}

		private void ButtonGenerate_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				Dat.Generate();
			}
#endif
		}

		private void ButtonDebugRewardUi_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.RewardUi.Debug();
			}
#endif
		}

		private void ButtonDebugAscendancySelectionUi_Click(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.AscendancySelectionUi.Debug();
			}
#endif
		}

		private void ButtonDebugAtlasUi_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.AtlasUi.Debug();
			}
#endif
		}

		private void ButtonDebugBanditPanel_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.BanditPanel.Debug();
			}
#endif
		}

		private void ButtonDebugCadiroOfferUi_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.CadiroOfferUi.Debug();
			}
#endif
		}

		private void ButtonDebugCardTradeUi_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.CardTradeUi.Debug();
			}
#endif
		}

		private void ButtonDebugChallengesUi_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.ChallengesUi.Debug();
			}
#endif
		}

		private void ButtonDebugChatPanel_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.ChatPanel.Debug();
			}
#endif
		}

		private void ButtonDebugContextMenu_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.ContextMenu.Debug();
			}
#endif
		}

		private void ButtonDebugCursorItemOverlay_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.CursorItemOverlay.Debug();
			}
#endif
		}

		private void ButtonDebugDebugOverlay_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.DebugOverlay.Debug();
			}
#endif
		}

		private void ButtonDebugDisplayNoteUi_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.DisplayNoteUi.Debug();
			}
#endif
		}

		private void ButtonDebugGlobalWarningDialog_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.GlobalWarningDialog.Debug();
			}
#endif
		}

		private void ButtonDebugGuildStashUi_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.GuildStashUi.Debug();
			}
#endif
		}

		private void ButtonDebugInstanceManagerUi_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.InstanceManagerUi.Debug();
			}
#endif
		}

		private void ButtonDebugInventoryUi_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.InventoryUi.Debug();
			}
#endif
		}

		private void ButtonDebugLabyrinthMapSelectorUi_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.LabyrinthMapSelectorUi.Debug();
			}
#endif
		}

		private void ButtonDebugLabyrinthMapUi_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.LabyrinthMapUi.Debug();
			}
#endif
		}

		private void ButtonDebugMapDeviceUi_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.MapDeviceUi.Debug();
			}
#endif
		}

		private void ButtonDebugMasterDeviceUi_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.MasterDeviceUi.Debug();
			}
#endif
		}

		private void ButtonDebugNotificationHud_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.NotificationHud.Debug();
			}
#endif
		}

		private void ButtonDebugNpcDialogUi_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.NpcDialogUi.Debug();
			}
#endif
		}

		private void ButtonDebugPartyHud_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.PartyHud.Debug();
			}
#endif
		}

		private void ButtonDebugPurchaseUi_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.PurchaseUi.Debug();
			}
#endif
		}

		private void ButtonDebugQuestTrackerOverlay_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.QuestTrackerOverlay.Debug();
			}
#endif
		}

		private void ButtonDebugQuickFlaskHud_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.QuickFlaskHud.Debug();
			}
#endif
		}

		private void ButtonDebugResurrectPanel_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.ResurrectPanel.Debug();
			}
#endif
		}

		private void ButtonDebugSacrificeUi_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.SacrificeUi.Debug();
			}
#endif
		}

		private void ButtonDebugSellUi_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.SellUi.Debug();
			}
#endif
		}

		private void ButtonDebugSkillBarHud_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.SkillBarHud.Debug();
			}
#endif
		}

		private void ButtonDebugSkillGemHud_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.SkillGemHud.Debug();
			}
#endif
		}

		private void ButtonDebugSkillsUi_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.SkillsUi.Debug();
			}
#endif
		}

		private void ButtonDebugSocialUi_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.SocialUi.Debug();
			}
#endif
		}

		private void ButtonDebugSplitStackUi_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.SplitStackUi.Debug();
			}
#endif
		}

		private void ButtonDebugStashUi_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.StashUi.Debug();
			}
#endif
		}

		private void ButtonDebugCurrencyTab_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.StashUi.CurrencyTab.Debug();
			}
#endif
		}

		private void ButtonDebugDivinationTab_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.StashUi.DivinationTab.Debug();
			}
#endif
		}

		private void ButtonDebugEssenceTab_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.StashUi.EssenceTab.Debug();
			}
#endif
		}

		private void ButtonDebugStoneAltarUi_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.StoneAltarUi.Debug();
			}
#endif
		}

		private void ButtonDebugTimerHud_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.TimerHud.Debug();
			}
#endif
		}

		private void ButtonDebugTradeUi_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.TradeUi.Debug();
			}
#endif
		}

		private void ButtonDebugTrialPlaqueUi_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.TrialPlaqueUi.Debug();
			}
#endif
		}

		private void ButtonDebugWorldPanel_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.WorldUi.Debug();
			}
#endif
		}

		private void ButtonDumpInstanceInfo_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InstanceInfo.Dump();
			}
#endif
		}

		private void ButtonDumpLocalData_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.LocalData.Debug();
			}
#endif
		}

		private void ButtonDumpTerrainData_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.TerrainData.Debug();
			}
#endif
		}

		private void ButtonDebugInstanceInfo_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InstanceInfo.Debug();
			}
#endif
		}

		private void ButtonGenerateItemClasses_OnClick(object sender, RoutedEventArgs e)
		{
			var classStrings = new HashSet<string>();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var item in Dat.BaseItemTypes)
				{
					classStrings.Add(item.ItemClass);
				}
			}

			// Class for Mystery Leaguestone, only adds confusion
			classStrings.Remove("Currency");

			var sb = new StringBuilder();
			sb.AppendLine("namespace Loki.Game.GameData");
			sb.AppendLine("{");
			sb.AppendLine("\tpublic class ItemClasses");
			sb.AppendLine("\t{");
			foreach (var classStr in classStrings)
			{
				sb.AppendLine(
					$"\t\tpublic const string {new string(classStr.Where(c => !char.IsWhiteSpace(c)).ToArray())} = \"{classStr}\";");
			}
			sb.AppendLine("\t}");
			sb.AppendLine("}");

			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\ItemClasses.cs", sb.ToString());
			Log.Info("\"ItemClasses.cs\" has been successfully generated.");
		}

		private void ButtonSaveInstanceInfoToFile_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				if (LokiPoe.IsInGame)
				{
					LokiPoe.InstanceInfo.SaveToFile();
				}
			}
#endif
		}

		private void ButtonSaveLocalDataToFile_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				if (LokiPoe.IsInGame)
				{
					LokiPoe.LocalData.SaveToFile();
				}
			}
#endif
		}

		private void ButtonDumpLabyrinthData_OnClick(object sender, RoutedEventArgs e)
		{
			using (LokiPoe.AcquireFrame())
			{
				if (LokiPoe.IsInGame)
				{
					LokiPoe.LocalData.Labyrinth.Dump();
				}
			}
		}

		private void ButtonDebugDivineFont_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				if (LokiPoe.IsInGame)
				{
					LokiPoe.InGameState.DivineFontUi.Debug();
				}
			}
#endif
		}

		private void ButtonDebugPantheon_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				if (LokiPoe.IsInGame)
				{
					LokiPoe.InGameState.PantheonUi.Debug();
				}
			}
#endif
		}

		private void ButtonSavePlayerToFile_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				if (LokiPoe.IsInGame)
				{
					Directory.CreateDirectory("dump");

					File.WriteAllText(
						string.Format("dump\\Me-{0}.txt", DateTime.Now.ToString("s").Replace(":", "_").Replace("-", "_")),
						LokiPoe.Me.Dump());
				}
			}
#endif
		}

		private void ButtonDumpPPantheonPanelLayout_OnClick(object sender, RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var entry in Dat.PantheonPanelLayout)
				{
					sb.Append(entry.ToString());
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\PantheonPanelLayout-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("PantheonPanelLayout saved!");
		}

		private void ButtonDumpWords_OnClick(object sender, RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("[Index] [...]");
			sb.AppendLine();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var entry in Dat.Words)
				{
					sb.Append(entry.ToString());
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\Words-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("Words saved!");
		}

		private void ButtonDumpFlavourText_OnClick(object sender, RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("[Index] [...]");
			sb.AppendLine();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var entry in Dat.FlavourText)
				{
					sb.Append(entry.ToString());
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\FlavourText-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("FlavourText saved!");
		}

		private void ButtonDumpLabyrinthTrials_OnClick(object sender, RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("[Index] [...]");
			sb.AppendLine();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var entry in Dat.LabyrinthTrials)
				{
					sb.AppendFormat("[{0}] [{1}] [{2}]", entry.Id, entry.WorldArea.Id, entry.WorldArea.Name);
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\LabyrinthTrials-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("LabyrinthTrials saved!");
		}

		private void ButtonDebugTutorialUi_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.TutorialUi.Debug();
			}
#endif
		}

		private void ButtonDumpStates_OnClick(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.StateManager.Debug();
			}
#endif
		}

		private void ButtonDumpSkillGems_Click(object sender, RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("[Index] [...]");
			sb.AppendLine();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var entry in Dat.SkillGems)
				{
					sb.Append(entry.ToString());
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\SkillGems-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("SkillGems saved!");
		}

		private void ButtonDumpItemExperiencePerLevel_Click(object sender, RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("[Index] [Item] [Level] [Experience]");
			sb.AppendLine();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var entry in Dat.ItemExperiencePerLevel)
				{
					sb.AppendFormat("[{0}] [{1}] [{2}]", entry.BaseItemTypeWrapper.Name, entry.Level, entry.Experience);
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\ItemExperiencePerLevel-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("ItemExperiencePerLevel saved!");
		}

		private void ButtonDumpSkillData_Click(object sender, RoutedEventArgs e)
		{
			Dat.DumpSkillData();
		}

		private void ButtonGenerateKnownNames_Click(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.StashUi.DivinationTab.GenerateKnownNames();
			}
#endif
		}

		private void ButtonGenerateCurrencyStashTabLayoutCode_Click(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				Dat.GenerateCurrencyStashTabLayoutCode();
			}
#endif
		}

		private void ButtonGenerateDivinationCardStashTabLayoutCode_Click(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				Dat.GenerateDivinationCardStashTabLayoutCode();
			}
#endif
		}

		private void ButtonGenerateEssenceStashTabLayoutCode_Click(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				Dat.GenerateEssenceStashTabLayoutCode();
			}
#endif
		}

		private void ButtonGenerateFragmentStashTabLayoutCode_Click(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				Dat.GenerateFragmentStashTabLayoutCode();
			}
#endif
		}

		private void ButtonDebugFragmentTab_Click(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.StashUi.FragmentTab.Debug();
			}
#endif
		}

		private void ButtonDebugProphecyPopupUi_Click(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.ProphecyPopupUi.Debug();
			}
#endif
		}

		private void ButtonDebugBestiary_Click(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.BeastCapturedUi.Debug();
			}
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.NewBeastRecipeUi.Debug();
			}
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.BloodAltarUi.Debug();
			}
#endif
		}

		private void ButtonDumpBestiaryCapturableMonsters_Click(object sender, RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var entry in Dat.BestiaryCapturableMonsters)
				{
					sb.Append(entry.ToString());
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\BestiaryCapturableMonsters-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("BestiaryCapturableMonsters saved!");
		}

		private void ButtonDumpBestiaryGenus_Click(object sender, RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var entry in Dat.BestiaryGenus)
				{
					sb.Append(entry.ToString());
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\BestiaryGenus-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("BestiaryGenus saved!");
		}

		private void ButtonDumpBestiaryGroups_Click(object sender, RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var entry in Dat.BestiaryGroups)
				{
					sb.Append(entry.ToString());
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\BestiaryGroups-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("BestiaryGroups saved!");
		}

		private void ButtonDumpBestiaryFamilies_Click(object sender, RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var entry in Dat.BestiaryFamilies)
				{
					sb.Append(entry.ToString());
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\BestiaryFamilies-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("BestiaryFamilies saved!");
		}

		private void ButtonDumpBestiaryEncounters_Click(object sender, RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var entry in Dat.BestiaryEncounters)
				{
					sb.Append(entry.ToString());
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\BestiaryEncounters-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("BestiaryEncounters saved!");
		}

		private void ButtonDumpBestiaryNets_Click(object sender, RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var entry in Dat.BestiaryNets)
				{
					sb.Append(entry.ToString());
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\BestiaryNets-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("BestiaryNets saved!");
		}

		private void ButtonDumpBestiaryRecipeComponent_Click(object sender, RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var entry in Dat.BestiaryRecipeComponent)
				{
					sb.Append(entry.ToString());
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\BestiaryRecipeComponent-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("BestiaryRecipeComponent saved!");
		}

		private void ButtonDumpBestiaryRecipeItemCreation_Click(object sender, RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var entry in Dat.BestiaryRecipeItemCreation)
				{
					sb.Append(entry.ToString());
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\BestiaryRecipeItemCreation-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("BestiaryRecipeItemCreation saved!");
		}

		private void ButtonDumpBestiaryRecipes_Click(object sender, RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var entry in Dat.BestiaryRecipes)
				{
					sb.Append(entry.ToString());
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\BestiaryRecipes-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("BestiaryRecipes saved!");
		}

		private void ButtonSaveUiDataToFile_Click(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				if (LokiPoe.IsInGame)
				{
					LokiPoe.InGameState.SaveUiToFile();
				}
			}
#endif
		}

		private void ButtonSavePlayerSkillDataToFile_Click(object sender, RoutedEventArgs e)
		{
			using (LokiPoe.AcquireFrame())
			{
				if (LokiPoe.IsInGame)
				{
					var sb = new StringBuilder();
					foreach(var skill in LokiPoe.Me.AvailableSkills)
					{
						sb.AppendLine(skill.ToString());
						sb.AppendLine();
					}

					//Log.Debug(sb.ToString());

					var fn = string.Format("dump\\AvailableSkills-{0}.txt", DateTime.Now.ToString("s").Replace(":", "_").Replace("-", "_"));
					Directory.CreateDirectory("dump");
					File.WriteAllText(fn, sb.ToString());
					Log.DebugFormat("AvailableSkills saved!");

					try
					{
						Process.Start(Path.GetFullPath(fn));
					}
					catch
					{
					}
				}
			}
		}

		private void ButtonDebugShopUi_Click(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.ShopUi.Debug();
			}
#endif
		}

		private void ButtonDebugIncursion_Click(object sender, RoutedEventArgs e)
		{
#if DEV_DEBUG
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.TempleOfAtzoatlUi.Debug();
			}
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.CorruptionAltarUi.Debug();
			}
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.TableOfSacrificeUi.Debug();
			}
			using (LokiPoe.AcquireFrame())
			{
				LokiPoe.InGameState.LapidaryLensUi.Debug();
			}
#endif
		}

		private void ButtonDumpIncursionRooms_OnClick(object sender, RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var entry in Dat.IncursionRooms)
				{
					sb.Append(entry.ToString());
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\IncursionRooms-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("IncursionRooms saved!");
		}

		private void ButtonDumpIncursionArchitect_OnClick(object sender, RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			using (LokiPoe.AcquireFrame())
			{
				foreach (var entry in Dat.IncursionArchitect)
				{
					sb.Append(entry.ToString());
					sb.AppendLine();
				}
			}
			//Log.Debug(sb.ToString());
			Directory.CreateDirectory("dump");
			File.WriteAllText("dump\\IncursionArchitect-" + LokiPoe.SupportedClientVersion + ".txt", sb.ToString());
			Log.DebugFormat("IncursionArchitect saved!");
		}
	}
}
