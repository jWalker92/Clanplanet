using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogDataDecompressor
{
    public partial class Form1 : Form
    {
        string decompText;

        public Form1()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                decompText = DecompressString(textBox1.Text);
                if (!string.IsNullOrWhiteSpace(decompText))
                {
                    var list = JsonConvert.DeserializeObject<List<ErrorDataPackage>>(decompText);
                    foreach (var item in list)
                    {
                        listBox1.Items.Add(item);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public static string DecompressString(string compressedText)
        {
            byte[] gZipBuffer = Convert.FromBase64String(compressedText);
            using (var memoryStream = new MemoryStream())
            {
                int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
                memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

                var buffer = new byte[dataLength];

                memoryStream.Position = 0;
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            webBrowser1.DocumentText = ((ErrorDataPackage)listBox1.SelectedItem).ErrorData;
        }
    }

    public class ErrorDataPackage
    {
        public string ErrorData;
        public string Origin;
        public DateTime TimeStamp;

        public string ListDisplay
        {
            get { return TimeStamp.ToString("dd.MM. HH.mm.ss - ") + Origin; }
        }
    }
}
