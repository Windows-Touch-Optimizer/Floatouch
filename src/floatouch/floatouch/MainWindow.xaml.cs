using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace floatouch
{

    public static class MyEnumExtensions
    {
        public static string ToDescriptionString(this MainWindow.VisualStateType val)
        {
            DescriptionAttribute[] attributes = (DescriptionAttribute[])val
               .GetType()
               .GetField(val.ToString())
               .GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : string.Empty;
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public enum VisualStateType
        {
            [Description("Normal")]
            Normal,
            [Description("FloatButtonMouseDown")]
            FloatButtonMouseDown,
            [Description("FloatButtonMouseUp")]
            FloatButtonMouseUp,
            [Description("EdgeButtonMouseDown_")]
            EdgeButtonMouseDown,
        }

        float buttonClickZoom = -10;

        float floatButtonLeft = 200;
        float floatButtonTop = 150;
        float floatButtonWidth = 50;
        float floatButtonHeight = 50;

        float edgeButtonBeginZoom = -20;

        float edgeButtonWidth = 50;
        float edgeButtonHeight = 50;

        float edgeRadius = 80;
        float edgeButtonCount = 5;
        Button[] edgeButtons;

        Duration defaultDuration = TimeSpan.FromMilliseconds(200);

        VisualStateType visualState = VisualStateType.Normal;
        int idx = -1;

        void SetVisualState(VisualStateType t, int _idx = -1)
        {
            visualState = t;
            idx = _idx;
            if (idx == -1)
            {
                VisualStateManager.GoToElementState(this, t.ToDescriptionString(), true);
            }
            else
            {
                VisualStateManager.GoToElementState(this, t.ToDescriptionString() + idx, true);
            }
        }

        DoubleAnimation CreateDoubleAnimation(DependencyObject target, String path, double to, Duration? duration = null)
        {
            if (duration == null) duration = defaultDuration;
            var animation = new DoubleAnimation();
            animation.Duration = defaultDuration;
            animation.To = to;
            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, new PropertyPath(path));
            return animation;
        }



        VisualState CreateVisualState(VisualStateType t, int idx = -1)
        {
            var visualState = new VisualState();
            var storyBoard = new Storyboard();
            var centerLeft = floatButtonLeft + floatButtonWidth / 2;
            var centerTop = floatButtonTop + floatButtonHeight / 2;

            switch (t)
            {
                case VisualStateType.FloatButtonMouseDown:
                    storyBoard.Children.Add(CreateDoubleAnimation(floatButton, "Width", floatButtonWidth + buttonClickZoom));
                    storyBoard.Children.Add(CreateDoubleAnimation(floatButton, "Height", floatButtonHeight + buttonClickZoom));
                    storyBoard.Children.Add(CreateDoubleAnimation(floatButton, "(Canvas.Left)", floatButtonLeft - buttonClickZoom / 2));
                    storyBoard.Children.Add(CreateDoubleAnimation(floatButton, "(Canvas.Top)", floatButtonTop - buttonClickZoom / 2));

                    break;
                default:
                    storyBoard.Children.Add(CreateDoubleAnimation(floatButton, "Width", floatButtonWidth));
                    storyBoard.Children.Add(CreateDoubleAnimation(floatButton, "Height", floatButtonHeight));
                    storyBoard.Children.Add(CreateDoubleAnimation(floatButton, "(Canvas.Left)", floatButtonLeft));
                    storyBoard.Children.Add(CreateDoubleAnimation(floatButton, "(Canvas.Top)", floatButtonTop));
                    break;
            }

            for (int i = 0; i < edgeButtonCount; i++)
            {
                double d = Math.PI / (edgeButtonCount - 1) * i;
                double sin = Math.Sin(d);
                double cos = Math.Cos(d);
                double radius = 0;
                double width = 0;
                double height = 0;

                switch (t)
                {
                    case VisualStateType.Normal:
                        radius = 0;
                        width = edgeButtonWidth + edgeButtonBeginZoom;
                        height = edgeButtonHeight + edgeButtonBeginZoom;
                        break;
                    case VisualStateType.EdgeButtonMouseDown:
                        if (i == idx)
                        {
                            width = edgeButtonWidth + buttonClickZoom;
                            height = edgeButtonHeight + buttonClickZoom;
                            radius = edgeRadius - (buttonClickZoom / 2);
                        }
                        else
                        {
                            width = edgeButtonWidth;
                            height = edgeButtonHeight;
                            radius = edgeRadius;
                        }
                        break;
                    default:
                        radius = edgeRadius;
                        width = edgeButtonWidth;
                        height = edgeButtonHeight;
                        break;
                }
                double left = centerLeft - radius * sin - width / 2;
                double top = centerTop - radius * cos - height / 2;

                storyBoard.Children.Add(CreateDoubleAnimation(edgeButtons[i], "Width", width));
                storyBoard.Children.Add(CreateDoubleAnimation(edgeButtons[i], "Height", height));
                storyBoard.Children.Add(CreateDoubleAnimation(edgeButtons[i], "(Canvas.Left)", left));
                storyBoard.Children.Add(CreateDoubleAnimation(edgeButtons[i], "(Canvas.Top)", top));
            }

            visualState.Storyboard = storyBoard;
            if (idx == -1)
            {
                visualState.Name = t.ToDescriptionString();
            }
            else
            {
                visualState.Name = t.ToDescriptionString() + idx;
            }

            return visualState;
        }

        public MainWindow()
        {
            InitializeComponent();

            edgeButtons = new Button[] { edgeButton1, edgeButton2, edgeButton3, edgeButton4, edgeButton5 };
            var state = VisualStateManager.GetVisualStateGroups(this);
            VisualStateGroup x = VisualStateManager.GetVisualStateGroups(this)[0] as VisualStateGroup;

            x.States.Add(CreateVisualState(VisualStateType.FloatButtonMouseDown));
            x.States.Add(CreateVisualState(VisualStateType.FloatButtonMouseUp));
            x.States.Add(CreateVisualState(VisualStateType.Normal));

            for (int i = 0; i < edgeButtonCount; i++)
            {
                int _idx = i;
                x.States.Add(CreateVisualState(VisualStateType.EdgeButtonMouseDown, i));
                edgeButtons[i].PreviewMouseDown += (object sender, MouseButtonEventArgs e) =>
                {
                    SetVisualState(VisualStateType.EdgeButtonMouseDown, _idx);
                };
                edgeButtons[i].PreviewMouseUp += (object sender, MouseButtonEventArgs e) =>
                {
                    SetVisualState(VisualStateType.Normal);
                };
            }

            VisualStateManager.GoToElementState(this, "Normal", true);

            //VisualStateManager.GetVisualStateGroups(this).Add(CreateVisualState(VisualStateType.Normal));

        }

        private void floatButton_PreviewTouchDown(object sender, TouchEventArgs e)
        {

        }

        private void floatButton_PreviewTouchUp(object sender, TouchEventArgs e)
        {
        }

        private void floatButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            toNormalAfterClick = false;
            if (visualState != VisualStateType.Normal)
            {
                toNormalAfterClick = true;
            }

            SetVisualState(VisualStateType.FloatButtonMouseDown);

        }

        bool toNormalAfterClick = false;
        private void floatButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (toNormalAfterClick)
            {
                SetVisualState(VisualStateType.Normal);

            }
            else
            {
                SetVisualState(VisualStateType.FloatButtonMouseUp);

            }

        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // this.DragMove();
        }
    }
}
