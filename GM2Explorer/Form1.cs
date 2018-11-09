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

        private AudioFileReader wavFile;
        private WaveOutEvent wavOutput;
        private VorbisWaveReader vorbisFile;
        byte[] loadedAudio;
        bool isOgg = false;

        public Form1()
        {
            InitializeComponent();
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
            reader.BaseStream.Seek(0x3C, SeekOrigin.Begin);
            uint version = reader.ReadUInt32();
            reader.BaseStream.Seek(0x14, SeekOrigin.Begin);
            uint projnameoffset = reader.ReadUInt32();
            reader.BaseStream.Seek(projnameoffset - 4, SeekOrigin.Begin);
            uint projnamelength = reader.ReadUInt32();
            string projname = string.Join("", reader.ReadChars((int)projnamelength));
            if (version == 1)
            {
                texOffset = 0x4;
            }
            this.Text = "GM2Explorer - Reading game " + projname;
            //Begin texture and audio search
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
                    AudioGroup audo;
                    audo.name = "";
                    audo.fileNames = new List<string>();
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
                    AudioGroupList.Add(audo);
                    fileOffsets.Clear();
                    break;
                }
            }
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            //Begin sprite search
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                uint currentOffset = (uint)reader.BaseStream.Position;
                byte[] magicBytes = reader.ReadBytes(4);
                string magic = string.Join("", Encoding.UTF8.GetChars(magicBytes));
                if (magic == "SPRT")
                {
                    uint size = reader.ReadUInt32();
                    uint spriteCount = reader.ReadUInt32();
                    statusProgress.Value = 0;
                    statusProgress.Maximum = (int)spriteCount;
                    List<uint> spriteOffsets = new List<uint>();
                    for (int i = 0; i < spriteCount; i++)
                    {
                        spriteOffsets.Add(reader.ReadUInt32());
                    }
                    for (int i = 0; i < spriteCount; i++)
                    {
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
                            try
                            {
                                reader.BaseStream.Seek(texOffsets[t], SeekOrigin.Begin);
                                sprt.x.Add(reader.ReadUInt16());
                                sprt.y.Add(reader.ReadUInt16());
                                sprt.width.Add(reader.ReadUInt16());
                                sprt.height.Add(reader.ReadUInt16());
                                reader.BaseStream.Seek(0xC, SeekOrigin.Current);
                                sprt.sheet.Add(reader.ReadUInt16());
                            }
                            catch
                            {
                                Console.WriteLine("Something happened!!\ntex:" + t + "\nCurrent position: 0x" + reader.BaseStream.Position.ToString("X8"));
                            }
                        }
                        reader.BaseStream.Seek(nameOffset - 4, SeekOrigin.Begin);
                        uint nameLength = reader.ReadUInt32();
                        string name = string.Join("", reader.ReadChars((int)nameLength));
                        sprt.name = name;
                        SPRTList.Add(sprt);
                        statusProgress.Value++;
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
                        AudioGroup audo;
                        audo.name = "";
                        audo.fileNames = new List<string>();
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
                        AudioGroupList.Add(audo);
                        break;
                    }
                }
            }
            reader.Dispose();
            reader = new BinaryReader(new MemoryStream(file));
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                uint currentOffset = (uint)reader.BaseStream.Position;
                byte[] magicBytes = reader.ReadBytes(4);
                string magic = string.Join("", Encoding.UTF8.GetChars(magicBytes));
                if (magic == "AGRP")
                {
                    uint size = reader.ReadUInt32() + 8;
                    uint entryCount = reader.ReadUInt32();
                    uint entryListStart = (uint)reader.BaseStream.Position;
                    List<uint> entryOffsets = new List<uint>();
                    for (int i = 0; i < entryCount; i++)
                    {
                        entryOffsets.Add(reader.ReadUInt32());
                    }
                    for (int i = 0; i < entryCount; i++)
                    {
                        AudioGroup audo = AudioGroupList[i];
                        reader.BaseStream.Seek(entryOffsets[i], SeekOrigin.Begin);
                        uint nameOffset = reader.ReadUInt32();
                        reader.BaseStream.Seek(nameOffset - 4, SeekOrigin.Begin);
                        uint nameLength = reader.ReadUInt32();
                        string groupName = string.Join("", reader.ReadChars((int)nameLength));
                        audo.name = groupName;
                        AudioGroupList[i] = audo;
                    }
                    break;
                }
            }
            for (int i = 0; i < AudioGroupList.Count; i++)
            {
                for (int f = 0; f < AudioGroupList[i].files.Count; f++)
                {
                    AudioGroupList[i].fileNames.Add("");
                }
            }
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                uint currentOffset = (uint)reader.BaseStream.Position;
                byte[] magicBytes = reader.ReadBytes(4);
                string magic = string.Join("", Encoding.UTF8.GetChars(magicBytes));
                if (magic == "SOND")
                {
                    uint size = reader.ReadUInt32() + 8;
                    uint entryCount = reader.ReadUInt32();
                    uint entryListStart = (uint)reader.BaseStream.Position;
                    List<uint> entryOffsets = new List<uint>();
                    for (int i = 0; i < entryCount; i++)
                    {
                        entryOffsets.Add(reader.ReadUInt32());
                    }
                    for (int i = 0; i < entryCount; i++)
                    {
                        reader.BaseStream.Seek(entryOffsets[i], SeekOrigin.Begin);
                        uint nameOffset = reader.ReadUInt32();
                        long pos = reader.BaseStream.Position;
                        reader.BaseStream.Seek(nameOffset - 4, SeekOrigin.Begin);
                        uint nameLength = reader.ReadUInt32();
                        string audioName = string.Join("", reader.ReadChars((int)nameLength));
                        reader.BaseStream.Seek(pos + 0x18, SeekOrigin.Begin);
                        int groupIndex = reader.ReadInt32();
                        int audioIndex = reader.ReadInt32();
                        if (audioIndex != -1)
                        {
                            AudioGroupList[groupIndex].fileNames[audioIndex] = audioName;
                        }
                    }
                    break;
                }
            }
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
            this.Text = "GM2Explorer";
            this.Cursor = Cursors.Default;
            this.Enabled = true;
            this.BringToFront();
            audioList.EndUpdate();
            texList.EndUpdate();
            spriteList.EndUpdate();
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

                    loadedAudio = AudioGroupList[audioList.SelectedNode.Parent.Index].files[audioList.SelectedNode.Index];
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
            if (save.ShowDialog() == DialogResult.OK)
            {
                for (int i = 0; i < AudioGroupList.Count; i++)
                {
                    string dir = Path.GetDirectoryName(save.FileName) + "\\" + AudioGroupList[i].name;
                    if (Directory.Exists(dir))
                    {
                        Directory.Delete(dir, true);
                    }
                    Directory.CreateDirectory(dir);
                    statusProgress.Value = 0;
                    statusProgress.Maximum = AudioGroupList[i].files.Count;
                    for (int f = 0; f < AudioGroupList[i].files.Count; f++)
                    {
                        statusProgress.Value++;
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
            }
        }

        private void spriteList_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                SPRT sprt = SPRTList[spriteList.SelectedIndex];
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
        }

        private void spriteNum_ValueChanged(object sender, EventArgs e)
        {
            SPRT sprt = SPRTList[spriteList.SelectedIndex];
            Bitmap image = Crop(TXTR[sprt.sheet[(int)spriteNum.Value]], new Rectangle((int)sprt.x[(int)spriteNum.Value], (int)sprt.y[(int)spriteNum.Value], (int)sprt.width[(int)spriteNum.Value], (int)sprt.height[(int)spriteNum.Value]));
            spriteDisplay.Image = image;
        }

        private void exportSpriteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "PNG Image File|*.png";
            save.DefaultExt = ".png";
            save.AddExtension = true;
            save.FileName = SPRTList[spriteList.SelectedIndex].name + "_" + spriteNum.Value + ".png";
            if (save.ShowDialog() == DialogResult.OK)
            {
                SPRT sprt = SPRTList[spriteList.SelectedIndex];
                Crop(TXTR[sprt.sheet[(int)spriteNum.Value]], new Rectangle((int)sprt.x[(int)spriteNum.Value], (int)sprt.y[(int)spriteNum.Value], (int)sprt.width[(int)spriteNum.Value], (int)sprt.height[(int)spriteNum.Value])).Save(save.FileName, System.Drawing.Imaging.ImageFormat.Png);
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
            if (save.ShowDialog() == DialogResult.OK)
            {
                statusProgress.Value = 0;
                statusProgress.Maximum = SPRTList.Count;
                for (int i = 0; i < SPRTList.Count; i++)
                {
                    SPRT sprt = SPRTList[i];
                    for (int t = 0; t < SPRTList[i].sheet.Count; t++)
                    {
                        Crop(TXTR[sprt.sheet[t]], new Rectangle((int)sprt.x[t], (int)sprt.y[t], (int)sprt.width[t], (int)sprt.height[t])).Save(Path.GetDirectoryName(save.FileName) + "\\" + SPRTList[i].name + "_" + t + ".png", System.Drawing.Imaging.ImageFormat.Png);
                    }
                    statusProgress.Value = i + 1;
                }
            }
        }
    }
}
