using SkiaSharp;
using System.ComponentModel;
using System.Windows;
using UILayout;

namespace PsarcConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SkiaLayout ui = new SkiaLayout();

        MainInterface mainInterface;

        public MainWindow()
        {
            Title = "PsarcConverter v0.1.3";

            InitializeComponent();

            Layout.Current.DefaultFont = new UIFont { Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), TextSize = 16 };

            ui.RootUIElement = mainInterface = new MainInterface();

            SkiaCanvas.SetLayout(ui);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            mainInterface.SaveOptions();

            base.OnClosing(e);
        }
    }
}
