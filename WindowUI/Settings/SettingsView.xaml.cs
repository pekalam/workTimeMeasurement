using System.Windows.Controls;
using Prism.Mvvm;

namespace WindowUI.Settings
{
    /// <summary>
    /// Interaction logic for SettingsView
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }
    }

    public class SettingsViewModel : BindableBase
    {

    }
}
