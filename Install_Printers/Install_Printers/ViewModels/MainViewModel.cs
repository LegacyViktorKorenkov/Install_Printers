using Install_Printers.Actions;
using Install_Printers.ProcessClass;
using Install_Printers_Lib.Actions;
using Install_Printers_Lib.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Install_Printers.ViewModels
{
    class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        Use_Install_Printers_Api _api;

        private Printer _printerDriver;

        private string _output;
        /// <summary>
        /// Service messages
        /// </summary>
        public string Output
        {
            get => _output;
            set { _output = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Output))); }
        }

        private string _selectedPrinter;
        /// <summary>
        /// Selected printer object
        /// </summary>
        public string SelectedPrinter
        {
            get => _selectedPrinter;
            set
            {
                Task.Run(async () =>
                {
                    _selectedPrinter = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedPrinter)));
                    await GetInstallPrinterInfo();
                    if (_printerDriver != null && _printerDriver.NetPrinter) IsNetworkPrinter = true;
                    else IsNetworkPrinter = false;
                });
            }
        }

        private string _printerNetName;
        /// <summary>
        /// Printer network name
        /// </summary>
        public string PrinterNetName
        {
            get => _printerNetName;
            set { _printerNetName = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PrinterNetName))); }
        }

        private ObservableCollection<Printer> printers;
        /// <summary>
        /// List of printers connected to the computer
        /// </summary>
        public ObservableCollection<Printer> Printers
        {
            get => printers;
            set { printers = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Printers))); }
        }

        private bool _isNetworkPrinter;

        public bool IsNetworkPrinter
        {
            get => _isNetworkPrinter;
            set { _isNetworkPrinter = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsNetworkPrinter))); }
        }

        private bool _isStartProgressBar;
        public bool IsStartProgressBar
        {
            get => _isStartProgressBar;
            set { _isStartProgressBar = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsStartProgressBar))); }
        }

        private ObservableCollection<string> instaledPrintersList;
        public ObservableCollection<string> InstaledPrintersList
        {
            get => instaledPrintersList;
            set { instaledPrintersList = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InstaledPrintersList))); }
        }

        private string selectedInstalledPrinter;
        public string SelectedInstalledPrinter
        {
            get => selectedInstalledPrinter;
            set { selectedInstalledPrinter = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedInstalledPrinter))); }
        }

        PrintersInfo printerInfo;

        public ICommand GetPrinterDriverCommand => new RelayCommand<object>(async obj =>
        {
            InstallDrivers install;
            DeleteFoldersFiles delete = new DeleteFoldersFiles($@"Temp", true);

            Output = "Проверка введенных данных";

            if (_isStartProgressBar)
            {
                if (!string.IsNullOrWhiteSpace(PrinterNetName) && await PrinterNetName.CheckLanStatus())
                {
                    if (_printerDriver.NetPrinter)
                    {
                        try
                        {
                            Output = "Установка.";
                            IsStartProgressBar = true;
                            install = new InstallDrivers(_printerDriver, PrinterNetName, new Unzip($@"Temp\{SelectedPrinter}"));
                            await install.Start();

                            InstaledPrintersList = new ObservableCollection<string>(new PrintersInfo().InstalledPrinters());
                            Output = "Установка завершена.";
                        }
                        catch (Exception e)
                        {
                            Output = e.Message;
                        }

                        if (Directory.Exists($@"Temp"))
                        {
                            try
                            {
                                delete.StartDelete();
                            }
                            catch (Exception e)
                            {
                                Output = e.Message;
                            }
                        }

                        IsStartProgressBar = false;

                    }
                    else
                    {
                        Output = "Выбранный вами принтер не является сетевым.";
                    }
                }
                else
                {
                    Output = "Проверьте правильность введенных данных.";
                }
            }
            else
            {
                try
                {
                    Output = "Установка.";
                    IsStartProgressBar = true;
                    install = new InstallDrivers(_printerDriver, new Unzip($@"Temp\{SelectedPrinter}"));
                    await install.Start();

                    InstaledPrintersList = new ObservableCollection<string>(new PrintersInfo().InstalledPrinters());
                    Output = "Установка завершена.";
                }
                catch (Exception e)
                {
                    Output = e.Message;
                }

                if (Directory.Exists($@"Temp"))
                {
                    try
                    {
                        delete.StartDelete();
                    }
                    catch (Exception e)
                    {
                        Output = e.Message;
                    }
                }

                IsStartProgressBar = false;
            }
        });

        public ICommand CloseLoadWindow => new RelayCommand<object>(obj =>
        {
            Application.Current.Shutdown();
        });

        public ICommand InstallDefaultPrinter => new RelayCommand<object>(obj =>
        {
            try
            {
                printerInfo.DefaultPrinter(SelectedInstalledPrinter);
            }
            catch (Exception e)
            {
                Output = e.Message;
            }
        });

        public ICommand DeletePrinter => new RelayCommand<object>(obj =>
        {
            try
            {
                Task.Run(() =>
               {
                   try
                   {
                       var temp = SelectedInstalledPrinter;

                       printerInfo.DeletePrinter(SelectedInstalledPrinter);

                       //printerInfo.DeletePrinter(@"\\test\test2\Printer exmple");

                       InstaledPrintersList = new ObservableCollection<string>(new PrintersInfo().InstalledPrinters());

                       Output = $"{temp} - удален";
                   }
                   catch (Exception e)
                   {
                       Output = e.Message;
                   }
               });
            }
            catch (Exception e)
            {
                Output = e.Message;
            }
        });

        public ICommand TestingPrint => new RelayCommand<object>(obj =>
        {
            try
            {
                printerInfo.TestPrinteing(SelectedInstalledPrinter);
            }
            catch (Exception e)
            {
                Output = e.Message;
            }
        });

        public ICommand RefreshPrintersList => new RelayCommand<object>(async obj =>
        {
            var temp = await _api.GetPrinters();

            Printers = new ObservableCollection<Printer>(temp.OrderBy(x => x.PrinterName));

            InstaledPrintersList = new ObservableCollection<string>(new PrintersInfo().InstalledPrinters());
        });

        public MainViewModel(List<Printer> printers)
        {
            _api = new Use_Install_Printers_Api();

            Printers = new ObservableCollection<Printer>(printers.OrderBy(x => x.PrinterName));

            InstaledPrintersList = new ObservableCollection<string>(new PrintersInfo().InstalledPrinters());

            printerInfo = new PrintersInfo();
        }

        public MainViewModel()
        {

        }

        private async Task GetInstallPrinterInfo()
        {
            foreach (var p in Printers)
            {
                if (p.PrinterName == SelectedPrinter)
                {
                    _printerDriver = await _api.GetPrinterDriver(p.Id);

                    break;
                }
            }
        }
    }
}
