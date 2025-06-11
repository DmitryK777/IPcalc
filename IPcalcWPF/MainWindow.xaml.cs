using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows;

namespace IPCalculator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Calculate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ip = IPAddress.Parse(IpAddressTextBox.Text);
                string maskInput = SubnetMaskTextBox.Text;

                IPAddress subnetMask;
                if (maskInput.StartsWith("/"))
                {
                    int cidr = int.Parse(maskInput.TrimStart('/'));
                    subnetMask = CidrToMask(cidr);
                }
                else
                {
                    subnetMask = IPAddress.Parse(maskInput);
                }

                byte[] ipBytes = ip.GetAddressBytes();
                byte[] maskBytes = subnetMask.GetAddressBytes();

                byte[] networkBytes = new byte[4];
                byte[] broadcastBytes = new byte[4];

                for (int i = 0; i < 4; i++)
                {
                    networkBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
                    broadcastBytes[i] = (byte)(ipBytes[i] | (~maskBytes[i]));
                }

                var network = new IPAddress(networkBytes);
                var broadcast = new IPAddress(broadcastBytes);

                int hostBits = 32 - maskBytes.Sum(b => CountBits(b));
                int hostCount = (hostBits == 0) ? 0 : (int)Math.Pow(2, hostBits) - 2;

                var firstHost = hostCount > 0 ? new IPAddress(IncrementAddress(networkBytes)) : network;
                var lastHost = hostCount > 0 ? new IPAddress(DecrementAddress(broadcastBytes)) : broadcast;

                NetworkAddressText.Text = $"Сеть: {network}";
                BroadcastAddressText.Text = $"Broadcast: {broadcast}";
                HostRangeText.Text = $"Диапазон хостов: {firstHost} - {lastHost}";
                HostCountText.Text = $"Количество хостов: {hostCount}";
                IpClassText.Text = $"Класс IP: {GetIpClass(ip)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private IPAddress CidrToMask(int cidr)
        {
            uint mask = cidr == 0 ? 0 : uint.MaxValue << (32 - cidr);
            byte[] bytes = BitConverter.GetBytes(mask);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return new IPAddress(bytes);
        }

        private int CountBits(byte b)
        {
            int count = 0;
            while (b != 0)
            {
                count += b & 1;
                b >>= 1;
            }
            return count;
        }

        private byte[] IncrementAddress(byte[] addr)
        {
            byte[] result = (byte[])addr.Clone();
            for (int i = 3; i >= 0; i--)
            {
                if (result[i] < 255)
                {
                    result[i]++;
                    break;
                }
                result[i] = 0;
            }
            return result;
        }

        private byte[] DecrementAddress(byte[] addr)
        {
            byte[] result = (byte[])addr.Clone();
            for (int i = 3; i >= 0; i--)
            {
                if (result[i] > 0)
                {
                    result[i]--;
                    break;
                }
                result[i] = 255;
            }
            return result;
        }

        private string GetIpClass(IPAddress ip)
        {
            byte firstOctet = ip.GetAddressBytes()[0];
            if (firstOctet < 128) return "A";
            else if (firstOctet < 192) return "B";
            else if (firstOctet < 224) return "C";
            else if (firstOctet < 240) return "D (Multicast)";
            else return "E (Experimental)";
        }
    }
}