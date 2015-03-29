using Medo.Net;
using System;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace Attache {
    internal partial class MainForm : Form {
        public MainForm() {
            InitializeComponent();
            Medo.Windows.Forms.State.SetupOnLoadAndClose(this, list);

            this.Tiny.Listen();
            this.Tiny.PacketReceived += Tiny_PacketReceived;
        }

        readonly TinyMessage Tiny = new TinyMessage() { ProductFilter = "Attache" };

        void Tiny_PacketReceived(object sender, TinyPacketEventArgs e) {
            var packet = e.Packet;
            this.Invoke((Action)delegate() {
                var hostName = packet[".Host"];
                var group = NewGroup(hostName);

                switch (packet.Operation) {
                    case "Beat":
                        UpdateItem(group, "IPv4", (packet["IPv4"] ?? "").Replace(" ", "   "));
                        UpdateItem(group, "IPv6", (packet["IPv6"] ?? "").Replace(" ", "   "));
                        UpdateItem(group, "IPv6 (local)", (packet["IPv6_Local"] ?? "").Replace(" ", "   "));

                        long uptime;
                        if (long.TryParse(packet["Uptime"], NumberStyles.Integer, CultureInfo.InvariantCulture, out uptime)) {
                            if (uptime < 60 * 60) {
                                UpdateItem(group, "Last boot", string.Format("more than {0} minutes ago", uptime / 60));
                            } else if (uptime < 60 * 60 * 24) {
                                UpdateItem(group, "Last boot", string.Format("more than {0} hours ago", uptime / 60 / 60));
                            } else {
                                UpdateItem(group, "Last boot", string.Format("more than {0} days ago", uptime / 60 / 60 / 24));
                            }
                        } else {
                            UpdateItem(group, "Last boot", "");
                        }
                        break;

                    case "VolumeInfo":
                        foreach (var item in packet) {
                            if (item.Key.Length == 1) {
                                var letter = item.Key;
                                var sb = new StringBuilder();
                                sb.Append(item.Value);

                                long size;
                                if (long.TryParse(packet[letter + "_Size"], NumberStyles.Integer, CultureInfo.InvariantCulture, out size)) {
                                    sb.Append(" ");
                                    long free;
                                    if (long.TryParse(packet[letter + "_Free"], NumberStyles.Integer, CultureInfo.InvariantCulture, out free)) {
                                        sb.AppendFormat("({1:0} GB available, {0:0} GB total)", size / 1024.0 / 1024 / 1024, free / 1024.0 / 1024 / 1024);
                                    } else {
                                        sb.AppendFormat("({0:0} GB total)", size / 1024.0 / 1024 / 1024);
                                    }
                                }

                                UpdateItem(group, "Volume " + letter + ":", sb.ToString());
                            }
                        }
                        break;

                    default: break;
                }
            });
        }

        private void MainForm_Load(object sender, EventArgs e) {

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            this.Tiny.Close();
        }


        private ListViewGroup NewGroup(string hostName) {
            foreach (ListViewGroup iGroup in list.Groups) {
                if (string.Equals(iGroup.Tag.ToString(), hostName, StringComparison.Ordinal)) {
                    return iGroup;
                }
            }

            var group = new ListViewGroup(hostName) { Tag = hostName };
            list.Groups.Add(group);
            return group;
        }

        private void UpdateItem(ListViewGroup group, string name, string value) {
            var key = group.Tag.ToString() + ":" + name;

            ListViewItem item = null;
            foreach (ListViewItem iItem in list.Items) {
                if (string.Equals(iItem.Tag.ToString(), key, StringComparison.Ordinal)) {
                    item = iItem;
                    break;
                }
            }
            if (item == null) {
                item = new ListViewItem(name) { Tag = key, Group = group };
                item.SubItems.Add("");
                list.Items.Add(item);
            }

            if (!string.Equals(item.SubItems[1].Text, value, StringComparison.Ordinal)) {
                item.SubItems[1].Text = value;
            }
        }

    }
}
