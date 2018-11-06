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
using System.Threading;
using NAudio.Wave;
using NAudio.Vorbis;

namespace GM2Explorer
{
    public partial class Form1 : Form
    {
        public struct AUDOstruct
        {
            public string fileName;
            public List<byte[]> files;
        }

        List<Bitmap> TXTR = new List<Bitmap>();
        List<AUDOstruct> AUDO = new List<AUDOstruct>();

        private AudioFileReader wavFile;
        private WaveOutEvent wavOutput;
        private VorbisWaveReader vorbisFile;
        byte[] loadedAudio;
        bool isOgg = false;
        uint txtrOffset = 0;
        uint audoOffset = 0;

        public Form1()
        {
            InitializeComponent();
        }

        public void ReadData(string path)
        {
            if (wavOutput != null) wavOutput.Dispose();
            if (wavFile != null) wavFile.Dispose();
            if (vorbisFile != null) vorbisFile.Dispose();
            textureDisplay.Image = null;
            loadedAudio = new byte[] { };
            isOgg = false;
            audioList.Nodes.Clear();
            texList.Items.Clear();
            audioList.BeginUpdate();
            texList.BeginUpdate();
            TXTR.Clear();
            AUDO.Clear();
            statusProgress.Value = 0;
            this.Enabled = false;
            this.Cursor = Cursors.WaitCursor;
            BinaryReader reader;
            byte[] file = { };
            if (!path.EndsWith(".exe"))
            {
                if (File.Exists(path + "\\data.win"))
                {
                    file = File.ReadAllBytes(path + "\\data.win");
                }
                else if (File.Exists(path + "\\game.win"))
                {
                    file = File.ReadAllBytes(path + "\\game.win");
                }
            }
            else
            {
                reader = new BinaryReader(new FileStream(path, FileMode.Open));
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    uint currentOffset = (uint)reader.BaseStream.Position;
                    string FORMmagic = string.Join("", reader.ReadChars(4));
                    reader.BaseStream.Seek(4, SeekOrigin.Current);
                    string GEN8magic = string.Join("", reader.ReadChars(4));
                    if (FORMmagic == "FORM" && GEN8magic == "GEN8")
                    {
                        reader.BaseStream.Seek(currentOffset + 4, SeekOrigin.Begin);
                        uint fileLength = reader.ReadUInt32() + 8;
                        reader.BaseStream.Seek(currentOffset, SeekOrigin.Current);
                        file = reader.ReadBytes((int)fileLength);
                        break;
                    }
                }
            }
            reader = new BinaryReader(new MemoryStream(file));
            int texOffset = 0x8;
            reader.BaseStream.Seek(0x10, SeekOrigin.Begin);
            uint version = reader.ReadUInt32();
            uint projnameoffset = reader.ReadUInt32();
            reader.BaseStream.Seek(projnameoffset - 4, SeekOrigin.Begin);
            uint projnamelength = reader.ReadUInt32();
            string projname = string.Join("", reader.ReadChars((int)projnamelength));
            if (version == 4097 && projname != "NXTALE" && projname != "DELTARUNE")
            {
                texOffset = 0x4;
            }
            this.Text = "GM2Explorer - Reading game " + projname;
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                uint currentOffset = (uint)reader.BaseStream.Position;
                byte[] magicBytes = reader.ReadBytes(4);
                string magic = string.Join("", Encoding.UTF8.GetChars(magicBytes));
                if (magic == "TXTR") //TXTR
                {
                    //Console.WriteLine("Found TXTR Section at 0x" + currentOffset.ToString("X8"));
                    uint size = reader.ReadUInt32() + 8;
                    uint fileCount = reader.ReadUInt32();
                    uint fileListStart = (uint)reader.BaseStream.Position;
                    statusProgress.Value = 0;
                    statusProgress.Maximum = (int)fileCount;
                    List<uint> fileOffsets = new List<uint>();
                    for (int f = 0; f < fileCount; f++)
                    {
                        reader.BaseStream.Seek(fileListStart + (f * 4), SeekOrigin.Begin);
                        uint texDataOffset = reader.ReadUInt32();
                        reader.BaseStream.Seek(texDataOffset + texOffset, SeekOrigin.Begin);
                        fileOffsets.Add(reader.ReadUInt32());
                    }
                    for (int f = 0; f < fileOffsets.Count; f++)
                    {
                        statusProgress.Value++;
                        //Console.WriteLine("Reading texture " + f + " at offset 0x" + fileOffsets[f].ToString("X8"));
                        List<byte> tex = new List<byte>();
                        reader.BaseStream.Seek(fileOffsets[f], SeekOrigin.Begin);
                        while (reader.BaseStream.Position < reader.BaseStream.Length)
                        {
                            tex.Add(reader.ReadByte());
                            if (tex[tex.Count - 1] == 0x82)
                            {
                                if (tex[tex.Count - 4] == 0xAE && tex[tex.Count - 3] == 0x42 && tex[tex.Count - 2] == 0x60)
                                {
                                    break;
                                }
                            }
                        }
                        try
                        {
                            MemoryStream stream = new MemoryStream(tex.ToArray());
                            Image img = Image.FromStream(stream);
                            TXTR.Add(new Bitmap(img));
                            stream.Dispose();
                            img.Dispose();
                            tex.Clear();
                        } catch { }
                    }
                    fileOffsets.Clear();
                    reader.BaseStream.Seek(currentOffset + 4, SeekOrigin.Begin);
                }
                if (magic == "AUDO") //AUDO
                {
                    audioList.Nodes.Add("data.win");
                    AUDOstruct audo;
                    audo.fileName = "data.win";
                    audo.files = new List<byte[]>();
                    Console.WriteLine("Found AUDO Section at 0x" + currentOffset.ToString("X8"));
                    uint size = reader.ReadUInt32() + 8;
                    uint fileCount = reader.ReadUInt32();
                    uint fileListStart = (uint)reader.BaseStream.Position;
                    statusProgress.Value = 0;
                    statusProgress.Maximum = (int)fileCount;
                    List<uint> fileOffsets = new List<uint>();
                    for (int f = 0; f < fileCount; f++)
                    {
                        uint audoDataOffset = reader.ReadUInt32();
                        fileOffsets.Add(audoDataOffset);
                    }
                    for (int f = 0; f < fileOffsets.Count; f++)
                    {
                        reader.BaseStream.Seek(fileOffsets[f], SeekOrigin.Begin);
                        statusProgress.Value++;
                        //Console.WriteLine("Reading audio " + f + " at offset 0x" + fileOffsets[f].ToString("X8"));
                        List<byte> audio = new List<byte>();
                        uint fileLength = reader.ReadUInt32();
                        audio.AddRange(reader.ReadBytes((int)fileLength));
                        audo.files.Add(audio.ToArray());
                        audio.Clear();
                    }
                    AUDO.Add(audo);
                    fileOffsets.Clear();
                    for (int f = 0; f < audo.files.Count; f++)
                    {
                        audioList.Nodes[0].Nodes.Add("audio_" + f);
                    }
                    break;
                }
            }
            reader.Dispose();
            if (path.EndsWith(".exe"))
            {
                path = path.Replace("\\" + path.Split('\\').Last(), "");
            }
            string[] audiogroups = Directory.GetFiles(path, "audiogroup*.dat");
            for (int i = 0; i < audiogroups.Length; i++)
            {
                this.Text = "GM2Explorer - Reading \"" + path + "\\" + audiogroups[i].Replace(path + "\\", "") + "\"";
                reader = new BinaryReader(new FileStream(audiogroups[i], FileMode.Open));
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    uint currentOffset = (uint)reader.BaseStream.Position;
                    byte[] magicBytes = reader.ReadBytes(4);
                    string magic = string.Join("", Encoding.UTF8.GetChars(magicBytes));
                    if (magic == "AUDO") //AUDO
                    {
                        AUDOstruct audo;
                        audioList.Nodes.Add(audiogroups[i].Replace(path + "\\", ""));
                        audo.fileName = audiogroups[i].Replace(path, "");
                        audo.files = new List<byte[]>();
                        //Console.WriteLine("Found AUDO Section at 0x" + currentOffset.ToString("X8"));
                        uint size = reader.ReadUInt32() + 8;
                        uint fileCount = reader.ReadUInt32();
                        uint fileListStart = (uint)reader.BaseStream.Position;
                        statusProgress.Value = 0;
                        statusProgress.Maximum = (int)fileCount;
                        List<uint> fileOffsets = new List<uint>();
                        for (int f = 0; f < fileCount; f++)
                        {
                            uint audoDataOffset = reader.ReadUInt32();
                            fileOffsets.Add(audoDataOffset);
                        }
                        for (int f = 0; f < fileOffsets.Count; f++)
                        {
                            reader.BaseStream.Seek(fileOffsets[f], SeekOrigin.Begin);
                            statusProgress.Value++;
                            //Console.WriteLine("Reading audio " + f + " at offset 0x" + fileOffsets[f].ToString("X8"));
                            List<byte> audio = new List<byte>();
                            uint fileLength = reader.ReadUInt32();
                            audio.AddRange(reader.ReadBytes((int)fileLength));
                            audo.files.Add(audio.ToArray());
                            audio.Clear();
                        }
                        AUDO.Add(audo);
                        for (int f = 0; f < audo.files.Count; f++)
                        {
                            audioList.Nodes[audioList.Nodes.Count - 1].Nodes.Add("audio_" + f);
                        }
                        break;
                    }
                }
            }
            reader.Dispose();
            for (int i = 0; i < TXTR.Count; i++)
            {
                texList.Items.Add("tex_" + i);
            }
            this.Text = "GM2Explorer";
            this.Cursor = Cursors.Default;
            this.Enabled = true;
            this.BringToFront();
            audioList.EndUpdate();
            texList.EndUpdate();
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
            Bitmap image = TXTR[texList.SelectedIndex];
            textureDisplay.Image = image;
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
                statusProgress.Value = 0;
                statusProgress.Maximum = TXTR.Count;
                for (int i = 0; i < TXTR.Count; i++)
                {
                    statusProgress.Value = i + 1;
                    TXTR[i].Save(Path.GetDirectoryName(save.FileName) + "\\tex_" + i + ".png", System.Drawing.Imaging.ImageFormat.Png);
                }
            }
        }

        private void audioList_AfterSelect(object sender, TreeViewEventArgs e)
        {
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

                    loadedAudio = AUDO[audioList.SelectedNode.Parent.Index].files[audioList.SelectedNode.Index];
                    wavOutput = new WaveOutEvent();
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
                    wavOutput.Stop();
                }
                else
                {
                    playPause.Text = "Stop";
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
            loadedAudio = AUDO[audioList.SelectedNode.Parent.Index].files[audioList.SelectedNode.Index];
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
            if (save.ShowDialog() == DialogResult.OK)
            {
                for (int i = 0; i < AUDO.Count; i++)
                {
                    string dir = Path.GetDirectoryName(save.FileName) + "\\" + AUDO[i].fileName;
                    if (Directory.Exists(dir))
                    {
                        Directory.Delete(dir, true);
                    }
                    Directory.CreateDirectory(dir);
                    statusProgress.Value = 0;
                    statusProgress.Maximum = AUDO[i].files.Count;
                    for (int f = 0; f < AUDO[i].files.Count; f++)
                    {
                        statusProgress.Value++;
                        if (BitConverter.ToUInt32(AUDO[i].files[f], 0) == 0x5367674F)
                        {
                            File.WriteAllBytes(dir + "\\audio_" + f + ".ogg", AUDO[i].files[f]);
                        }
                        else if (BitConverter.ToUInt32(AUDO[i].files[f], 0) == 0x46464952)
                        {
                            File.WriteAllBytes(dir + "\\audio_" + f + ".wav", AUDO[i].files[f]);
                        }
                    }
                }
            }
        }

        private void replaceTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Portable Network Graphic Image Files|*.png";
            open.DefaultExt = ".png";
            if (open.ShowDialog() == DialogResult.OK)
            {
                /*if (File.Exists(open.FileName))
                {
                    Bitmap bitmap = new Bitmap(open.FileName);
                    if (bitmap.Height == TXTR[texList.SelectedIndex].Height && bitmap.Width == TXTR[texList.SelectedIndex].Width)
                    {
                        TXTR[texList.SelectedIndex].Dispose();
                        TXTR[texList.SelectedIndex] = bitmap;
                        MessageBox.Show("Successfully replaced texture", "GM2Explorer");
                    }
                    bitmap.Dispose();
                }*/
            }
        }
    }
}
