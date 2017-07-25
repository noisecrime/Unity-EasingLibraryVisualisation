using UnityEngine;
using System;
using System.Runtime.CompilerServices;

namespace NoiseCrimeStudios.Core.AnimationCurves
{
	/// <summary>
	/// Runtime version of Unity Editor Class.
	/// Used mostly for correct tangent assignment to animation keys.
	/// May want to examine UnityEditor.CurveUtility for differences in tangent setting and updating the graph.
	/// </summary>
	public class RuntimeAnimationUtility
	{
		/// <summary>
		///   <para>Describes the type of modification that caused OnCurveWasModified to fire.</para>
		/// </summary>
		public enum CurveModifiedType
		{
			CurveDeleted,
			CurveModified,
			ClipModified
		}

		/// <summary>
		///   <para>Tangent constraints on Keyframe.</para>
		/// </summary>
		public enum TangentMode
		{
			/// <summary>
			///   <para>The tangent can be freely set by dragging the tangent handle.</para>
			/// </summary>
			Free,
			/// <summary>
			///   <para>The tangents are automatically set to make the curve go smoothly through the key.</para>
			/// </summary>
			Auto,
			/// <summary>
			///   <para>The tangent points towards the neighboring key.</para>
			/// </summary>
			Linear,
			/// <summary>
			///   <para>The curve retains a constant value between two keys.</para>
			/// </summary>
			Constant
		}

		// Removed OnCurveWasModified

		private static int kBrokenMask = 1;

		private static int kLeftTangentMask = 6;

		private static int kRightTangentMask = 24;

		private static float Internal_CalculateLinearTangent(AnimationCurve curve, int index, int toIndex)
		{
			float num = curve[index].time - curve[toIndex].time;
			if (Mathf.Approximately(num, 0f))
			{
				return 0f;
			}
			return (curve[index].value - curve[toIndex].value) / num;
		}

		private static TangentMode Internal_GetKeyLeftTangentMode(Keyframe key)
		{
			return (TangentMode)((key.tangentMode & kLeftTangentMask) >> 1);
		}

		private static TangentMode Internal_GetKeyRightTangentMode(Keyframe key)
		{
			return (TangentMode)((key.tangentMode & kRightTangentMask) >> 3);
		}

		private static bool Internal_GetKeyBroken(Keyframe key)
		{
			return (key.tangentMode & kBrokenMask) != 0;
		}

		private static void Internal_UpdateTangents(AnimationCurve curve, int index)
		{
			if (index < 0 || index >= curve.length)
			{
				throw new ArgumentException("Index out of bounds.");
			}
			Keyframe key = curve[index];
			if (Internal_GetKeyLeftTangentMode(key) == TangentMode.Linear && index >= 1)
			{
				key.inTangent = Internal_CalculateLinearTangent(curve, index, index - 1);
				curve.MoveKey(index, key);
			}
			if (Internal_GetKeyRightTangentMode(key) == TangentMode.Linear && index + 1 < curve.length)
			{
				key.outTangent = Internal_CalculateLinearTangent(curve, index, index + 1);
				curve.MoveKey(index, key);
			}
			if (Internal_GetKeyLeftTangentMode(key) == TangentMode.Auto || Internal_GetKeyRightTangentMode(key) == TangentMode.Auto)
			{
				curve.SmoothTangents(index, 0f);
			}
			if (Internal_GetKeyLeftTangentMode(key) == TangentMode.Free && Internal_GetKeyRightTangentMode(key) == TangentMode.Free && !Internal_GetKeyBroken(key))
			{
				key.outTangent = key.inTangent;
				curve.MoveKey(index, key);
			}
			if (Internal_GetKeyLeftTangentMode(key) == TangentMode.Constant)
			{
				key.inTangent = float.PositiveInfinity;
				curve.MoveKey(index, key);
			}
			if (Internal_GetKeyRightTangentMode(key) == TangentMode.Constant)
			{
				key.outTangent = float.PositiveInfinity;
				curve.MoveKey(index, key);
			}
		}

		/// <summary>
		///   <para>Change the specified keyframe broken tangent flag.</para>
		/// </summary>
		/// <param name="curve">The curve to modify.</param>
		/// <param name="index">Keyframe index.</param>
		/// <param name="broken">Broken flag.</param>
		public static void SetKeyBroken(AnimationCurve curve, int index, bool broken)
		{
			if (curve == null)
			{
				throw new ArgumentNullException("curve");
			}
			if (index < 0 || index >= curve.length)
			{
				return;
			}
			Keyframe key = curve[index];
			if (broken)
			{
				key.tangentMode |= kBrokenMask;
			}
			else
			{
				key.tangentMode &= ~kBrokenMask;
			}
			curve.MoveKey(index, key);
			Internal_UpdateTangents(curve, index);
		}

		public static void SetKeyLeftTangentMode(AnimationCurve curve, int index, TangentMode tangentMode)
		{
			if (curve == null)
			{
				throw new ArgumentNullException("curve");
			}
			if (index < 0 || index >= curve.length)
			{
				return;
			}
			Keyframe key = curve[index];
			if (tangentMode != TangentMode.Free)
			{
				key.tangentMode |= kBrokenMask;
			}
			key.tangentMode &= ~kLeftTangentMask;
			key.tangentMode |= (int)((int)tangentMode << 1);
			curve.MoveKey(index, key);
			Internal_UpdateTangents(curve, index);
		}

		public static void SetKeyRightTangentMode(AnimationCurve curve, int index, TangentMode tangentMode)
		{
			if (curve == null)
			{
				throw new ArgumentNullException("curve");
			}
			if (index < 0 || index >= curve.length)
			{
				return;
			}
			Keyframe key = curve[index];
			if (tangentMode != TangentMode.Free)
			{
				key.tangentMode |= kBrokenMask;
			}
			key.tangentMode &= ~kRightTangentMask;
			key.tangentMode |= (int)((int)tangentMode << 3);
			curve.MoveKey(index, key);
			Internal_UpdateTangents(curve, index);
		}
		
	
		/// <summary>
		///   <para>Set the additive reference pose from referenceClip at time for animation clip clip.</para>
		/// </summary>
		/// <param name="clip">The animation clip to be used.</param>
		/// <param name="referenceClip">The animation clip containing the reference pose.</param>
		/// <param name="time">Time that defines the reference pose in referenceClip.</param>
		// [WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void SetAdditiveReferencePose(AnimationClip clip, AnimationClip referenceClip, float time);

		// [WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern bool IsValidPolynomialCurve(AnimationCurve curve);

		// [WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void ConstrainToPolynomialCurve(AnimationCurve curve);

		// [WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool CurveSupportsProcedural(AnimationCurve curve);

		// [WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool GetGenerateMotionCurves(AnimationClip clip);

		// [WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void SetGenerateMotionCurves(AnimationClip clip, bool value);

		// [WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool HasGenericRootTransform(AnimationClip clip);

		// [WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool HasMotionFloatCurves(AnimationClip clip);

		// [WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool HasMotionCurves(AnimationClip clip);

		// [WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool HasRootCurves(AnimationClip clip);

		// [WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool AmbiguousBinding(string path, int classID, Transform root);
	}
}
