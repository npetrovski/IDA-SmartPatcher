using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace IDASmartPatcher
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(Form1_DragEnter);
            this.DragDrop += new DragEventHandler(Form1_DragDrop);
        }

        void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                e.Effect = DragDropEffects.Copy;
            }
        }

        void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                if (file.ToLower().EndsWith("dif"))
                {
                    PatchBox.Text = file;
                }
                else
                {
                    TargteBox.Text = file;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "Open IDA DIF file";
            fdlg.InitialDirectory = @"c:\";
            fdlg.Filter = "Dif files (*.dif)|*.dif";
            fdlg.FilterIndex = 2;
            fdlg.RestoreDirectory = true;
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                PatchBox.Text = fdlg.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "Open target file";
            fdlg.InitialDirectory = @"c:\";
            fdlg.Filter = "All files (*.*)|*.*";
            fdlg.FilterIndex = 2;
            fdlg.RestoreDirectory = true;
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                TargteBox.Text = fdlg.FileName;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            descr.Text = "Apply IDA patch (.dif) into a target file. Just select your" + Environment.NewLine + "patch file and a target and hit \"Patch\" button.";
            this.TopMost = checkBox1.Checked;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = checkBox1.Checked;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (PatchBox.Text == String.Empty || TargteBox.Text == String.Empty || !File.Exists(PatchBox.Text) || !File.Exists(TargteBox.Text))
            {
                MessageBox.Show("Invalid target file or patch.", "Error !!!");
                return;
            }

            if (!patchFile(false))
            {
                DialogResult questionYN = MessageBox.Show("Target file does not match the expected byte sequence. Are you sure you want to patch?", "Warning !!!", MessageBoxButtons.YesNo);
                if (questionYN == DialogResult.No)
                {
                    return;
                }
            }

            // Apply the patch
            if (patchFile(true))
            {
                MessageBox.Show("Target file has been successfully patched.", "Congratulations !!!");
            }
        }

        protected bool patchFile(bool apply = false)
        {

            string line = "";
            ScanFormatted scan = new ScanFormatted();

            BinaryReader targetfile = new BinaryReader(File.Open(TargteBox.Text, FileMode.Open));
            StreamReader patchfile = new System.IO.StreamReader(PatchBox.Text);
            try
            {


                while ((line = patchfile.ReadLine()) != null)
                {
                    scan.Parse((string)line, "%x: %x %x");
                    if (3 == scan.Results.Count)
                    {
                        targetfile.BaseStream.Seek(Convert.ToInt64(scan.Results[0]), SeekOrigin.Begin);

                        int currentByte = targetfile.BaseStream.ReadByte();

                        if (apply)
                        {
                            targetfile.BaseStream.WriteByte(Convert.ToByte(scan.Results[2]));
                        } else if (currentByte != (int) Convert.ToByte(scan.Results[1])) {
                            patchfile.Close();
                            targetfile.Close();
                            return false;
                        }

                    }
                }


            }
            catch (IOException ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);

                return false;
            }
            finally
            {
                if (patchfile != null)
                    patchfile.Close();
                if (targetfile != null)
                    targetfile.Close();

                
            }

            return true;
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            if (PatchBox.Text == String.Empty || TargteBox.Text == String.Empty || !File.Exists(PatchBox.Text) || !File.Exists(TargteBox.Text))
            {
                MessageBox.Show("Invalid target file or patch.", "Error !!!");
                return;
            }

            if (patchFile(false))
            {
                MessageBox.Show("Target file contains the expected byte sequence. You are ready to patch.", "Congratulations !!!");
            }
            else
            {
                MessageBox.Show("Target file does not match the expected byte sequence.", "Warning !!!");
            }
        }
    }
}
