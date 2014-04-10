///
/// from http://www.codeproject.com/Articles/49853/Better-WPF-Circular-Progress-Bar
/// 

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows.Shapes;

namespace CmisSync
{
    /// <span class="code-SummaryComment"><summary></span>
    /// A circular type progress bar, that is simliar to popular web based
    /// progress bars
    /// <span class="code-SummaryComment"></summary></span>
    public partial class CircularProgressBar
    {
        #region Data
        private readonly DispatcherTimer animationTimer;
        #endregion

        #region Constructor
        public CircularProgressBar()
        {
            InitializeComponent();

            animationTimer = new DispatcherTimer(
                DispatcherPriority.ContextIdle, Dispatcher);
            animationTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
        }
        #endregion

        #region Private Methods
        private void Start()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            animationTimer.Tick += HandleAnimationTick;
            animationTimer.Start();
        }

        private void Stop()
        {
            animationTimer.Stop();
            Mouse.OverrideCursor = null;
            animationTimer.Tick -= HandleAnimationTick;
        }

        private void HandleAnimationTick(object sender, EventArgs e)
        {
            SpinnerRotate.Angle = (SpinnerRotate.Angle + 36) % 360;
        }
       private void HandleLoaded(object sender, RoutedEventArgs e)
        {
            const double offset = Math.PI;
            const double step = Math.PI * 2 / 8.0;

            SetPosition(C0, offset, 0.0, step);
            SetPosition(C1, offset, 1.0, step);
            SetPosition(C2, offset, 2.0, step);
            SetPosition(C3, offset, 3.0, step);
            SetPosition(C4, offset, 4.0, step);
            SetPosition(C5, offset, 5.0, step);
            SetPosition(C6, offset, 6.0, step);
            SetPosition(C7, offset, 7.0, step);
        }

        private void SetPosition(Ellipse ellipse, double offset,
            double posOffSet, double step)
        {
            double radius = 8;
            ellipse.SetValue(Canvas.LeftProperty, radius
                + Math.Sin(offset + posOffSet * step) * radius);
            ellipse.SetValue(Canvas.TopProperty, radius
                + Math.Cos(offset + posOffSet * step) * radius);
        }

        private void HandleUnloaded(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void HandleVisibleChanged(object sender,
            DependencyPropertyChangedEventArgs e)
        {
            bool isVisible = (bool)e.NewValue;

            if (isVisible)
                Start();
            else
                Stop();
        }
        #endregion
    }
}