using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Threading;
using System.ComponentModel;
using Photo_Importer.Properties;
using System.Media;
using System.Diagnostics;

namespace Photo_Importer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private List<DriveInfo> Drives { get; set; }
		private int FileCount { get; set; }
		private int FilesProcessed { get; set; }
		private int MaxPath { get { return 30; } }
		BackgroundWorker bw;

		public MainWindow()
		{
			InitializeComponent();
			DeleteCheckbox.IsChecked = Settings.Default.Delete;
			Drives = DriveInfo.GetDrives().Where(drive => drive.DriveType == DriveType.Removable).ToList();
			bw = new BackgroundWorker();
			bw.WorkerReportsProgress = true;
			bw.DoWork += Bw_DoWork;
			bw.ProgressChanged += Bw_ProgressChanged;
			bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
			FolderNameTextBox.Focus();
			//Reset Settings

			//Settings.Default.Reset();
			//Settings.Default.Save();

			if (Settings.Default.Path == "UNSET")
			{
				Settings.Default.Path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
				Settings.Default.Save();
			}

			if (Drives.Count == 1)
			{
				SourcePath.Content = PathShortner(Strings.StartLabelText, Drives.FirstOrDefault().Name);
				DestinationPath.Content = PathShortner(Strings.FinishLabelText, Settings.Default.Path);
				RecursiveFileCount(Drives[0].RootDirectory.FullName);
				OverallProgressBar.Maximum = FileCount;
			}
			else
			{
				if (MessageBox.Show(Strings.NoDriveSettings, Strings.NoDrive, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
				{
					SettingsWindow settings = new SettingsWindow();
					settings.Show();
					this.Close();
				}
				else
				{
					this.Close();
				}
			}
			var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
			this.Left = desktopWorkingArea.Right - this.Width;
			this.Top = desktopWorkingArea.Bottom - this.Height;
		}

		private string PathShortner(string prefix, string path)
		{
			string output;
			output = prefix;
			output += "\t";
			if (path.Length > MaxPath + 1)
			{
				output += path.Substring(0, 3);
				output += "...";
				output += path.Substring((path.Length - MaxPath), MaxPath);
				return output;
			}
			else
			{
				output += path;
				return output;
			}
		}

		private void DeleteCheckbox_Checked(object sender, RoutedEventArgs e)
		{
			Settings.Default.Delete = true;
			Settings.Default.Save();
		}

		private void DeleteCheckbox_Unchecked(object sender, RoutedEventArgs e)
		{
			Settings.Default.Delete = false;
			Settings.Default.Save();
		}

		private void StartButton_Click(object sender, RoutedEventArgs e)
		{
			StartButton.IsEnabled = false;
			string path = Drives[0].RootDirectory.FullName;
			string newPath = FolderNameTextBox.Text;
			if (newPath.Length > 0)
			{
				newPath = Settings.Default.Path + "\\" + newPath + "\\";
				string[] args = { path, newPath, };
				if (!Directory.Exists(newPath))
				{
					Directory.CreateDirectory(newPath);
				}
				bw.RunWorkerAsync(args);
			}
			else
			{
				MessageBox.Show(Strings.NoFolder);
			}
		}

		private void RecursiveFileTransfer(string path, string newPath, int fileNumber)
		{
			string[] filePaths = Directory.GetFiles(path);
			FilesProcessed++;
			foreach (string filePath in filePaths)
			{
				string extension = System.IO.Path.GetExtension(filePath);
				string fileName = System.IO.Path.GetFileName(filePath);
				string newFile = newPath + fileName;
				if (Settings.Default.Rename)
				{
					bool keepRunning = true;
					while (keepRunning)
					{
						newFile = newPath + fileNumber.ToString("D5") + extension;
						if (File.Exists(newFile))
						{
							fileNumber++;
						}
						else
						{
							keepRunning = false;
						}
					}
				}
				if (Settings.Default.Extensions.Contains(extension.ToLower()))
				{
					if (Settings.Default.Delete)
					{
						File.Move(filePath, newFile);
					}
					else
					{
						File.Copy(filePath, newFile);
					}
					bw.ReportProgress(FilesProcessed, new string[] { filePath, newFile });
				}
				fileNumber++;
			}
			string[] directoryPaths = Directory.GetDirectories(path);
			foreach (string directoryPath in directoryPaths)
			{
				RecursiveFileTransfer(directoryPath, newPath, fileNumber);
			}
			filePaths = Directory.GetFiles(path);
			directoryPaths = Directory.GetDirectories(path);
			if (filePaths.Count() == 0 && directoryPaths.Count() == 0)
			{
				Directory.Delete(path);
			}
		}

		private void RecursiveFileCount(string path)
		{
			string[] filePaths = Directory.GetFiles(path);
			FileCount += filePaths.Length;
			string[] directoryPaths = Directory.GetDirectories(path);
			foreach (string directoryPath in directoryPaths)
			{
				RecursiveFileCount(directoryPath);
			}
		}

		private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			string newPath = FolderNameTextBox.Text;
			if (Settings.Default.OpenAfterCopy)
			{
				Process.Start(Settings.Default.Path + "\\" + newPath + "\\");
			}
			if (Settings.Default.NotifyComplete)
			{
				MessageBox.Show(Strings.CopyComplete);
			}
			else
			{
				SystemSounds.Beep.Play();
			}
			this.Close();
		}

		private void Bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			OverallProgressBar.Value = e.ProgressPercentage;
			string[] args = e.UserState as string[];
			SourcePath.Content = PathShortner(Strings.StartLabelText, args[0]);
			DestinationPath.Content = PathShortner(Strings.FinishLabelText, args[1]);
		}

		private void Bw_DoWork(object sender, DoWorkEventArgs e)
		{
			string[] args = e.Argument as string[];
			RecursiveFileTransfer(args[0], args[1], 1);
		}
	}
}