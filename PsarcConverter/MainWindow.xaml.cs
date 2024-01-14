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

        public MainWindow()
        {
            InitializeComponent();

            ui.RootUIElement = new MainInterface();

            SkiaCanvas.SetLayout(ui);
        }
    }
}
