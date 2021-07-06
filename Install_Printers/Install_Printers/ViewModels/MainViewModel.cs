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
            set
            {
                Task.Run(async () =>
                {
                    selectedPrinter = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedPrinter)));
                    await GetInstallPrinterInfo();
                    if (printerDriver != null && printerDriver.NetPrinter) NetworkPrinter = true;
                    else NetworkPrinter = false;
                });
            }
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

        private bool isStartProgressBar;
        public bool IsStartProgressBar
        {
            get => isStartProgressBar;
            set { isStartProgressBar = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsStartProgressBar))); }
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

            if (NetworkPrinter)
            {
                if (!string.IsNullOrWhiteSpace(PrinterNetName) && await PrinterNetName.CheckLanStatus())
                {
                    if (printerDriver.NetPrinter)
                    {
                        try
                        {
                            Output = "Установка.";
                            IsStartProgressBar = true;
                            install = new InstallDrivers(printerDriver, PrinterNetName, new Unzip($@"Temp\{SelectedPrinter}"));
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
                    install = new InstallDrivers(printerDriver, new Unzip($@"Temp\{SelectedPrinter}"));
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
            var temp = await api.GetPrinters();

            Printers = new ObservableCollection<Printer>(temp.OrderBy(x => x.PrinterName));

            InstaledPrintersList = new ObservableCollection<string>(new PrintersInfo().InstalledPrinters());
        });

        public MainViewModel(List<Printer> printers)
        {
            api = new Use_Install_Printers_Api();

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
                    printerDriver = await api.GetPrinterDriver(p.Id);

                    break;
                }
            }
        }
    }
}
