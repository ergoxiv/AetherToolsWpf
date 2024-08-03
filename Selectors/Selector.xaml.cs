// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Selectors;

using PropertyChanged;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using XivToolsWpf;

/// <summary>
/// Interaction logic for Selector.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class Selector : UserControl, INotifyPropertyChanged
{
	public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(object), typeof(Selector), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnValueChangedStatic)));
	public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(Selector), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnValueChangedStatic)));

	private static readonly Dictionary<Type, string?> SearchInputs = new();
	private static readonly Dictionary<Type, double> ScrollPositions = new();
	private readonly List<object> entries = new();
	private readonly CollectionViewSource filteredItemsViewSource = new();

	private bool searching = false;
	private bool idle = true;
	private string[]? searchQuery;
	private bool xamlLoading = false;

	/// <summary>
	/// Initializes a new instance of the <see cref="Selector"/> class.
	/// </summary>
	public Selector()
	{
		this.InitializeComponent();
		this.xamlLoading = true;
		this.ContentArea.DataContext = this;

		this.PropertyChanged += this.OnPropertyChanged;
		this.ProgressBar.Visibility = Visibility.Visible;

		this.filteredItemsViewSource.Source = this.entries;
		this.filteredItemsViewSource.Filter += this.OnFilter;
	}

	/// <summary>
	/// Delegate for the selection changed event.
	/// </summary>
	/// <param name="close">Indicates whether to close the selector.</param>
	public delegate void SelectorSelectedEvent(bool close);

	/// <summary>
	/// Delegate for the filter event.
	/// </summary>
	/// <param name="item">The item to evaluate the filter against.</param>
	/// <param name="search">The search query.</param>
	/// <returns>True if the item matches the filter; otherwise, false.</returns>
	public delegate bool FilterEvent(object item, string[]? search);

	/// <summary>
	/// Delegate for the sort event.
	/// </summary>
	/// <param name="itemA">The first item to compare.</param>
	/// <param name="itemB">The second item to compare.</param>
	/// <returns>An integer that indicates the relative order of the items being compared.
	/// </returns>
	public delegate int SortEvent(object itemA, object itemB);

	/// <summary>
	/// Delegate for the load items event.
	/// </summary>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public delegate Task GetItemsEvent();

	/// <summary>
	/// Event that is raised when a property value changes.
	/// </summary>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>
	/// Event that is raised when an item is filtered.
	/// </summary>
	public event FilterEvent? Filter;

	/// <summary>
	/// Event that is raised when items are sorted.
	/// </summary>
	public event SortEvent? Sort;

	/// <summary>
	/// Event that is raised when the selection changes.
	/// </summary>
	public event SelectorSelectedEvent? SelectionChanged;

	/// <summary>
	/// Event that is raised to load items.
	/// </summary>
	public event GetItemsEvent? LoadItems;

	/// <summary>
	/// Gets the filtered items.
	/// </summary>
	public ICollectionView FilteredItems => this.filteredItemsViewSource.View;

	/// <summary>
	/// Gets or sets a value indicating whether search is enabled. Default is true.
	/// </summary>
	public bool SearchEnabled { get; set; } = true;

	/// <summary>
	/// Gets or sets a value indicating whether there is a search query. Default is false.
	/// </summary>
	// TODO: Rename HasSearch to something more descriptive.
	public bool HasSearch { get; set; } = false;

	/// <summary>
	/// Gets or sets the type of the objects in the selector.
	/// </summary>
	public Type? ObjectType { get; set; }

	/// <summary>
	/// Gets all entries in the selector.
	/// </summary>
	public IEnumerable<object> Entries => this.entries;

	/// <summary>
	/// Gets or sets the value of the selector. Represents the currently selected item.
	/// </summary>
	// TODO: Rename Value to SelectedItem.
	public object? Value
	{
		get => this.GetValue(ValueProperty);
		set => this.SetValue(ValueProperty, value);
	}

	/// <summary>
	/// Gets or sets the item template for the selector.
	/// </summary>
	public DataTemplate ItemTemplate
	{
		get => (DataTemplate)this.GetValue(ItemTemplateProperty);
		set => this.SetValue(ItemTemplateProperty, value);
	}

	/// <summary>
	/// Gets or sets the scroll position of the selector.
	/// </summary>
	public double ScrollPosition
	{
		get => this.ScrollViewer?.VerticalOffset ?? 0;
		set => this.ScrollViewer?.ScrollToVerticalOffset(value);
	}

	/// <summary>
	/// Gets the contextual logger for the selector class.
	/// </summary>
	private static ILogger Log => Serilog.Log.ForContext<Selector>();

	/// <summary>
	/// Gets the scroll viewer for the list box.
	/// </summary>
	private ScrollViewer? ScrollViewer
	{
		get
		{
			Decorator? border = VisualTreeHelper.GetChild(this.ListBox, 0) as Decorator;
			if (border == null)
				return null;

			return border.Child as ScrollViewer;
		}
	}

	/// <summary>
	/// Called when the selector is closed.
	/// </summary>
	public void OnClosed()
	{
	}

	/// <summary>
	/// Clears all items from the selector list box.
	/// </summary>
	public void ClearItems()
	{
		lock (this.entries)
		{
			this.entries.Clear();
		}
	}

	/// <summary>
	/// Adds an item to the selector list box.
	/// </summary>
	/// <param name="item">The item to add.</param>
	public void AddItem(object item)
	{
		lock (this.entries)
		{
			this.entries.Add(item);
			this.ObjectType ??= item.GetType();
		}
	}

	/// <summary>
	/// Adds a collection of items to the selector list box.
	/// </summary>
	/// <param name="items">The items to add.</param>
	public void AddItems(IEnumerable<object> items)
	{
		lock (this.entries)
		{
			this.entries.AddRange(items);
			this.ObjectType ??= items.FirstOrDefault()?.GetType();
		}
	}

	/// <summary>
	/// Triggers an asynchronous filter event of the items in the selector list box.
	/// </summary>
	/// <remarks>
	/// This method does not wait for the filter task to complete.
	/// </remarks>
	public void FilterItems()
	{
		Task.Run(this.DoFilter);
	}

	/// <summary>
	/// Trigger a filter event of the items in the selector list box.
	/// </summary>
	/// <remarks>
	/// Unlike <see cref="FilterItems"/>, this method is awaitable.
	/// </remarks>
	public Task FilterItemsAsync()
	{
		return this.DoFilter();
	}

	/// <summary>
	/// Raises the item selection changed event.
	/// </summary>
	public void RaiseSelectionChanged()
	{
		this.SelectionChanged?.Invoke(false);
	}

	/// <summary>
	/// Called when the value of a dependency property changes.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	private static void OnValueChangedStatic(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is Selector view)
		{
			view.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(e.Property.Name));
		}
	}

	/// <summary>
	/// Called when the selector is loaded.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		if (this.ObjectType != null)
		{
			if (SearchInputs.TryGetValue(this.ObjectType, out var searchInput))
				this.SearchBox.Text = searchInput;

			if (ScrollPositions.TryGetValue(this.ObjectType, out var scrollPosition))
				this.ScrollPosition = scrollPosition;
		}

		Keyboard.Focus(this.SearchBox);
		this.SearchBox.CaretIndex = int.MaxValue;
		this.xamlLoading = false;

		if (this.LoadItems != null)
		{
			Task.Run(async () =>
			{
				await Dispatch.NonUiThread();
				await this.LoadItems.Invoke();

				await this.FilterItemsAsync();

				await Dispatch.MainThread();
				this.ProgressBar.Visibility = Visibility.Collapsed;

				this.ListBox.ScrollIntoView(this.Value);
			});
		}
		else
		{
			this.ProgressBar.Visibility = Visibility.Collapsed;
		}
	}

	/// <summary>
	/// Called when the selector is unloaded.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		if (this.ObjectType == null)
			return;

		SearchInputs[this.ObjectType] = this.SearchBox.Text;
		ScrollPositions[this.ObjectType] = this.ScrollPosition;
	}

	/// <summary>
	/// An event that is raised when the text in the search box changes.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	private void OnTextChanged(object sender, TextChangedEventArgs e)
	{
		if (this.ObjectType == null)
			return;

		string str = this.SearchBox.Text;

		this.HasSearch = !string.IsNullOrWhiteSpace(str);

		SearchInputs[this.ObjectType] = str;
		Task.Run(async () => { await this.Search(str); });

		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.HasSearch)));
	}

	/// <summary>
	/// An event that is raised when the clear search button is clicked.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	private void OnClearSearchClicked(object sender, RoutedEventArgs e)
	{
		this.SearchBox.Text = string.Empty;
	}

	/// <summary>
	/// An event that is raised when any of the public properties change state.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(this.entries))
		{
			Task.Run(this.DoFilter);
		}
	}

	/// <summary>
	/// Performs a search with the specified query.
	/// </summary>
	/// <param name="str">The search query.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	private async Task Search(string str)
	{
		this.idle = false;

		if (!this.xamlLoading)
			await Task.Delay(50);

		try
		{
			while (this.searching)
				await Task.Delay(100);

			this.searching = true;
			string currentInput = await Application.Current.Dispatcher.InvokeAsync<string>(() =>
			{
				return this.SearchBox.Text;
			});

			// If the input was changed, abort this task
			if (str != currentInput)
			{
				this.searching = false;
				return;
			}

			this.searchQuery = string.IsNullOrEmpty(str) ? null : str.ToLower().Split(' ');

			await Task.Run(this.DoFilter);
			this.searching = false;
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to perform search");
		}

		this.idle = true;
	}

	/// <summary>
	/// An event that is raised when an update is triggered on the list of filtered items.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	private void OnFilter(object sender, FilterEventArgs e)
	{
		if (this.Filter == null)
		{
			e.Accepted = true;
			return;
		}

		e.Accepted = this.Filter.Invoke(e.Item, this.searchQuery);
	}

	/// <summary>
	/// Performs selector item filtering.
	/// </summary>
	/// <returns>A task that represents the asynchronous operation.</returns>
	private async Task DoFilter()
	{
		this.idle = false;

		if (!this.SearchEnabled)
			this.searchQuery = null;

		await Application.Current.Dispatcher.InvokeAsync(() =>
		{
			this.filteredItemsViewSource.View.Refresh();
		});

		this.idle = true;
	}

	/// <summary>
	/// An event that is raised when the selection changes in the selector list box.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (e.AddedItems.Count <= 0 || this.searching)
			return;

		this.RaiseSelectionChanged();
	}

	/// <summary>
	/// An event that is raised when a key is pressed in the search box.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	private async void OnSearchBoxKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key != Key.Enter)
			return;

		while (!this.idle)
			await Task.Delay(10);

		if (this.FilteredItems.IsEmpty)
			return;

		this.FilteredItems.MoveCurrentToFirst();
		this.Value = this.FilteredItems.CurrentItem;
	}

	/// <summary>
	/// An event that is raised when an item in the list box is double-clicked.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	private void OnDoubleClick(object sender, MouseButtonEventArgs e)
	{
		Point pos = e.GetPosition(this.ListBox);

		// over scrollbar
		if (pos.X > this.ListBox.ActualWidth - SystemParameters.VerticalScrollBarWidth)
			return;

		this.SelectionChanged?.Invoke(true);
	}

	/// <summary>
	/// Comparison logic for comparing objects stored in the selector.
	/// </summary>
	private class Compare : IComparer<object>
	{
		private readonly SortEvent sortDelegate;

		/// <summary>
		/// Initializes a new instance of the <see cref="Compare"/> class.
		/// </summary>
		/// <param name="sortDelegate">The sort delegate.</param>
		public Compare(SortEvent sortDelegate)
		{
			this.sortDelegate = sortDelegate ?? throw new ArgumentNullException(nameof(sortDelegate));
		}

		/// <summary>
		/// Compares two objects.
		/// </summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns>An integer that indicates the relative order of the objects being compared.</returns>
		int IComparer<object>.Compare(object? x, object? y)
		{
			if (x == null || y == null)
				return 0;

			return this.sortDelegate.Invoke(x, y);
		}
	}
}
