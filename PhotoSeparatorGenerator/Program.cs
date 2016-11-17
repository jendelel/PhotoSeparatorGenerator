using System;
using System.Text;
using System.IO;
using System.Drawing;

namespace PhotoSeparatorGenerator
{
    class Program
    {

        static void Main(string[] args)
        {

            StreamWriter logger = new StreamWriter("log.txt");

            if (args.Length == 1)
            {
                if (!Directory.Exists(args[0]))
                {
                    Console.WriteLine("Invalid directory");
                    Environment.Exit(1);
                }
                using (var generator = new SeparatorGenerator("folderIcon.png", logger))
                {
                    generator.GenerateSeparatorsForDirectory(args[0]);
                }
            }
            else if (args.Length == 2 && args[1] == "-1")
            {
                if (!Directory.Exists(args[0]))
                {
                    Console.WriteLine("Invalid directory");
                    Environment.Exit(1);
                }
                DirectoryInfo di = new DirectoryInfo(args[0]);
                using (var generator = new SeparatorGenerator("folderIcon.png", logger))
                {
                    foreach (var dir in di.GetFileSystemInfos("*", SearchOption.TopDirectoryOnly))
                    {
                        if (dir is DirectoryInfo)
                        {
                            generator.GenerateSeparatorsForDirectory(dir.FullName);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"Usage: program.exe dirPath [-1].");
            }
        }
    }



    class SeparatorGenerator : IDisposable
    {

        readonly int WIDTH = 800;
        readonly int HEIGHT = 600;
        readonly Image FOLDER_ICON;
        StreamWriter logger;

        public SeparatorGenerator(string folderIconPath, StreamWriter logger)
        {
            FOLDER_ICON = Image.FromFile("folderIcon.png");
            this.logger = logger;
        }

        public void GenerateSeparatorsForDirectory(string parentDir)
        {
            DirectoryInfo di = new DirectoryInfo(parentDir);
            foreach (var file in di.GetFileSystemInfos("*", SearchOption.AllDirectories))
            {
                if (file is DirectoryInfo)
                {
                    if (((DirectoryInfo)file).GetFiles("*", SearchOption.TopDirectoryOnly).Length > 0)
                    {
                        string dirPath = file.FullName.Substring(di.FullName.Length + 1);
                        string savePath = Path.Combine(file.FullName, $"_{file.Name}_Separator.jpg");
                        GeneratePicture(savePath, dirPath.Split(Path.DirectorySeparatorChar));
                    }
                }
            }
        }

        public void GeneratePicture(string savePath, params string[] labels)
        {
            using (Bitmap b = new Bitmap(WIDTH, HEIGHT))
            {
                Graphics g = Graphics.FromImage(b);
                g.Clear(Color.Black);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Draw image to the top center
                g.DrawImage(FOLDER_ICON, (WIDTH / 2f) - 25, 60, 50, 50);

                // Draw the labels
                float topY = HEIGHT / 2f;
                for (int i = 0; i < labels.Length; i++)
                {

                    float fontSize = 22f;
                    if (i == labels.Length - 1) fontSize = 35f;

                    string label = labels[i] + "\\";
                    if (i == labels.Length - 1) label = labels[i];

                    Font f = new Font(new FontFamily("Arial"), fontSize);
                    var textSize = g.MeasureString(labels[i], f);
                    float topX = WIDTH / 2 - (textSize.Width / 2);
                    g.DrawString(label, f, Brushes.Red, topX, topY);
                    topY += textSize.Height + 7;
                }

                b.Save(savePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                Console.WriteLine(savePath);
                logger.WriteLine(savePath);
            }
        }

        public void Dispose()
        {
            FOLDER_ICON.Dispose();
        }
    }
}