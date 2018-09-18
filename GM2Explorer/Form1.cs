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

        public Form1()
        {
            InitializeComponent();
        }

        public void ReadData(string path)
        {
            if (wavOutput != null) wavOutput.Dispose();
            if (wavFile != null) wavFile.Dispose();
            if (vorbisFile != null) vorbisFile.Dispose();
            loadedAudio = new byte[] { };
            isOgg = false;
            audioList.Nodes.Clear();
            texList.Items.Clear();
            TXTR.Clear();
            AUDO.Clear();
            int texOffset = 0x8;
            statusProgress.Value = 0;
            this.Enabled = false;
            this.Cursor = Cursors.WaitCursor;
            byte[] file = { };
            if (File.Exists(path + "\\data.win"))
            {
                file = File.ReadAllBytes(path + "\\data.win");
            }
            else
            {
                string[] exes = Directory.GetFiles(path, path.Split('\\').Last().Replace(" ", "") + ".exe");
                file = File.ReadAllBytes(exes[0]);
                for (int i = 0; i < file.Length; i++)
                {
                    uint FORMmagic = BitConverter.ToUInt32(file, i);
                    uint GEN8magic = BitConverter.ToUInt32(file, i + 0x8);
                    if (FORMmagic == 1297239878 && GEN8magic == 944653639)
                    {
                        file = file.Skip(i).ToArray();
                        break;
                    }
                }
            }
            switch (BitConverter.ToUInt32(file, 0x10))
            {
                case 4353:
                    {
                        texOffset = 0x8;
                        break;
                    }
                case 4097:
                    {
                        texOffset = 0x4;
                        break;
                    }
            }
            this.Text = "GM2Explorer - Reading \"" + path + "\\data.win\"";
            for (int i = 0; i < file.Length; i++)
            {
                uint magic = 0;
                if (i < file.Length - 3)
                {
                    magic = BitConverter.ToUInt32(file, i);
                }
                if (magic == 1381259348) //TXTR
                {
                    //Console.WriteLine("Found TXTR Section at 0x" + i.ToString("X8"));
                    uint size = BitConverter.ToUInt32(file, i + 0x4);
                    uint fileCount = BitConverter.ToUInt32(file, i + 0x8);
                    statusProgress.Value = 0;
                    statusProgress.Maximum = (int)fileCount;
                    List<uint> fileOffsets = new List<uint>();
                    for (int f = 0; f < fileCount; f++)
                    {
                        uint texDataOffset = BitConverter.ToUInt32(file, i + 0xC + (f * 0x4));
                        fileOffsets.Add(BitConverter.ToUInt32(file, (int)texDataOffset + texOffset));
                    }
                    for (int f = 0; f < fileOffsets.Count; f++)
                    {
                        statusProgress.Value++;
                        //Console.WriteLine("Reading texture " + f + " at offset 0x" + fileOffsets[f].ToString("X8"));
                        List<byte> tex = new List<byte>();
                        if (f < fileOffsets.Count - 1)
                        {
                            tex.AddRange(file.Skip((int)fileOffsets[f]).Take((int)fileOffsets[f + 1] - (int)fileOffsets[f]));
                        }
                        else
                        {
                            tex.AddRange(file.Skip((int)fileOffsets[f]).Take(((int)size + i) - (int)fileOffsets[f]));
                        }
                        MemoryStream stream = new MemoryStream(tex.ToArray());
                        Image img = Image.FromStream(stream);
                        TXTR.Add(new Bitmap(img));
                        stream.Dispose();
                        img.Dispose();
                        tex.Clear();
                    }
                    fileOffsets.Clear();
                }
                else if (magic == 1329878337) //AUDO
                {
                    audioList.Nodes.Add("data.win");
                    AUDOstruct audo;
                    audo.fileName = "data.win";
                    audo.files = new List<byte[]>();
                    //Console.WriteLine("Found TXTR Section at 0x" + i.ToString("X8"));
                    uint size = BitConverter.ToUInt32(file, i + 0x4);
                    uint fileCount = BitConverter.ToUInt32(file, i + 0x8);
                    statusProgress.Value = 0;
                    statusProgress.Maximum = (int)fileCount;
                    List<uint> fileOffsets = new List<uint>();
                    for (int f = 0; f < fileCount; f++)
                    {
                        uint audoDataOffset = BitConverter.ToUInt32(file, i + 0xC + (f * 0x4));
                        fileOffsets.Add(audoDataOffset);
                    }
                    for (int f = 0; f < fileOffsets.Count; f++)
                    {
                        statusProgress.Value++;
                        //Console.WriteLine("Reading audio " + f + " at offset 0x" + fileOffsets[f].ToString("X8"));
                        List<byte> audio = new List<byte>();
                        uint fileLength = BitConverter.ToUInt32(file, (int)fileOffsets[f]);
                        audio.AddRange(file.Skip((int)fileOffsets[f] + 0x4).Take((int)fileLength));
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
            string[] audiogroups = Directory.GetFiles(path, "audiogroup*.dat");
            for (int i = 0; i < audiogroups.Length; i++)
            {
                this.Text = "GM2Explorer - Reading \"" + path + "\\" + audiogroups[i].Replace(path + "\\", "") + "\"";
                byte[] audiogroup = File.ReadAllBytes(audiogroups[i]);
                for (int b = 0; b < audiogroup.Length; b++)
                {
                    uint magic = 0;
                    if (b < audiogroup.Length - 3)
                    {
                        magic = BitConverter.ToUInt32(audiogroup, b);
                    }
                    if (magic == 1329878337) //AUDO
                    {
                        audioList.Nodes.Add(audiogroups[i].Replace(path + "\\", ""));
                        AUDOstruct audo;
                        audo.fileName = audiogroups[i].Replace(path, "");
                        audo.files = new List<byte[]>();
                        //Console.WriteLine("Found TXTR Section at 0x" + i.ToString("X8"));
                        uint size = BitConverter.ToUInt32(audiogroup, b + 0x4);
                        uint fileCount = BitConverter.ToUInt32(audiogroup, b + 0x8);
                        statusProgress.Value = 0;
                        statusProgress.Maximum = (int)fileCount;
                        List<uint> fileOffsets = new List<uint>();
                        for (int f = 0; f < fileCount; f++)
                        {
                            uint audoDataOffset = BitConverter.ToUInt32(audiogroup, b + 0xC + (f * 0x4));
                            fileOffsets.Add(audoDataOffset);
                        }
                        for (int f = 0; f < fileOffsets.Count; f++)
                        {
                            statusProgress.Value++;
                            //Console.WriteLine("Reading audio " + f + " at offset 0x" + fileOffsets[f].ToString("X8"));
                            List<byte> audio = new List<byte>();
                            uint fileLength = BitConverter.ToUInt32(audiogroup, (int)fileOffsets[f]);
                            audio.AddRange(audiogroup.Skip((int)fileOffsets[f] + 0x4).Take((int)fileLength));
                            audo.files.Add(audio.ToArray());
                            audio.Clear();
                        }
                        AUDO.Add(audo);
                        for (int f = 0; f < audo.files.Count; f++)
                        {
                            audioList.Nodes[i + 1].Nodes.Add("audio_" + f);
                        }
                        break;
                    }
                }
            }
            for (int i = 0; i < TXTR.Count; i++)
            {
                texList.Items.Add("tex_" + i);
            }
            this.Text = "GM2Explorer";
            this.Cursor = Cursors.Default;
            this.Enabled = true;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "GameMaker 2 Data Archives|data.win|EXE Executable Files|*.exe";
            if (open.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(open.FileName))
                {
                    ReadData(Path.GetDirectoryName(open.FileName));
                }
            }
        }

        private void texList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Bitmap texture = TXTR[texList.SelectedIndex];
            textureDisplay.Image = texture;
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
    }
}
