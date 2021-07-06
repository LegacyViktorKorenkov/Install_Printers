using Install_Printers_Lib.Actions;
using Install_Printers_Lib.Models;
using Install_Printers_WPF.Actions;
using Install_Printers_WPF.ProcessClass;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace Install_Printers_WPF.ViewModels
{
    class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        Use_Install_Printers_Api api;

        private Printer printerDriver;

        private string output;
        public string Output
        {
            get => output;
            set { output = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Output))); }
        }

        private string selectedPrinter;
        public string SelectedPrinter
        {
            get => selectedPrinter;
            set { selectedPrinter = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedPrinter))); GetInstallPrinterInfo(); if (printerDriver.NetPrinter) NetworkPrinter = true; }
        }

        private string printerNetName;
        public string PrinterNetName
        {
            get => printerNetName;
            set { printerNetName = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PrinterNetName))); }
        }

        private ObservableCollection<Printer> printers;
        public ObservableCollection<Printer> Printers
        {
            get => printers;
            set { printers = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Printers))); }
        }

        private bool networkPrinter;
        public bool NetworkPrinter
        {
            get => networkPrinter;
            set { networkPrinter = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NetworkPrinter))); }
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
            DownloadDriver download = new DownloadDriver(printerDriver);
            InstallDrivers install;

            Output = "Проверка введенных данных";

            if (NetworkPrinter)
            {
                if (!string.IsNullOrWhiteSpace(PrinterNetName) && await PrinterNetName.CheckLanStatus())
                {
                    if (printerDriver.NetPrinter)
                    {
                        try
                        {
                            Output = "Установка.";
                            install = new InstallDrivers(printerDriver, PrinterNetName, new Unzip($@"Temp\{SelectedPrinter}-driver"));
                            await install.Start();

                            InstaledPrintersList = new ObservableCollection<string>(new PrintersInfo().InstalledPrinters());
                            Output = "Установка завершена.";
                        }
                        catch (Exception e)
                        {
                            Output = e.Message;
                        }
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
                    install = new InstallDrivers(printerDriver, new Unzip($@"Temp\{SelectedPrinter}-driver"));
                    await install.Start();

                    InstaledPrintersList = new ObservableCollection<string>(new PrintersInfo().InstalledPrinters());
                    Output = "Установка завершена.";
                }
                catch (Exception e)
                {
                    Output = e.Message;
                }
            }
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
                printerInfo.DeletePrinter(SelectedInstalledPrinter);

                InstaledPrintersList = new ObservableCollection<string>(new PrintersInfo().InstalledPrinters());
            }
            catch (Exception e)
            {
                Output = e.Message;
            }
        });

        public ICommand TestingPrint => new RelayCommand<object>(obj =>
        {
            printerInfo.TestPrinteing(SelectedInstalledPrinter);
        });

        public MainViewModel()
        {
            api = new Use_Install_Printers_Api();

            Printers = new ObservableCollection<Printer>(api.GetPrinters());

            InstaledPrintersList = new ObservableCollection<string>(new PrintersInfo().InstalledPrinters());

            printerInfo = new PrintersInfo();
        }

        private void GetInstallPrinterInfo()
        {
            foreach (var p in Printers)
            {
                if (p.PrinterName == SelectedPrinter)
                {
                    printerDriver = api.GetPrinterDriver(p.Id);

                    break;
                }
            }
        }
    }
}
