using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SemantiCore.Interfaces
{
    public class GlobalWindow : Window
    {
        public GlobalWindow() : base()
        {
                
        }

        public bool IsShowing { get; private set; }
        protected override void OnClosed(EventArgs e)
        {
            IsShowing = false;
            base.OnClosed(e);
        }
        public new void Hide()
        {
            IsShowing = false;
            base.Hide(); 
        }
        public new void Show()
        {
            IsShowing = true;
            base.Show();
        } 
    }
}
