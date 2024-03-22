using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using UILayout;
using PsarcUtil;
using SkiaSharp;

namespace PsarcConverter
{
    public class MainInterface : VerticalStack
    {
        List<string> parseFiles = new();
        List<string> parseFolders = new();

        NinePatchWrapper topSection;

        VerticalStack fileStack;
        VerticalStack folderStack;

        VerticalStack convertStack;
        TextBlock currentlyConverting;
        int songsConverted;
        TextBlock songsConvertedText;
        TextButton convertButton;

        OpenFileDialog openFileDialog = new OpenFileDialog();
        FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();

        Thread convertThread;
        bool abortConversion;

        static MainInterface()
        {
            Layout.Current.DefaultFont = new UIFont { Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), TextSize = 16 };

            Layout.Current.DefaultOutlineNinePatch = Layout.Current.AddImage("ShadowedOutline");

            Layout.Current.DefaultPressedNinePatch = Layout.Current.AddImage("ButtonPressed");
            Layout.Current.DefaultUnpressedNinePatch = Layout.Current.AddImage("ButtonUnpressed");

            Layout.Current.DefaultDragImage = Layout.Current.GetImage("ButtonPressed");

            Layout.Current.AddImage("Outline");

            Layout.Current.DefaultForegroundColor = UIColor.Black;
        }

        public MainInterface()
        {
            HorizontalAlignment = EHorizontalAlignment.Stretch;
            VerticalAlignment = EVerticalAlignment.Stretch;
            Padding = new LayoutPadding(5);
            BackgroundColor = UIColor.Lerp(UIColor.White, UIColor.Black, 0.1f);

            parseFiles.Add(@"C:\Program Files (x86)\Steam\steamapps\common\Rocksmith2014\songs.psarc");
            parseFolders.Add(@"C:\Program Files (x86)\Steam\steamapps\common\Rocksmith2014\dlc");

            topSection = new NinePatchWrapper(Layout.Current.GetImage("Outline"))
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Top
            };
            Children.Add(topSection);

            VerticalStack topStack = new VerticalStack()
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                ChildSpacing = 5
            };
            topSection.Child = topStack;

            topStack.Children.Add(new TextBlock("Files:")
            {
                Padding = new LayoutPadding(0, 10)
            });

            fileStack = new VerticalStack()
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                ChildSpacing = 5
            };
            topStack.Children.Add(fileStack);

            topStack.Children.Add(new TextButton("Add File")
            {
                ClickAction = AddFile
            });

            topStack.Children.Add(new TextBlock("Folders:")
            {
                Padding = new LayoutPadding(0, 10)
            });

            folderStack = new VerticalStack
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                ChildSpacing = 5
            };
            topStack.Children.Add(folderStack);

            topStack.Children.Add(new TextButton("Add Folder")
            {
                ClickAction = AddFolder
            });

            UpdateSources();

            NinePatchWrapper bottomSection = new NinePatchWrapper(Layout.Current.GetImage("Outline"))
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Stretch
            };
            Children.Add(bottomSection);

            Dock convertDock = new Dock();
            bottomSection.Child = convertDock;

            convertStack = new VerticalStack()
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                ChildSpacing = 10,
            };
            convertDock.Children.Add(convertStack);

            convertStack.Children.Add(new TextBlock("Conversion status:")
            {
                Padding = new LayoutPadding(0, 10)
            });

            songsConvertedText = new TextBlock("");
            convertStack.Children.Add(songsConvertedText);

            currentlyConverting = new TextBlock("");
            convertStack.Children.Add(currentlyConverting);

            convertButton = new TextButton("Convert Files")
            {
                VerticalAlignment = EVerticalAlignment.Bottom,
                ClickAction = ConvertFiles
            };

            convertDock.Children.Add(convertButton);
        }

        void AddFolder()
        {
            if (openFolderDialog.ShowDialog() == DialogResult.OK)
            {
                if (!string.IsNullOrEmpty(openFolderDialog.SelectedPath) && !parseFolders.Contains(openFolderDialog.SelectedPath))
                {
                    parseFolders.Add(openFolderDialog.SelectedPath);

                    UpdateSources();
                }
            }
        }

        void DeleteFolder(string path)
        {
            parseFolders.Remove(path);

            UpdateSources();
        }

        void AddFile()
        {
            openFileDialog.Filter = "Psarc Files|*.psarc";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (!string.IsNullOrEmpty(openFileDialog.FileName) && !parseFiles.Contains(openFileDialog.FileName))
                {
                    parseFiles.Add(openFileDialog.FileName);

                    UpdateSources();
                }
            }
        }

        void DeleteFile(string file)
        {
            parseFiles.Remove(file);

            UpdateSources();
        }

        void UpdateSources()
        {
            folderStack.Children.Clear();
            
            foreach (string folder in parseFolders)
            {
                HorizontalStack stack = new HorizontalStack()
                {
                    ChildSpacing = 10
                };
                folderStack.Children.Add(stack);

                stack.Children.Add(new TextBlock(folder)
                {
                    VerticalAlignment = EVerticalAlignment.Center
                });

                stack.Children.Add(new TextButton("X")
                {
                    TextColor = UIColor.Lerp(UIColor.Red, UIColor.Black, 0.5f),
                    ClickAction = delegate { DeleteFolder(folder); },                    
                });
            }

            fileStack.Children.Clear();

            foreach (string file in parseFiles)
            {
                HorizontalStack stack = new HorizontalStack()
                {
                    ChildSpacing = 10
                };
                fileStack.Children.Add(stack);

                stack.Children.Add(new TextBlock(file)
                {
                    VerticalAlignment = EVerticalAlignment.Center
                });

                stack.Children.Add(new TextButton("X")
                {
                    TextColor = UIColor.Lerp(UIColor.Red, UIColor.Black, 0.5f),
                    ClickAction = delegate { DeleteFile(file); },
                });
            }

            UpdateContentLayout();
        }

        bool UpdateConvert(string text)
        {
            currentlyConverting.Text = text;

            songsConverted++;
            songsConvertedText.Text = songsConverted + " Songs";

            convertStack.UpdateContentLayout();

            if (abortConversion)
            {
                abortConversion = false;

                return false;
            }

            return true;
        }

        void ConvertFiles()
        {
            if (convertThread != null)
            {
                abortConversion = true;

                convertThread.Join();
                convertThread = null;

                convertButton.Text = "Convert Files";

                convertButton.UpdateContentLayout();
            }
            else
            {
                convertThread = new Thread(new ThreadStart(DoConvert));
                convertThread.Start();

                convertButton.Text = "Abort Conversion";

                UpdateContentLayout();
            }
        }

        void DoConvert()
        {
            songsConverted = 0;

            PsarcUtil.PsarcConverter converter = new PsarcUtil.PsarcConverter(@"C:\Share\JamSongs", convertAudio: false);
            converter.UpdateAction = UpdateConvert;

            foreach (string file in parseFiles)
            {
                if (!converter.ConvertPsarc(file))
                    return;
            }

            foreach (string folder in parseFolders)
            {
                if (!converter.ConvertFolder(folder))
                    return;
            }
        }
    }
}
