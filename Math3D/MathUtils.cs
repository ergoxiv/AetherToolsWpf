// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Math3D;

using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using XivToolsWpf.Math3D.Extensions;

public static class MathUtils
{
	public static readonly Matrix3D IdentityMatrix = Matrix3D.Identity;
	public static readonly Matrix3D ZeroMatrix = new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
	public static readonly Matrix4x4 ZeroMatrix4x4 = new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
	public static readonly Vector3D XAxis = new(1, 0, 0);
	public static readonly Vector3D YAxis = new(0, 1, 0);
	public static readonly Vector3D ZAxis = new(0, 0, 1);

	private const double DEG_TO_RAD = Math.PI / 180.0;
	private const float DEG_TO_RADF = (float)(Math.PI / 180.0);
	private const double RAD_TO_DEG = 180.0 / Math.PI;
	private const float RAD_TO_DEGF = (float)(180.0 / Math.PI);

	private const float LOOK_DIR_EPSILON = 1e-10f;
	private const float PARALLEL_VEC_DOT_THRESHOLD = 0.9999f;
	private const float MIN_FOV = 0.001f;
	private const float MAX_FOV = 179.999f;
	private const float MIN_ORTHO_SIZE = 1e-5f;

	/// <summary>Gets the aspect ratio of the specified size.</summary>
	/// <param name="size">The size to calculate the aspect ratio for.</param>
	/// <returns>The aspect ratio of the size.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double GetAspectRatio(Size size) => size.Width / size.Height;

	/// <summary>Converts degrees to radians.</summary>
	/// <param name="degrees">The angle in degrees.</param>
	/// <returns>The angle in radians.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double DegreesToRadians(double degrees) => degrees * DEG_TO_RAD;

	/// <summary>Converts degrees to radians.</summary>
	/// <param name="degrees">The angle in degrees.</param>
	/// <returns>The angle in radians.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float DegreesToRadians(float degrees) => degrees * DEG_TO_RADF;

	/// <summary>Converts radians to degrees.</summary>
	/// <param name="radians">The angle in radians.</param>
	/// <returns>The angle in degrees.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double RadiansToDegrees(double radians) => radians * RAD_TO_DEG;

	/// <summary>Converts radians to degrees.</summary>
	/// <param name="radians">The angle in radians.</param>
	/// <returns>The angle in degrees.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float RadiansToDegrees(float radians) => radians * RAD_TO_DEGF;

	/// <summary>
	/// Computes the effective view matrix for the given camera.
	/// </summary>
	/// <param name="camera">The camera to compute the view matrix for.</param>
	/// <returns>The view matrix for the camera.</returns>
	/// <exception cref="ArgumentNullException">Thrown when the camera is null.</exception>
	/// <exception cref="ArgumentException">Thrown when the camera type is unsupported.</exception>
	public static Matrix3D GetViewMatrix(Camera camera)
	{
		ArgumentNullException.ThrowIfNull(camera);

		if (camera is MatrixCamera matrixCamera)
			return matrixCamera.ViewMatrix;

		if (camera is ProjectionCamera projectionCamera)
		{
			if (TryGetViewMatrixInternal(projectionCamera, applyTransform: false, out Matrix4x4 view))
				return view.ToMatrix3D();

			return IdentityMatrix; // Fallback
		}

		throw new ArgumentException($"Unsupported camera type '{camera.GetType().FullName}'.", nameof(camera));
	}

	/// <summary>
	/// Computes the effective projection matrix for the given camera.
	/// </summary>
	/// <param name="camera">The camera to compute the projection matrix for.</param>
	/// <param name="aspectRatio">The aspect ratio of the viewport.</param>
	/// <returns>The projection matrix for the camera.</returns>
	/// <exception cref="ArgumentNullException">Thrown when the camera is null.</exception>
	/// <exception cref="ArgumentException">Thrown when the camera type is unsupported.</exception>
	public static Matrix3D GetProjectionMatrix(Camera camera, double aspectRatio)
	{
		ArgumentNullException.ThrowIfNull(camera);

		if (camera is MatrixCamera matrixCamera)
			return matrixCamera.ProjectionMatrix;

		if (TryGetProjectionMatrixInternal(camera, (float)aspectRatio, out Matrix4x4 proj))
			return proj.ToMatrix3D();

		throw new ArgumentException($"Unsupported camera type '{camera.GetType().FullName}'.", nameof(camera));
	}

	/// <summary>
	/// Finds the nearest <see cref="Viewport3DVisual"/> ancestor of the given visual and
	/// calculates the transform from the visual to the viewport's coordinate space.
	/// </summary>
	/// <remarks>
	/// The viewport's coordinate space is considered to be the world space.
	/// </remarks>
	/// <param name="visual">
	/// The visual to start searching from.
	/// </param>
	/// <param name="modelToWorld">
	/// The output relative visual to viewport transform.
	/// Returns the accumulated transform from the visual to the viewport if a viewport is found;
	/// Otherwise, the method returns the identity matrix.
	/// </param>
	/// <returns>
	/// The nearest <see cref="Viewport3DVisual"/> ancestor, or null if none is found.
	/// </returns>
	public static Viewport3DVisual? FindViewport(DependencyObject visual, out Matrix4x4 modelToWorld)
	{
		Matrix4x4 accumulatedTransform = Matrix4x4.Identity;
		DependencyObject current = visual;

		while (current != null)
		{
			if (current is ModelVisual3D modelVisual)
			{
				var transform = modelVisual.Transform;
				if (transform != null)
				{
					var matrix = transform.Value.ToMatrix4x4();
					if (!matrix.IsIdentity)
					{
						accumulatedTransform *= matrix;
					}
				}
			}
			else if (current is Viewport3DVisual viewport)
			{
				modelToWorld = accumulatedTransform;
				return viewport;
			}

			current = VisualTreeHelper.GetParent(current);
		}

		modelToWorld = Matrix4x4.Identity;
		return null;
	}

	/// <summary>
	/// Finds the nearest <see cref="Viewport3DVisual"/> ancestor of the given visual and
	/// calculates the transform from the visual to the viewport's coordinate space.
	/// </summary>
	/// <remarks>
	/// This variant of the method returns a direct child of the found viewport, which allows
	/// the caller to cache the viewport and calculate the model-to-world transform later.
	/// If you don't need to cache, use <see cref="FindViewport(DependencyObject, out Matrix4x4)"/> instead.
	/// </remarks>
	/// <param name="visual">
	/// The visual to start searching from.
	/// </param>
	/// <param name="root3D">
	/// The direct child of the found <see cref="Viewport3DVisual"/>, or null if none is found.
	/// </param>
	/// <returns>
	/// The nearest <see cref="Viewport3DVisual"/> ancestor, or null if none is found.
	/// </returns>
	public static Viewport3DVisual? FindViewport(Visual3D visual, out Visual3D? root3D)
	{
		root3D = null;
		DependencyObject current = visual;
		Visual3D? lastV3D = visual;

		while (current != null)
		{
			if (current is Viewport3DVisual viewport)
			{
				root3D = lastV3D; // This is the child of the Viewport
				return viewport;
			}

			if (current is Visual3D v3d)
				lastV3D = v3d;

			current = VisualTreeHelper.GetParent(current);
		}
		return null;
	}

	/// <summary>
	/// Attempts to calculate the full transformation matrix
	/// from a 3D Visual object's local space to the 2D Screen/Viewport space.
	/// </summary>
	/// <remarks>
	/// This is a convenience method that encapsulates: 
	/// Model-to-World → World-to-Camera (View) → Camera-to-NDC (Projection) → NDC-to-Screen.
	/// </remarks>
	/// <param name="visual">The source 3D visual.</param>
	/// <param name="screenMatrix">The resulting transformation matrix.</param>
	/// <returns>
	/// True if the transformation matrix was successfully calculated; otherwise, false.
	/// </returns>
	public static bool TryTransformVisualToViewport(DependencyObject visual, out Matrix3D screenMatrix)
	{
		var success = TryTransformVisualToViewport(visual, out Matrix4x4 result);
		screenMatrix = success ? result.ToMatrix3D() : ZeroMatrix;
		return success;
	}

	/// <summary>
	/// Attempts to calculate the full transformation matrix
	/// from a 3D Visual object's local space to the 2D Screen/Viewport space.
	/// </summary>
	/// <remarks>
	/// This is a convenience method that encapsulates: 
	/// Model-to-World → World-to-Camera (View) → Camera-to-NDC (Projection) → NDC-to-Screen.
	/// </remarks>
	/// <param name="visual">The source 3D visual.</param>
	/// <param name="screenMatrix">The resulting transformation matrix.</param>
	/// <param name="viewport">
	/// The ancestor viewport found during the search, if any.
	/// </param>
	/// <returns>
	/// True if the transformation matrix was successfully calculated; otherwise, false.
	/// </returns>
	public static bool TryTransformVisualToViewport(DependencyObject visual, out Matrix3D screenMatrix, out Viewport3DVisual? viewport)
	{
		var success = TryTransformVisualToViewport(visual, out Matrix4x4 result, out viewport);
		screenMatrix = success ? result.ToMatrix3D() : ZeroMatrix;
		return success;
	}

	/// <summary>
	/// Attempts to calculate the full transformation matrix
	/// from a 3D Visual object's local space to the 2D Screen/Viewport space.
	/// </summary>
	/// <remarks>
	/// This is a convenience method that encapsulates: 
	/// Model-to-World → World-to-Camera (View) → Camera-to-NDC (Projection) → NDC-to-Screen.
	/// </remarks>
	/// <param name="visual">The source 3D visual.</param>
	/// <param name="screenMatrix">The resulting transformation matrix.</param>
	/// <returns>
	/// True if the transformation matrix was successfully calculated; otherwise, false.
	/// </returns>
	public static bool TryTransformVisualToViewport(DependencyObject visual, out Matrix4x4 screenMatrix)
	{
		var viewport = FindViewport(visual, out Matrix4x4 modelToWorld);
		if (viewport == null)
		{
			screenMatrix = default;
			return false;
		}

		return TryTransformVisualToViewport(viewport, modelToWorld, out screenMatrix);
	}

	/// <summary>
	/// Attempts to calculate the full transformation matrix
	/// from a 3D Visual object's local space to the 2D Screen/Viewport space.
	/// </summary>
	/// <remarks>
	/// This is a convenience method that encapsulates: 
	/// Model-to-World → World-to-Camera (View) → Camera-to-NDC (Projection) → NDC-to-Screen.
	/// </remarks>
	/// <param name="visual">The source 3D visual.</param>
	/// <param name="screenMatrix">The resulting transformation matrix.</param>
	/// <param name="viewport">
	/// The ancestor viewport found during the search, if any.
	/// </param>
	/// <returns>
	/// True if the transformation matrix was successfully calculated; otherwise, false.
	/// </returns>
	public static bool TryTransformVisualToViewport(DependencyObject visual, out Matrix4x4 screenMatrix, out Viewport3DVisual? viewport)
	{
		viewport = FindViewport(visual, out Matrix4x4 modelToWorld);
		if (viewport == null)
		{
			screenMatrix = default;
			return false;
		}

		return TryTransformVisualToViewport(viewport, modelToWorld, out screenMatrix);
	}

	/// <summary>
	/// Calculates the full transformation matrix from the given model-to-world matrix
	/// to the 2D Screen/Viewport space of the specified viewport.
	/// </summary>
	/// <param name="viewport">
	/// The viewport to calculate the matrix for.
	/// </param>
	/// <param name="modelToWorld">
	/// The model-to-world transformation matrix.
	/// </param>
	/// <param name="modelToScreen">
	/// The output model-to-screen transformation matrix, including view, projection, and viewport transforms.
	/// </param>
	/// <returns>
	/// True if the transformation matrix was successfully calculated; otherwise, false.
	/// </returns>
	public static bool TryTransformVisualToViewport(Viewport3DVisual viewport, Matrix4x4 modelToWorld, out Matrix4x4 modelToScreen)
	{
		if (!TryGetViewProjectionViewportMatrix(viewport, out Matrix4x4 viewProj))
		{
			modelToScreen = default;
			return false;
		}

		modelToScreen = modelToWorld * viewProj;
		return true;
	}

	/// <summary>
	/// Calculates the combined View-Projection-Screen matrix for the given viewport.
	/// </summary>
	/// <param name="viewport">
	/// The viewport to calculate the matrix for.
	/// </param>
	/// <param name="result">
	/// The resulting combined matrix.
	/// </param>
	/// <returns>
	/// True if the transformation matrix was successfully calculated; otherwise, false.
	/// </returns>
	public static bool TryGetViewProjectionViewportMatrix(Viewport3DVisual viewport, out Matrix4x4 result)
	{
		if (viewport == null)
		{
			result = default;
			return false;
		}

		var camera = viewport.Camera;
		Rect rect = viewport.Viewport;

		// Combine view and camera projection matrices
		if (!TryGetViewProjectionMatrix(camera, rect.Size, out Matrix4x4 viewProj))
		{
			result = Matrix4x4.Identity;
			return false;
		}

		// Map normalized device coordinates (-1 to 1) to screen space (0 to Width/Height)
		Matrix4x4 viewportMat = ConvertNDCToScreenMatrix(rect);

		// Combine transforms: World -> View projection -> Screen
		result = viewProj * viewportMat;
		return true;
	}

	/// <summary>
	/// Transforms the axis-aligned bounding box 'bounds' by 'transform'.
	/// </summary>
	/// <param name="bounds">The AABB to transform.</param>
	/// <param name="transform">The transform.</param>
	/// <returns>Transformed AABB.</returns>
	public static Rect3D TransformBounds(Rect3D bounds, Matrix3D transform)
	{
		double x1 = bounds.X;
		double y1 = bounds.Y;
		double z1 = bounds.Z;
		double x2 = bounds.X + bounds.SizeX;
		double y2 = bounds.Y + bounds.SizeY;
		double z2 = bounds.Z + bounds.SizeZ;

		Point3D[] points =
		[
			new(x1, y1, z1), new(x1, y1, z2), new(x1, y2, z1), new(x1, y2, z2),
			new(x2, y1, z1), new(x2, y1, z2), new(x2, y2, z1), new(x2, y2, z2),
		];

		transform.Transform(points);

		// Re-use the 1 and 2 variables to stand for smallest and largest
		Point3D p = points[0];
		x1 = x2 = p.X;
		y1 = y2 = p.Y;
		z1 = z2 = p.Z;

		for (int i = 1; i < points.Length; i++)
		{
			p = points[i];

			x1 = Math.Min(x1, p.X);
			y1 = Math.Min(y1, p.Y);
			z1 = Math.Min(z1, p.Z);
			x2 = Math.Max(x2, p.X);
			y2 = Math.Max(y2, p.Y);
			z2 = Math.Max(z2, p.Z);
		}

		return new Rect3D(x1, y1, z1, x2 - x1, y2 - y1, z2 - z1);
	}

	/// <summary>
	/// Normalizes v if |v| > 0. This normalization is slightly different from Vector3D.Normalize().
	/// Here we just divide by the length but Vector3D.Normalize tries to avoid overflow when
	/// finding the length.
	/// </summary>
	/// <param name="v">The vector to normalize.</param>
	/// <returns>'true' if v was normalized.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryNormalize(ref Vector3D v)
	{
		double length = v.Length;

		if (length != 0)
		{
			v /= length;
			return true;
		}

		return false;
	}

	/// <summary>Computes the center of 'box'.</summary>
	/// <param name="box">The Rect3D we want the center of.</param>
	/// <returns>The center point.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Point3D GetCenter(Rect3D box)
	{
		return new Point3D(box.X + (box.SizeX / 2), box.Y + (box.SizeY / 2), box.Z + (box.SizeZ / 2));
	}

	/// <summary>
	/// Finds the nearest point on a ray to a given point.
	/// </summary>
	/// <param name="rayOrigin">The origin of the ray.</param>
	/// <param name="rayDirection">The direction of the ray.</param>
	/// <param name="pnt">The point to find the nearest point to.</param>
	/// <returns>The nearest point on the ray to the given point.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Point3D NearestPointOnRay(Point3D rayOrigin, Vector3D rayDirection, Point3D pnt)
	{
		rayDirection.Normalize();
		Vector3D v = pnt - rayOrigin;
		double d = Vector3D.DotProduct(v, rayDirection);
		return rayOrigin + (rayDirection * d);
	}

	/// <summary>
	/// Converts normalized device coordinates (-1 to 1) to screen space (0 to Width/Height).
	/// </summary>
	/// <param name="viewport">
	/// The viewport rectangle.
	/// </param>
	/// <returns>
	/// A transformation matrix of the screen space.
	/// </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Matrix4x4 ConvertNDCToScreenMatrix(Rect viewport)
	{
		float scaleX = (float)(viewport.Width / 2);
		float scaleY = (float)(viewport.Height / 2);
		float offsetX = (float)(viewport.X + scaleX);
		float offsetY = (float)(viewport.Y + scaleY);
		return new Matrix4x4(scaleX, 0, 0, 0, 0, -scaleY, 0, 0, 0, 0, 1, 0, offsetX, offsetY, 0, 1);
	}

	/// <summary>
	/// Converts normalized device coordinates (-1 to 1) to screen space (0 to Width/Height).
	/// </summary>
	/// <param name="viewport">
	/// The viewport rectangle.
	/// </param>
	/// <returns>
	/// A transformation matrix of the screen space.
	/// </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Matrix3D ConvertNDCToScreenMatrix3D(Rect viewport)
	{
		double scaleX = viewport.Width / 2;
		double scaleY = viewport.Height / 2;
		double offsetX = viewport.X + scaleX;
		double offsetY = viewport.Y + scaleY;
		return new Matrix3D(scaleX, 0, 0, 0, 0, -scaleY, 0, 0, 0, 0, 1, 0, offsetX, offsetY, 0, 1);
	}

	private static bool TryGetViewProjectionMatrix(Camera camera, Size viewportSize, out Matrix4x4 result)
	{
		if (viewportSize.Width <= 0 || viewportSize.Height <= 0)
		{
			result = Matrix4x4.Identity;
			return false;
		}

		// Apply camera transform to compute view matrix
		if (camera is not ProjectionCamera projCam ||
			!TryGetViewMatrixInternal(projCam, applyTransform: true, out Matrix4x4 view))
		{
			result = Matrix4x4.Identity;
			return false;
		}

		// Calculate projection matrix
		float aspectRatio = (float)(viewportSize.Width / viewportSize.Height);
		if (!TryGetProjectionMatrixInternal(camera, aspectRatio, out Matrix4x4 proj))
		{
			result = default;
			return false;
		}

		result = view * proj;
		return true;
	}

	private static bool TryGetViewMatrixInternal(ProjectionCamera camera, bool applyTransform, out Matrix4x4 viewMatrix)
	{
		Vector3 pos = camera.Position.FromMedia3DPoint();
		Vector3 lookDir = camera.LookDirection.FromMedia3DVector();
		Vector3 upDir = camera.UpDirection.FromMedia3DVector();

		if (applyTransform && camera.Transform is Transform3D transform && !transform.Value.IsIdentity)
		{
			Matrix4x4 camM = transform.Value.ToMatrix4x4();
			pos = Vector3.Transform(pos, camM);
			lookDir = Vector3.TransformNormal(lookDir, camM);
			upDir = Vector3.TransformNormal(upDir, camM);
		}

		float lookLenSq = lookDir.LengthSquared();
		if (lookLenSq < LOOK_DIR_EPSILON)
		{
			viewMatrix = default;
			return false;
		}

		Vector3 nLook = lookDir / MathF.Sqrt(lookLenSq);
		Vector3 nUp = Vector3.Normalize(upDir);
		float dot = MathF.Abs(Vector3.Dot(nLook, nUp));

		// Parallel vector correction
		if (dot > PARALLEL_VEC_DOT_THRESHOLD)
		{
			float absX = MathF.Abs(nLook.X);
			float absY = MathF.Abs(nLook.Y);
			float absZ = MathF.Abs(nLook.Z);

			if (absX <= absY && absX <= absZ) nUp = Vector3.UnitX;
			else if (absY <= absX && absY <= absZ) nUp = Vector3.UnitY;
			else nUp = Vector3.UnitZ;
		}

		// D3DXMatrixLookAtRH equivalent (WPF uses RH)
		// WPF LookDirection is Vector, CreateLookAt expects Target Point (Pos + Look)
		viewMatrix = Matrix4x4.CreateLookAt(pos, pos + nLook, nUp);
		return true;
	}

	private static bool TryGetProjectionMatrixInternal(Camera camera, float aspectRatio, out Matrix4x4 projMatrix)
	{
		projMatrix = camera switch
		{
			PerspectiveCamera persCam => GetProjectionMatrixInternal(persCam, aspectRatio),
			OrthographicCamera ortho => GetProjectionMatrixInternal(ortho, aspectRatio),
			_ => default
		};

		return camera is PerspectiveCamera or OrthographicCamera;
	}

	private static Matrix4x4 GetProjectionMatrixInternal(OrthographicCamera camera, double aspectRatio)
	{
		Debug.Assert(camera != null, "Caller needs to ensure camera is non-null.");

		// This math is identical to what you find documented for
		// D3DXMatrixOrthoRH with the exception that in WPF only
		// the camera's width is specified.  Height is calculated
		// from width and the aspect ratio.
		float w = MathF.Max((float)camera.Width, MIN_ORTHO_SIZE);
		float h = (float)(w / aspectRatio);
		float zn = (float)camera.NearPlaneDistance;
		float zf = (float)camera.FarPlaneDistance;
		float m33 = 1.0f / (zn - zf);
		float m43 = zn * m33;

		return new Matrix4x4(2.0f / w, 0, 0, 0, 0, 2.0f / h, 0, 0, 0, 0, m33, 0, 0, 0, m43, 1);
	}

	private static Matrix4x4 GetProjectionMatrixInternal(PerspectiveCamera camera, double aspectRatio)
	{
		Debug.Assert(camera != null, "Caller needs to ensure camera is non-null.");

		// This math is identical to what you find documented for
		// D3DXMatrixPerspectiveFovRH with the exception that in
		// WPF the camera's horizontal rather the vertical
		// field-of-view is specified.
		float hFoV = Math.Clamp((float)camera.FieldOfView, MIN_FOV, MAX_FOV);
		float hFovRad = DegreesToRadians(hFoV);
		float zn = (float)camera.NearPlaneDistance;
		float zf = (float)camera.FarPlaneDistance;

		float xScale = 1.0f / MathF.Tan(hFovRad * 0.5f);
		float yScale = (float)aspectRatio * xScale;
		float m33 = (zf == float.PositiveInfinity) ? -1.0f : (zf / (zn - zf));
		float m43 = zn * m33;

		return new Matrix4x4(xScale, 0, 0, 0, 0, yScale, 0, 0, 0, 0, m33, -1, 0, 0, m43, 0);
	}
}