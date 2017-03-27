using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

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

		public List<ControllerItem> controllersList;

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

		private void buttonVinFine_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var vin = tbVin.Text;
				int vin_id;

				string controller_Id_String, VCI_String;
				
				//Fine Vin id
				using (OdbcCommand com = new OdbcCommand(
					"SELECT VIN_Id FROM VCI_AsBuilt_Id WHERE (VIN_1To8 = ?) AND (VIN_10 = ?) AND (VIN_11 = ?)", Connection))
				{
					com.Parameters.AddWithValue("?1", vin.Substring(0, 8));
					com.Parameters.AddWithValue("?2", vin.Substring(9, 1));
					com.Parameters.AddWithValue("?3", vin.Substring(10, 1));

					using (OdbcDataReader reader = com.ExecuteReader())
					{
						if (!reader.HasRows)
						{
							var text = String.Format("Семейство машин для указанного VIN не найдено в БД.\n [{0}]{1}[{2}]{3}",
								vin.Substring(0, 8),
								vin[8],
								vin.Substring(9, 2),
								vin.Substring(11)
								);
							MessageBox.Show(text);
							return;
						}
						reader.Read();

						vin_id = reader.GetInt32(0);
					}
				}

				//Fine Car
				using (OdbcCommand com = new OdbcCommand(
					"SELECT Controller_Id_String, VCI_String FROM VCI_AsBuilt_String WHERE (VIN_Id = ?) AND (VIN_12To17 = ?)", Connection))
				{
					com.Parameters.AddWithValue("?1", vin_id);
					com.Parameters.AddWithValue("?2", vin.Substring(11, 6));

					using (OdbcDataReader reader = com.ExecuteReader())
					{
						if (!reader.HasRows)
						{
							var text = String.Format("Машина для указанного VIN не найдена в БД.\n {0}[{1}]",
								vin.Substring(0, 11),
								vin.Substring(11, 6)
								);
							MessageBox.Show("Машина для указанного VIN не найден в БД.");
							return;
						}
						reader.Read();

						controller_Id_String = reader.GetString(0);
						VCI_String = reader.GetString(1);
					}
				}

				var splitedId = controller_Id_String.Split(',');
				var splitedVCI = VCI_String.Split(',');
				var lastOfVIN =  int.Parse(vin.Substring(11,6));

				controllersList = new List<ControllerItem>();

				for (int i = 0; i < splitedId.Length; i++)
				{
					var controler = new ControllerItem
					{
						ContrillerId = int.Parse(splitedId[i]),
						VCI_Controller = int.Parse(splitedVCI[i])
					};
					controler.VCI_Car = controler.VCI_Controller + lastOfVIN;
					controllersList.Add(controler);
				}

				//Fine Controllers Descriptions
				var sql = String.Format("SELECT Controller_Id, Controller_Name, Description FROM Controller_Description WHERE (Language_Code = 1033) AND (Controller_Id IN ({0}))",
					controller_Id_String);
				using (OdbcCommand com = new OdbcCommand(sql , Connection))
				{
					//com.Parameters.AddWithValue("?1", controller_Id_String);
					//com.Parameters.AddWithValue("?1", controllersList.Select(x => x.ContrillerId).ToArray());

					using (OdbcDataReader reader = com.ExecuteReader())
					{
						while (reader.Read())
						{
							var controllerId = reader.GetInt32(0);
							var controller = controllersList.FirstOrDefault(x => x.ContrillerId == controllerId);
							if (controller != null)
							{
								controller.ControllerName = reader.GetString(1);
								controller.ControllerDiscription = reader.GetString(2);
							}
						}
					}
				}

				gridControllers.ItemsSource = controllersList;
			}
			catch (Exception ex)
			{
				MessageBox.Show(String.Format("Ошибка при запросе: {0}", ex.ToString()));
			}

		}

		private void tbVin_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			buttonVinFine.IsEnabled = tbVin.Text.Length == 17;
		}
	}
}
