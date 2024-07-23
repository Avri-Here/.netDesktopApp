using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ImageConverterApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public List<string> FilterImageFiles(string[] files)
        {
            string[] allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff" };
            return files.Where(file => allowedExtensions.Contains(Path.GetExtension(file).ToLower())).ToList();
        }

        private async void Window_Drop(object sender, DragEventArgs e)
        {
            StatusTextBlock.Text = "Drop event triggered";
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] allFilesFromDragEvent = (string[])e.Data.GetData(DataFormats.FileDrop);
                StatusTextBlock.Text = $"All Files dropped: {string.Join(", ", allFilesFromDragEvent)}";
                List<string> imageFiles = FilterImageFiles(allFilesFromDragEvent);

                if (imageFiles.Count > 0)
                {
                    ConversionProgressBar.Maximum = imageFiles.Count;
                    ConversionProgressBar.Value = 0;

                    foreach (string file in imageFiles)
                    {
                        StatusTextBlock.Text = $"Converting {file} to JPG...";
                        await ConvertToJpg(file);
                        ConversionProgressBar.Value += 1;
                    }

                    StatusTextBlock.Text = "Conversion finished for all files!";
                }
                else
                {
                    StatusTextBlock.Text = "No valid image files found in the drop event.";
                }
            }
        }

        private Task ConvertToJpg(string inputFile)
        {
            return Task.Run(() =>
            {
                string outputDirectory = Path.Combine(Path.GetDirectoryName(inputFile), "output");
                Directory.CreateDirectory(outputDirectory); // Ensure the output directory exists

                string outputFile = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(inputFile) + ".jpg");
                string arguments = $"-i \"{inputFile}\" \"{outputFile}\"";

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process())
                {
                    process.StartInfo = startInfo;
                    process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
                    process.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                }

                Console.WriteLine($"Conversion finished for {inputFile}!");
            });
        }
    }
}
