using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Loki;
using Loki.Common;

namespace Legacy.ObjectExplorer
{
	/// <summary>Settings for the Dev tab. </summary>
	public class ObjectExplorerSettings : JsonSettings
	{
		private static ObjectExplorerSettings _instance;

		/// <summary>The current instance for this class. </summary>
		public static ObjectExplorerSettings Instance => _instance ?? (_instance = new ObjectExplorerSettings());

		/// <summary>The default ctor. Will use the settings path "ObjectExplorer".</summary>
		public ObjectExplorerSettings()
			: base(GetSettingsFilePath(Configuration.Instance.Name, string.Format("{0}.json", "ObjectExplorer")))
		{
			// Setup defaults here if needed for properties that don't support DefaultValue.
		}

		private string _leftColumnDefinitionHeight;
		private string _rightColumnDefinitionHeight;
		private string _splitterColumnDefinitionHeight;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="leftColumnDefinition"></param>
		/// <param name="splitterColumnDefinition"></param>
		/// <param name="rightColumnDefinition"></param>
		public void LoadColumnDefinitions(ColumnDefinition leftColumnDefinition, ColumnDefinition splitterColumnDefinition,
			ColumnDefinition rightColumnDefinition)
		{
			var converter = new GridLengthConverter();
			// ReSharper disable PossibleNullReferenceException
			leftColumnDefinition.Width = (GridLength) converter.ConvertFromString(LeftColumnDefinitionHeight);
			splitterColumnDefinition.Width = (GridLength)converter.ConvertFromString(SplitterColumnDefinitionHeight);
			rightColumnDefinition.Width = (GridLength)converter.ConvertFromString(RightColumnDefinitionHeight);
			// ReSharper restore PossibleNullReferenceException
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="leftColumnDefinition"></param>
		/// <param name="splitterColumnDefinition"></param>
		/// <param name="rightColumnDefinition"></param>
		public void SaveColumnDefinitions(ColumnDefinition leftColumnDefinition, ColumnDefinition splitterColumnDefinition,
			ColumnDefinition rightColumnDefinition)
		{
			var converter = new GridLengthConverter();
			LeftColumnDefinitionHeight = converter.ConvertToString(leftColumnDefinition.Width);
			SplitterColumnDefinitionHeight = converter.ConvertToString(splitterColumnDefinition.Width);
			RightColumnDefinitionHeight = converter.ConvertToString(rightColumnDefinition.Width);
		}

		/// <summary>The left row grid splitter position.</summary>
		[DefaultValue("*")]
		public string LeftColumnDefinitionHeight
		{
			get
			{
				return _leftColumnDefinitionHeight;
			}
			set
			{
				if (value.Equals(_leftColumnDefinitionHeight))
				{
					return;
				}
				_leftColumnDefinitionHeight = value;
				NotifyPropertyChanged(() => LeftColumnDefinitionHeight);
			}
		}

		/// <summary>The bottom row grid splitter position.</summary>
		[DefaultValue("*")]
		public string RightColumnDefinitionHeight
		{
			get
			{
				return _rightColumnDefinitionHeight;
			}
			set
			{
				if (value.Equals(_rightColumnDefinitionHeight))
				{
					return;
				}
				_rightColumnDefinitionHeight = value;
				NotifyPropertyChanged(() => RightColumnDefinitionHeight);
			}
		}

		/// <summary>The splitter row grid splitter position.</summary>
		[DefaultValue("Auto")]
		public string SplitterColumnDefinitionHeight
		{
			get
			{
				return _splitterColumnDefinitionHeight;
			}
			set
			{
				if (value.Equals(_splitterColumnDefinitionHeight))
				{
					return;
				}
				_splitterColumnDefinitionHeight = value;
				NotifyPropertyChanged(() => SplitterColumnDefinitionHeight);
			}
		}
	}
}
