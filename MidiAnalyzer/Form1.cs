/*
MIT License

Copyright (c) 2020 salt26

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Midi;
using DbscanImplementation;

namespace MidiAnalyzer
{
    public partial class Form1 : Form
    {
        private const bool RESOLUTION_64 = true;    // 길이의 최소 단위를 false이면 32분음표로, true이면 64분음표로 사용

        private const int DBSCAN_EPSILON = 5;

        private const int DBSCAN_MINIMUM_POINTS = 2;

        public List<TrackInfo> tracks = new List<TrackInfo>();

        private string filePath = "";
        private int pDuration = 3;
        private int pOnset = 3;
        private int pPitchVariance = 4;
        private int pPitchRank = 1;
        private int pPitchCount = 3;
        private int pEpsilon = 5;
        private int pMinimumPoints = 2;

        public Form1()
        {
            InitializeComponent();
            Task.Run(() => { new Test.SFXTest(); });
        }

        private void Form1_Load(object sender, EventArgs eventArgs)
        {
            //StartFromDirectory();
        }

        private void StartFromDirectory()
        {
            string path = @".\..\..\..\midi";
            if (Directory.Exists(path))
            {
                DirectoryInfo di = new DirectoryInfo(path);

                foreach (var file in di.GetFiles())
                {
                    if (file.Extension.ToLowerInvariant() == ".mid" ||
                        file.Extension.ToLowerInvariant() == ".midi")
                    {
                        Console.WriteLine(file.Name);
                        this.filePath = file.FullName;
                        Task.Run(() => Start());
                    }
                }
            }
        }

        private void Start()
        {
            if (filePath == "") return;

            bool strictMode = true;
            MidiFile mf = new MidiFile(filePath, strictMode);
            string fileName = FilePathToName(filePath);

            Console.WriteLine("Format {0}, Tracks {1}, Delta Ticks Per Quarter Note {2}",
                mf.FileFormat, mf.Tracks, mf.DeltaTicksPerQuarterNote);

            #region Time signature preprocessing

            List<TimeSignatureEvent> timeSignatureList = mf.Events[0].OfType<TimeSignatureEvent>().ToList();
            timeSignatureList.Sort((e1, e2) => Math.Sign(e1.AbsoluteTime - e2.AbsoluteTime));
            if (timeSignatureList.Count == 0 || timeSignatureList[0].AbsoluteTime > 0)
            {
                timeSignatureList.Insert(0, new TimeSignatureEvent(0, 4, 2, 24, 8));
            }
            timeSignatureList.Add(new TimeSignatureEvent(long.MaxValue, 4, 2, 24, 8));

            // 이 아래 코드와 같은 알고리즘을 skyline 구현 시 사용할 수 있다.
            List<int> duplicatedIndices = new List<int>();
            for (int i = 0; i < timeSignatureList.Count - 1; i++)
            {
                if (timeSignatureList[i].AbsoluteTime == timeSignatureList[i + 1].AbsoluteTime)
                {
                    duplicatedIndices.Add(i);
                }
            }
            for (int i = duplicatedIndices.Count - 1; i >= 0; i--)
            {
                timeSignatureList.RemoveAt(duplicatedIndices[i]);
            }

            #endregion

            #region Key signature preprocessing

            List<KeySignatureEvent> keySignatureList = mf.Events[0].OfType<KeySignatureEvent>().ToList();
            keySignatureList.Sort((e1, e2) => Math.Sign(e1.AbsoluteTime - e2.AbsoluteTime));
            if (keySignatureList.Count == 0 || keySignatureList[0].AbsoluteTime > 0)
            {
                keySignatureList.Insert(0, new KeySignatureEvent(0, 0, 0));
            }
            keySignatureList.Add(new KeySignatureEvent(0, 0, long.MaxValue));

            // 이 아래 코드와 같은 알고리즘을 skyline 구현 시 사용할 수 있다.
            duplicatedIndices = new List<int>();
            for (int i = 0; i < keySignatureList.Count - 1; i++)
            {
                if (keySignatureList[i].AbsoluteTime == keySignatureList[i + 1].AbsoluteTime)
                {
                    duplicatedIndices.Add(i);
                }
            }
            for (int i = duplicatedIndices.Count - 1; i >= 0; i--)
            {
                keySignatureList.RemoveAt(duplicatedIndices[i]);
            }

            #endregion

            int measureCount = 0;   // 이 곡의 최장 마디 번호

            // TPB: Ticks per beat (= mf.DeltaTicksPerQuarterNote)
            // 32분음표는 TPB / 8, 한 마디는 TPB * 4 * 분자 / 분모
            // 새로운 박자표가 도입되면 그 앞에서 마디를 끊는다.
            // 박자표가 없거나 여러 개 겹쳐 있는 경우도 처리

            for (int n = 0; n < mf.Tracks; n++)
            {
                #region Load original score

                if (mf.Events[n].OfType<NoteOnEvent>().Count() == 0) continue;
                TrackInfo track = new TrackInfo(fileName, n);

                int measureNum = 0;
                long nextMeasureTime = 0;
                List<long> barTimes = new List<long>();

                int timeSignatureIndex = 0;
                KeyValuePair<int, int> timeSignature = new KeyValuePair<int, int>(
                    timeSignatureList[0].Numerator,
                    1 << timeSignatureList[0].Denominator);
                long nextTimeSignatureTime = timeSignatureList[1].AbsoluteTime;

                int keySignatureIndex = 0;
                long nextKeySignatureTime = keySignatureList[1].AbsoluteTime;

                foreach (var midiEvent in mf.Events[n])
                {
                    if (MidiEvent.IsNoteOn(midiEvent))
                    {
                        while ((midiEvent.AbsoluteTime >= nextTimeSignatureTime && timeSignatureIndex < timeSignatureList.Count - 1) ||
                            midiEvent.AbsoluteTime >= nextMeasureTime)
                        {
                            bool b = false;
                            if (midiEvent.AbsoluteTime >= nextMeasureTime)
                            {
                                measureNum++;
                                barTimes.Add(nextMeasureTime);

                                Measure m = new Measure();
                                m.songName = track.songName;
                                m.trackNum = track.trackNum;
                                m.measureNum = measureNum;
                                m.startTime = nextMeasureTime;
                                m.timeSignature = timeSignature;
                                m.melodicContourID = 0;

                                while (m.startTime >= nextKeySignatureTime && keySignatureIndex <= keySignatureList.Count - 3)
                                {
                                    keySignatureIndex++;
                                    nextKeySignatureTime = keySignatureList[keySignatureIndex + 1].AbsoluteTime;
                                }
                                m.key = Measure.GetKeyFromKeySignature(keySignatureList[keySignatureIndex]);

                                nextMeasureTime += mf.DeltaTicksPerQuarterNote * 4 * timeSignature.Key / timeSignature.Value;

                                m.endTime = nextMeasureTime;
                                track.measures.Add(m);

                                b = true;
                            }
                            if (midiEvent.AbsoluteTime >= nextTimeSignatureTime && timeSignatureIndex <= timeSignatureList.Count - 3)
                            {
                                timeSignatureIndex++;
                                nextTimeSignatureTime = timeSignatureList[timeSignatureIndex + 1].AbsoluteTime;
                                if (b)
                                {
                                    nextMeasureTime -= mf.DeltaTicksPerQuarterNote * 4 * timeSignature.Key / timeSignature.Value;
                                }
                                timeSignature = new KeyValuePair<int, int>(timeSignatureList[timeSignatureIndex].Numerator,
                                    1 << timeSignatureList[timeSignatureIndex].Denominator);

                                if (b)
                                {
                                    nextMeasureTime += mf.DeltaTicksPerQuarterNote * 4 * timeSignature.Key / timeSignature.Value;

                                    track.measures.Last().timeSignature = timeSignature;
                                    track.measures.Last().endTime = nextMeasureTime;
                                }
                                if (!b)
                                {
                                    // 마디 중간에 박자표가 튀어나오는 경우 박자표 앞에 세로선을 그어 마디를 구분한다.
                                    // 이로써 한 마디 안에는 하나의 박자표만 적용됨을 보장한다.
                                    measureNum++;
                                    barTimes.Add(timeSignatureList[timeSignatureIndex].AbsoluteTime);

                                    track.measures.Last().endTime = timeSignatureList[timeSignatureIndex].AbsoluteTime;

                                    Measure m = new Measure();
                                    m.songName = track.songName;
                                    m.trackNum = track.trackNum;
                                    m.measureNum = measureNum;
                                    m.startTime = timeSignatureList[timeSignatureIndex].AbsoluteTime;
                                    m.timeSignature = timeSignature;

                                    while (m.startTime >= nextKeySignatureTime && keySignatureIndex <= keySignatureList.Count - 3)
                                    {
                                        keySignatureIndex++;
                                        nextKeySignatureTime = keySignatureList[keySignatureIndex + 1].AbsoluteTime;
                                    }
                                    m.key = Measure.GetKeyFromKeySignature(keySignatureList[keySignatureIndex]);

                                    nextMeasureTime = timeSignatureList[timeSignatureIndex].AbsoluteTime +
                                        mf.DeltaTicksPerQuarterNote * 4 * timeSignature.Key / timeSignature.Value;

                                    m.endTime = nextMeasureTime;
                                    track.measures.Add(m);
                                }
                            }
                        }
                        NoteOnEvent noteOn = (NoteOnEvent)midiEvent;
                        Note note;
                        if (RESOLUTION_64)
                        {
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                            note = new Note(
                                noteOn.NoteNumber,
                                noteOn.Velocity,
                                (int)Math.Round(noteOn.NoteLength / (mf.DeltaTicksPerQuarterNote / 16f)),
                                measureNum,
                                (int)Math.Round((noteOn.AbsoluteTime - barTimes.Last()) / (mf.DeltaTicksPerQuarterNote / 16f)),
                                noteOn.Channel,
                                timeSignature.Key,
                                timeSignature.Value);
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
                        }
                        else
                        {
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                            note = new Note(
                                noteOn.NoteNumber,
                                noteOn.Velocity,
                                (int)Math.Round(noteOn.NoteLength / (mf.DeltaTicksPerQuarterNote / 8f)) * 2,
                                measureNum,
                                (int)Math.Round((noteOn.AbsoluteTime - barTimes.Last()) / (mf.DeltaTicksPerQuarterNote / 8f)) * 2,
                                noteOn.Channel,
                                timeSignature.Key,
                                timeSignature.Value);
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
                        }
                        track.score.Add(note);
                        track.measures.Last().originalScore.Add(note);
                    }

                    /*
                    if (!MidiEvent.IsNoteOff(midiEvent))
                    {
                        var timeSignature2 = mf.Events[0].OfType<TimeSignatureEvent>().FirstOrDefault();
                        Console.WriteLine("{0} {1}\r\n", ToMBT(midiEvent.AbsoluteTime, mf.DeltaTicksPerQuarterNote, timeSignature2), midiEvent);
                    }
                    */
                }

                if (measureCount < measureNum)
                {
                    measureCount = measureNum;
                }

                Console.WriteLine(track.songName + ": track " + track.trackNum);
                /*
                foreach (Note note in track.score)
                {
                    Console.WriteLine(note.ToString());
                }
                Console.WriteLine("------------------------------------------------");
                foreach (Measure measure in track.measures)
                {
                    Console.WriteLine(measure.ToString());
                }
                Console.WriteLine("------------------------------------------------");
                */

                #endregion

                foreach (Measure measure in track.measures)
                {
                    #region Construct monophonic score

                    foreach (Note note in measure.originalScore)
                    {
                        Note note2 = measure.monophonicScore.Find(e => e.Measure == note.Measure && e.Position == note.Position);
                        if (note2 != null && note.Pitch * 4 + note.Velocity > note2.Pitch * 4 + note2.Velocity)
                        {
                            // 음 높이와 음 세기를 모두 고려하여, monophonic melody에 남길 음표 선정
                            measure.monophonicScore.Remove(note2);
                            measure.monophonicScore.Add(note);
                        }
                        else if (note2 == null)
                        {
                            measure.monophonicScore.Add(note);
                        }
                    }
                    measure.monophonicScore.Sort((e1, e2) =>
                    {
                        if (e1.Measure != e2.Measure) return Math.Sign(e1.Measure - e2.Measure);
                        else return e1.Position - e2.Position;
                    });

                    /*
                    foreach (Note note in measure.monophonicScore)
                    {
                        Console.WriteLine(note.ToString());
                    }
                    */

                    #endregion

                    #region Consturct melodic contour

                    measure.melodicContour = new MelodicContour(measure.monophonicScore,
                        64 * measure.timeSignature.Key / measure.timeSignature.Value);
                    //measure.melodicContour.Print();
                    measure.melodicContourOutput = measure.melodicContour.PrintToString();

                    #endregion

                    //Console.WriteLine("-------------");
                }

                tracks.Add(track);
            }

            foreach (TrackInfo track in tracks.FindAll(e => e.songName == FileNameWithoutExtension(fileName)))
            {
                track.measureCount = measureCount;
            }


            #region Recognize chord

            for (int m = 0; m < measureCount; m++)
            {
                List<Note> scoreFromAllTracks = new List<Note>();
                foreach (TrackInfo track in tracks.FindAll(e => e.songName == FileNameWithoutExtension(fileName)))
                {
                    if (m >= track.measures.Count) break;
                    scoreFromAllTracks.AddRange(track.measures[m].originalScore);
                }

                Chord chord = Chord.RecognizeChordFromScore(scoreFromAllTracks);

                foreach (TrackInfo track in tracks.FindAll(e => e.songName == FileNameWithoutExtension(fileName)))
                {
                    if (m >= track.measures.Count) break;
                    track.measures[m].chord = chord;
                }

                if (chord.type != Chord.Type.NULL)
                    Console.WriteLine("chord of measure " + (m + 1) + ": " + chord.root + chord.type);
            }

            #endregion

            #region DBSCAN clustering for melodic contour

            for (int n = 0; n < tracks.Count; n++)
            {
                TrackInfo track = tracks[n];

                Console.WriteLine();
                Console.WriteLine(track.songName + ": track " + track.trackNum);

                List<KeyValuePair<int, MelodicContour>> melodicContourData = new List<KeyValuePair<int, MelodicContour>>();

                foreach (Measure measure in track.measures)
                {
                    if (measure.monophonicScore.Count > 0)
                        melodicContourData.Add(new KeyValuePair<int, MelodicContour>(measure.measureNum, measure.melodicContour));
                }


                DbscanAlgorithm<KeyValuePair<int, MelodicContour>> dbscan =
                    new DbscanAlgorithm<KeyValuePair<int, MelodicContour>>((e1, e2) => e1.Value.Distance(e2.Value));

                DbscanResult<KeyValuePair<int, MelodicContour>> result = dbscan.ComputeClusterDbscan(
                    melodicContourData.ToArray(), DBSCAN_EPSILON, DBSCAN_MINIMUM_POINTS);               // DBSCAN parameter setting

                track.dbscanResult = result;

                foreach (var p in result.Clusters)
                {
                    Console.WriteLine("Cluster " + p.Key);
                    string clusterOutput = "";
                    if (p.Value.Count() > 0)
                    {
                        track.representatives[p.Key] = p.Value[0].Feature;
                        foreach (var point in p.Value)
                        {
                            clusterOutput += point.Feature.Key + ", ";
                        }
                        clusterOutput = clusterOutput.Substring(0, clusterOutput.LastIndexOf(','));
                        clusterOutput += "번 마디 포함\n";
                    }
                    foreach (var point in p.Value)
                    {
                        track.measures.Find(e => e.measureNum == point.Feature.Key).melodicContourID = p.Key;
                        //Console.WriteLine("Measure " + point.Feature.Key + ": " + point.PointType);
                        //point.Feature.Value.Print();

                        clusterOutput += "----------------------------------------\n";
                        clusterOutput += point.Feature.Key + "번 마디";
                        if (track.representatives[p.Key].Key == point.Feature.Key)
                        {
                            clusterOutput += " (대표 멜로디 형태)";
                        }
                        clusterOutput += "\n";
                        clusterOutput += point.Feature.Value.PrintToString();
                    }
                    track.clusterOutputs[p.Key] = clusterOutput;
                    Console.WriteLine(clusterOutput);
                    //Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~");
                }

                Console.WriteLine("Noise");
                int uniqueID = -1 * (result.Clusters.Count + 1);
                string clusterOutput2 = "";

                foreach (var point in result.Noise)
                {
                    clusterOutput2 += point.Feature.Key + ", ";
                }
                clusterOutput2 = clusterOutput2.Substring(0, clusterOutput2.LastIndexOf(','));
                clusterOutput2 += "번 마디 포함\n";

                foreach (var point in result.Noise)
                {
                    // No cluster
                    track.measures.Find(e => e.measureNum == point.Feature.Key).melodicContourID = uniqueID;
                    //Console.WriteLine("Measure " + point.Feature.Key + " -> " + uniqueID);
                    //point.Feature.Value.Print();

                    clusterOutput2 += "----------------------------------------\n";
                    clusterOutput2 += point.Feature.Key + "번 마디\n";
                    clusterOutput2 += point.Feature.Value.PrintToString();

                    uniqueID--;
                }
                track.clusterOutputs[0] = clusterOutput2;
                Console.WriteLine(clusterOutput2);

                Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~");

                List<int> melodicContourSequence = new List<int>();
                foreach (Measure m in track.measures)
                {
                    Console.Write(m.melodicContourID + " ");
                    melodicContourSequence.Add(m.melodicContourID);
                }

                Console.WriteLine();

                Console.WriteLine("Repeated Melodic Contour ID Sequences with 4 or more length");
                List<List<int>> rs = LongestRepeatedSubstrings(melodicContourSequence, 4);
                foreach (List<int> s in rs)
                {
                    for (int i = 0; i < s.Count; i++)
                    {
                        Console.Write(s[i] + " ");
                    }
                    Console.WriteLine();
                }
            }

            #endregion

            Console.WriteLine("Analysis terminated.");

        }

        private string ToMBT(long eventTime, int ticksPerQuarterNote, TimeSignatureEvent timeSignature)
        {
            int beatsPerBar = timeSignature == null ? 4 : timeSignature.Numerator;
            int ticksPerBar = timeSignature == null ? ticksPerQuarterNote * 4 : (timeSignature.Numerator * ticksPerQuarterNote * 4) / (1 << timeSignature.Denominator);
            int ticksPerBeat = ticksPerBar / beatsPerBar;
            long bar = 1 + (eventTime / ticksPerBar);
            long beat = 1 + ((eventTime % ticksPerBar) / ticksPerBeat);
            long tick = eventTime % ticksPerBeat;
            return String.Format("{0}:{1}:{2}", bar, beat, tick);
        }

        public List<List<int>> LongestRepeatedSubstrings(List<int> melodicContourIDSequence, int minLength = 4)
        {
            List<List<int>> results = new List<List<int>>();
            List<int> endIndices = new List<int>();
            for (int i = 0; i <= melodicContourIDSequence.Count - (minLength * 2); i++)
            {
                int maxLength = -1;
                List<int> maxSequence = null;
                for (int j = i + minLength; j <= melodicContourIDSequence.Count - minLength; j++)
                {
                    int length = minLength;
                    bool b = true;
                    bool findAtLeastOne = false;
                    while (b)
                    {
                        if (j + length >= melodicContourIDSequence.Count || i + length >= j)
                        {
                            b = false;
                            length--;
                            break;
                        }

                        for (int k = 0; k < length; k++)
                        {
                            if (melodicContourIDSequence[i + k] != melodicContourIDSequence[j + k])
                            {
                                b = false;
                                length--;
                                break;
                            }
                        }
                        if (b)
                        {
                            findAtLeastOne = true;
                            length++;
                        }
                    }
                    if (findAtLeastOne && length > maxLength)
                    {
                        maxSequence = melodicContourIDSequence.GetRange(i, length);
                        maxLength = length;
                    }
                }

                // Remove duplicates
                if (maxLength >= minLength && !endIndices.Contains(i + maxLength - 1))
                {
                    bool isReplicated = false;
                    foreach (List<int> l in results)
                    {
                        bool isEqual1 = true, isEqual2 = true;
                        for (int j = 0; j < Math.Min(l.Count, maxLength); j++)
                        {
                            if (l[j] != maxSequence[j])
                            {
                                isEqual1 = false;
                            }
                            if (l[l.Count - 1 - j] != maxSequence[maxLength - 1 - j])
                            {
                                isEqual2 = false;
                            }
                            if (!isEqual1 && !isEqual2)
                            {
                                break;
                            }
                        }
                        if (isEqual1 || isEqual2)
                        {
                            isReplicated = true;
                            break;
                        }
                    }
                    if (!isReplicated)
                    {
                        results.Add(maxSequence);
                        endIndices.Add(i + maxLength - 1);
                    }
                }
            }
            return results;
        }

        private void loadMidiButton_Click(object sender, EventArgs e)
        {
            fileTextBox.Clear();
            filePath = "";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog1.FileName;
                analysisStartButton.Enabled = true;
            }
            else
            {
                analysisStartButton.Enabled = false;
            }
            openFileDialog1.FileName = FilePathToName(filePath);
            fileTextBox.Text = FilePathToName(filePath);
        }

        public string FilePathToName(string filePath)
        {
            try
            {
                return filePath.Substring(filePath.LastIndexOf('\\') + 1);
            }
            catch (ArgumentOutOfRangeException)
            {
                return "";
            }
        }
        public string FileNameWithoutExtension(string fileName)
        {
            try
            {
                return fileName.Substring(0, fileName.LastIndexOf('.'));
            }
            catch (ArgumentOutOfRangeException)
            {
                return "";
            }
        }

        private void defaultSettingButton_Click(object sender, EventArgs e)
        {
            pDuration = 3;
            pOnset = 3;
            pPitchVariance = 4;
            pPitchRank = 1;
            pPitchCount = 3;
            pEpsilon = 5;
            pMinimumPoints = 2;

            numericDuration.Value = pDuration;
            numericOnset.Value = pOnset;
            numericPitchVariance.Value = pPitchVariance;
            numericPitchRank.Value = pPitchRank;
            numericPitchCount.Value = pPitchCount;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/salt26/midi-analyzer");
        }

        private void numericDuration_ValueChanged(object sender, EventArgs e)
        {
            pDuration = (int)numericDuration.Value;
        }

        private void numericOnset_ValueChanged(object sender, EventArgs e)
        {
            pOnset = (int)numericOnset.Value;
        }

        private void numericPitchVariance_ValueChanged(object sender, EventArgs e)
        {
            pPitchVariance = (int)numericPitchVariance.Value;
        }

        private void numericPitchRank_ValueChanged(object sender, EventArgs e)
        {
            pPitchRank = (int)numericPitchRank.Value;
        }

        private void numericPitchCount_ValueChanged(object sender, EventArgs e)
        {
            pPitchCount = (int)numericPitchCount.Value;
        }

        private void numericEpsilon_ValueChanged(object sender, EventArgs e)
        {
            pEpsilon = (int)numericEpsilon.Value;
        }

        private void numericMinimumPoints_ValueChanged(object sender, EventArgs e)
        {
            pMinimumPoints = (int)numericMinimumPoints.Value;
        }

        private void analysisStartButton_Click(object sender, EventArgs e)
        {
            Task.Run(() => Start());
            panel1.Visible = false;
            panel1.Enabled = false;
        }
    }
}
