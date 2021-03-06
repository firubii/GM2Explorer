﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using NAudio.Wave;
using NAudio.Vorbis;

namespace GM2Explorer
{
    public partial class Form1 : Form
    {
        public struct AudioGroup
        {
            public string name;
            public List<string> fileNames;
            public List<byte[]> files;
        }
        public struct SPRT
        {
            public string name;
            public List<ushort> x;
            public List<ushort> y;
            public List<ushort> width;
            public List<ushort> height;
            public List<ushort> sheet;
        }

        List<SPRT> SPRTList = new List<SPRT>();
        List<Bitmap> TXTR = new List<Bitmap>();
        List<AudioGroup> AudioGroupList = new List<AudioGroup>();
        List<string> STRGList = new List<string>();

        private AudioFileReader wavFile;
        private WaveOutEvent wavOutput;
        private VorbisWaveReader vorbisFile;
        byte[] loadedAudio;
        bool isOgg = false;
        bool forceStop = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void WavOutput_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (!isOgg)
            {
                wavFile.Position = 0;
            }
            else
            {
                vorbisFile.Position = 0;
            }
            if (loop.Checked && !forceStop)
            {
                wavOutput.Play();
                forceStop = false;
            }
            else
            {
                playPause.Text = "Play";
            }
        }

        public Bitmap Crop(Bitmap b, Rectangle r)
        {
            Bitmap nb = new Bitmap(r.Width, r.Height);
            Graphics g = Graphics.FromImage(nb);
            g.DrawImage(b, -r.X, -r.Y);
            return nb;
        }

        public void ReadData(string path)
        {
            ProgressWindow progress = new ProgressWindow();
            progress.Show();

            if (wavOutput != null) wavOutput.Dispose();
            if (wavFile != null) wavFile.Dispose();
            if (vorbisFile != null) vorbisFile.Dispose();
            textureDisplay.Image = null;
            spriteDisplay.Image = null;
            loadedAudio = new byte[] { };
            isOgg = false;
            audioList.Nodes.Clear();
            texList.Items.Clear();
            spriteList.Items.Clear();
            audioList.BeginUpdate();
            texList.BeginUpdate();
            spriteList.BeginUpdate();
            TXTR.Clear();
            AudioGroupList.Clear();
            SPRTList.Clear();
            STRGList.Clear();
            stringList.Items.Clear();
            this.Enabled = false;
            this.Cursor = Cursors.WaitCursor;
            BinaryReader reader = null;
            string file = "";
            if (!path.EndsWith(".exe"))
            {
                if (File.Exists(path + "\\data.win"))
                {
                    file = path + "\\data.win";
                }
                else if (File.Exists(path + "\\game.win"))
                {
                    file = path + "\\game.win";
                }
            }
            else
            {
                progress.SetText("Searching for data.win...");
                reader = new BinaryReader(new FileStream(path, FileMode.Open));
                bool found = false;
                while (reader.BaseStream.Position < reader.BaseStream.Length - 4)
                {
                    long pos = reader.BaseStream.Position;
                    byte[] FORMmagic = reader.ReadBytes(4);
                    if (FORMmagic.SequenceEqual(new byte[] { 0x46, 0x4F, 0x52, 0x4D }))
                    {
                        uint fileLength = reader.ReadUInt32() + 8;
                        byte[] GEN8magic = reader.ReadBytes(4);
                        if (GEN8magic.SequenceEqual(new byte[] { 0x47, 0x45, 0x4E, 0x38 }))
                        {
                            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
                            if (Directory.Exists(@"C:\gmetemp"))
                            {
                                Directory.Delete(@"C:\gmetemp", true);
                            }
                            Directory.CreateDirectory(@"C:\gmetemp");
                            File.WriteAllBytes(@"C:\gmetemp\tmp.win", reader.ReadBytes((int)fileLength));
                            file = @"C:\gmetemp\tmp.win";
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    progress.Close();
                    MessageBox.Show("Unable to find data.win");
                    return;
                }
            }
            reader = new BinaryReader(new FileStream(file, FileMode.Open));
            int texOffset = 0x8;
            reader.BaseStream.Seek(0x3C, SeekOrigin.Begin);
            uint version = reader.ReadUInt32();
            reader.BaseStream.Seek(0x38, SeekOrigin.Begin);
            uint projnameoffset = reader.ReadUInt32();
            reader.BaseStream.Seek(projnameoffset - 4, SeekOrigin.Begin);
            string projname = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));
            if (version == 1)
            {
                texOffset = 0x4;
            }
            this.Text = "GM2Explorer - Reading game " + projname;
            
            progress.SetText("Finding next section...");
            //Begin asset search
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            //Start at FORM
            reader.BaseStream.Seek(8, SeekOrigin.Current);
            //GEN8
            reader.BaseStream.Seek(4, SeekOrigin.Current);
            uint sectLen = reader.ReadUInt32();
            reader.BaseStream.Seek(sectLen, SeekOrigin.Current);
            //OPTN
            if (Encoding.UTF8.GetString(reader.ReadBytes(4)) == "OPTN")
            {
                sectLen = reader.ReadUInt32();
                reader.BaseStream.Seek(sectLen, SeekOrigin.Current);
            }
            else
            {
                reader.BaseStream.Seek(-0x4, SeekOrigin.Current);
            }
            //LANG
            if (Encoding.UTF8.GetString(reader.ReadBytes(4)) == "LANG")
            {
                sectLen = reader.ReadUInt32();
                reader.BaseStream.Seek(sectLen, SeekOrigin.Current);
            }
            else
            {
                reader.BaseStream.Seek(-0x4, SeekOrigin.Current);
            }
            //EXTN
            if (Encoding.UTF8.GetString(reader.ReadBytes(4)) == "EXTN")
            {
                sectLen = reader.ReadUInt32();
                reader.BaseStream.Seek(sectLen, SeekOrigin.Current);
            }
            else
            {
                reader.BaseStream.Seek(-0x4, SeekOrigin.Current);
            }
            //SOND
            reader.BaseStream.Seek(4, SeekOrigin.Current);
            sectLen = reader.ReadUInt32();
            uint sondOffs = (uint)reader.BaseStream.Position;
            reader.BaseStream.Seek(sectLen, SeekOrigin.Current);
            //AGRP
            progress.SetText("Reading AGRP Section...");
            reader.BaseStream.Seek(4, SeekOrigin.Current);
            sectLen = reader.ReadUInt32();
            uint currentOffset = (uint)reader.BaseStream.Position;

            uint entryCount = reader.ReadUInt32();
            uint entryListStart = (uint)reader.BaseStream.Position;
            List<uint> entryOffsets = new List<uint>();
            progress.SetProgressValue(0);
            progress.SetProgressMax((int)entryCount);
            for (int i = 0; i < entryCount; i++)
            {
                entryOffsets.Add(reader.ReadUInt32());
            }
            for (int i = 0; i < entryCount; i++)
            {
                progress.SetProgressValue(i);
                progress.SetText($"Reading AGRP Section... {i}/{entryCount} - {(int)(((float)i/(float)entryCount)*100)}");
                AudioGroup _audo = new AudioGroup();
                _audo.fileNames = new List<string>();
                _audo.files = new List<byte[]>();
                reader.BaseStream.Seek(entryOffsets[i], SeekOrigin.Begin);
                uint nameOffset = reader.ReadUInt32();
                reader.BaseStream.Seek(nameOffset - 4, SeekOrigin.Begin);
                string groupName = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));
                _audo.name = groupName;
                AudioGroupList.Add(_audo);
            }
            if (entryCount == 0)
            {
                AudioGroup _audo = new AudioGroup();
                _audo.fileNames = new List<string>();
                _audo.files = new List<byte[]>();
                _audo.name = "audiogroup_default";
                AudioGroupList.Add(_audo);
            }

            //Jump back to SOND and fill out the data
            reader.BaseStream.Seek(sondOffs, SeekOrigin.Begin);
            progress.SetText("Reading SOND Section...");
            entryCount = reader.ReadUInt32();
            entryListStart = (uint)reader.BaseStream.Position;
            entryOffsets = new List<uint>();

            progress.SetProgressValue(0);
            progress.SetProgressMax((int)entryCount);
            for (int i = 0; i < entryCount; i++)
            {
                entryOffsets.Add(reader.ReadUInt32());
            }
            for (int i = 0; i < entryCount; i++)
            {
                progress.SetProgressValue(i);
                progress.SetText($"Reading SOND Section... {i}/{entryCount} - {(int)(((float)i / (float)entryCount) * 100)}");
                reader.BaseStream.Seek(entryOffsets[i], SeekOrigin.Begin);
                uint nameOffset = reader.ReadUInt32();
                long pos = reader.BaseStream.Position;
                reader.BaseStream.Seek(nameOffset - 4, SeekOrigin.Begin);
                string audioName = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));
                reader.BaseStream.Seek(pos + 0x18, SeekOrigin.Begin);
                int groupIndex = reader.ReadInt32();
                int audioIndex = reader.ReadInt32();
                if (audioIndex != -1)
                {
                    AudioGroup _audo = AudioGroupList[groupIndex];
                    if (_audo.fileNames.Count < audioIndex + 1)
                    {
                        _audo.fileNames.AddRange(new string[(audioIndex + 1) - _audo.fileNames.Count]);
                    }
                    _audo.fileNames[audioIndex] = audioName;
                    AudioGroupList[groupIndex] = _audo;
                }
            }

            reader.BaseStream.Seek(currentOffset, SeekOrigin.Begin);
            reader.BaseStream.Seek(sectLen, SeekOrigin.Current);
            //SPRT
            progress.SetText("Reading SPRT Section...");
            reader.BaseStream.Seek(4, SeekOrigin.Current);
            sectLen = reader.ReadUInt32();
            currentOffset = (uint)reader.BaseStream.Position;
            
            uint spriteCount = reader.ReadUInt32();
            progress.SetProgressValue(0);
            progress.SetProgressMax((int)spriteCount);
            List<uint> spriteOffsets = new List<uint>();
            for (int i = 0; i < spriteCount; i++)
            {
                spriteOffsets.Add(reader.ReadUInt32());
            }
            for (int i = 0; i < spriteCount; i++)
            {
                progress.SetProgressValue(i);
                progress.SetText($"Reading SPRT section... {i}/{spriteCount} - {(int)(((float)i / (float)spriteCount) * 100)}");
                SPRT sprt = new SPRT();
                sprt.x = new List<ushort>();
                sprt.y = new List<ushort>();
                sprt.width = new List<ushort>();
                sprt.height = new List<ushort>();
                sprt.sheet = new List<ushort>();
                reader.BaseStream.Seek(spriteOffsets[i], SeekOrigin.Begin);
                uint nameOffset = reader.ReadUInt32();
                reader.BaseStream.Seek(0x34, SeekOrigin.Current);
                if (version == 2)
                {
                    reader.BaseStream.Seek(0x14, SeekOrigin.Current);
                }
                uint texCount = reader.ReadUInt32();
                List<uint> texOffsets = new List<uint>();
                List<Bitmap> sprites = new List<Bitmap>();
                for (int t = 0; t < texCount; t++)
                {
                    texOffsets.Add(reader.ReadUInt32());
                }
                for (int t = 0; t < texCount; t++)
                {
                    reader.BaseStream.Seek(texOffsets[t], SeekOrigin.Begin);
                    sprt.x.Add(reader.ReadUInt16());
                    sprt.y.Add(reader.ReadUInt16());
                    sprt.width.Add(reader.ReadUInt16());
                    sprt.height.Add(reader.ReadUInt16());
                    reader.BaseStream.Seek(0xC, SeekOrigin.Current);
                    sprt.sheet.Add(reader.ReadUInt16());
                }
                reader.BaseStream.Seek(nameOffset - 4, SeekOrigin.Begin);
                string name = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));
                sprt.name = name;
                SPRTList.Add(sprt);
            }
            reader.BaseStream.Seek(currentOffset, SeekOrigin.Begin);
            reader.BaseStream.Seek(sectLen, SeekOrigin.Current);
            progress.SetText("Finding next Section...");
            //BGND
            if (Encoding.UTF8.GetString(reader.ReadBytes(4)) == "BGND")
            {
                sectLen = reader.ReadUInt32();
                reader.BaseStream.Seek(sectLen, SeekOrigin.Current);
            }
            else
            {
                reader.BaseStream.Seek(-0x4, SeekOrigin.Current);
            }
            //PATH
            if (Encoding.UTF8.GetString(reader.ReadBytes(4)) == "PATH")
            {
                sectLen = reader.ReadUInt32();
                reader.BaseStream.Seek(sectLen, SeekOrigin.Current);
            }
            else
            {
                reader.BaseStream.Seek(-0x4, SeekOrigin.Current);
            }
            //SCPT
            if (Encoding.UTF8.GetString(reader.ReadBytes(4)) == "SCPT")
            {
                sectLen = reader.ReadUInt32();
                reader.BaseStream.Seek(sectLen, SeekOrigin.Current);
            }
            else
            {
                reader.BaseStream.Seek(-0x4, SeekOrigin.Current);
            }
            //GLOB
            if (Encoding.UTF8.GetString(reader.ReadBytes(4)) == "GLOB")
            {
                sectLen = reader.ReadUInt32();
                reader.BaseStream.Seek(sectLen, SeekOrigin.Current);
            }
            else
            {
                reader.BaseStream.Seek(-0x4, SeekOrigin.Current);
            }
            //SHDR
            if (Encoding.UTF8.GetString(reader.ReadBytes(4)) == "SHDR")
            {
                sectLen = reader.ReadUInt32();
                reader.BaseStream.Seek(sectLen, SeekOrigin.Current);
            }
            else
            {
                reader.BaseStream.Seek(-0x4, SeekOrigin.Current);
            }
            //FONT
            if (Encoding.UTF8.GetString(reader.ReadBytes(4)) == "FONT")
            {
                sectLen = reader.ReadUInt32();
                reader.BaseStream.Seek(sectLen, SeekOrigin.Current);
            }
            else
            {
                reader.BaseStream.Seek(-0x4, SeekOrigin.Current);
            }
            //TMLN
            if (Encoding.UTF8.GetString(reader.ReadBytes(4)) == "TMLN")
            {
                sectLen = reader.ReadUInt32();
                reader.BaseStream.Seek(sectLen, SeekOrigin.Current);
            }
            else
            {
                reader.BaseStream.Seek(-0x4, SeekOrigin.Current);
            }
            //OBJT
            if (Encoding.UTF8.GetString(reader.ReadBytes(4)) == "OBJT")
            {
                sectLen = reader.ReadUInt32();
                reader.BaseStream.Seek(sectLen, SeekOrigin.Current);
            }
            else
            {
                reader.BaseStream.Seek(-0x4, SeekOrigin.Current);
            }
            //ROOM
            if (Encoding.UTF8.GetString(reader.ReadBytes(4)) == "ROOM")
            {
                sectLen = reader.ReadUInt32();
                reader.BaseStream.Seek(sectLen, SeekOrigin.Current);
            }
            else
            {
                reader.BaseStream.Seek(-0x4, SeekOrigin.Current);
            }
            //DAFL
            if (Encoding.UTF8.GetString(reader.ReadBytes(4)) == "DAFL")
            {
                sectLen = reader.ReadUInt32();
                reader.BaseStream.Seek(sectLen, SeekOrigin.Current);
            }
            else
            {
                reader.BaseStream.Seek(-0x4, SeekOrigin.Current);
            }
            //EMBI
            if (Encoding.UTF8.GetString(reader.ReadBytes(4)) == "EMBI")
            {
                sectLen = reader.ReadUInt32();
                reader.BaseStream.Seek(sectLen, SeekOrigin.Current);
            }
            else
            {
                reader.BaseStream.Seek(-0x4, SeekOrigin.Current);
            }
            //TPAG
            if (Encoding.UTF8.GetString(reader.ReadBytes(4)) == "TPAG")
            {
                sectLen = reader.ReadUInt32();
                reader.BaseStream.Seek(sectLen, SeekOrigin.Current);
            }
            else
            {
                reader.BaseStream.Seek(-0x4, SeekOrigin.Current);
            }
            //TGIN
            if (Encoding.UTF8.GetString(reader.ReadBytes(4)) == "TGIN")
            {
                sectLen = reader.ReadUInt32();
                reader.BaseStream.Seek(sectLen, SeekOrigin.Current);
            }
            else
            {
                reader.BaseStream.Seek(-0x4, SeekOrigin.Current);
            }
            //CODE
            if (Encoding.UTF8.GetString(reader.ReadBytes(4)) == "CODE")
            {
                sectLen = reader.ReadUInt32();
                reader.BaseStream.Seek(sectLen, SeekOrigin.Current);
            }
            else
            {
                reader.BaseStream.Seek(-0x4, SeekOrigin.Current);
            }
            //VARI
            if (Encoding.UTF8.GetString(reader.ReadBytes(4)) == "VARI")
            {
                sectLen = reader.ReadUInt32();
                reader.BaseStream.Seek(sectLen, SeekOrigin.Current);
            }
            else
            {
                reader.BaseStream.Seek(-0x4, SeekOrigin.Current);
            }
            //FUNC
            if (Encoding.UTF8.GetString(reader.ReadBytes(4)) == "FUNC")
            {
                sectLen = reader.ReadUInt32();
                reader.BaseStream.Seek(sectLen, SeekOrigin.Current);
            }
            else
            {
                reader.BaseStream.Seek(-0x4, SeekOrigin.Current);
            }
            //STRG
            reader.BaseStream.Seek(4, SeekOrigin.Current);
            sectLen = reader.ReadUInt32();
            currentOffset = (uint)reader.BaseStream.Position;

            if (MessageBox.Show("Do you want to load the STRG (strings) section?\nNote: This will take a lot longer and will take up a lot more RAM", "GM2Explorer", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                progress.SetText("Reading STRG Section...");
                uint stringCount = reader.ReadUInt32();
                progress.SetProgressValue(0);
                progress.SetProgressMax((int)stringCount);
                for (int i = 0; i < stringCount; i++)
                {
                    progress.SetProgressValue(i);
                    progress.SetText($"Reading STRG section... {i}/{stringCount} - {(int)(((float)i / (float)stringCount) * 100)}");
                    uint pos = (uint)reader.BaseStream.Position;
                    reader.BaseStream.Seek(reader.ReadUInt32(), SeekOrigin.Begin);
                    //Console.WriteLine($"{i}: 0x{pos.ToString("X8")} >> 0x{reader.BaseStream.Position.ToString("X8")}");
                    STRGList.Add(Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32())));
                    reader.BaseStream.Seek(pos + 4, SeekOrigin.Begin);
                }
            }

            reader.BaseStream.Seek(currentOffset, SeekOrigin.Begin);
            reader.BaseStream.Seek(sectLen, SeekOrigin.Current);
            //TXTR
            progress.SetText("Reading TXTR Section...");
            reader.BaseStream.Seek(4, SeekOrigin.Current);
            sectLen = reader.ReadUInt32();
            currentOffset = (uint)reader.BaseStream.Position;

            uint sectionEndOffset = currentOffset + sectLen;
            uint fileCount = reader.ReadUInt32();
            uint fileListStart = (uint)reader.BaseStream.Position;
            progress.SetProgressValue(0);
            progress.SetProgressMax((int)fileCount);
            List<uint> fileOffsets = new List<uint>();
            for (int f = 0; f < fileCount; f++)
            {
                uint pos = (uint)reader.BaseStream.Position;
                reader.BaseStream.Seek(reader.ReadUInt32() + texOffset, SeekOrigin.Begin);
                fileOffsets.Add(reader.ReadUInt32());
                reader.BaseStream.Seek(pos + 4, SeekOrigin.Begin);
            }
            for (int f = 0; f < fileOffsets.Count; f++)
            {
                progress.SetProgressValue(f);
                progress.SetText($"Reading TXTR section... {f}/{fileCount} - {(int)(((float)f / (float)fileCount) * 100)}");
                reader.BaseStream.Seek(fileOffsets[f], SeekOrigin.Begin);
                byte[] tex = new byte[] { };
                if (f == fileOffsets.Count - 1)
                {
                    tex = reader.ReadBytes((int)(sectionEndOffset - fileOffsets[f]));
                }
                else
                {
                    tex = reader.ReadBytes((int)(fileOffsets[f + 1] - fileOffsets[f]));
                }
                try
                {
                    MemoryStream stream = new MemoryStream(tex);
                    Image img = Image.FromStream(stream);
                    TXTR.Add(new Bitmap(img));
                    stream.Dispose();
                    img.Dispose();
                }
                catch { }
            }
            fileOffsets.Clear();
            reader.BaseStream.Seek(currentOffset, SeekOrigin.Begin);
            reader.BaseStream.Seek(sectLen, SeekOrigin.Current);
            //AUDO
            progress.SetText("Reading AUDO Section...");
            reader.BaseStream.Seek(4, SeekOrigin.Current);
            sectLen = reader.ReadUInt32();

            AudioGroup audo = AudioGroupList[0];
            //Console.WriteLine("Found AUDO Section at 0x" + currentOffset.ToString("X8"));
            fileCount = reader.ReadUInt32();
            fileListStart = (uint)reader.BaseStream.Position;
            progress.SetProgressValue(0);
            progress.SetProgressMax((int)fileCount);
            fileOffsets = new List<uint>();
            for (int f = 0; f < fileCount; f++)
            {
                uint audoDataOffset = reader.ReadUInt32();
                fileOffsets.Add(audoDataOffset);
            }
            for (int f = 0; f < fileOffsets.Count; f++)
            {
                progress.SetProgressValue(f);
                progress.SetText($"Reading TXTR section... {f}/{fileCount} - {(int)(((float)f / (float)fileCount) * 100)}");
                reader.BaseStream.Seek(fileOffsets[f], SeekOrigin.Begin);
                //Console.WriteLine("Reading audio " + f + " at offset 0x" + fileOffsets[f].ToString("X8"));
                List<byte> audio = new List<byte>();
                uint fileLength = reader.ReadUInt32();
                audio.AddRange(reader.ReadBytes((int)fileLength));
                audo.files.Add(audio.ToArray());
                audio.Clear();
            }
            AudioGroupList[0] = audo;
            fileOffsets.Clear();
            reader.Dispose();

            progress.SetProgressValue(0);
            progress.SetText("Reading AudioGroups...");
            if (path.EndsWith(".exe"))
            {
                path = path.Replace("\\" + path.Split('\\').Last(), "");
            }
            string[] audiogroups = Directory.GetFiles(path, "audiogroup*.dat");
            progress.SetProgressMax(audiogroups.Length);
            for (int i = 0; i < audiogroups.Length; i++)
            {
                progress.SetProgressValue(i);
                progress.SetText($"Reading TXTR section... {i}/{audiogroups.Length} - {(int)(((float)i / (float)audiogroups.Length) * 100)}");

                this.Text = "GM2Explorer - Reading \"" + path + "\\" + audiogroups[i].Replace(path + "\\", "") + "\"";
                reader = new BinaryReader(new FileStream(audiogroups[i], FileMode.Open));
                reader.BaseStream.Seek(0x10, SeekOrigin.Begin);

                audo = AudioGroupList[i + 1];
                audo.files = new List<byte[]>();
                //Console.WriteLine("Found AUDO Section at 0x" + currentOffset.ToString("X8"));
                fileCount = reader.ReadUInt32();
                fileListStart = (uint)reader.BaseStream.Position;
                fileOffsets = new List<uint>();
                for (int f = 0; f < fileCount; f++)
                {
                    uint audoDataOffset = reader.ReadUInt32();
                    fileOffsets.Add(audoDataOffset);
                }
                for (int f = 0; f < fileOffsets.Count; f++)
                {
                    reader.BaseStream.Seek(fileOffsets[f], SeekOrigin.Begin);
                    //Console.WriteLine("Reading audio " + f + " at offset 0x" + fileOffsets[f].ToString("X8"));
                    List<byte> audio = new List<byte>();
                    uint fileLength = reader.ReadUInt32();
                    audio.AddRange(reader.ReadBytes((int)fileLength));
                    audo.files.Add(audio.ToArray());
                    audio.Clear();
                }
                AudioGroupList[i + 1] = audo;
            }
            reader.Dispose();

            texList.BeginUpdate();
            spriteList.BeginUpdate();
            audioList.BeginUpdate();
            stringList.BeginUpdate();

            for (int i = 0; i < TXTR.Count; i++)
            {
                texList.Items.Add("tex_" + i);
            }
            for (int i = 0; i < SPRTList.Count; i++)
            {
                spriteList.Items.Add(SPRTList[i].name);
            }
            for (int i = 0; i < AudioGroupList.Count; i++)
            {
                audioList.Nodes.Add(AudioGroupList[i].name);
                for (int a = 0; a < AudioGroupList[i].files.Count; a++)
                {
                    if (AudioGroupList[i].fileNames[a] != "")
                    {
                        audioList.Nodes[i].Nodes.Add(AudioGroupList[i].fileNames[a]);
                    }
                    else
                    {
                        audioList.Nodes[i].Nodes.Add("audio_" + a);
                    }
                }
            }
            for (int i = 0; i < STRGList.Count; i++)
            {
                stringList.Items.Add(STRGList[i]);
            }

            texList.EndUpdate();
            spriteList.EndUpdate();
            audioList.EndUpdate();
            stringList.EndUpdate();

            file = "";
            reader.Dispose();
            sNum.Maximum = TXTR.Count - 1;
            this.Text = "GM2Explorer";
            this.Cursor = Cursors.Default;
            this.Enabled = true;
            this.BringToFront();
            audioList.EndUpdate();
            texList.EndUpdate();
            spriteList.EndUpdate();

            if (Directory.Exists(@"C:\gmetemp"))
            {
                Directory.Delete(@"C:\gmetemp", true);
            }

            progress.Close();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "GameMaker Data Archives|*.win|EXE Executable Files|*.exe";
            if (open.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(open.FileName))
                {
                    if (open.FileName.EndsWith(".exe"))
                    {
                        ReadData(open.FileName);
                    }
                    else
                    {
                        ReadData(Path.GetDirectoryName(open.FileName));
                    }
                }
            }
        }

        private void texList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (texList.SelectedIndex <= TXTR.Count - 1)
            {
                Bitmap image = TXTR[texList.SelectedIndex];
                textureDisplay.Image = image;
            }
        }

        private void saveTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "PNG Image File|*.png";
            save.DefaultExt = ".png";
            save.AddExtension = true;
            save.FileName = "tex_" + texList.SelectedIndex + ".png";
            if (save.ShowDialog() == DialogResult.OK)
            {
                TXTR[texList.SelectedIndex].Save(save.FileName, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        private void exportAllTexturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "";
            save.AddExtension = false;
            save.ValidateNames = false;
            save.CheckFileExists = false;
            save.CheckPathExists = true;
            save.FileName = "Select a Folder";
            if (save.ShowDialog() == DialogResult.OK)
            {
                for (int i = 0; i < TXTR.Count; i++)
                {
                    TXTR[i].Save(Path.GetDirectoryName(save.FileName) + "\\tex_" + i + ".png", System.Drawing.Imaging.ImageFormat.Png);
                }
            }
        }

        private void audioList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            forceStop = true;
            if (audioList.SelectedNode.Parent != null)
            {
                try
                {
                    if (wavOutput != null)
                    {
                        wavOutput.Stop();
                        wavOutput.Dispose();
                    }
                    if (wavFile != null)
                    {
                        wavFile.Dispose();
                    }
                    if (vorbisFile != null)
                    {
                        vorbisFile.Dispose();
                    }
                    if (Directory.Exists(@"C:\gmetemp"))
                    {
                        Directory.Delete(@"C:\gmetemp", true);
                    }
                    Directory.CreateDirectory(@"C:\gmetemp");

                    loadedAudio = AudioGroupList[audioList.SelectedNode.Parent.Index].files[audioList.SelectedNode.Index];

                    wavOutput = new WaveOutEvent();
                    wavOutput.PlaybackStopped += WavOutput_PlaybackStopped;
                    if (BitConverter.ToUInt32(loadedAudio, 0) == 0x5367674F)
                    {
                        File.WriteAllBytes(@"C:\gmetemp\temp.ogg", loadedAudio);
                        vorbisFile = new VorbisWaveReader(@"C:\gmetemp\temp.ogg");
                        wavOutput.Init(vorbisFile);
                        isOgg = true;
                    }
                    else if (BitConverter.ToUInt32(loadedAudio, 0) == 0x46464952)
                    {
                        File.WriteAllBytes(@"C:\gmetemp\temp.wav", loadedAudio);
                        wavFile = new AudioFileReader(@"C:\gmetemp\temp.wav");
                        wavOutput.Init(wavFile);
                        isOgg = false;
                    }
                    playPause.Text = "Play";
                    filelabel.Text = "File: " + audioList.SelectedNode.Parent.Text + "\\" + audioList.SelectedNode.Text;
                } catch { }
            }
        }

        private void playPause_Click(object sender, EventArgs e)
        {
            if (wavFile != null || vorbisFile != null)
            {
                if (wavOutput.PlaybackState == PlaybackState.Playing)
                {
                    playPause.Text = "Play";
                    wavOutput.Pause();
                }
                else
                {
                    forceStop = false;
                    playPause.Text = "Pause";
                    wavOutput.Play();
                }
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (wavOutput != null)
            {
                wavOutput.Stop();
                wavOutput.Dispose();
            }
            if (wavFile != null)
            {
                wavFile.Dispose();
            }
            if (vorbisFile != null)
            {
                vorbisFile.Dispose();
            }
            if (Directory.Exists(@"C:\gmetemp"))
            {
                Directory.Delete(@"C:\gmetemp", true);
            }
        }

        private void exportAudioToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadedAudio = AudioGroupList[audioList.SelectedNode.Parent.Index].files[audioList.SelectedNode.Index];
            SaveFileDialog save = new SaveFileDialog();
            string extension = ".wav";
            save.Filter = "WAV Audio File|*.wav";
            if (isOgg)
            {
                extension = ".ogg";
                save.Filter = "OGG Vorbis File|*.ogg";
            }
            save.DefaultExt = extension;
            save.AddExtension = true;
            save.FileName = audioList.SelectedNode.Text + extension;
            if (save.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllBytes(save.FileName, loadedAudio);
            }
        }

        private void exportAllAudioToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "";
            save.AddExtension = false;
            save.ValidateNames = false;
            save.CheckFileExists = false;
            save.CheckPathExists = true;
            save.FileName = "Select a Folder";
            save.Title = "Select a Folder";
            if (save.ShowDialog() == DialogResult.OK)
            {
                ProgressWindow progress = new ProgressWindow();
                progress.Show();
                for (int i = 0; i < AudioGroupList.Count; i++)
                {
                    string dir = Path.GetDirectoryName(save.FileName) + "\\" + AudioGroupList[i].name;
                    if (Directory.Exists(dir))
                    {
                        Directory.Delete(dir, true);
                    }
                    Directory.CreateDirectory(dir);
                    progress.SetText($"Saving AudioGroup {AudioGroupList[i].name}...");
                    progress.SetProgressMax(AudioGroupList[i].files.Count);
                    for (int f = 0; f < AudioGroupList[i].files.Count; f++)
                    {
                        progress.SetProgressValue(f);
                        progress.SetText($"Saving AudioGroup {AudioGroupList[i].name}... {f}/{AudioGroupList[i].files.Count} - {(int)(((float)f/(float)AudioGroupList[i].files.Count)*100)}%");
                        string extension = "";
                        if (BitConverter.ToUInt32(AudioGroupList[i].files[f], 0) == 0x5367674F)
                        {
                            extension = ".ogg";
                        }
                        else if (BitConverter.ToUInt32(AudioGroupList[i].files[f], 0) == 0x46464952)
                        {
                            extension = ".wav";
                        }
                        if (AudioGroupList[i].fileNames[f] != "")
                        {
                            File.WriteAllBytes(dir + "\\" + AudioGroupList[i].fileNames[f] + extension, AudioGroupList[i].files[f]);
                        }
                        else
                        {
                            File.WriteAllBytes(dir + "\\audio_" + f + extension, AudioGroupList[i].files[f]);
                        }
                    }
                }
                progress.Close();
            }
        }

        private void spriteList_SelectedIndexChanged(object sender, EventArgs e)
        {
            SPRT sprt = SPRTList[spriteList.SelectedIndex];
            try
            {
                Bitmap image = Crop(TXTR[sprt.sheet[0]], new Rectangle((int)sprt.x[0], (int)sprt.y[0], (int)sprt.width[0], (int)sprt.height[0]));
                spriteDisplay.Image = image;
                spriteNum.Value = 0;
                spriteNum.Maximum = SPRTList[spriteList.SelectedIndex].sheet.Count - 1;
                spriteCount.Text = "/" + spriteNum.Maximum;
            }
            catch
            {
                MessageBox.Show("Error: This sprite might be broken!", "GM2Explorer");
            }
            xNum.Value = sprt.x[0];
            yNum.Value = sprt.y[0];
            wNum.Value = sprt.width[0];
            hNum.Value = sprt.height[0];
            sNum.Value = sprt.sheet[0];
        }

        private void spriteNum_ValueChanged(object sender, EventArgs e)
        {
            SPRT sprt = SPRTList[spriteList.SelectedIndex];
            Bitmap image = Crop(TXTR[sprt.sheet[(int)spriteNum.Value]], new Rectangle((int)sprt.x[(int)spriteNum.Value], (int)sprt.y[(int)spriteNum.Value], (int)sprt.width[(int)spriteNum.Value], (int)sprt.height[(int)spriteNum.Value]));
            spriteDisplay.Image = image;

            xNum.Value = sprt.x[(int)spriteNum.Value];
            yNum.Value = sprt.y[(int)spriteNum.Value];
            wNum.Value = sprt.width[(int)spriteNum.Value];
            hNum.Value = sprt.height[(int)spriteNum.Value];
            sNum.Value = sprt.sheet[(int)spriteNum.Value];
        }

        private void exportSpriteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool exportSheet = false;
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "PNG Image File|*.png";
            save.DefaultExt = ".png";
            save.AddExtension = true;
            if (SPRTList[spriteList.SelectedIndex].sheet.Count > 1)
            {
                if (MessageBox.Show("This entry has multiple sprites.\nWould you like to export as a sprite sheet?", this.Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    save.FileName = SPRTList[spriteList.SelectedIndex].name + ".png";
                    exportSheet = true;
                }
                else
                {
                    save.FileName = SPRTList[spriteList.SelectedIndex].name + "_" + spriteNum.Value + ".png";
                }
            }
            else
            {
                save.FileName = SPRTList[spriteList.SelectedIndex].name + ".png";
            }
            if (save.ShowDialog() == DialogResult.OK)
            {
                SPRT sprt = SPRTList[spriteList.SelectedIndex];
                if (exportSheet)
                {
                    List<Bitmap> sprList = new List<Bitmap>();
                    for (int t = 0; t < SPRTList[spriteList.SelectedIndex].sheet.Count; t++)
                    {
                        try
                        {
                            sprList.Add(Crop(TXTR[sprt.sheet[t]], new Rectangle((int)sprt.x[t], (int)sprt.y[t], (int)sprt.width[t], (int)sprt.height[t])));
                        }
                        catch { }
                    }
                    int h = 0;
                    int w = 0;
                    for (int t = 0; t < sprList.Count; t++)
                    {
                        if (sprList[t].Width > w) w = sprList[t].Width;
                        if (sprList[t].Height > h) h = sprList[t].Height;
                    }
                    int wfull = w * sprList.Count;

                    Bitmap finalImg = new Bitmap(wfull, h);
                    int xOff = 0;
                    for (int t = 0; t < sprList.Count; t++)
                    {
                        for (int y = 0; y < sprList[t].Height; y++)
                        {
                            for (int x = 0; x < sprList[t].Width; x++)
                            {
                                finalImg.SetPixel(x + (t * w), y + (h - sprList[t].Height), sprList[t].GetPixel(x, y));
                            }
                        }
                    }

                    finalImg.Save(save.FileName, System.Drawing.Imaging.ImageFormat.Png);
                    finalImg.Dispose();
                    sprList.Clear();
                }
                else
                {
                    Crop(TXTR[sprt.sheet[(int)spriteNum.Value]], new Rectangle((int)sprt.x[(int)spriteNum.Value], (int)sprt.y[(int)spriteNum.Value], (int)sprt.width[(int)spriteNum.Value], (int)sprt.height[(int)spriteNum.Value])).Save(save.FileName, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
        }

        private void exportAllSpritesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "";
            save.AddExtension = false;
            save.ValidateNames = false;
            save.CheckFileExists = false;
            save.CheckPathExists = true;
            save.FileName = "Select a Folder";
            save.Title = "Select a Folder";
            if (save.ShowDialog() == DialogResult.OK)
            {
                ProgressWindow progress = new ProgressWindow();
                progress.Show();
                progress.SetProgressMax(SPRTList.Count);

                bool exportSheet = false;
                if (MessageBox.Show("Would you like to export sprite sheets for sprites with multiple entries?", this.Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
                    exportSheet = true;

                for (int i = 0; i < SPRTList.Count; i++)
                {
                    progress.SetProgressValue(i);
                    progress.SetText($"Saving Sprites... {i}/{SPRTList.Count} - {(int)(((float)i / (float)SPRTList.Count) * 100)}%");
                    SPRT sprt = SPRTList[i];
                    if (SPRTList[i].sheet.Count > 1)
                    {
                        if (exportSheet)
                        {
                            List<Bitmap> sprList = new List<Bitmap>();
                            for (int t = 0; t < SPRTList[i].sheet.Count; t++)
                            {
                                try
                                {
                                    sprList.Add(Crop(TXTR[sprt.sheet[t]], new Rectangle((int)sprt.x[t], (int)sprt.y[t], (int)sprt.width[t], (int)sprt.height[t])));
                                }
                                catch { }
                            }
                            int h = 0;
                            int w = 0;
                            for (int t = 0; t < sprList.Count; t++)
                            {
                                if (sprList[t].Width > w) w = sprList[t].Width;
                                if (sprList[t].Height > h) h = sprList[t].Height;
                            }
                            int wfull = w * sprList.Count;
                            
                            Bitmap finalImg = new Bitmap(wfull, h);
                            int xOff = 0;
                            for (int t = 0; t < sprList.Count; t++)
                            {
                                for (int y = 0; y < sprList[t].Height; y++)
                                {
                                    for (int x = 0; x < sprList[t].Width; x++)
                                    {
                                        finalImg.SetPixel(x + (t * w), y + (h - sprList[t].Height), sprList[t].GetPixel(x, y));
                                    }
                                }
                            }

                            finalImg.Save(Path.GetDirectoryName(save.FileName) + "\\" + SPRTList[i].name + ".png", System.Drawing.Imaging.ImageFormat.Png);
                            finalImg.Dispose();
                            sprList.Clear();
                        }
                        else
                        {
                            for (int t = 0; t < SPRTList[i].sheet.Count; t++)
                            {
                                try
                                {
                                    Crop(TXTR[sprt.sheet[t]], new Rectangle((int)sprt.x[t], (int)sprt.y[t], (int)sprt.width[t], (int)sprt.height[t])).Save(Path.GetDirectoryName(save.FileName) + "\\" + SPRTList[i].name + "_" + t + ".png", System.Drawing.Imaging.ImageFormat.Png);
                                }
                                catch { }
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            Crop(TXTR[sprt.sheet[0]], new Rectangle((int)sprt.x[0], (int)sprt.y[0], (int)sprt.width[0], (int)sprt.height[0])).Save(Path.GetDirectoryName(save.FileName) + "\\" + SPRTList[i].name + ".png", System.Drawing.Imaging.ImageFormat.Png);
                        }
                        catch { }
                    }
                }
                progress.Close();
            }
        }

        private void stop_Click(object sender, EventArgs e)
        {
            if (wavOutput != null)
            {
                forceStop = true;
                wavOutput.Stop();
                if (!isOgg)
                {
                    wavFile.Position = 0;
                }
                else
                {
                    vorbisFile.Position = 0;
                }
            }
        }

        private void exportAllStringsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Plaintext File|*";
            save.FileName = "strings.txt";
            if (save.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllLines(save.FileName, STRGList);
            }
        }
    }
}
