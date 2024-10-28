using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace MultiUserSimulator
{
    public partial class MainWindow : Window
    {
        private TcpClient client;
        private TcpListener listener;
        private StreamWriter writer;
        private StreamReader reader;
        private string currentUserName;
        private string serverIpAddress;
        private bool isServer;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            serverIpAddress = IpAddressTextBox.Text;
            currentUserName = UserNameTextBox.Text;

            if (!string.IsNullOrWhiteSpace(serverIpAddress) && !string.IsNullOrWhiteSpace(currentUserName))
            {
                try
                {
                    // Запуск сервера
                    if (!isServer)
                    {
                        listener = new TcpListener(IPAddress.Any, 8888);
                        listener.Start();
                        Thread listenerThread = new Thread(ListenForClients);
                        listenerThread.Start();
                        isServer = true;
                    }

                    // Подключение клиента
                    client = new TcpClient(serverIpAddress, 8888);
                    NetworkStream stream = client.GetStream();
                    writer = new StreamWriter(stream) { AutoFlush = true };
                    reader = new StreamReader(stream);
                    ChatLog.Items.Add($"{currentUserName} подключился к серверу.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка подключения: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Введите корректный IP-адрес и имя пользователя.");
            }
        }

        private void ListenForClients()
        {
            while (true)
            {
                var client = listener.AcceptTcpClient();
                var stream = client.GetStream();
                var reader = new StreamReader(stream);
                while (true)
                {
                    string message = reader.ReadLine();
                    if (message != null)
                    {
                        Dispatcher.Invoke(() => ChatLog.Items.Add(message));
                    }
                }
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageTextBox.Text;
            if (!string.IsNullOrWhiteSpace(message) && writer != null)
            {
                writer.WriteLine($"{DateTime.Now}: {currentUserName}: {message}");
                MessageTextBox.Clear();
            }
            else
            {
                MessageBox.Show("Введите сообщение.");
            }
        }
    }
}
