using Install_Printers.ProcessClass;
using Install_Printers_Lib.Actions;
using Install_Printers_Lib.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Install_Printers.ViewModels
{

    class LoadWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _output;
        /// <summary>
        /// Service messages
        /// </summary>
        public string Output
        {
            get => _output;
            set
            {
                _output = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Output)));
            }
        }

        private bool _isProgressBarStart;
        /// <summary>
        /// Progress bar management
        /// </summary>
        public bool IsProgressBarStart
        {
            get => _isProgressBarStart;
            set
            {
                _isProgressBarStart = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsProgressBarStart)));
            }
        }

        private Use_Install_Printers_Api api;

        public ICommand CloseLoadWindow => new RelayCommand<object>(obj =>
        {
            Application.Current.Shutdown();
        });

        public LoadWindowViewModel()
        {
            Output = "Загрузка";

            api = new Use_Install_Printers_Api();

            IsProgressBarStart = true;

            Task.Run(() => LoadData());
        }

        /// <summary>
        /// Download the required data
        /// </summary>
        /// <returns></returns>
        private async Task LoadData()
        {
            List<Printer> tempPrintersList = await api.GetPrinters();

            if (tempPrintersList.Count > 0)
                Application.Current.Dispatcher.Invoke(() => LoadMainWindow(tempPrintersList));
            else
            {
                IsProgressBarStart = false;
                Output = "Ошибка загрузки данных. Повторите попытку позже или проверьте подключение.";
            }
        }

        /// <summary>
        /// Passes the data context and loads the main window
        /// </summary>
        /// <param name="printers"></param>
        private void LoadMainWindow(List<Printer> printers)
        {
            MainViewModel mainView = new MainViewModel(printers);

            try
            {
                MainWindow mainWindow = new MainWindow()
                {
                    DataContext = mainView
                };

                Output = "Загрузка главного окна.";

                mainWindow.Show();

                Actions.WindowsUsage.CloseWindow("CheckAndLoadWindow");
            }
            catch (Exception e)
            {
                Output = e.Message;
            }
        }
    }
}
