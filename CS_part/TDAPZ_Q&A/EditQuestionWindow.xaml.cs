using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TDAPZ_Q_A
{
    /// <summary>
    /// Interaction logic for EditQuestionWindow.xaml
    /// </summary>
    public partial class EditQuestionWindow : Window
    {
        private OleDbConnection connection = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\VScodeParser\\TDAPZParser\\TDPAZ_db.accdb;OLE DB Services=-1");
        public EditQuestionWindow()
        {
            InitializeComponent();
        }

        private void BackButtom_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private async void EditQuestion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string question = txtQuestion.Text.Trim();
                string answer = txtAnswer.Text.Trim();
               
                if (string.IsNullOrWhiteSpace(question))
                {
                    MessageBox.Show("Enter question!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ShakeTextBox(txtQuestion);
                    await Task.Delay(2000);
                    return;
                }
                if (!CheckQuestionLenght(question))
                {
                    await Task.Delay(2000);
                    return;
                }
                if (string.IsNullOrWhiteSpace(answer))
                {
                    MessageBox.Show("Enter answer!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ShakeTextBox(txtAnswer);
                    await Task.Delay(2000);
                    return;
                }
                if (!CheckAnswerLenght(answer))
                {
                    await Task.Delay(2000);
                    return;
                }
                if (CheckIfQuestionExists(question))
                {
                    MessageBox.Show("Question found! Updating answer...", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Question not found. Cannot update.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    ShakeTextBox(txtQuestion);
                    await Task.Delay(2000);
                    return;
                }


                connection.Open();
                string editQuery = "Update [QuestionsAndAnswers] Set Answer = ? WHERE Question = ?";

                using (OleDbCommand editCommand = new OleDbCommand(editQuery, connection))
                {
                    editCommand.Parameters.AddWithValue("?", answer);
                    editCommand.Parameters.AddWithValue("?", question);

                    int rowsAffected = editCommand.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Answer updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        txtQuestion.Clear();
                        txtAnswer.Clear();
                    }
                    else
                    {
                        MessageBox.Show("No matching question found!", "Not found", MessageBoxButton.OK, MessageBoxImage.Warning);
                        ShakeTextBox(txtAnswer);
                        ShakeTextBox(txtQuestion);
                        await Task.Delay(2000);
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
                EditQuestion_Click(btmEditQuestion, new RoutedEventArgs());
            }
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
        private bool CheckQuestionLenght(string question)
        {
            bool islenghtcorrect = false;
            try
            {
                if (question.Length <= 100)
                {
                    islenghtcorrect = false;
                    MessageBox.Show("Question too short!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    ShakeTextBox(txtQuestion);
                }
                else   islenghtcorrect = true;
                
               
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while checking user: " + ex.Message);
            }
            finally
            {
    
            }
            return islenghtcorrect;
        }

        private bool CheckAnswerLenght(string answer)
        {
            bool islenghtcorrect = false;
            try
            {
                if (answer.Length < 1)
                {
                    islenghtcorrect = false;
                    MessageBox.Show("Answer not correct format!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    ShakeTextBox(txtAnswer);
                }
                else islenghtcorrect = true;


            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while checking user: " + ex.Message);
            }
            finally
            {

            }
            return islenghtcorrect;
        }

        private bool CheckIfQuestionExists(string question)
        {
            bool questionExists = false;
            try
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM QuestionsAndAnswers WHERE Question = ?";
                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("?", question);

                int questionCount = (int)command.ExecuteScalar();
                questionExists = questionCount > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while checking user: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }
            return questionExists;
        }



        private void ShakeTextBox(TextBox textBox)
        {
            // Анімація тремтіння
            TranslateTransform transform = new TranslateTransform();
            textBox.RenderTransform = transform;

            DoubleAnimation animation = new DoubleAnimation(0, 10, new Duration(TimeSpan.FromMilliseconds(60)));
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
    }
}
