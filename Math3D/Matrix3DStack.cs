// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Math3D;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

/// <summary>A stack of <see cref="Matrix3D"/> objects.</summary>
public class Matrix3DStack : IEnumerable<Matrix3D>, ICollection, IReadOnlyCollection<Matrix3D>
{
	private readonly Stack<Matrix3D> storage = new();

	/// <summary>Gets the number of elements contained in the stack.</summary>
	public int Count => this.storage.Count;

	/// <inheritdoc/>
	bool ICollection.IsSynchronized => ((ICollection)this.storage).IsSynchronized;

	/// <inheritdoc/>
	object ICollection.SyncRoot => ((ICollection)this.storage).SyncRoot;

	/// <summary>
	/// Returns the object at the top of the stack without removing it.
	/// </summary>
	/// <returns>The object at the top of the stack.</returns>
	public Matrix3D Peek() => this.storage.Peek();

	/// <summary>
	/// Inserts an object at the top of the stack.
	/// </summary>
	/// <param name="item">The object to push onto the stack.</param>
	public void Push(Matrix3D item) => this.storage.Push(item);

	/// <summary>
	/// Appends a matrix to the top matrix in the stack.
	/// </summary>
	/// <param name="item">The matrix to append.</param>
	public void Append(Matrix3D item)
	{
		if (this.storage.Count > 0)
		{
			Matrix3D top = this.storage.Pop();
			top.Append(item);
			this.storage.Push(top);
		}
		else
		{
			this.storage.Push(item);
		}
	}

	/// <summary>
	/// Prepends a matrix to the top matrix in the stack.
	/// </summary>
	/// <param name="item">The matrix to prepend.</param>
	public void Prepend(Matrix3D item)
	{
		if (this.storage.Count > 0)
		{
			Matrix3D top = this.storage.Pop();
			top.Prepend(item);
			this.storage.Push(top);
		}
		else
		{
			this.storage.Push(item);
		}
	}

	/// <summary>
	/// Removes and returns the object at the top of the stack.
	/// </summary>
	/// <returns>The object removed from the top of the stack.</returns>
	public Matrix3D Pop() => this.storage.Pop();

	/// <summary>
	/// Removes all objects from the stack.
	/// </summary>
	public void Clear() => this.storage.Clear();

	/// <summary>
	/// Determines whether an element is in the stack.
	/// </summary>
	/// <param name="item">The object to locate in the stack.</param>
	/// <returns>true if item is found in the stack; otherwise, false.</returns>
	public bool Contains(Matrix3D item) => this.storage.Contains(item);

	/// <inheritdoc/>
	void ICollection.CopyTo(Array array, int index)
	{
		((ICollection)this.storage).CopyTo(array, index);
	}

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<Matrix3D>)this).GetEnumerator();
	}

	/// <inheritdoc/>
	IEnumerator<Matrix3D> IEnumerable<Matrix3D>.GetEnumerator()
	{
		return this.storage.GetEnumerator();
	}

	/// <summary>
	/// Attempts to return the object at the top of the stack without removing it.
	/// </summary>
	/// <param name="result">The object at the top of the stack, if found.</param>
	/// <returns>true if an object was found; otherwise, false.</returns>
	public bool TryPeek(out Matrix3D result) => this.storage.TryPeek(out result);

	/// <summary>
	/// Attempts to remove and return the object at the top of the stack.
	/// </summary>
	/// <param name="result">The object removed from the top of the stack, if found.</param>
	/// <returns>true if an object was found and removed; otherwise, false.</returns>
	public bool TryPop(out Matrix3D result) => this.storage.TryPop(out result);
}
