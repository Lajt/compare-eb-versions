using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Loki.Bot;
using Loki.Common.MVVM;
using Loki.Game.GameData;
using Loki.Game.Objects;
using Newtonsoft.Json;

namespace Legacy.ItemFilterEditor
{
	/// <summary>The properties to filter items.</summary>
	public class Filter : NotificationObject, IEquatable<Filter>
	{
		/// <summary>
		/// The name of this filter.
		/// </summary>
		public string Name => Guid.ToString();

		/// <summary>
		/// Attempts to match an item based on the specified evaluation type.
		/// </summary>
		/// <param name="item">The item to match.</param>
		/// <param name="type">The type of filter to match.</param>
		/// <returns>true if this filter matches the item, and false otherwise.</returns>
		public bool Match(Item item, EvaluationType type)
		{
			return false;
		}

		private Guid? _cachedGuid;
		private string _description;
		private bool _enabled;
		private bool? _hasToBeShaper;
		private bool? _hasToBeElder;
		private bool? _hasToBeElderOrShaper;
		private bool? _hasToBeIdentified;
		private bool? _hasToBeUnidentified;
		private int? _itemLevelMax;
		private int? _itemLevelMin;
		private int? _maxHeight;
		private int? _maxLinks;
		private int? _maxQuality;
		private int? _maxSockets;
		private int? _maxWidth;
		private int? _minHeight;
		private int? _minLinks;
		private int? _minQuality;
		private int? _minSockets;
		private int? _minWidth;
		private int? _highestImplicitValue;
		private bool? _nameMatchAnyRatherThanAll;
		private bool? _nameRegex;
		private ObservableCollection<string> _names;
		private List<Rarity> _rarities;
		private ObservableCollection<string> _socketColors;
		private bool? _typeMatchAnyRatherThanAll;
		private bool? _typeRegex;
		private ObservableCollection<string> _types;
		private ObservableCollection<string> _affixes;

		#region Equality members

		/// <summary>
		///     Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		///     true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(Filter other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}
			if (ReferenceEquals(this, other))
			{
				return true;
			}
			return Guid.Equals(other.Guid);
		}

		/// <summary>
		///     Determines whether the specified <see cref="T:System.Object" /> is equal to the current
		///     <see cref="T:System.Object" />.
		/// </summary>
		/// <returns>
		///     true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />;
		///     otherwise, false.
		/// </returns>
		/// <param name="obj">The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Object" />. </param>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((Filter)obj);
		}

		/// <summary>
		///     Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		///     A hash code for the current <see cref="T:System.Object" />.
		/// </returns>
		public override int GetHashCode()
		{
			return Guid.GetHashCode();
		}

		/// <summary> </summary>
		public static bool operator ==(Filter left, Filter right)
		{
			return Equals(left, right);
		}

		/// <summary> </summary>
		public static bool operator !=(Filter left, Filter right)
		{
			return !Equals(left, right);
		}

		#endregion

		/// <summary> </summary>
		public Filter()
		{
			_enabled = true;
		}

		/// <summary>The filter description.</summary>
		public string Description
		{
			get
			{
				return _description;
			}
			set
			{
				if (value == _description)
				{
					return;
				}
				_description = value;
				NotifyPropertyChanged(() => Description);
			}
		}

		/// <summary>Used for filter comparisons </summary>
		[JsonIgnore]
		public Guid Guid
		{
			get
			{
				if (_cachedGuid == null)
				{
					_cachedGuid = Guid.NewGuid();
				}
				return _cachedGuid.Value;
			}
		}

		/// <summary>Should this filter be checked.</summary>
		[DefaultValue(true)]
		public bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				if (value.Equals(_enabled))
				{
					return;
				}
				_enabled = value;
				NotifyPropertyChanged(() => Enabled);
			}
		}

		/// <summary>A list of types to match.</summary>
		public ObservableCollection<string> Types
		{
			get
			{
				return _types;
			}
			set
			{
				if (Equals(value, _types))
				{
					return;
				}
				_types = value;
				NotifyPropertyChanged(() => Types);
			}
		}

		/// <summary>Should type regex be used?</summary>
		public bool? TypeRegex
		{
			get
			{
				return _typeRegex;
			}
			set
			{
				if (value.Equals(_typeRegex))
				{
					return;
				}
				_typeRegex = value;
				NotifyPropertyChanged(() => TypeRegex);
			}
		}

		/// <summary>Should any type entry be matched rather than all.</summary>
		public bool? TypeMatchAnyRatherThanAll
		{
			get
			{
				return _typeMatchAnyRatherThanAll;
			}
			set
			{
				if (value.Equals(_typeMatchAnyRatherThanAll))
				{
					return;
				}
				_typeMatchAnyRatherThanAll = value;
				NotifyPropertyChanged(() => TypeMatchAnyRatherThanAll);
			}
		}

		/// <summary>A list of names to match.</summary>
		public ObservableCollection<string> Names
		{
			get
			{
				return _names;
			}
			set
			{
				if (Equals(value, _names))
				{
					return;
				}
				_names = value;
				NotifyPropertyChanged(() => Names);
			}
		}

		/// <summary>Should name regex be used?</summary>
		public bool? NameRegex
		{
			get
			{
				return _nameRegex;
			}
			set
			{
				if (value.Equals(_nameRegex))
				{
					return;
				}
				_nameRegex = value;
				NotifyPropertyChanged(() => NameRegex);
			}
		}

		/// <summary>Should any name entry be matched rather than all.</summary>
		public bool? NameMatchAnyRatherThanAll
		{
			get
			{
				return _nameMatchAnyRatherThanAll;
			}
			set
			{
				if (value.Equals(_nameMatchAnyRatherThanAll))
				{
					return;
				}
				_nameMatchAnyRatherThanAll = value;
				NotifyPropertyChanged(() => NameMatchAnyRatherThanAll);
			}
		}

		/// <summary>The min item level for the item.</summary>
		public int? ItemLevelMin
		{
			get
			{
				return _itemLevelMin;
			}
			set
			{
				if (value == _itemLevelMin)
				{
					return;
				}
				_itemLevelMin = value;
				NotifyPropertyChanged(() => ItemLevelMin);
			}
		}

		public int? HighestImplicitValue
		{
			get
			{
				return _highestImplicitValue;
			}
			set
			{
				if (value == _highestImplicitValue)
				{
					return;
				}
				_highestImplicitValue = value;
				NotifyPropertyChanged(() => HighestImplicitValue);
			}
		}

		/// <summary>The max item level for the item.</summary>
		public int? ItemLevelMax
		{
			get
			{
				return _itemLevelMax;
			}
			set
			{
				if (value == _itemLevelMax)
				{
					return;
				}
				_itemLevelMax = value;
				NotifyPropertyChanged(() => ItemLevelMax);
			}
		}

		/// <summary>A list of rarites the item must match any of.</summary>
		public List<Rarity> Rarities
		{
			get
			{
				return _rarities;
			}
			set
			{
				if (Equals(value, _rarities))
				{
					return;
				}
				_rarities = value;
				NotifyPropertyChanged(() => Rarities);
			}
		}

		/// <summary>The min quality of the item.</summary>
		public int? MinQuality
		{
			get
			{
				return _minQuality;
			}
			set
			{
				if (value == _minQuality)
				{
					return;
				}
				_minQuality = value;
				NotifyPropertyChanged(() => MinQuality);
			}
		}

		/// <summary>The max quality of the item.</summary>
		public int? MaxQuality
		{
			get
			{
				return _maxQuality;
			}
			set
			{
				if (value == _maxQuality)
				{
					return;
				}
				_maxQuality = value;
				NotifyPropertyChanged(() => MaxQuality);
			}
		}

		/// <summary>The min sockets the item must contain.</summary>
		public int? MinSockets
		{
			get
			{
				return _minSockets;
			}
			set
			{
				if (value == _minSockets)
				{
					return;
				}
				_minSockets = value;
				NotifyPropertyChanged(() => MinSockets);
			}
		}

		/// <summary>The max sockets the item must contain.</summary>
		public int? MaxSockets
		{
			get
			{
				return _maxSockets;
			}
			set
			{
				if (value == _maxSockets)
				{
					return;
				}
				_maxSockets = value;
				NotifyPropertyChanged(() => MaxSockets);
			}
		}

		/// <summary>The min link the item must contain.</summary>
		public int? MinLinks
		{
			get
			{
				return _minLinks;
			}
			set
			{
				if (value == _minLinks)
				{
					return;
				}
				_minLinks = value;
				NotifyPropertyChanged(() => MinLinks);
			}
		}

		/// <summary>The max link the item must contain.</summary>
		public int? MaxLinks
		{
			get
			{
				return _maxLinks;
			}
			set
			{
				if (value == _maxLinks)
				{
					return;
				}
				_maxLinks = value;
				NotifyPropertyChanged(() => MaxLinks);
			}
		}

		/// <summary>The min width of the item.</summary>
		public int? MinWidth
		{
			get
			{
				return _minWidth;
			}
			set
			{
				if (value == _minWidth)
				{
					return;
				}
				_minWidth = value;
				NotifyPropertyChanged(() => MinWidth);
			}
		}

		/// <summary>The max width of the item.</summary>
		public int? MaxWidth
		{
			get
			{
				return _maxWidth;
			}
			set
			{
				if (value == _maxWidth)
				{
					return;
				}
				_maxWidth = value;
				NotifyPropertyChanged(() => MaxWidth);
			}
		}

		/// <summary>The min height of the item.</summary>
		public int? MinHeight
		{
			get
			{
				return _minHeight;
			}
			set
			{
				if (value == _minHeight)
				{
					return;
				}
				_minHeight = value;
				NotifyPropertyChanged(() => MinHeight);
			}
		}

		/// <summary>The max height of the item.</summary>
		public int? MaxHeight
		{
			get
			{
				return _maxHeight;
			}
			set
			{
				if (value == _maxHeight)
				{
					return;
				}
				_maxHeight = value;
				NotifyPropertyChanged(() => MaxHeight);
			}
		}

		/// <summary>A list of affixes to match.</summary>
		public ObservableCollection<string> Affixes
		{
			get
			{
				return _affixes;
			}
			set
			{
				if (Equals(value, _affixes))
				{
					return;
				}
				_affixes = value;
				NotifyPropertyChanged(() => Affixes);
			}
		}

		/// <summary>A list of socket colors to match.</summary>
		public ObservableCollection<string> SocketColors
		{
			get
			{
				return _socketColors;
			}
			set
			{
				if (Equals(value, _socketColors))
				{
					return;
				}
				_socketColors = value;
				NotifyPropertyChanged(() => SocketColors);
			}
		}

		/// <summary>Does this item have to be identified.</summary>
		public bool? HasToBeIdentified
		{
			get
			{
				return _hasToBeIdentified;
			}
			set
			{
				if (value.Equals(_hasToBeIdentified))
				{
					return;
				}
				_hasToBeIdentified = value;
				NotifyPropertyChanged(() => HasToBeIdentified);
			}
		}

		/// <summary>Does this item have to be unidentified.</summary>
		public bool? HasToBeUnidentified
		{
			get
			{
				return _hasToBeUnidentified;
			}
			set
			{
				if (value.Equals(_hasToBeUnidentified))
				{
					return;
				}
				_hasToBeUnidentified = value;
				NotifyPropertyChanged(() => HasToBeUnidentified);
			}
		}

		/// <summary>Does this item have to be a shaper item.</summary>
		public bool? HasToBeShaper
		{
			get
			{
				return _hasToBeShaper;
			}
			set
			{
				if (value.Equals(_hasToBeShaper))
				{
					return;
				}
				_hasToBeShaper = value;
				NotifyPropertyChanged(() => HasToBeShaper);
			}
		}

		/// <summary>Does this item have to be an elder item.</summary>
		public bool? HasToBeElder
		{
			get
			{
				return _hasToBeElder;
			}
			set
			{
				if (value.Equals(_hasToBeElder))
				{
					return;
				}
				_hasToBeElder = value;
				NotifyPropertyChanged(() => HasToBeElder);
			}
		}

		/// <summary>Does this item have to be an elder or shaper item.</summary>
		public bool? HasToBeElderOrShaper
		{
			get
			{
				return _hasToBeElderOrShaper;
			}
			set
			{
				if (value.Equals(_hasToBeElderOrShaper))
				{
					return;
				}
				_hasToBeElderOrShaper = value;
				NotifyPropertyChanged(() => HasToBeElderOrShaper);
			}
		}

		#region Overrides of Object

		public override string ToString()
		{
			return Description;
		}

		#endregion
	}
}
