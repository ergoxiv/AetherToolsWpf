// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.DragAndDrop;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

public class DropTargetInsertionAdorner(UIElement adornedElement, DragEventArgs args, DropTargetInsertionAdorner.InsertPositions position = DropTargetInsertionAdorner.InsertPositions.Top) : DropTargetAdorner(adornedElement, args)
{
	public readonly InsertPositions InsertPosition = position;

	private static readonly Pen Pen;
	private static readonly PathGeometry Triangle;

	static DropTargetInsertionAdorner()
	{
		// Create the pen and triangle in a static constructor and freeze them to improve performance.
		const int triangleSize = 3;

		Pen = new Pen(Brushes.Gray, 2);
		Pen.Freeze();

		var firstLine = new LineSegment(new Point(0, -triangleSize), false);
		firstLine.Freeze();
		var secondLine = new LineSegment(new Point(0, triangleSize), false);
		secondLine.Freeze();

		var figure = new PathFigure { StartPoint = new Point(triangleSize, 0) };
		figure.Segments.Add(firstLine);
		figure.Segments.Add(secondLine);
		figure.Freeze();

		Triangle = new PathGeometry();
		Triangle.Figures.Add(figure);
		Triangle.Freeze();
	}

	public enum InsertPositions
	{
		Left,
		Top,
		Right,
		Bottom,
	}

	protected override void OnRender(DrawingContext drawingContext, DragEventArgs args)
	{
		FrameworkElement target = args.GetTarget();
		ItemsControl? itemParent = target.FindParent<ItemsControl>();

		if (itemParent == null)
			return;

		////int index = Math.Min(DropInfo.InsertIndex, itemParent.Items.Count - 1);
		UIElement itemContainer = (UIElement)itemParent.ItemContainerGenerator.ContainerFromItem(target.DataContext);

		if (itemContainer == null)
			return;

		object? context = args.GetContext<object>();

		var itemRect = new Rect(itemContainer.TranslatePoint(default, this.AdornedElement), itemContainer.RenderSize);
		Point point1, point2;
		double rotation = 0;

		switch (this.InsertPosition)
		{
			case InsertPositions.Left:
				{
					point1 = new Point(itemRect.X, itemRect.Y);
					point2 = new Point(itemRect.X, itemRect.Bottom);
					rotation = 90;
					break;
				}

			case InsertPositions.Top:
				{
					point1 = new Point(itemRect.X, itemRect.Y);
					point2 = new Point(itemRect.Right, itemRect.Y);
					break;
				}

			case InsertPositions.Right:
				{
					itemRect.X += itemContainer.RenderSize.Width;
					point1 = new Point(itemRect.X, itemRect.Y);
					point2 = new Point(itemRect.X, itemRect.Bottom);
					rotation = 90;
					break;
				}

			case InsertPositions.Bottom:
				{
					itemRect.Y += itemContainer.RenderSize.Height;
					point1 = new Point(itemRect.X, itemRect.Y);
					point2 = new Point(itemRect.Right, itemRect.Y);
					break;
				}
		}

		drawingContext.DrawLine(Pen, point1, point2);
		DrawTriangle(drawingContext, point1, rotation);
		DrawTriangle(drawingContext, point2, 180 + rotation);
	}

	private static void DrawTriangle(DrawingContext drawingContext, Point origin, double rotation)
	{
		drawingContext.PushTransform(new TranslateTransform(origin.X, origin.Y));
		drawingContext.PushTransform(new RotateTransform(rotation));

		drawingContext.DrawGeometry(Pen.Brush, null, Triangle);

		drawingContext.Pop();
		drawingContext.Pop();
	}
}
