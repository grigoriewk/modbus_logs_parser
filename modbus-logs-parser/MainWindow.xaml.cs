using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace test_app_for_techart
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e) //open file
        {
            OpenFileDialog openFileDialog = new OpenFileDialog(); //example is raw_log_example & raw_log_example2
            openFileDialog.Filter = "Log files (*.log)|*.log|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                logWithData.Text = File.ReadAllText(openFileDialog.FileName);
        }

        private void btnSaveReport_Click(object sender, RoutedEventArgs e) //save file
        {
            LogConverter converter = new LogConverter();
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == true)
            {
                    File.WriteAllText(saveFileDialog.FileName, logParseResult.Text);
            }
        }

        private void btnStartConvertingLog_Click(object sender, RoutedEventArgs e)
        {
            LogConverter converter = new LogConverter();
            converter.LoadCommands(@"C:\Users\kiril\source\repos\modbus-logs-parser\modbus-logs-parser\commands.vcb"); //домашнее задание - убрать прямые ссылки
            converter.LoadExceptions(@"C:\Users\kiril\source\repos\modbus-logs-parser\modbus-logs-parser\exceptions.vcb");
            List<LogEntry> logs = converter.ParseLog(logWithData.Text);
                    logParseResult.Text = converter.ConverToTXT(logs);
            
        }
    }
}
