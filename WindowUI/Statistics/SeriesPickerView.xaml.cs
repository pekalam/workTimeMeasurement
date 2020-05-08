using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Prism.Mvvm;

namespace WindowUI.Statistics
{
    /// <summary>
    /// Interaction logic for SeriesPickerView.xaml
    /// </summary>
    public partial class SeriesPickerView : UserControl
    {
        public SeriesPickerView()
        {
            InitializeComponent();
        }
    }

    public class SeriesPickerViewModel : BindableBase
    {
        private bool _showMouse = true;
        private bool _showWatchingScreen = true;
        private bool _showKeyboard = true;
        private bool _showAway = true;

        public bool ShowMouse
        {
            get => _showMouse;
            set => SetProperty(ref _showMouse, value);
        }

        public bool ShowWatchingScreen
        {
            get => _showWatchingScreen;
            set => SetProperty(ref _showWatchingScreen, value);
        }

        public bool ShowKeyboard
        {
            get => _showKeyboard;
            set => SetProperty(ref _showKeyboard, value);
        }

        public bool ShowAway
        {
            get => _showAway;
            set => SetProperty(ref _showAway, value);
        }
    }
}
