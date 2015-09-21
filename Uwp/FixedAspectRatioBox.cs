namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Windows.Foundation;
	using Windows.UI.Xaml;
	using Windows.UI.Xaml.Controls;

	/// <summary>
	/// Attempts to force a fixed aspect ratio onto its contents by altering its own size in the measure/arrange
	/// layout passes. This will not always succeed (if everything is set to stretch) but we do what we can.
	/// </summary>
	/// <remarks>
	/// If the control is set to stretch in exactly one direction, the size of the stretching direction is determined
	/// by the aspect ratio calculation, with the other remaining fixed at content size and no further logic applying.
	/// 
	/// Otherwise, the control prefers to expand in order to achieve the desired aspect ratio to fit the contents.
	/// However, it will never go beyond the bounds suggested by the parent control and will always shrink if required.
	/// If ShrinkToFit is true, the control will not even try to expand and will shrink to the desired aspect ratio.
	/// 
	/// If the contents are not configured to use all of the area, the desired aspect ratio might not be achieved.
	/// </remarks>
	public sealed class FixedAspectRatioBox : ContentControl
	{
		#region double AspectRatio (depencency property)
		/// <summary>
		/// The aspect ratio to use when performing layout.
		/// </summary>
		/// <value>Defaults to 1. Must be greater than 0.</value>
		public double AspectRatio
		{
			get { return (double)GetValue(AspectRatioProperty); }
			set
			{
				if (double.IsNaN(value) || double.IsInfinity(value))
					throw new ArgumentOutOfRangeException(nameof(value), "Value must be a number.");

				if (value <= 0)
					throw new ArgumentOutOfRangeException(nameof(value), "Value must be greater than 0.");

				SetValue(AspectRatioProperty, value);
			}
		}

		public static readonly DependencyProperty AspectRatioProperty =
			DependencyProperty.Register("AspectRatio", typeof(double),
				typeof(FixedAspectRatioBox), new PropertyMetadata((double)1, OnAspectRatioPropertyChanged));


		private static void OnAspectRatioPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var instance = (FixedAspectRatioBox)d;

			instance.InvalidateMeasure();
		}
		#endregion

		#region bool ShrinkToFit (depencency property)
		/// <summary>
		/// If true, always shrinks the area instead of expanding, to achieve the target aspect ratio.
		/// </summary>
		public bool ShrinkToFit
		{
			get { return (bool)GetValue(ShrinkToFitProperty); }
			set { SetValue(ShrinkToFitProperty, value); }
		}

		public static readonly DependencyProperty ShrinkToFitProperty =
			DependencyProperty.Register("ShrinkToFit", typeof(bool),
				typeof(FixedAspectRatioBox), new PropertyMetadata(default(bool), OnShrinkToFitPropertyChanged));

		private static void OnShrinkToFitPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var instance = (FixedAspectRatioBox)d;

			instance.InvalidateMeasure();
		}
		#endregion

		public FixedAspectRatioBox()
		{
			DefaultStyleKey = typeof(FixedAspectRatioBox);
		}

		private Size MeasureWithFixedAspectRatio(Size contentsSize, Size maximumSize)
		{
			bool unlimitedHeight = double.IsNaN(contentsSize.Height)
			                       || double.IsInfinity(contentsSize.Height)
			                       || contentsSize.Height == 0;
			bool unlimitedWidth = double.IsNaN(contentsSize.Width)
			                      || double.IsInfinity(contentsSize.Width)
			                      || contentsSize.Width == 0;

			// Can't do much if we are stretching all over the place.
			if (unlimitedHeight && unlimitedWidth)
				return contentsSize;

			// We limit the stretch direction, if there is one.
			if (unlimitedHeight)
				return new Size(contentsSize.Width, contentsSize.Width / AspectRatio);

			if (unlimitedWidth)
				return new Size(contentsSize.Height * AspectRatio, contentsSize.Height);

			// If no stretch, we fall back to basic math.
			double a = contentsSize.Width / contentsSize.Height;

			var size = contentsSize;

			// If we are allowed to expand, let's try to expand.
			if (!ShrinkToFit)
			{
				if (a > AspectRatio)
				{
					// Original is too short.
					size = new Size(contentsSize.Width, contentsSize.Width / AspectRatio);
				}
				else
				{
					// Original is too narrow or possibly exactly correct.
					size = new Size(contentsSize.Height * AspectRatio, contentsSize.Height);
				}
			}

			// We may have gone over the boundaries provided by the parent!
			// Constrain to original size and then do a final shrink-pass that fixes up any lost area.
			size = new Size(Math.Min(size.Width, maximumSize.Width), Math.Min(size.Height, maximumSize.Height));

			if (a > AspectRatio)
			{
				// Too wide.
				return new Size(size.Height * AspectRatio, size.Height);
			}
			else
			{
				// Too tall or possibly exactly correct.
				return new Size(size.Width, size.Width / AspectRatio);
			}
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			// It appears that messing in the Arrange pass is not required - as long as we measure the
			// right aspect ratio, it seems to get used. Might run into some special cases in the future, though?
			// We will cross those bridges when we crash into them. Meanwhile, this seems to work fine.

			// Let the base class measure the contents and see what we get.
			var contentsSize = base.MeasureOverride(availableSize);

			// Adjust request of children to match the aspect ratio.
			var size = MeasureWithFixedAspectRatio(contentsSize, availableSize);

			return size;
		}
	}
}