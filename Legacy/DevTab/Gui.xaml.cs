using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using log4net;
using Loki;
using Loki.Bot;
using Loki.Common;
using Microsoft.Win32;

namespace Legacy.DevTab
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
			TextBoxCode.Text = DevTabSettings.Instance.Code;
		}

		internal void ButtonExecuteText_Click(object sender, RoutedEventArgs e)
		{
			DevTabSettings.Instance.Code = TextBoxCode.Text;
			try
			{
				var @class = TextBoxClassName.Text;
				if (string.IsNullOrEmpty(@class))
					return;
				var code = TextBoxCode.Text;
				if (string.IsNullOrEmpty(code))
					return;
				var assemblies = TextBoxAssemblies.Text.Split(new[]
				{
					'\r', '\n', '\t'
				},
					StringSplitOptions.RemoveEmptyEntries).ToList();
				Configuration.Instance.SaveAll();
				ThreadPool.QueueUserWorkItem(u => Dev_Execute(@class, code, assemblies));
			}
			catch (Exception ex)
			{
				Log.Error("An exception occurred:", ex);
			}
		}

		internal void ButtonExecuteFile_Click(object sender, RoutedEventArgs e)
		{
			DevTabSettings.Instance.Code = TextBoxCode.Text;
			try
			{
				string text;
				var @class = TextBoxClassName.Text;
				try
				{
					text = File.ReadAllText(TextBoxFileName.Text);
				}
				catch (Exception ex)
				{
					Log.Error(ex);
					return;
				}
				var assemblies = TextBoxAssemblies.Text.Split(new[]
				{
					'\r', '\n', '\t'
				},
					StringSplitOptions.RemoveEmptyEntries).ToList();
				if (string.IsNullOrEmpty(@class))
					return;
				if (string.IsNullOrEmpty(text))
					return;
				Configuration.Instance.SaveAll();
				ThreadPool.QueueUserWorkItem(u => Dev_Execute(@class, text, assemblies));
			}
			catch (Exception ex)
			{
				Log.Error("An exception occurred:", ex);
			}
		}

		private void ButtonChooseFile_Click(object sender, RoutedEventArgs e)
		{
			DevTabSettings.Instance.Code = TextBoxCode.Text;
			try
			{
				var openDialog = new OpenFileDialog();
				var ret = openDialog.ShowDialog();
				if (ret != null && ret.Value)
				{
					DevTabSettings.Instance.FileName = openDialog.FileName;
				}
			}
			catch (Exception ex)
			{
				Log.Error("An exception occurred:", ex);
			}
		}

		private static void Dev_Execute(string @class, string code, IEnumerable<string> userassemblies)
		{
			try
			{
				using (var cs = RoslynCodeCompiler.CreateLatestCSharpProvider())
				{
					// Setup the compilation process.
					var options = new CompilerParameters
					{
						GenerateExecutable = false,
						GenerateInMemory = false,
					};

					var assemblies = userassemblies.Select(s => s.ToLowerInvariant()).ToList();

					// Reference all currently loaded dynamic assemblies.
					foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
					{
						// Drew: We need to try/catch this, because sometimes we get an anon dynamic assembly from python loaded,
						// that doesn't have a CodeBase, so an exception is thrown and things break.
						try
						{
							if (asm.IsDynamic)
								continue;

							if (string.IsNullOrEmpty(asm.Location))
								continue;

							if (!options.ReferencedAssemblies.Contains(asm.Location))
							{
								options.ReferencedAssemblies.Add(asm.Location);
								assemblies.Remove(System.IO.Path.GetFileName(asm.Location).ToLowerInvariant());
							}
						}
						catch (Exception)
						{
						}
					}

					// Reference all user assemblies that weren't already added.
					foreach (var assembly in assemblies)
					{
						if (!options.ReferencedAssemblies.Contains(assembly))
						{
							options.ReferencedAssemblies.Add(assembly);
						}
					}

					// Compile the user's code.
					var res = cs.CompileAssemblyFromSource(options, code);

					// Handle errors.
					if (res.Errors.Count > 0)
					{
						var sb = new StringBuilder();
						foreach (CompilerError err in res.Errors)
						{
							sb.AppendFormat("Line number " + err.Line + ", Error Number: " + err.ErrorNumber + ", '" +
							                err.ErrorText + ";");
							sb.AppendLine();
						}
						throw new Exception(sb.ToString());
					}

					// Execute the user's code and log the result.
					var type = res.CompiledAssembly.GetType(@class);
					var obj = Activator.CreateInstance(type);
					var output = type.GetMethod("Execute").Invoke(obj, new object[]
					{
					});
					if (output != null)
					{
						Log.Info(output);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("An exception occurred:", ex);
			}
		}

		private void TextBoxCode_OnLostFocus(object sender, RoutedEventArgs e)
		{
			DevTabSettings.Instance.Code = TextBoxCode.Text;
		}
	}
}
