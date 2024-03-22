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
            InitializeComponent();

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
