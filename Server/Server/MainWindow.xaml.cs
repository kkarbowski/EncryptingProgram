﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Security;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

		private static int iterations = 2;
		private static int keySize = 256;

		private static string hash = "SHA1";
		private static string salt = "aselrias38490a32"; // Random
		private static string vector = "8947az34awl34kjq"; // Random

//endregion

		public static string Encrypt(string value, string password)
		{
			return Encrypt<AesManaged>(value, password);
		}

		public static string Encrypt<T>(string value, string password)
				where T : SymmetricAlgorithm, new()
		{
			byte[] vectorBytes = Encoding.ASCII.GetBytes(vector);
			byte[] saltBytes = Encoding.ASCII.GetBytes(salt);
			byte[] valueBytes = Encoding.UTF8.GetBytes(value);


			
			byte[] encrypted;

			using (T cipher = new T())
			{
				PasswordDeriveBytes _passwordBytes =
					new PasswordDeriveBytes(password, saltBytes, hash, Siterations);
				byte[] keyBytes = _passwordBytes.GetBytes(keySize / 8);

				cipher.Mode = CipherMode.CBC;

				using (ICryptoTransform encryptor = cipher.CreateEncryptor(keyBytes, vectorBytes))
				{
					using (MemoryStream to = new MemoryStream())
					{
						using (CryptoStream writer = new CryptoStream(to, encryptor, CryptoStreamMode.Write))
						{
							writer.Write(valueBytes, 0, valueBytes.Length);
							writer.FlushFinalBlock();
							encrypted = to.ToArray();
						}
					}
				}
				cipher.Clear();
			}
			return Convert.ToBase64String(encrypted);
		}

		private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".png";
            dlg.Filter = "png Files (*.png)|*.png|mp3 Files (*.mp3)|*.mp3|avi Files (*.avi)|*.avi|txt Files (*.txt)|*.txt";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                selectedFileTextBox.Text = filename;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            int BufferSize = 1024, tries = 0;
            byte[] SendingBuffer = null;
            TcpClient client = null;
            NetworkStream netstream = null;
            try
            {
                while (tries++ < 100) {
                    try
                    {
                        client = new TcpClient("127.0.0.1", 5000);
                        netstream = client.GetStream();
                        break;
                    }
                    catch (Exception e1) { }
                }
                string extension = System.IO.Path.GetExtension(selectedFileTextBox.Text);
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(filenameTextBox.Text+extension);
                netstream.Write(bytesToSend, 0, bytesToSend.Length);

                while (netstream.ReadByte() != 'O') { };
                
                FileStream Fs = new FileStream(selectedFileTextBox.Text, FileMode.Open, FileAccess.Read);
                int NoOfPackets = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Fs.Length) / Convert.ToDouble(BufferSize)));
                progressBar.Maximum = NoOfPackets;
                int TotalLength = (int)Fs.Length;
                int CurrentPacketLength;

                netstream.WriteTimeout = 5000;
                for (int i = 0; i < NoOfPackets; i++)
                {
                    if (TotalLength > BufferSize)
                    {
                        CurrentPacketLength = BufferSize;
                        TotalLength = TotalLength - CurrentPacketLength;
                    }
                    else
                        CurrentPacketLength = TotalLength;

                    SendingBuffer = new byte[CurrentPacketLength];
                    Fs.Read(SendingBuffer, 0, CurrentPacketLength);
                    netstream.Write(SendingBuffer, 0, (int)SendingBuffer.Length);

                    if (progressBar.Value >= progressBar.Maximum)
                        progressBar.Value = progressBar.Minimum;
                    progressBar.Value++;
                    progressBar.UpdateLayout();
                }
                netstream.Flush();
                while (netstream.ReadByte() != 'O') { }

                Fs.Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error");
                //Console.WriteLine(ex.Message);
            }
            finally
            {
                netstream.Close();
                client.Close();
            }
        }


        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
