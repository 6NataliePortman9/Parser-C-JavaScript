using System;
using System.IO;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TDAPZ_Q_A
{
    public partial class MainWindow : Window
    {
        private OleDbConnection connection = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\VScodeParser\\TDAPZParser\\TDPAZ_db.accdb;OLE DB Services=-1");
        public MainWindow()
        {
            InitializeComponent();
        }

       private async void FindQuestion_Click(object sender, RoutedEventArgs e)
{
    try
    {
        string question = txtQuestion.Text.Trim();

        if (string.IsNullOrWhiteSpace(question))
        {
            ShakeTextBox(txtQuestion);
            await Task.Delay(2000);
            return;
        }

          connection.Open();
            string selectQuery = "SELECT Answer FROM [QuestionsAndAnswers] WHERE Question = ?";

            using (OleDbCommand selectCommand = new OleDbCommand(selectQuery, connection))
            {
                selectCommand.Parameters.AddWithValue("?", question);

                using (OleDbDataReader reader = selectCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        txtAnswer.Text = reader["Answer"].ToString();
                    }
                    else
                    {
                        txtAnswer.Text = "Not found";
                            ShakeTextBox(txtAnswer);
                            ShakeTextBox(txtQuestion);
                            await Task.Delay(2000);
                        }
                }
            }
        
    }
    catch (Exception ex)
    {
        ShakeTextBox(txtQuestion);
        MessageBox.Show("An error occurred: " + ex.Message);
    }
    finally
    {
                connection.Close();
    }
  }
        private void txtQuestion_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FindQuestion_Click(btmFindQuestion, new RoutedEventArgs()); // Симулюємо натискання кнопки Find
            }
        }

        private void AddQuestion_Click(object sender, RoutedEventArgs e)
        {
            AddQuestionWindow addQuestionWindow = new AddQuestionWindow();
            addQuestionWindow.Show();
        }

        private void EditQuestion_Click(object sender, RoutedEventArgs e)
        {
            EditQuestionWindow editQuestionWindow = new EditQuestionWindow();
            editQuestionWindow.Show();
        }
        private void txtQuestion_GotFocus(object sender, RoutedEventArgs e)
        {
            txtQuestion.Clear();
            txtAnswer.Clear();
        }

        private bool isPowerShellFirstRun = true;
        private Process currentPowerShellProcess = null;

        private void StartParser_Click(object sender, RoutedEventArgs e)
        {
            // Спочатку закриваємо попередній процес, якщо він існує
            if (currentPowerShellProcess != null && !currentPowerShellProcess.HasExited)
            {
                try
                {
                    currentPowerShellProcess.Kill();
                    currentPowerShellProcess.WaitForExit();
                }
                catch (Exception ex)
                {
                    // Обробка помилок при закритті процесу
                    Console.WriteLine($"Error closing previous process: {ex.Message}");
                }
            }

            // Запускаємо новий процес
            string command = "cd C:\\VScodeParser\\TDAPZparser; node index.js";

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoExit -Command \"{command}\"",
                UseShellExecute = false
            };

            // Зберігаємо посилання на новий процес
            currentPowerShellProcess = Process.Start(psi);

            isPowerShellFirstRun = false;
        }
        private void ShakeTextBox(TextBox textBox)
        {
            // Анімація тремтіння
            TranslateTransform transform = new TranslateTransform();
            textBox.RenderTransform = transform;

            DoubleAnimation animation = new DoubleAnimation(0, 12, new Duration(TimeSpan.FromMilliseconds(60)));
            animation.AutoReverse = true;
            animation.RepeatBehavior = new RepeatBehavior(5); // 7 разів

            transform.BeginAnimation(TranslateTransform.XProperty, animation);

            // Підсвічуємо червоним
            textBox.BorderBrush = Brushes.Red;
            textBox.Background = Brushes.LightCoral; // Зміна фону

            // Повертаємо колір через 0,87 секунди
            Task.Delay(870).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() =>
                {
                    textBox.BorderBrush = Brushes.Gray;
                    textBox.Background = Brushes.White; // Повертаємо фон
                });
            });
        }
        /////////  NEW METHOD \\\\\\\\\
        private void ConvertQuestion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                connection.Open();
                string pathName = @"D:\Prg\TDAPZ_Q&A\TDAPZ_Q&A\Converted_DB\TDAPZ_MONGO.txt";

                if (File.Exists(pathName))
                {
                    File.Delete(pathName);
                }

                string getMaxIdQuery = "SELECT MAX(ID) FROM [QuestionsAndAnswers]";
                int maxId = 0;
                using (OleDbCommand maxIdCommand = new OleDbCommand(getMaxIdQuery, connection))
                {
                    object result = maxIdCommand.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        maxId = Convert.ToInt32(result);
                    }
                }

                int questionID = 0;
                StringBuilder allData = new StringBuilder();

                for (questionID = 1; questionID <= maxId; questionID++)
                {
                    string selectQuery = "SELECT Question FROM [QuestionsAndAnswers] WHERE [ID] = ?";
                    using (OleDbCommand selectCommand = new OleDbCommand(selectQuery, connection))
                    {
                        selectCommand.Parameters.AddWithValue("?", questionID);
                        using (OleDbDataReader reader = selectCommand.ExecuteReader())
                        {
                            string questionaccedb;
                            if (reader.Read())
                            {
                                questionaccedb = reader["Question"].ToString();
                                allData.AppendLine("{");
                                allData.AppendLine($"question {questionID}:'{questionaccedb}'");
                                //allData.AppendLine($"\"question\": \"{questionaccedb}\",");
                                allData.AppendLine("},");
                            }
                            else
                            {
                                questionaccedb = reader["Question"].ToString();
                                allData.AppendLine("\n##################");
                                allData.AppendLine($"question {questionID}: not found!");
                                //allData.AppendLine($"question: not found!");
                                allData.AppendLine("##################\n");
                            }
                        }
                        File.WriteAllText(pathName, allData.ToString());
                    }
                }
                if (File.Exists(pathName))
                {
                    MessageBox.Show("File is created!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("File is not created", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }
            catch (Exception ex)
            {
                ShakeTextBox(txtQuestion);
                MessageBox.Show("An error occurred: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }
        /////////  NEW METHOD \\\\\\\\\
    }
}
