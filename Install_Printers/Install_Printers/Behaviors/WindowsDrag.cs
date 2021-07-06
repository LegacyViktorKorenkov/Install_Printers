using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Install_Printers.Behaviors
{
    class WindowsDrag : Behavior<Window>
    {
        public Window window;

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.MouseLeftButtonDown += MouseDragMove;
        }

        private void MouseDragMove(object sender, MouseButtonEventArgs e)
        {
            if (sender is Window)
            {
                window = sender as Window;
            }

            window.DragMove();
        }
    }
}
