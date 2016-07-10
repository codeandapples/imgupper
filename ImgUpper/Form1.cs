using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Security;
using System.Xml.Linq;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace ImageUpper
{
    public partial class ImageUps : Form
    {

        Auth auth = new Auth();


        public ImageUps()
        {
            InitializeComponent();
        }


        private void InitializeOpenFileDialog()
        {
            // Open file dialog and filter for images
            this.openFileDialog1.Filter =
                "Images (*.PNG;*.JPG;*.GIF;*.BMP)|*.PNG;*.JPG;*.GIF;*.BMP|" +
                "All files (*.*)|*.*";

            // Multiple image selection
            this.openFileDialog1.Multiselect = true;
            this.openFileDialog1.Title = "Select Images";
        }


        private void button2_Click(object sender, EventArgs e)
        {
            // Select images to upload

            InitializeOpenFileDialog();

            DialogResult dr = this.openFileDialog1.ShowDialog();

            StringBuilder sd = new StringBuilder();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                // Read the files
                foreach (String file in openFileDialog1.FileNames)
                {
                    try
                    {
                        // Get the full path of the file and put it in the first listbox
                        listBox1.Items.Add(Path.GetFullPath(file));
                    }

                    catch (SecurityException ex)
                    {
                        MessageBox.Show("Security error.\n\n" +
                            "Error message: " + ex.Message + "\n\n" +
                            "Details (send to Support):\n\n" + ex.StackTrace);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Cannot display the image: " + file.Substring(file.LastIndexOf('\\'))
                            + ". You may not have permission to read the file, or " +
                            "it may be corrupt.\n\nReported error: " + ex.Message);
                    }
                }
            }

        }


        private void button1_Click(object sender, EventArgs e)
        {

            // If the first listbox doesn't contain anything display an error message
            if (listBox1.Items.Count == 0)
            {
                MessageBox.Show("Please select an image or images first.", "Error - No Images");
            }

            else
            {
                // Counter that will be used for the progress bar
                int counterprogressbar = 100 / listBox1.Items.Count;

                // reset the progress bar to 0 each time the Upload button is clicked on
                this.progressBar1.Value = 0;

                foreach (string m in listBox1.Items)
                {

                    // Add the original filename without the path to the Original File Name box
                    listBox3.Items.Add(Path.GetFileName(m).ToString());

                    System.Threading.Thread.Sleep(2000);

                    // Upload the images
                    using (var w = new WebClient())
                    {
                        // Add headers to authorize using the client ID
                        w.Headers.Add("Authorization", "Client-ID " + auth.myclientid);

                        // Convert the image to a base64 string and save in a collection along with calling "image"
                        var values = new NameValueCollection
                        {
                            {
                                "image", Convert.ToBase64String(File.ReadAllBytes(m))
                            }
                        };

                        byte[] response = w.UploadValues("https://api.imgur.com/3/upload.xml", values);

                        var responseback = XDocument.Load(new MemoryStream(response));

                        // Regex to parse for anything between <link> and </link> to get the URL of the uploaded image
                        Regex regexlink = new Regex("<link>(.*)</link>");

                        // Regex to parse for height and width of the image
                        Regex regexwidth = new Regex("<width>(.*)</width>");
                        Regex regexheight = new Regex("<height>(.*)</height>");

                        // Parse responseback with the regex
                        string urlimage = regexlink.Match(responseback.ToString()).Groups[1].ToString();

                        // Add the url for each image uploaded to the listbox. It'll do this in order
                        // to correspond with the image and URL as it does it one by one for the loop
                        listBox2.Items.Add(urlimage);

                        string urlimagewidth = regexwidth.Match(responseback.ToString()).Groups[1].ToString();
                        string urlimageheight = regexheight.Match(responseback.ToString()).Groups[1].ToString();

                        // Add the width and height of the image to the Dimensions box
                        listBox4.Items.Add(urlimagewidth + " x " + urlimageheight);

                        /* Increment the progress bar by however many are in the first listbox divided by 100,
                           increment by this value each time the for loop runs */
                        this.progressBar1.Increment(counterprogressbar);

                    }
                }
            }

        }


        private void button6_Click(object sender, EventArgs e)
        {
            // Clear items in the Selected Images box

            listBox1.Items.Clear();
        }


        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
        }


        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();
            StringBuilder sbu = new StringBuilder();

            foreach (string m in listBox1.SelectedItems)
            {
                sbu.Append(m.ToString());
                sbu.Append(Environment.NewLine);
            }

            if (string.IsNullOrEmpty(sbu.ToString()))
            {
                MessageBox.Show("Please select something to copy first.", "Error");
            }
            else if (sbu != null)
            {
                Clipboard.SetText(sbu.ToString());
            }
        }


        private void contextMenuStripListBox3_Opening(object sender, CancelEventArgs e)
        {

        }


        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Remove items by right-clicking items in the Selected Images box and clicking Remove
            for (int i = listBox1.SelectedIndices.Count - 1; i >= 0; i--)
            {
                listBox1.Items.RemoveAt(listBox1.SelectedIndices[i]);
            }
        }


        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Copy items from the Uploaded Images section

            Clipboard.Clear();

            StringBuilder sbu = new StringBuilder();

            foreach (string m in listBox2.SelectedItems)
            {
                sbu.Append(m.ToString());
                sbu.Append(Environment.NewLine);
            }

            if (string.IsNullOrEmpty(sbu.ToString()))
            {
                MessageBox.Show("Please select something to copy first.", "Error");
            }
            else if (sbu != null)
            {
                Clipboard.SetText(sbu.ToString());
            }
        }


        private void removeToolStripMenuItem1_Click(object sender, EventArgs e)
        {

            // Remove items from the Uploaded Images box by right-clicking and choosing Remove

            for (int i = listBox1.SelectedIndices.Count - 1; i >= 0; i--)
            {
                listBox2.Items.RemoveAt(listBox2.SelectedIndices[i]);
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            // Copy all links

            Clipboard.Clear();

            StringBuilder sbu = new StringBuilder();

            foreach (string m in listBox2.Items)
            {
                sbu.Append(m.ToString());
                sbu.Append(Environment.NewLine);
            }

            if (string.IsNullOrEmpty(sbu.ToString()))
            {
                MessageBox.Show("Please upload something first.", "Error");
            }
            else if (sbu != null)
            {
                Clipboard.SetText(sbu.ToString());
            }
        }


        private void BtnClearAllLinks_Click(object sender, EventArgs e)
        {
            // Clear all links button

            listBox2.Items.Clear();
            listBox3.Items.Clear();
            listBox4.Items.Clear();
        }


        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // From the menu: File -> Exit

            Application.Exit();
        }

        private void clearAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // From the menu: Edit -> Clear All

            listBox1.Items.Clear();
            listBox2.Items.Clear();
            listBox3.Items.Clear();
            listBox4.Items.Clear();
        }


        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // From the menu: Help -> About

            MessageBox.Show("ImgUpper v0.1a.\n\nCreated by codeandapples on GitHub.", "About");

        }


        private void openLinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Open uploaded images in a browser from the right-click menu

            System.Diagnostics.Process.Start(listBox2.SelectedItem.ToString());
        }


        private void BtnExportAllLinksToTextFile_Click(object sender, EventArgs e)
        {
            // Export all uploaded images to a text file
            
            if (listBox2.Items.Count == 0)
            {
                MessageBox.Show("Please upload something first.");
            }


            else
            {
                SaveFileDialog save = new SaveFileDialog();
                save.FileName = "list_of_selected_links.txt";
                save.Filter = "Text File | *.txt";

                if (save.ShowDialog() == DialogResult.OK)
                {
                    StreamWriter writer = new StreamWriter(save.OpenFile());
                    for (int i = 0; i < listBox2.SelectedItems.Count; i++)
                    {
                        writer.WriteLine(listBox2.SelectedItems[i].ToString());
                    }
                    writer.Dispose();
                    writer.Close();
                }
            }
        }


        private void button3_Click_1(object sender, EventArgs e)
        {
            // Copy all URLs for images uploaded

            if (listBox2.Items.Count == 0)
            {
                MessageBox.Show("Please upload something first.");
            }


            else
            {
                SaveFileDialog save = new SaveFileDialog();
                save.FileName = "list_of_all_links.txt";
                save.Filter = "Text File | *.txt";

                if (save.ShowDialog() == DialogResult.OK)
                {
                    StreamWriter writer = new StreamWriter(save.OpenFile());
                    for (int i = 0; i < listBox2.Items.Count; i++)
                    {
                        writer.WriteLine(listBox3.Items[i].ToString() + "\t" + listBox2.Items[i].ToString());
                    }
                    writer.Dispose();
                    writer.Close();
                }
            }
        }


        private void ImageUps_KeyDown(object sender, KeyEventArgs e)
        {
            // Keyboard shorcuts:

            // Open file dialog to choose images when pressing CTRL + O
            if (e.Control && e.KeyCode.ToString() == "O")
            {
                button2.PerformClick();
            }

            // Upload the images that have been selected using CTRL + U
            if (e.Control && e.KeyCode.ToString() == "U")
            {
                button1.PerformClick();
            }

            // Copy all links for images that were uploaded using CTRL + C
            if (e.Control && e.KeyCode.ToString() == "C")
            {
                BtnCopyAllLinks.PerformClick();
            }

            // Clear everything from the boxes using CTRL + D
            if (e.Control && e.KeyCode.ToString() == "D")
            {
                listBox1.Items.Clear();
                listBox2.Items.Clear();
                listBox3.Items.Clear();
                listBox4.Items.Clear();
            }
        }


        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            // Drag-and-drop images onto the Selected Images box

            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                e.Effect = DragDropEffects.All;
            }

        }


        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {
            string[] droppedfiles = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (string file in droppedfiles)
            {
                listBox1.Items.Add(Path.GetFullPath(file));
            }
        }


        private string getFileName(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }


    }
}