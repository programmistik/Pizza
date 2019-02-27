using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HashCodePizza
{
    public class MainViewModel : ViewModelBase
    {
        private string pathToFile;
        public string PathToFile { get => pathToFile; set => Set(ref pathToFile, value); }

        private RelayCommand selectFileCommand;
        public RelayCommand SelectFileCommand
        {
            get => selectFileCommand ?? (selectFileCommand = new RelayCommand(
                () =>
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "IN files (*.in)|*.in";

                    if (openFileDialog.ShowDialog() == true)
                    {
                        PathToFile = openFileDialog.FileName;                        
                    }

                   
                }
            ));
        }

        private RelayCommand okCommand;
        public RelayCommand OkCommand
        {
            get => okCommand ?? (okCommand = new RelayCommand(
                () =>
                {
                    if (!string.IsNullOrEmpty(PathToFile))
                    {
                        var myPizza = LoadPizzaFile();

                        var Slices = myPizza.CutPizza();


                        var Score = Slices.Sum(item => item.GetSize());
                        MessageBox.Show($"Score: {Score}");

                        
                        using (StreamWriter sw = new StreamWriter(Path.GetDirectoryName(PathToFile)+"\\"+ Path.GetFileNameWithoutExtension(PathToFile) + ".out"))
                        {
                            sw.WriteLine(Slices.Count);
                            foreach (Slice slice in Slices)
                            {
                                sw.Write(slice.MinRow);
                                sw.Write(' ');
                                sw.Write(slice.MinCol);
                                sw.Write(' ');
                                sw.Write(slice.MaxRow);
                                sw.Write(' ');
                                sw.Write(slice.MaxCol);

                                sw.WriteLine();
                            }
                        }
                    }
                    else
                        MessageBox.Show("Select file");
                   
                }
            ));
        }

        private MyPizza LoadPizzaFile()
        {
            // Load data
            using (StreamReader sr = new StreamReader(PathToFile))
            {
                var line = sr.ReadLine();
                var Parts = line.Split(' ');
                var Rows = int.Parse(Parts[0]);
                var Columns = int.Parse(Parts[1]);
                var MinIngPerSlice = int.Parse(Parts[2]);
                var MaxSliceSize = int.Parse(Parts[3]);
                var PizzaArray = new int[Rows, Columns];

                for (int r = 0; r < Rows; r++)
                {
                    line = sr.ReadLine();
                    for (int c = 0; c < Columns; c++)
                    {
                        if (line[c] == 'T')
                            PizzaArray[r, c] = 1; //Tomato;
                        else if (line[c] == 'M')
                            PizzaArray[r, c] = 2; //Mushroom;
                    }
                }

                return new MyPizza(Rows, Columns, PizzaArray, MinIngPerSlice, MaxSliceSize);
            }
        }
    }
}
