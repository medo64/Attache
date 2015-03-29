using System;
using System.Windows.Forms;

namespace Attache {
    internal static class Tray {

        private static NotifyIcon Notify;
        private static Form Form;

        internal static void Show() {
            Tray.Notify = new NotifyIcon();
            Tray.Notify.ContextMenu = new ContextMenu();
            Tray.Notify.ContextMenu.MenuItems.Add(new MenuItem("Exit", Tray_Exit_OnClick));
            Tray.Notify.Icon = Attache.Properties.Resources.TrayIcon;
            Tray.Notify.Text = "Attaché";
            Tray.Notify.Visible = true;

            Tray.Notify.MouseDoubleClick += delegate(object sender, MouseEventArgs e) {
                if ((Tray.Form == null) || Tray.Form.IsDisposed ) {
                    Tray.Form = new MainForm();
                }
                Tray.Form.Show();
                Tray.Form.Activate();
            };
        }


        internal static void Hide() {
            Tray.Notify.Visible = false;
        }

        internal static bool IsVisible {
            get { return (Tray.Notify != null) && (Tray.Notify.Visible); }
        }


        private static void Tray_Exit_OnClick(object sender, EventArgs e) {
            Application.Exit();
        }

    }
}
