using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
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
    static class Keyboard
    {
        public static void SimulateKeyStroke(char? key, bool ctrl = false, bool alt = false, bool shift = false, bool win = false, bool tab = false)
        {
            List<ushort> keys = new List<ushort>();

            if (ctrl)
                keys.Add(VK_CONTROL);

            if (alt)
                keys.Add(VK_MENU);

            if (shift)
                keys.Add(VK_SHIFT);

            if (win)
                keys.Add(VK_LWIN);

            if (tab)
                keys.Add(VK_TAB);

            if (key != null)
            {
                keys.Add((key.Value));
            }

            INPUT input = new INPUT();
            input.type = INPUT_KEYBOARD;
            int inputSize = Marshal.SizeOf(input);

            for (int i = 0; i < keys.Count; ++i)
            {
                input.mkhi.ki.wVk = keys[i];

                bool isKeyDown = (GetAsyncKeyState(keys[i]) & 0x10000) != 0;

                if (!isKeyDown)
                    SendInput(1, ref input, inputSize);
            }

            input.mkhi.ki.dwFlags = KEYEVENTF_KEYUP;
            for (int i = keys.Count - 1; i >= 0; --i)
            {
                input.mkhi.ki.wVk = keys[i];
                SendInput(1, ref input, inputSize);
            }
        }

        [DllImport("user32.dll")]
        static extern uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(ushort vKey);

        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        struct HARDWAREINPUT
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct MOUSEKEYBDHARDWAREINPUT
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;

            [FieldOffset(0)]
            public KEYBDINPUT ki;

            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        struct INPUT
        {
            public int type;
            public MOUSEKEYBDHARDWAREINPUT mkhi;
        }

        const int INPUT_KEYBOARD = 1;
        const uint KEYEVENTF_KEYUP = 0x0002;

        const ushort VK_SHIFT = 0x10;
        const ushort VK_CONTROL = 0x11;
        const ushort VK_MENU = 0x12;
        const ushort VK_LWIN = 0x5B;
        const ushort VK_TAB = 0x09;
        public const char VK_LEFT = (char)0x25;
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public enum VisualStateType
        {
            [Description("Normal_")]
            Normal,
            [Description("FloatButtonMouseDown")]
            FloatButtonMouseDown,
            [Description("FloatButtonMouseUp")]
            FloatButtonMouseUp,
            [Description("FloatButtonTouchUp")]
            FloatButtonTouchUp,
            [Description("EdgeButtonMouseDown_")]
            EdgeButtonMouseDown,
            [Description("EdgeButtonTouchUp_")]
            EdgeButtonTouchUp,
        }

        float buttonClickZoom = -20;

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

        int defaultDuration = 200;

        int halfDuration = 100;

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

        DoubleAnimation CreateDoubleAnimation(DependencyObject target, String path, double to, int beginTime = 0, int duration = 200)
        {
            var animation = new DoubleAnimation();
            animation.Duration = TimeSpan.FromMilliseconds(duration);
            animation.BeginTime = TimeSpan.FromMilliseconds(beginTime);
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
                case VisualStateType.FloatButtonTouchUp:
                    storyBoard.Children.Add(CreateDoubleAnimation(floatButton, "Width", floatButtonWidth + buttonClickZoom, 0, halfDuration));
                    storyBoard.Children.Add(CreateDoubleAnimation(floatButton, "Height", floatButtonHeight + buttonClickZoom, 0, halfDuration));
                    storyBoard.Children.Add(CreateDoubleAnimation(floatButton, "(Canvas.Left)", floatButtonLeft - buttonClickZoom / 2, 0, halfDuration));
                    storyBoard.Children.Add(CreateDoubleAnimation(floatButton, "(Canvas.Top)", floatButtonTop - buttonClickZoom / 2, 0, halfDuration));
                    storyBoard.Children.Add(CreateDoubleAnimation(floatButton, "Width", floatButtonWidth, halfDuration, halfDuration));
                    storyBoard.Children.Add(CreateDoubleAnimation(floatButton, "Height", floatButtonHeight, halfDuration, halfDuration));
                    storyBoard.Children.Add(CreateDoubleAnimation(floatButton, "(Canvas.Left)", floatButtonLeft, halfDuration, halfDuration));
                    storyBoard.Children.Add(CreateDoubleAnimation(floatButton, "(Canvas.Top)", floatButtonTop, halfDuration, halfDuration));
                    break;
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
                int duration = halfDuration;
                int beginTime = 0;
                switch (t)
                {
                    case VisualStateType.Normal:
                        radius = 0;
                        width = edgeButtonWidth + edgeButtonBeginZoom;
                        height = edgeButtonHeight + edgeButtonBeginZoom;
                        if (i == idx)
                        {
                            duration = 100;
                            beginTime = 200;

                            storyBoard.Children.Add(CreateDoubleAnimation(edgeButtons[i], "Width", edgeButtonWidth + buttonClickZoom, 0, 200));
                            storyBoard.Children.Add(CreateDoubleAnimation(edgeButtons[i], "Height", edgeButtonHeight + buttonClickZoom, 0, 200));
                        }
                        break;
                    case VisualStateType.EdgeButtonMouseDown:
                        if (i == idx)
                        {
                            width = edgeButtonWidth + buttonClickZoom;
                            height = edgeButtonHeight + buttonClickZoom;
                            radius = edgeRadius;
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

                storyBoard.Children.Add(CreateDoubleAnimation(edgeButtons[i], "Width", width, beginTime, duration));
                storyBoard.Children.Add(CreateDoubleAnimation(edgeButtons[i], "Height", height, beginTime, duration));
                storyBoard.Children.Add(CreateDoubleAnimation(edgeButtons[i], "(Canvas.Left)", left, beginTime, duration));
                storyBoard.Children.Add(CreateDoubleAnimation(edgeButtons[i], "(Canvas.Top)", top, beginTime, duration));
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
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int GWL_EXSTYLE = -20;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
        protected override void OnSourceInitialized(EventArgs e)
        {
            var hWnd = new WindowInteropHelper(this).Handle;
            int style = GetWindowLong(hWnd, GWL_EXSTYLE);
            SetWindowLong(hWnd, GWL_EXSTYLE, style | WS_EX_NOACTIVATE);

            base.OnSourceInitialized(e);
        }
        public MainWindow()
        {

            InitializeComponent();

            this.Topmost = true;

            edgeButtons = new Button[] { edgeButton1, edgeButton2, edgeButton3, edgeButton4, edgeButton5 };
            var state = VisualStateManager.GetVisualStateGroups(this);
            VisualStateGroup x = VisualStateManager.GetVisualStateGroups(this)[0] as VisualStateGroup;

            x.States.Add(CreateVisualState(VisualStateType.FloatButtonMouseDown));
            x.States.Add(CreateVisualState(VisualStateType.FloatButtonMouseUp));
            x.States.Add(CreateVisualState(VisualStateType.FloatButtonTouchUp));
            x.States.Add(CreateVisualState(VisualStateType.Normal));

            for (int i = 0; i < edgeButtonCount; i++)
            {
                int _idx = i;
                x.States.Add(CreateVisualState(VisualStateType.EdgeButtonMouseDown, i));
                x.States.Add(CreateVisualState(VisualStateType.Normal, i));
                edgeButtons[i].PreviewMouseDown += (object sender, MouseButtonEventArgs e) =>
                {
                    SetVisualState(VisualStateType.EdgeButtonMouseDown, _idx);
                };
                edgeButtons[i].PreviewMouseUp += (object sender, MouseButtonEventArgs e) =>
                {
                    edgeButtonEventHandler(_idx);
                    SetVisualState(VisualStateType.Normal, _idx);
                };
            }

            SetVisualState(VisualStateType.Normal);

            //VisualStateManager.GetVisualStateGroups(this).Add(CreateVisualState(VisualStateType.Normal));

        }

        private void floatButton_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            //toNormalAfterClick = false;
            //if (visualState != VisualStateType.Normal)
            //{
            //    toNormalAfterClick = true;
            //}

            //SetVisualState(VisualStateType.FloatButtonMouseDown);
        }

        private void floatButton_PreviewTouchUp(object sender, TouchEventArgs e)
        {
        }

        private void floatButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var hWnd = new WindowInteropHelper(this).Handle;
            int style = GetWindowLong(hWnd, GWL_EXSTYLE);
            SetWindowLong(hWnd, GWL_EXSTYLE, style | WS_EX_NOACTIVATE);

            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }
            toNormalAfterClick = false;
            if (visualState != VisualStateType.Normal)
            {
                toNormalAfterClick = true;
            }

            SetVisualState(VisualStateType.FloatButtonMouseDown);

        }

        bool toNormalAfterClick = false;
        private double getDistance(Button btn, MouseButtonEventArgs e)
        {
            Point point = e.GetPosition(btn);
            double x = point.X - btn.Width / 2;
            double y = point.Y - btn.Height / 2;
            return x * x + y * y;
        }

        private void floatButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }
            Point p = e.GetPosition(edgeButton5);
            double x = p.X;
            double y = p.Y;

            if (e.StylusDevice == null)
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
            else
            {
                if (toNormalAfterClick)
                {
                    SetVisualState(VisualStateType.Normal);
                }
                else
                {
                    double minDist = 1e9;
                    int minIdx = -1;
                    for (int i = 0; i < edgeButtonCount; i++)
                    {
                        var d = getDistance(edgeButtons[i], e);
                        if (d < minDist)
                        {
                            minDist = d;
                            minIdx = i;
                        }
                    }
                    if (minDist < getDistance(floatButton, e) && getDistance(floatButton, e) > edgeRadius / 2)
                    {
                        edgeButtonEventHandler(minIdx);
                        SetVisualState(VisualStateType.Normal, minIdx);
                    }
                    else
                    {
                        SetVisualState(VisualStateType.FloatButtonTouchUp);
                    }

                }

            }


        }
        private void edgeButtonEventHandler(int idx)
        {
            if (idx == 0)
            {
                Keyboard.SimulateKeyStroke(null, win: true, tab: true);
            }
            if (idx == 1)
            {
                Keyboard.SimulateKeyStroke('C', ctrl: true);
            }
            if (idx == 2)
            {
                Keyboard.SimulateKeyStroke(Keyboard.VK_LEFT, alt: true);
            }
            if (idx == 3)
            {
                Keyboard.SimulateKeyStroke('V', ctrl: true);
            }
            if (idx == 4)
            {
                Keyboard.SimulateKeyStroke('D', win: true);
            }


        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // this.DragMove();
            // 
            if (e.ChangedButton == MouseButton.Right)
            {
                draging = true;
            }
        }

        bool draging = false;
        private void Window_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                draging = false;
            }
        }

        private void Window_TouchMove(object sender, TouchEventArgs e)
        {
            if (!draging)
            {
                return;
            }
            var dpi = VisualTreeHelper.GetDpi(this);
            TouchPoint pointFromWindow = e.GetTouchPoint(this); //relative to Top-Left corner of Window
            TouchPoint pointFromButton = e.GetTouchPoint(floatButton);
            double dx = pointFromButton.Position.X - floatButton.Width / 2 - pointFromWindow.Position.X;
            double dy = pointFromButton.Position.Y - floatButton.Height / 2 - pointFromWindow.Position.Y;
            Point locationFromScreen = this.PointToScreen(new Point(pointFromWindow.Position.X, pointFromWindow.Position.Y)); //translate to coordinates relative to Top-Left corner of Screen

            this.Top = locationFromScreen.Y / dpi.DpiScaleY + dy;
            this.Left = locationFromScreen.X / dpi.DpiScaleX + dx;

        }
    }
}
