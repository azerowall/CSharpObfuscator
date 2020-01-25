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
using Microsoft.Win32;
using System.Threading;
using System.ComponentModel;

namespace CSObfuscator
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

        private void btnObfuscate_Click(object sender, RoutedEventArgs e)
        {
            // установка параметров обфускации
            ObfuscateParameters parameters = new ObfuscateParameters();
            parameters |= ObfuscateParameters.RemoveFormattingCharacters | ObfuscateParameters.RenameLocals;
            parameters |= cbRenameParameters.IsChecked.Value ? ObfuscateParameters.RenameParameters : 0;
            parameters |= cbRenameGlobals.IsChecked.Value ? ObfuscateParameters.RenameGlobals : 0;
            parameters |= cbObfuscateLiterals.IsChecked.Value ? ObfuscateParameters.ObfuscateLiterals : 0;
            parameters |= cbReplaceConstructions.IsChecked.Value ? ObfuscateParameters.ReplaceConstructions : 0;
            
            if (String.IsNullOrWhiteSpace(tbCode.Text))
                return;
            
            btnObfuscate.Visibility = Visibility.Collapsed;
            pbObfProgress.Visibility = Visibility.Visible;
            menuOpen.IsEnabled = false;
            menuSaveAs.IsEnabled = false;
            
            // запуск процесса обфускации отдельным потоком
            var obf = new Obfuscator(tbCode.Text, parameters);
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += (o, args) => {
                try
                {
                    args.Result = ((Obfuscator)args.Argument).Obfuscate();
                }
                catch (Exception ex)
                {
                    args.Result = ex;
                }
            };
            bw.RunWorkerCompleted += (o, args) => {
                pbObfProgress.Visibility = Visibility.Collapsed;
                btnObfuscate.Visibility = Visibility.Visible;
                menuOpen.IsEnabled = true;
                menuSaveAs.IsEnabled = true;

                if (args.Result is Exception)
                    MessageBox.Show(((Exception)args.Result).Message);
                else
                    tbCode.Text = (string)args.Result;
            };
            bw.RunWorkerAsync(obf);
        }

        private void tbCode_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (String.Compare(System.IO.Path.GetExtension(files[0]), ".cs", true) == 0)
                    tbCode.Text = File.ReadAllText(files[0]);
            }
        }

        private void tbCode_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Handled = true;
            }
        }

        private void menuOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.Filter = "CSharp Code |*.cs";
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == true)
                tbCode.Text = File.ReadAllText(ofd.FileName);
        }

        private void menuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(tbCode.Text))
                return;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.CheckFileExists = false;
            sfd.Filter = "CSharp Code |*.cs";
            sfd.AddExtension = true;
            sfd.DefaultExt = ".cs";
            if (sfd.ShowDialog() == true)
                File.WriteAllText(sfd.FileName, tbCode.Text);
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Программа: CSharpObfuscator\n" +
                "Предназначение: обфускация исходного кода на языке C#\n" +
                "Автор: Валгуснов Алексей\n" +
                "Год: 2018", "О программе");
        }
    }
}
