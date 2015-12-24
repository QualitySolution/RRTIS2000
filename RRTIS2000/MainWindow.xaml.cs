using Microsoft.Win32;
using System;
using System.Data.Odbc;
using System.Windows;
using System.IO;

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

					LableResult.Content = String.Format("Загружен файл {0} байт.", ReadedFile.Length);
					btSave.IsEnabled = true;
				}
			}
		}

		private void btSave_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.FileName = String.Format("{0}.bin", number); // Default file name
			dlg.DefaultExt = ".bin"; // Default file extension
			//dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

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
	}
}
