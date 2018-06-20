using System.Collections.ObjectModel;

namespace Legacy.ItemFilterEditor
{
	public class Category
	{
		/// <summary> </summary>
		public Category()
		{
			Filters = new ObservableCollection<Filter>();
		}

		#region Overrides of Object

		public override string ToString()
		{
			return "Category: " + Description;
		}

		#endregion

		/// <summary> </summary>
		public string Description { get; set; }

		/// <summary> </summary>
		public MyEvaluationType Type { get; set; }

		/// <summary> </summary>
		public ObservableCollection<Filter> Filters { get; set; }
	}
}
