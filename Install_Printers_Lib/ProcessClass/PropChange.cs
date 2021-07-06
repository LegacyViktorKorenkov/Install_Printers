using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Install_Printers_Lib.ProcessClass
{
    public class PropChange : INotifyPropertyChanged
    {
        /// <summary>
        /// Событие изменения значения свойства 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Обработка изменения знаяения свойства
        /// </summary>
        /// <param name="prop"></param>
        public void OnPropertyChange([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
