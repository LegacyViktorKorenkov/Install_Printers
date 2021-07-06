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
        public string Output
        {
            get => _output;
            set
            {
                _output = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Output)));
            }
        }

        private bool _startProgBar;
        public bool StartProgBar
        {
            get => _startProgBar;
            set
            {
                _startProgBar = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartProgBar)));
            }
        }

        Use_Install_Printers_Api api;

        public ICommand CloseLoadWindow => new RelayCommand<object>(obj =>
        {
            Application.Current.Shutdown();
        });

        public LoadWindowViewModel()
        {
            Output = "Загрузка";

            api = new Use_Install_Printers_Api();

            StartProgBar = true;

            Task.Run(() => LoadData());
        }

        private async Task LoadData()
        {
            List<Printer> tempPrintersList = await api.GetPrinters();

            if (CheckList(tempPrintersList))
                Application.Current.Dispatcher.Invoke(() => LoadMainWindow(tempPrintersList));
            else
            {
                StartProgBar = false;
                Output = "Ошибка загрузки данных. Повторите попытку позже или проверьте подключение.";
            }
        }

        private bool CheckList(List<Printer> printers)
        {
            if (printers.Count > 0)
                return true;
            else
                return false;
        }

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
