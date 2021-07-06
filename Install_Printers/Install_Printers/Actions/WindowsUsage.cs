using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Install_Printers.Actions
{
    class WindowsUsage
    {
        /// <summary>
        /// Метод закрытия окон
        /// </summary>
        /// <param name="WindowName"></param>
        public static void CloseWindow(string WindowName)
        {
            // Закрытие окна ProgBar
            foreach (Window t in Application.Current.Windows)
            {
                if (t.Name.ToString() == $"{WindowName}")
                {
                    t.Close();
                }
            }
        }
    }
}
