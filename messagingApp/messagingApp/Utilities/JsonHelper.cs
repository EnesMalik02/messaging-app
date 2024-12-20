using System;
using System.Windows.Forms;

namespace messagingApp.Utilities
{
    public static class JsonHelper
    {
        public static void InvokeIfRequired(this Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new Action(() => action()));
            }
            else
            {
                action();
            }
        }

        // Additional helper methods can be added here
    }
}
