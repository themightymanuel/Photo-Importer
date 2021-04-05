using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Photo_Importer.Properties;

namespace Photo_Importer
{
	/// <summary>
	/// Interaction logic for Settings.xaml
	/// </summary>
	public partial class SettingsWindow : Window
    {
        System.Collections.Specialized.StringCollection Ext;
        public SettingsWindow()
        {
            InitializeComponent();
            DeleteCheckbox.IsChecked = Settings.Default.Delete;
            RenameCheckbox.IsChecked = Settings.Default.Rename;
            PopupCheckbox.IsChecked = Settings.Default.NotifyComplete;
            OpenCheckbox.IsChecked = Settings.Default.OpenAfterCopy;
            PathTextBox.Text = Settings.Default.Path;
            Ext = Settings.Default.Extensions;
            FileExtensionListBox.ItemsSource = Ext;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Delete = (bool)DeleteCheckbox.IsChecked;
            Settings.Default.Rename = (bool)RenameCheckbox.IsChecked;
            Settings.Default.NotifyComplete = (bool)PopupCheckbox.IsChecked;
            Settings.Default.OpenAfterCopy = (bool)OpenCheckbox.IsChecked;
            Settings.Default.Extensions = Ext;
            Settings.Default.Save();
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AddExtensionButton_Click(object sender, RoutedEventArgs e)
        {
            string newExtension = AddExtensionTextbox.Text;
            if(newExtension.First() != '.')
            {
                newExtension = "." + newExtension;
            }
            newExtension = newExtension.ToLower();
            Ext.Add(newExtension);
            FileExtensionListBox.ItemsSource = Ext;
            FileExtensionListBox.Items.Refresh();
        }

        private void DeleteExtensionButton_Click(object sender, RoutedEventArgs e)
        {
            string extensionSelected = FileExtensionListBox.SelectedItem as string;
            Ext.Remove(extensionSelected);
            FileExtensionListBox.ItemsSource = Ext;
            FileExtensionListBox.Items.Refresh();
        }

        private void FolderSelectButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = fbd.SelectedPath;
                Settings.Default.Path = path;
                PathTextBox.Text = Settings.Default.Path;
            }
        }
    }
}
