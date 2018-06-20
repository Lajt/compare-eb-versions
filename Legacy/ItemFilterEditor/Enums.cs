namespace Legacy.ItemFilterEditor
{
	/// <summary> </summary>
	public enum StatCategory
	{
		/// <summary> </summary>
		None,

		/// <summary> </summary>
		Explicit,

		/// <summary> </summary>
		Implicit,

		/// <summary> </summary>
		Total
	};

	/// <summary> </summary>
	public enum StatRequirement
	{
		/// <summary>
		/// The filter should not fail if this stat is not matched, 
		/// but at least one optional filters has to be matched.
		/// </summary>
		Optional,

		/// <summary>The filter should fail if this stat is not matched.</summary>
		Required,
	}

	/// <summary> </summary>
	public enum StatValueOperation
	{
		/// <summary> None </summary>
		None,

		// ReSharper disable once InconsistentNaming
		/// <summary> Less than</summary>
		LT,

		// ReSharper disable once InconsistentNaming
		/// <summary> Greater than</summary>
		GT,

		// ReSharper disable once InconsistentNaming
		/// <summary> Greater than or equal </summary>
		GTE,

		// ReSharper disable once InconsistentNaming
		/// <summary> Less than or equal</summary>
		LTE,

		// ReSharper disable once InconsistentNaming
		/// <summary> Equal</summary>
		E,

		// ReSharper disable once InconsistentNaming
		/// <summary> Not equal</summary>
		NE,
	}

	/// <summary>
	/// The different types of evaluation types for filters.
	/// NOTE: This mirrors the bot, but has to be done this way to work around an issue with 
	/// the build server and the baml created on the server.
	/// </summary>
	public enum MyEvaluationType
	{
		/// <summary>Nothing.</summary>
		None,

		/// <summary>Filter for looting an item.</summary>
		PickUp,

		/// <summary>Filter to save an item.</summary>
		Save,

		/// <summary>Filter for selling an item.</summary>
		Sell,

		/// <summary>Filter for buying an item.</summary>
		Buy,

		/// <summary>Filter for iding an item.</summary>
		Id,

		/// <summary>Filter for excluding an item from the ChaosRecipe plugin provided.</summary>
		ExcludeFromChaosRecipe,
	}

	/// <summary>
	/// A simple enumeration to define the rarities in the game.
	/// NOTE: This mirrors the bot, but has to be done this way to work around an issue with 
	/// the build server and the baml created on the server.
	/// </summary>
	public enum MyRarity
	{
		/// <summary>Normal.</summary>
		Normal = 0,

		/// <summary>Magic.</summary>
		Magic = 1,

		/// <summary>Rare.</summary>
		Rare = 2,

		/// <summary>Unique.</summary>
		Unique = 3,

		/// <summary>Gem.</summary>
		Gem = 4,

		/// <summary>Currency.</summary>
		Currency = 5,

		/// <summary>Quest.</summary>
		Quest = 6,

		/// <summary>Prophecy.</summary>
		Prophecy = 7
	}
}
