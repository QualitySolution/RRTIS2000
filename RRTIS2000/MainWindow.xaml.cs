using Microsoft.Win32;
using System;
using System.Data.Odbc;
using System.Windows;
using System.IO;
using System.Text;

namespace RRTIS2000
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		OdbcConnection connection;
		private OdbcConnection Connection{
			get
			{
				if (connection == null)
				{
					connection = new OdbcConnection("DSN=spsnaodb;Uid=tbadmin;Pwd=;");
				}
				if (connection.State != System.Data.ConnectionState.Open)
					connection.Open();
			return connection;
			}
		}

		private byte[] ReadedFile;
		private string number;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			try
			{

				using (OdbcCommand com = new OdbcCommand(
					"SELECT * FROM Calibration_Files WHERE Calibration_Part_No = ?", Connection))
				{
					com.Parameters.AddWithValue("@var", tbNumber.Text);

					using (OdbcDataReader reader = com.ExecuteReader())
					{
						if (!reader.HasRows)
						{
							LableResult.Content = "Указанный номер не найден в БД.";
							return;
						}
						reader.Read();

						ReadedFile = (byte[])reader.GetValue(1);
						number = tbNumber.Text;

						LableResult.Content = String.Format("Загружен файл gzip, размером {0} байт.", ReadedFile.Length);
						btSave.IsEnabled = btSave_Extract.IsEnabled = true;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(String.Format("Ошибка при запросе: {0}", ex.ToString()));
			}

		}

		private void btSave_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.FileName = String.Format("{0}.gzip", number); // Default file name
			dlg.DefaultExt = ".gzip"; // Default file extension

			// Show save file dialog box
			Nullable<bool> result = dlg.ShowDialog();

			// Process save file dialog box results
			if (result == true)
			{
				// Save document
				try
				{
					File.WriteAllBytes(dlg.FileName, ReadedFile);
					
				}
				catch (Exception _Exception)
				{
					// Error
					MessageBox.Show( String.Format("Exception caught in process: {0}", _Exception.ToString()));
				}

			}
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			var list = new StringBuilder();

			using (OdbcCommand com = new OdbcCommand(
				"SELECT Calibration_Part_No FROM Calibration_Files", Connection))
			{
				using (OdbcDataReader reader = com.ExecuteReader())
				{
					while (reader.Read())
					{
						list.AppendLine(reader.GetValue(0).ToString());
					}
				}
			}

			if (list.Length > 0)
			{
				Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
				dlg.FileName = String.Format("allNumbers.txt"); // Default file name
				dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

				// Show save file dialog box
				Nullable<bool> result = dlg.ShowDialog();

				// Process save file dialog box results
				if (result == true)
				{
					// Save document
					try
					{
						File.WriteAllText(dlg.FileName, list.ToString());

					}
					catch (Exception _Exception)
					{
						// Error
						MessageBox.Show(String.Format("Exception caught in process: {0}", _Exception.ToString()));
					}

					System.Diagnostics.Process.Start(dlg.FileName);
				}
			}
		}

		private void btSave_Extract_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
			fbd.Description = "Выберите папку для распаковки";
			var result = fbd.ShowDialog();

			// Process save file dialog box results
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				// Save document
				try
				{
					var tempDir = System.IO.Path.GetTempPath();
					var gzipFile = System.IO.Path.Combine(tempDir, String.Format("{0}.gzip", number));
					File.WriteAllBytes(gzipFile, ReadedFile);

					string command = "7z.exe";
					string arguments = String.Format("e -o{0} {1}", fbd.SelectedPath, gzipFile);

					System.Diagnostics.Process.Start(command, arguments);
				}
				catch (Exception _Exception)
				{
					// Error
					MessageBox.Show(String.Format("Exception caught in process: {0}", _Exception.ToString()));
				}

			}

		}
	}
}
