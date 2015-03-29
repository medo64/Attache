using Medo.Net;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Attache {
    internal static class AppServiceThread {

        private static Thread Thread;
        private static ManualResetEvent CancelEvent;

        public static void Start() {
            if (AppServiceThread.Thread != null) { return; }

            AppServiceThread.CancelEvent = new ManualResetEvent(false);
            AppServiceThread.Thread = new Thread(Run) {
                CurrentCulture = CultureInfo.InvariantCulture,
                IsBackground = true,
                Name = "Service"
            };
            AppServiceThread.Thread.Start();
        }

        public static void Stop() {
            if (AppServiceThread.CancelEvent != null) { //might be not running
                AppServiceThread.CancelEvent.Set();
                for (var i = 0; i < 100; i++) {
                    if (!AppServiceThread.Thread.IsAlive) { break; }
                    Thread.Sleep(10);
                }
                if (AppServiceThread.Thread.IsAlive) { AppServiceThread.Thread.Abort(); } //abort if still alive
                AppServiceThread.Thread = null;
            }
        }


        private static void Run(object parameter) {
            try {
                var swCollect = Stopwatch.StartNew();
                var swSend = Stopwatch.StartNew();

                using (var tiny = new Medo.Net.TinyMessage()) {
                    var beat = CreateBeatPacket();
                    var volumeInfo = CreateVolumeInfoPacket();

                    while (!AppServiceThread.CancelEvent.WaitOne(0, false)) {
                        if (swCollect.Elapsed.TotalSeconds >= Configuration.CollectionInterval) {
                            beat.Dispose();
                            volumeInfo.Dispose();
                            
                            beat = CreateBeatPacket();
                            volumeInfo = CreateVolumeInfoPacket();

                            swCollect.Restart();
                        }

                        if (swSend.Elapsed.TotalSeconds >= Configuration.MessageInterval) {
                            tiny.Send(beat);
                            tiny.Send(volumeInfo);

                            swSend.Restart();
                        }

                        Thread.Sleep(100);
                    }
                }
            } catch (ThreadAbortException) { }
        }


        private static TinyPacket CreateBeatPacket() {
            var utcTime = DateTime.UtcNow;
            var hostName = Dns.GetHostName();
            var hostEntry = Dns.GetHostEntry(hostName);

            var packet = new TinyPacket("Attaché", "Beat");
            packet.Add("Host", hostName);
            packet.Add("UtcTime", utcTime.ToString("O", CultureInfo.InvariantCulture));
            packet.Add("Time", utcTime.ToLocalTime().ToString("O", CultureInfo.InvariantCulture));
            packet.Add("Uptime", (Environment.TickCount / 1000).ToString("0", CultureInfo.InvariantCulture));

            var sbIPv4 = new StringBuilder();
            foreach (var address in hostEntry.AddressList) {
                if (address.AddressFamily != AddressFamily.InterNetwork) { continue; } //skip all not IPv4
                if (sbIPv4.Length > 0) { sbIPv4.Append(" "); }
                sbIPv4.Append(address.ToString());
            }
            packet.Add("IPv4", sbIPv4.ToString());

            var sbIPv6 = new StringBuilder();
            foreach (var address in hostEntry.AddressList) {
                if ((address.AddressFamily != AddressFamily.InterNetworkV6) || address.IsIPv6LinkLocal) { continue; } //skip all not IPv6 (global)
                if (sbIPv6.Length > 0) { sbIPv6.Append(" "); }
                sbIPv6.Append(address.ToString());
            }
            packet.Add("IPv6", sbIPv6.ToString());

            var sbIPv6Local = new StringBuilder();
            foreach (var address in hostEntry.AddressList) {
                if ((address.AddressFamily != AddressFamily.InterNetworkV6) || !address.IsIPv6LinkLocal) { continue; } //skip all not IPv6 (link local)
                if (sbIPv6Local.Length > 0) { sbIPv6Local.Append(" "); }
                sbIPv6Local.Append(address.ToString());
            }
            packet.Add("IPv6_Local", sbIPv6Local.ToString());

            return packet;
        }

        private static TinyPacket CreateVolumeInfoPacket() {
            var hostName = Dns.GetHostName();
            var utcTime = DateTime.UtcNow;

            var packet = new TinyPacket("Attaché", "VolumeInfo");
            packet.Add("Host", hostName);
            packet.Add("UtcTime", utcTime.ToString("O", CultureInfo.InvariantCulture));

            foreach (var drive in DriveInfo.GetDrives()) {
                var volumeLetter = drive.Name.Substring(0, 1);

                String volumeLabel = "";
                try {
                    volumeLabel = drive.VolumeLabel;
                } catch (IOException ex) {
                    volumeLabel = "{" + ex.Message + "}";
                }

                long volumeSize = -1;
                try {
                    volumeSize = drive.TotalSize;
                } catch (IOException) { }

                long volumeFree = -1;
                try {
                    volumeFree = drive.AvailableFreeSpace;
                } catch (IOException) { }

                String volumeFormat = null;
                try {
                    volumeFormat = drive.DriveFormat;
                } catch (IOException) { }

                packet.Add(volumeLetter, volumeLabel);
                if (volumeSize >= 0) { packet.Add(volumeLetter + "_Size", volumeSize.ToString("0" + CultureInfo.InvariantCulture)); }
                if (volumeSize >= 0) { packet.Add(volumeLetter + "_Free", volumeFree.ToString("0" + CultureInfo.InvariantCulture)); }
                if (volumeFormat != null) { packet.Add(volumeLetter + "_Format", volumeFormat); }
            }

            return packet;
        }

    }
}
