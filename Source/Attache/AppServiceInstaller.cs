using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace Attache {

    [RunInstaller(true)]
    public class AppServiceInstaller : Installer {

        private readonly ServiceProcessInstaller serviceProcessInstaller;
        private readonly ServiceInstaller serviceInstaller;

        public AppServiceInstaller() {
            serviceProcessInstaller = new ServiceProcessInstaller();
            serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            serviceProcessInstaller.Username = null;
            serviceProcessInstaller.Password = null;
            this.Installers.Add(serviceProcessInstaller);

            serviceInstaller = new ServiceInstaller();
            serviceInstaller.ServiceName = AppService.Instance.ServiceName;
            serviceInstaller.DisplayName = Medo.Reflection.CallingAssembly.Title;
            serviceInstaller.Description = Medo.Reflection.CallingAssembly.Description;
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            this.Installers.Add(serviceInstaller);
        }


        protected override void OnCommitted(IDictionary savedState) {
            base.OnCommitted(savedState);
            using (ServiceController sc = new ServiceController(AppService.Instance.ServiceName)) {
                sc.Start();
            }
        }

        protected override void OnBeforeUninstall(IDictionary savedState) {
            using (ServiceController sc = new ServiceController(AppService.Instance.ServiceName)) {
                if (sc.Status != ServiceControllerStatus.Stopped) {
                    sc.Stop();
                }
            }
            base.OnBeforeUninstall(savedState);
        }


        #region Dispose

        protected override void Dispose(bool disposing) {
            if (disposing ) {
                this.serviceProcessInstaller.Dispose();
                this.serviceInstaller.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

    }
}
