// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Selectors;

using PropertyChanged;
using Serilog;
using System;
using System.Collections.Concurrent;
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
/// Interaction logic for SelectorDrawer.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class Selector : UserControl, INotifyPropertyChanged
{
	public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(object), typeof(Selector), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnValueChangedStatic)));
	public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(Selector), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnValueChangedStatic)));
	public static readonly DependencyProperty ListBoxItemStyleProperty = DependencyProperty.Register(nameof(ListBoxItemStyle), typeof(Style), typeof(Selector), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnValueChangedStatic)));

	private static readonly Dictionary<Type, string?> SearchInputs = [];
	private static readonly Dictionary<Type, double> ScrollPositions = [];
	private readonly List<ItemEntry> entries = [];

	private bool searching = false;
	private bool idle = true;
	private string[]? searchQuery;
	private bool xamlLoading = false;
	private bool abortSearch = false;
	private bool filtering = false;

	public Selector()
	{
		this.InitializeComponent();
		this.xamlLoading = true;
		this.ContentArea.DataContext = this;

		this.PropertyChanged += this.OnPropertyChanged;
		this.ProgressBar.Visibility = Visibility.Visible;
	}

	public delegate void SelectorSelectedEvent(bool close);
	public delegate bool FilterEvent(object item, string[]? search);
	public delegate int SortEvent(object itemA, object itemB);
	public delegate Task GetItemsEvent();

	public event PropertyChangedEventHandler? PropertyChanged;
	public event FilterEvent? Filter;
	public event SortEvent? Sort;
	public event SelectorSelectedEvent? SelectionChanged;
	public event GetItemsEvent? LoadItems;

	public CollectionViewSource FilteredItems { get; set; } = new CollectionViewSource();

	public bool SearchEnabled { get; set; } = true;
	public bool HasSearch { get; set; } = false;
	public Type? ObjectType { get; set; }

	public IEnumerable<object> Entries => this.entries.Select(entry => entry.Item);

	public object? Value
	{
		get => this.GetValue(ValueProperty);
		set => this.SetValue(ValueProperty, value);
	}

	public DataTemplate ItemTemplate
	{
		get => (DataTemplate)this.GetValue(ItemTemplateProperty);
		set => this.SetValue(ItemTemplateProperty, value);
	}

	public Style ListBoxItemStyle
	{
		get => (Style)this.GetValue(ListBoxItemStyleProperty);
		set => this.SetValue(ListBoxItemStyleProperty, value);
	}

	public double ScrollPosition
	{
		get => this.ScrollViewer?.VerticalOffset ?? 0;
		set => this.ScrollViewer?.ScrollToVerticalOffset(value);
	}

	private static ILogger Log => Serilog.Log.ForContext<Selector>();

	private ScrollViewer? ScrollViewer
	{
		get
		{
			if (VisualTreeHelper.GetChild(this.ListBox, 0) is not Decorator border)
				return null;

			return border.Child as ScrollViewer;
		}
	}

	public void OnClosed()
	{
	}

	public void ClearItems()
	{
		lock (this.entries)
		{
			this.entries.Clear();
		}
	}

	public void AddItem(object item)
	{
		lock (this.entries)
		{
			var entry = new ItemEntry { Item = item, OriginalIndex = this.entries.Count };
			this.entries.Add(entry);

			if (this.ObjectType == null)
			{
				this.ObjectType = item.GetType();
			}
		}
	}

	public void AddItems(IEnumerable<object> items)
	{
		lock (this.entries)
		{
			if (this.ObjectType == null)
			{
				var firstItem = items.FirstOrDefault();
				if (firstItem != null)
				{
					this.ObjectType = firstItem.GetType();
				}
			}

			// Allocate enough space for all items to prevent resizing
			this.entries.Capacity = this.entries.Count + items.Count();

			foreach (object item in items)
			{
				var entry = new ItemEntry { Item = item, OriginalIndex = this.entries.Count };
				this.entries.Add(entry);
			}
		}
	}

	public void FilterItems() => Task.Run(this.DoFilter);

	public Task FilterItemsAsync() => this.DoFilter();

	public void RaiseSelectionChanged() => this.SelectionChanged?.Invoke(false);

	private static void OnValueChangedStatic(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is Selector view)
		{
			view.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(e.Property.Name));
		}
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		if (this.ObjectType != null && SearchInputs.TryGetValue(this.ObjectType, out var value))
		{
			this.SearchBox.Text = value;
		}

		Keyboard.Focus(this.SearchBox);
		this.SearchBox.CaretIndex = int.MaxValue;
		this.xamlLoading = false;

		// Capture the value from outside of the async task
		var currentValue = this.Value;

		if (this.LoadItems != null)
		{
			Task.Run(async () =>
			{
				await Dispatch.NonUiThread();
				await this.LoadItems.Invoke();

				await this.FilterItemsAsync();

				await Dispatch.MainThread();
				this.ProgressBar.Visibility = Visibility.Collapsed;

				if (this.ObjectType != null && ScrollPositions.TryGetValue(this.ObjectType, out double value))
				{
					this.ScrollPosition = value;
				}

				this.ListBox.ScrollIntoView(currentValue);
			});
		}
		else
		{
			this.ProgressBar.Visibility = Visibility.Collapsed;
		}
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		if (this.ObjectType == null)
			return;

		SearchInputs[this.ObjectType] = this.SearchBox.Text;
		ScrollPositions[this.ObjectType] = this.ScrollPosition;
	}

	private void OnSearchChanged(object sender, TextChangedEventArgs e)
	{
		if (this.ObjectType == null)
			return;

		string str = this.SearchBox.Text;

		this.HasSearch = !string.IsNullOrWhiteSpace(str);

		SearchInputs[this.ObjectType] = str;
		Task.Run(() => this.Search(str));

		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.HasSearch)));
	}

	private void OnClearSearchClicked(object sender, RoutedEventArgs e) => this.SearchBox.Text = string.Empty;

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(this.entries))
		{
			Task.Run(this.DoFilter);
		}
	}

	private async Task Search(string str)
	{
		this.idle = false;
		this.abortSearch = true;

		if (!this.xamlLoading)
			await Task.Delay(50);

		try
		{
			while (this.searching)
				await Task.Delay(100);

			this.searching = true;
			var currentInput = await Application.Current.Dispatcher.InvokeAsync(() => this.SearchBox.Text);

			// If the input was changed, abort this task
			if (str != currentInput)
			{
				this.searching = false;
				return;
			}

			this.searchQuery = string.IsNullOrEmpty(str) ? null : str.ToLower().Split(' ');

			this.abortSearch = false;
			await Task.Run(this.DoFilter);
			this.searching = false;
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to perform search");
		}

		this.idle = true;
	}

	private async Task DoFilter()
	{
		this.idle = false;

		if (!this.SearchEnabled)
			this.searchQuery = null;

		ConcurrentQueue<ItemEntry> entries;
		lock (this.entries)
		{
			entries = new ConcurrentQueue<ItemEntry>(this.entries);
		}

		ConcurrentBag<ItemEntry> filteredEntries = [];

		var tasks = Enumerable.Range(0, Environment.ProcessorCount).Select(_ => Task.Run(() =>
		{
			while (entries.TryDequeue(out var entry))
			{
				try
				{
					if (this.Filter != null && !this.Filter.Invoke(entry.Item, this.searchQuery))
					{
						continue;
					}
				}
				catch (Exception ex)
				{
					Log.Error(ex, $"Failed to filter selector item: {entry.Item}");
				}

				filteredEntries.Add(entry);

				if (this.abortSearch)
				{
					entries.Clear();
				}
			}
		})).ToArray();

		await Task.WhenAll(tasks);

		IOrderedEnumerable<ItemEntry>? sortedFilteredEntries = filteredEntries.OrderBy(cc => cc.OriginalIndex);

		if (this.Sort != null)
		{
			var comp = new Compare(this.Sort);
			sortedFilteredEntries = sortedFilteredEntries.OrderBy(cc => cc.Item, comp);
		}

		await Application.Current.Dispatcher.InvokeAsync(() =>
		{
			this.filtering = true;
			this.FilteredItems.Source = sortedFilteredEntries.Select(e => e.Item).ToList();
			this.ListBox.SelectedItem = null;
			this.filtering = false;
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.FilteredItems)));
		});

		this.idle = true;
	}

	private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (e.AddedItems.Count <= 0)
			return;

		if (this.searching || this.filtering)
			return;

		this.RaiseSelectionChanged();
	}

	private async void OnSearchBoxKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key != Key.Enter)
			return;

		while (!this.idle)
			await Task.Delay(10);

		ICollectionView collectionView = CollectionViewSource.GetDefaultView(this.FilteredItems.View);
		if (collectionView?.Cast<object>().Any() == true)
		{
			this.Value = collectionView.Cast<object>().FirstOrDefault();
			this.RaiseSelectionChanged();
		}
	}

	private void OnDoubleClick(object sender, MouseButtonEventArgs e)
	{
		Point pos = e.GetPosition(this.ListBox);

		// over scrollbar
		if (pos.X > this.ListBox.ActualWidth - SystemParameters.VerticalScrollBarWidth)
			return;

		this.SelectionChanged?.Invoke(true);
	}

	private struct ItemEntry
	{
		public object Item;
		public int OriginalIndex;
	}

	private class Compare(SortEvent filter) : IComparer<object>
	{
		private readonly SortEvent filter = filter;

		int IComparer<object>.Compare(object? x, object? y)
		{
			if (x == null || y == null)
				return 0;

			return this.filter.Invoke(x, y);
		}
	}
}
