﻿/*
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
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Midi;
using DbscanImplementation;

namespace MidiAnalyzer
{
    public partial class Form1 : Form
    {
        private const bool RESOLUTION_64 = true;    // 길이의 최소 단위를 false이면 32분음표로, true이면 64분음표로 사용

        public static Form1 form;

        public List<TrackInfo> tracks = new List<TrackInfo>();

        private string filePath = "";

        /// <summary>
        /// 편집 연산을 수행한 결과로 음표 또는 맨 앞 쉼표의 길이가 바뀌는 경우 발생하는 비용입니다.
        /// Delete, Insert, Replace, Delay에서만 발생할 수 있습니다.
        /// </summary>
        public int pDurationCost
        {
            get;
            private set;
        } = 3;

        /// <summary>
        /// 편집 연산을 수행한 결과로 음표의 시작 위치가 바뀌는 경우 발생하는 비용입니다.
        /// Move, DelayAndReplace에서만 발생할 수 있습니다.
        /// </summary>
        public int pOnsetCost
        {
            get;
            private set;
        } = 3;

        /// <summary>
        /// 편집 연산을 수행한 결과로 음표의 음 높이 변화가 바뀌는 경우 발생하는 비용입니다.
        /// </summary>
        public int pPitchVarianceCost
        {
            get;
            private set;
        } = 4;

        /// <summary>
        /// 편집 연산을 수행한 결과로 음표의 음 높이 클러스터 순위가 바뀌는 경우 발생하는 비용입니다.
        /// </summary>
        public int pPitchRankCost
        {
            get;
            private set;
        } = 1;

        /// <summary>
        /// 두 멜로디 형태의 음 높이 클러스터 개수 차이에 곱해져서 발생하는 비용입니다.
        /// </summary>
        public int pPitchCountCost
        {
            get;
            private set;
        } = 3;

        private int pDbscanEpsilon = 5;
        private int pDbscanMinimumPoints = 2;

        private MidiFile mf = null;
        private int seletedTrackIndex = -1;

        public Form1()
        {
            if (form == null)
                form = this;
            else return;

            InitializeComponent();
            //Task.Run(() => { new Test.SFXTest(); });
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
                        //Console.WriteLine(file.Name);
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
            try
            {
                mf = new MidiFile(filePath, strictMode);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Invoke(new EventHandler(delegate
                {
                    ShowErrorMessage();
                }));

                return;
            }
            string fileName = FilePathToName(filePath);

            //Console.WriteLine("Format {0}, Tracks {1}, Delta Ticks Per Quarter Note {2}",
            //    mf.FileFormat, mf.Tracks, mf.DeltaTicksPerQuarterNote);

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

                //Console.WriteLine(track.songName + ": track " + track.trackNum);
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
                    measure.melodicContourWithAbsolutePitch = new MelodicContour(measure.monophonicScore,
                        64 * measure.timeSignature.Key / measure.timeSignature.Value, false);
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

                //if (chord.type != Chord.Type.NULL)
                //    Console.WriteLine("chord of measure " + (m + 1) + ": " + chord.ToString());
            }

            #endregion

            Invoke(new EventHandler(delegate
            {
                form.ShowTrackMenu();
            }));

            #region DBSCAN clustering for melodic contour

            for (int n = 0; n < tracks.Count; n++)
            {
                TrackInfo track = tracks[n];
                track.status = TrackInfo.AnalysisStatus.Analyzing;
                Invoke(new EventHandler(delegate
                {
                    form.AfterStartClustering(n);
                }));

                //Console.WriteLine();
                //Console.WriteLine(track.songName + ": track " + track.trackNum);

                List<KeyValuePair<int, MelodicContour>> melodicContourData = new List<KeyValuePair<int, MelodicContour>>();

                foreach (Measure measure in track.measures)
                {
                    if (measure.monophonicScore.Count > 0)
                        melodicContourData.Add(new KeyValuePair<int, MelodicContour>(measure.measureNum, measure.melodicContour));
                }


                DbscanAlgorithm<KeyValuePair<int, MelodicContour>> dbscan =
                    new DbscanAlgorithm<KeyValuePair<int, MelodicContour>>((e1, e2) => e1.Value.Distance(e2.Value));

                DbscanResult<KeyValuePair<int, MelodicContour>> result = dbscan.ComputeClusterDbscan(
                    melodicContourData.ToArray(), pDbscanEpsilon, pDbscanMinimumPoints);               // DBSCAN parameter setting

                track.dbscanResult = result;

                foreach (var p in result.Clusters)
                {
                    //Console.WriteLine("Cluster " + p.Key);

                    string clusterOutput = "";
                    if (p.Value.Count() > 0)
                    {
                        clusterOutput += p.Key + "번 클러스터\n\n";
                        track.representatives[p.Key] = p.Value[0].Feature;
                        int k = 0;
                        foreach (var point in p.Value)
                        {
                            clusterOutput += point.Feature.Key + ", ";
                            k++;
                            if (k % 10 == 0) clusterOutput += "\n";
                        }
                        clusterOutput = clusterOutput.Substring(0, clusterOutput.LastIndexOf(','));
                        clusterOutput += "번 마디 포함\n";
                    }
                    foreach (var point in p.Value)
                    {
                        track.measures.Find(e => e.measureNum == point.Feature.Key).melodicContourID = p.Key;
                        //Console.WriteLine("Measure " + point.Feature.Key + ": " + point.PointType);
                        //point.Feature.Value.Print();

                        clusterOutput += "\n\n";
                        if (track.representatives[p.Key].Key == point.Feature.Key)
                        {
                            clusterOutput += point.Feature.Key + "번 마디의 멜로디 형태 (대표 멜로디 형태)\n";
                        }
                        else
                        {
                            clusterOutput += point.Feature.Key + "번 마디의 멜로디 형태\n";
                        }
                        clusterOutput += point.Feature.Value.PrintToString();
                    }
                    track.clusterIDs.Add(p.Key);
                    track.clusterOutputs[p.Key] = clusterOutput;
                    //Console.WriteLine(clusterOutput);
                    //Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~");
                }

                //Console.WriteLine("Noise");
                int uniqueID = -1 * (result.Clusters.Count + 1);
                string clusterOutput2 = "";

                if (result.Noise.Length > 0)
                {
                    clusterOutput2 += "단독 클러스터 (클러스터 번호가 음수인 마디의 모음)\n\n";
                    int k = 0;
                    foreach (var point in result.Noise)
                    {
                        clusterOutput2 += point.Feature.Key + ", ";
                        k++;
                        if (k % 10 == 0) clusterOutput2 += "\n";
                    }
                    clusterOutput2 = clusterOutput2.Substring(0, clusterOutput2.LastIndexOf(','));
                    clusterOutput2 += "번 마디 포함\n";

                    foreach (var point in result.Noise)
                    {
                        // No cluster
                        track.measures.Find(e => e.measureNum == point.Feature.Key).melodicContourID = uniqueID;
                        //Console.WriteLine("Measure " + point.Feature.Key + " -> " + uniqueID);
                        //point.Feature.Value.Print();

                        clusterOutput2 += "\n\n";
                        clusterOutput2 += point.Feature.Key + "번 마디의 멜로디 형태 -> " + uniqueID + "번 클러스터\n";
                        clusterOutput2 += point.Feature.Value.PrintToString();

                        uniqueID--;
                    }
                }
                else
                {
                    clusterOutput2 += "단독 클러스터로 분류된 마디 없음\n";
                }
                track.clusterIDs.Add(0);
                track.clusterOutputs[0] = clusterOutput2;
                //Console.WriteLine(clusterOutput2);

                track.clusterIDs.Sort();

                //Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~");

                List<int> melodicContourSequence = new List<int>();
                foreach (Measure m in track.measures)
                {
                    //Console.Write(m.melodicContourID + " ");
                    melodicContourSequence.Add(m.melodicContourID);
                }

                //Console.WriteLine();

                //Console.WriteLine("Repeated Melodic Contour ID Sequences with 4 or more length");
                track.repeatedMelodicContourSequences = LongestRepeatedSubstrings(melodicContourSequence, 4);
                foreach (KeyValuePair<int, List<int>> s in track.repeatedMelodicContourSequences)
                {
                    for (int i = 0; i < s.Value.Count; i++)
                    {
                        //Console.Write(s.Value[i] + " ");
                    }
                    //Console.WriteLine();
                }

                track.status = TrackInfo.AnalysisStatus.Complete;
                Invoke(new EventHandler(delegate
                {
                    form.AfterFinishClustering(n);
                }));
            }

            #endregion

            //Console.WriteLine("Analysis terminated.");

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

        public List<KeyValuePair<int, List<int>>> LongestRepeatedSubstrings(List<int> melodicContourIDSequence, int minLength = 4)
        {
            List< KeyValuePair<int, List<int>>> results = new List<KeyValuePair<int, List<int>>>();
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
                    foreach (KeyValuePair<int, List<int>> l in results)
                    {
                        bool isEqual1 = true, isEqual2 = true;
                        for (int j = 0; j < Math.Min(l.Value.Count, maxLength); j++)
                        {
                            if (l.Value[j] != maxSequence[j])
                            {
                                isEqual1 = false;
                            }
                            if (l.Value[l.Value.Count - 1 - j] != maxSequence[maxLength - 1 - j])
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
                        results.Add(new KeyValuePair<int, List<int>>(i, maxSequence));
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

        private void ShowTrackMenu()
        {
            if (tracks == null || tracks.Count == 0 || mf == null) return;
            fileNameTextBox.Text = "곡: " + FileNameWithoutExtension(FilePathToName(filePath));
            parameterTextBox.AppendText("유형 " + mf.FileFormat + ", 박자 당 tick 수: " + mf.DeltaTicksPerQuarterNote);
            parameterTextBox.AppendText(Environment.NewLine);
            parameterTextBox.AppendText("(" + pDurationCost + ", " + pOnsetCost + " / " +
                pPitchVarianceCost + ", " + pPitchRankCost + ", " + pPitchCountCost + " / " +
                pDbscanEpsilon + ", " + pDbscanMinimumPoints + ")");

            if (tracks.Count > 7)
            {
                tableLayoutPanel1.RowCount = tracks.Count;
            }
            else
            {
                tableLayoutPanel1.RowCount = tracks.Count + 1;
            }
            for (int i = 0; i < tracks.Count; i++)
            {
                var track = tracks[i];
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
                tableLayoutPanel1.Controls.Add(new Label() { 
                    Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right),
                    Font = fileNameTextBox.Font,
                    Size = new Size(144, 47),
                    Text = "트랙 " + track.trackNum,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Visible = true,
                    ForeColor = SystemColors.WindowText
                }, 0, i);

                // trackExploreButton
                Button trackExploreButton = new Button()
                {
                    Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right),
                    Enabled = true,
                    Font = parameterTextBox.Font,
                    Size = new Size(142, 41),
                    Text = "분석 대기 중",
                    UseVisualStyleBackColor = true,
                    Visible = true
                };
                tableLayoutPanel1.Controls.Add(trackExploreButton, 1, i);
                int j = i;
                trackExploreButton.Click += (object sender, EventArgs e) => { trackExploreButton_Click(sender, e, j); };
                //Console.WriteLine((tableLayoutPanel1.GetControlFromPosition(1, i)).ToString());
            }

            // 수직 스크롤 바 생겼을 때 수평 스크롤바 없애기
            int vertScrollWidth = SystemInformation.VerticalScrollBarWidth;
            tableLayoutPanel1.Padding = new Padding(0, 0, vertScrollWidth, 0);

            tableLayoutPanel1.Visible = true;
            //tabControl1.Visible = true;
        }

        private void ShowErrorMessage()
        {
            fileNameTextBox.Text = FileNameWithoutExtension(FilePathToName(filePath));
            parameterTextBox.AppendText("잘못된 MIDI 파일입니다.");
            parameterTextBox.AppendText(Environment.NewLine);
            parameterTextBox.AppendText("종료하고 다시 실행하세요.");
        }

        private void AfterStartClustering(int trackIndex)
        {
            tableLayoutPanel1.GetControlFromPosition(1, trackIndex).Text = "분석 중...";
        }

        private void AfterFinishClustering(int trackIndex)
        {
            tableLayoutPanel1.GetControlFromPosition(1, trackIndex).Text = "살펴보기!";

            if (seletedTrackIndex >= 0 && seletedTrackIndex == trackIndex)
            {
                // 마디 정보 탭이 아닌 탭을 활성화
                (tabControl1.TabPages[1] as TabPage).Enabled = true;
                (tabControl1.TabPages[2] as TabPage).Enabled = true;

                if (measureComboBox.SelectedItem != null)
                {
                    Measure measure = tracks[seletedTrackIndex].measures[measureComboBox.SelectedIndex];
                    if (measure != null)
                    {
                        measureClusterTextBox.Text = measure.melodicContourID.ToString();
                        measureClusterTextBox.Enabled = true;
                        measureClusterLabel.Enabled = true;
                        if (measure.melodicContourID != 0)
                        {
                            measureClusterButton.Enabled = true;
                            int i = measureComboBox.SelectedIndex;
                            measureClusterButton.Click += (object sender2, EventArgs e2) => { measureClusterButton_Click(sender2, e2, i); };
                        }
                        else
                        {
                            RemoveClickEvent(measureClusterButton);
                            measureClusterButton.Enabled = false;
                        }
                    }
                    else
                    {
                        RemoveClickEvent(measureClusterButton);
                        measureClusterButton.Enabled = false;
                    }
                }


                clusterComboBox.Text = "클러스터 번호 선택!";
                clusterComboBox.Items.Clear();

                clusterComboBox.Items.Add("단독 클러스터");
                for (int i = 1; i < tracks[seletedTrackIndex].clusterIDs.Count; i++)
                {
                    clusterComboBox.Items.Add(tracks[seletedTrackIndex].clusterIDs[i] + "번 클러스터");
                }
                clusterMainLabel.Text = "";


                structureMainLabel.Text = tracks[seletedTrackIndex].songName + " > 트랙 " +
                    tracks[seletedTrackIndex].trackNum;
                structurePatternComboBox.Items.Clear();
                structurePatternTextBox.Clear();
                structureSequenceTextBox.Clear();

                if (tracks[seletedTrackIndex].repeatedMelodicContourSequences.Count == 0)
                {
                    structurePatternComboBox.Text = "반복되는 멜로디 형태 패턴이 없습니다.";
                    structurePatternComboBox.Enabled = false;
                }
                else
                {
                    structurePatternComboBox.Text = "멜로디 형태 패턴 선택!";
                    structurePatternComboBox.Enabled = true;
                    foreach (KeyValuePair<int, List<int>> s in tracks[seletedTrackIndex].repeatedMelodicContourSequences)
                    {
                        string str = "";
                        for (int i = 0; i < s.Value.Count; i++)
                        {
                            str += s.Value[i] + " ";
                        }
                        structurePatternComboBox.Items.Add(str.TrimEnd(' '));
                    }
                }

                string str1 = "    마디 번호  ";
                string str2 = "클러스터 번호  ";
                for (int i = 0; i < tracks[seletedTrackIndex].measures.Count; i++)
                {
                    string a = tracks[seletedTrackIndex].measures[i].measureNum.ToString();
                    string b = tracks[seletedTrackIndex].measures[i].melodicContourID.ToString();
                    while (a.Length < b.Length)
                    {
                        a = " " + a;
                    }
                    while (a.Length > b.Length)
                    {
                        b = " " + b;
                    }
                    str1 += a + " ";
                    str2 += b + " ";
                }

                structureSequenceTextBox.AppendText(str1.TrimEnd(' '));
                structureSequenceTextBox.AppendText(Environment.NewLine);
                structureSequenceTextBox.AppendText(str2.TrimEnd(' '));

                // 커서가 텍스트의 시작 위치에 놓이도록 함
                structureSequenceTextBox.Focus();
                structureSequenceTextBox.SelectionStart = 0;
                structureSequenceTextBox.SelectionLength = 0;
                structureSequenceTextBox.ScrollToCaret();
            }
        }

        private void trackExploreButton_Click(object sender, EventArgs e, int trackIndex)
        {
            TrackInfo track = tracks[trackIndex];

            //tabControl1.SelectTab(0);

            measureTimeTextBox.Text = "";
            measureKeyTextBox.Text = "";
            measureChordTextBox.Text = "";
            measureMelodicContourTextBox.Text = "";
            measureClusterTextBox.Text = "";
            measureComboBox.Text = "마디 번호 선택!";
            clusterComboBox.Text = "클러스터 번호 선택!";
            structurePatternComboBox.Text = "멜로디 형태 패턴 선택!";

            measurePanel.Visible = false;
            clusterPanel.Visible = false;

            seletedTrackIndex = trackIndex;

            measureComboBox.Items.Clear();
            clusterComboBox.Items.Clear();

            for (int i = 0; i < track.measures.Count; i++)
            {
                measureComboBox.Items.Add(track.measures[i].measureNum + "번 마디");
            }
            for (int i = 0; i < tracks.Count; i++)
            {
                if (i == trackIndex) continue;
                tableLayoutPanel1.GetControlFromPosition(1, i).Enabled = true;
                tableLayoutPanel1.GetControlFromPosition(0, i).ForeColor = SystemColors.WindowText;
            }
            tableLayoutPanel1.GetControlFromPosition(1, trackIndex).Enabled = false;
            tableLayoutPanel1.GetControlFromPosition(0, trackIndex).ForeColor = Color.OrangeRed;

            if (track.status == TrackInfo.AnalysisStatus.Complete)
            {
                (tabControl1.TabPages[1] as TabPage).Enabled = true;
                (tabControl1.TabPages[2] as TabPage).Enabled = true;

                if (measureComboBox.SelectedItem != null)
                {
                    Measure measure = tracks[seletedTrackIndex].measures[measureComboBox.SelectedIndex];
                    if (measure != null)
                    {
                        measureClusterTextBox.Text = measure.melodicContourID.ToString();
                        measureClusterTextBox.Enabled = true;
                        measureClusterLabel.Enabled = true;
                        if (measure.melodicContourID != 0)
                        {
                            measureClusterButton.Enabled = true;
                            int i = measureComboBox.SelectedIndex;
                            measureClusterButton.Click += (object sender2, EventArgs e2) => { measureClusterButton_Click(sender2, e2, i); };
                        }
                        else
                        {
                            RemoveClickEvent(measureClusterButton);
                            measureClusterButton.Enabled = false;
                        }
                    }
                    else
                    {
                        RemoveClickEvent(measureClusterButton);
                        measureClusterButton.Enabled = false;
                    }
                }


                clusterComboBox.Items.Add("단독 클러스터");
                for (int i = 1; i < tracks[seletedTrackIndex].clusterIDs.Count; i++)
                {
                    clusterComboBox.Items.Add(tracks[seletedTrackIndex].clusterIDs[i] + "번 클러스터");
                }
                clusterMainLabel.Text = "";


                structureMainLabel.Text = tracks[seletedTrackIndex].songName + " > 트랙 " +
                        tracks[seletedTrackIndex].trackNum;
                structurePatternComboBox.Items.Clear();
                structurePatternTextBox.Clear();
                structureSequenceTextBox.Clear();

                if (tracks[seletedTrackIndex].repeatedMelodicContourSequences.Count == 0)
                {
                    structurePatternComboBox.Text = "반복되는 멜로디 형태 패턴이 없습니다.";
                    structurePatternComboBox.Enabled = false;
                }
                else
                {
                    structurePatternComboBox.Text = "멜로디 형태 패턴 선택!";
                    structurePatternComboBox.Enabled = true;
                    foreach (KeyValuePair<int, List<int>> s in tracks[seletedTrackIndex].repeatedMelodicContourSequences)
                    {
                        string str = "";
                        for (int i = 0; i < s.Value.Count; i++)
                        {
                            str += s.Value[i] + " ";
                        }
                        structurePatternComboBox.Items.Add(str.TrimEnd(' '));
                    }
                }

                string str1 = "    마디 번호  ";
                string str2 = "클러스터 번호  ";
                for (int i = 0; i < tracks[seletedTrackIndex].measures.Count; i++)
                {
                    string a = tracks[seletedTrackIndex].measures[i].measureNum.ToString();
                    string b = tracks[seletedTrackIndex].measures[i].melodicContourID.ToString();
                    while (a.Length < b.Length)
                    {
                        a = " " + a;
                    }
                    while (a.Length > b.Length)
                    {
                        b = " " + b;
                    }
                    str1 += a + " ";
                    str2 += b + " ";
                }

                structureSequenceTextBox.AppendText(str1.TrimEnd(' '));
                structureSequenceTextBox.AppendText(Environment.NewLine);
                structureSequenceTextBox.AppendText(str2.TrimEnd(' '));

                // 커서가 텍스트의 시작 위치에 놓이도록 함
                structureSequenceTextBox.Focus();
                structureSequenceTextBox.SelectionStart = 0;
                structureSequenceTextBox.SelectionLength = 0;
                structureSequenceTextBox.ScrollToCaret();
            }
            else
            {
                (tabControl1.TabPages[1] as TabPage).Enabled = false;
                (tabControl1.TabPages[2] as TabPage).Enabled = false;
                measureClusterTextBox.Enabled = false;
                measureClusterLabel.Enabled = false;
                measureClusterButton.Enabled = false;
                clusterMainLabel.Text = "몇 분 정도의 시간이 소요될 수 있습니다.";
                structureMainLabel.Text = "몇 분 정도의 시간이 소요될 수 있습니다.";
                structurePatternComboBox.Items.Clear();
                structurePatternTextBox.Clear();
                structureSequenceTextBox.Clear();
            }

            tabControl1.Visible = true;
        }

        private void defaultSettingButton_Click(object sender, EventArgs e)
        {
            pDurationCost = 3;
            pOnsetCost = 3;
            pPitchVarianceCost = 4;
            pPitchRankCost = 1;
            pPitchCountCost = 3;
            pDbscanEpsilon = 5;
            pDbscanMinimumPoints = 2;

            numericDuration.Value = pDurationCost;
            numericOnset.Value = pOnsetCost;
            numericPitchVariance.Value = pPitchVarianceCost;
            numericPitchRank.Value = pPitchRankCost;
            numericPitchCount.Value = pPitchCountCost;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/salt26/midi-analyzer");
        }

        private void numericDuration_ValueChanged(object sender, EventArgs e)
        {
            pDurationCost = (int)numericDuration.Value;
        }

        private void numericOnset_ValueChanged(object sender, EventArgs e)
        {
            pOnsetCost = (int)numericOnset.Value;
        }

        private void numericPitchVariance_ValueChanged(object sender, EventArgs e)
        {
            pPitchVarianceCost = (int)numericPitchVariance.Value;
        }

        private void numericPitchRank_ValueChanged(object sender, EventArgs e)
        {
            pPitchRankCost = (int)numericPitchRank.Value;
        }

        private void numericPitchCount_ValueChanged(object sender, EventArgs e)
        {
            pPitchCountCost = (int)numericPitchCount.Value;
        }

        private void numericEpsilon_ValueChanged(object sender, EventArgs e)
        {
            pDbscanEpsilon = (int)numericEpsilon.Value;
        }

        private void numericMinimumPoints_ValueChanged(object sender, EventArgs e)
        {
            pDbscanMinimumPoints = (int)numericMinimumPoints.Value;
        }

        private void analysisStartButton_Click(object sender, EventArgs e)
        {
            panel1.Visible = false;
            panel1.Enabled = false;
            panel2.Enabled = true;
            panel2.Visible = true;
            Task.Run(() => Start());
        }

        private void measureComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tracks == null || seletedTrackIndex < 0 ||
                seletedTrackIndex >= tracks.Count ||
                measureComboBox.SelectedIndex >= tracks[seletedTrackIndex].measures.Count)
                return;
            
            Measure measure = tracks[seletedTrackIndex].measures[measureComboBox.SelectedIndex];
            if (measure == null) return;

            measureMainLabel.Text = measure.songName + " > 트랙 " + measure.trackNum + " > " + measure.measureNum + "번 마디";
            measureTimeTextBox.Text = measure.timeSignature.Key + "/" + measure.timeSignature.Value;
            measureChordTextBox.Text = measure.chord.ToString();
            measureMelodicContourTextBox.Clear();
            foreach (string token in measure.melodicContourOutput.TrimEnd('\n').Split('\n'))
            {
                measureMelodicContourTextBox.AppendText(token);
                measureMelodicContourTextBox.AppendText(Environment.NewLine);
            }

            // 커서가 텍스트의 시작 위치에 놓이도록 함
            measureMelodicContourTextBox.Focus();
            measureMelodicContourTextBox.SelectionStart = 0;
            measureMelodicContourTextBox.SelectionLength = 0;
            measureMelodicContourTextBox.ScrollToCaret();

            RemoveClickEvent(measureClusterButton);
            RemoveClickEvent(measureOriginalScoreButton);
            RemoveClickEvent(measureMonophonicScoreButton);
            RemoveClickEvent(measureChordButton);
            // TODO 듣기! 버튼의 이벤트를 만들어야 한다.

            if (tracks[seletedTrackIndex].status == TrackInfo.AnalysisStatus.Complete)
            {
                measureClusterTextBox.Text = measure.melodicContourID.ToString();
                measureClusterTextBox.Enabled = true;
                measureClusterLabel.Enabled = true;
                if (measure.melodicContourID != 0)
                {
                    measureClusterButton.Enabled = true;
                    int i = measureComboBox.SelectedIndex;
                    measureClusterButton.Click += (object sender2, EventArgs e2) => { measureClusterButton_Click(sender2, e2, i); };
                }
                else
                {
                    measureClusterButton.Enabled = false;
                }
            }
            else
            {
                measureClusterTextBox.Text = "?";
                measureClusterTextBox.Enabled = false;
                measureClusterLabel.Enabled = false;
                measureClusterButton.Enabled = false;
            }

            switch (measure.key)
            {
                case Measure.Key.C:
                    measureKeyTextBox.Text = "C major";
                    break;
                case Measure.Key.G:
                    measureKeyTextBox.Text = "G major (# 1개)";
                    break;
                case Measure.Key.D:
                    measureKeyTextBox.Text = "D major (# 2개)";
                    break;
                case Measure.Key.A:
                    measureKeyTextBox.Text = "A major (# 3개)";
                    break;
                case Measure.Key.E:
                    measureKeyTextBox.Text = "E major (# 4개)";
                    break;
                case Measure.Key.B:
                    measureKeyTextBox.Text = "B major (# 5개)";
                    break;
                case Measure.Key.Gb:
                    measureKeyTextBox.Text = "F# major (# 6개)";
                    break;
                case Measure.Key.Db:
                    measureKeyTextBox.Text = "D♭ major (♭ 5개)";
                    break;
                case Measure.Key.Ab:
                    measureKeyTextBox.Text = "A♭ major (♭ 4개)";
                    break;
                case Measure.Key.Eb:
                    measureKeyTextBox.Text = "E♭ major (♭ 3개)";
                    break;
                case Measure.Key.Bb:
                    measureKeyTextBox.Text = "B♭ major (♭ 2개)";
                    break;
                case Measure.Key.F:
                    measureKeyTextBox.Text = "F major (♭ 1개)";
                    break;
                case Measure.Key.Cm:
                    measureKeyTextBox.Text = "C minor (♭ 3개)";
                    break;
                case Measure.Key.Gm:
                    measureKeyTextBox.Text = "G minor (♭ 2개)";
                    break;
                case Measure.Key.Dm:
                    measureKeyTextBox.Text = "D minor (♭ 1개)";
                    break;
                case Measure.Key.Am:
                    measureKeyTextBox.Text = "A minor";
                    break;
                case Measure.Key.Em:
                    measureKeyTextBox.Text = "E minor (# 1개)";
                    break;
                case Measure.Key.Bm:
                    measureKeyTextBox.Text = "B minor (# 2개)";
                    break;
                case Measure.Key.Gbm:
                    measureKeyTextBox.Text = "F# minor (# 3개)";
                    break;
                case Measure.Key.Dbm:
                    measureKeyTextBox.Text = "C# minor (# 4개)";
                    break;
                case Measure.Key.Abm:
                    measureKeyTextBox.Text = "G# minor (# 5개)";
                    break;
                case Measure.Key.Ebm:
                    measureKeyTextBox.Text = "D# minor (# 6개)";
                    break;
                case Measure.Key.Bbm:
                    measureKeyTextBox.Text = "B♭ minor (♭ 5개)";
                    break;
                case Measure.Key.Fm:
                    measureKeyTextBox.Text = "F minor (♭ 4개)";
                    break;
            }

            measureComboBox.Focus();
            measurePanel.Visible = true;
        }

        private void measureClusterButton_Click(object sender, EventArgs e, int measureIndex)
        {
            if (tracks == null || seletedTrackIndex < 0 ||
                seletedTrackIndex >= tracks.Count ||
                measureIndex >= tracks[seletedTrackIndex].measures.Count ||
                tracks[seletedTrackIndex].status != TrackInfo.AnalysisStatus.Complete)
                return;

            int melodicContourID = tracks[seletedTrackIndex].measures[measureIndex].melodicContourID;
            int index = -1;
            if (melodicContourID < 0)
            {
                index = 0;
            }
            else if (melodicContourID > 0)
            {
                index = tracks[seletedTrackIndex].clusterIDs.IndexOf(melodicContourID);
            }
            else
            {
                return;
            }

            tabControl1.SelectTab(1);
            clusterComboBox.SelectedIndex = index;
        }

        private void clusterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tracks == null || seletedTrackIndex < 0 ||
                seletedTrackIndex >= tracks.Count ||
                clusterComboBox.SelectedIndex >= tracks[seletedTrackIndex].clusterIDs.Count)
                return;

            clusterMainLabel.Text = tracks[seletedTrackIndex].songName + " > 트랙 " +
                tracks[seletedTrackIndex].trackNum + " > " + 
                clusterComboBox.SelectedItem.ToString();

            clusterTextBox.Clear();

            string output = tracks[seletedTrackIndex].clusterOutputs[
                tracks[seletedTrackIndex].clusterIDs[clusterComboBox.SelectedIndex]];

            if (tracks[seletedTrackIndex].status == TrackInfo.AnalysisStatus.Complete)
            {
                clusterTextBox.Enabled = true;

                foreach (string token in output.TrimEnd('\n').Split('\n'))
                {
                    clusterTextBox.AppendText(token);
                    clusterTextBox.AppendText(Environment.NewLine);
                }

                // 커서가 텍스트의 시작 위치에 놓이도록 함
                clusterTextBox.Focus();
                clusterTextBox.SelectionStart = 0;
                clusterTextBox.SelectionLength = 0;
                clusterTextBox.ScrollToCaret();
            }
            else
            {
                clusterTextBox.Enabled = false;
                clusterTextBox.Clear();
            }

            clusterComboBox.Focus();
            clusterPanel.Visible = true;
        }

        private void structurePatternComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            KeyValuePair<int, List<int>> l = tracks[seletedTrackIndex].repeatedMelodicContourSequences[
                structurePatternComboBox.SelectedIndex];

            MelodicContour mc = new MelodicContour(0);
            for (int i = 0; i < l.Value.Count; i++)
            {
                int melodicContourID = l.Value[i];
                int measureIndex = i + l.Key;
                if (tracks[seletedTrackIndex].representatives.ContainsKey(melodicContourID))
                {
                    Measure measure = tracks[seletedTrackIndex].measures.Find(el => el.measureNum == tracks[seletedTrackIndex].representatives[melodicContourID].Key);
                    if (measure == null) continue;
                    mc = mc.Concatenate(measure.melodicContour);
                }
                else
                {
                    // melodicContourID == 0 (cannot be negative)
                    mc = mc.Concatenate(new MelodicContour(64 * tracks[seletedTrackIndex].measures[i + l.Key].timeSignature.Key /
                        tracks[seletedTrackIndex].measures[i + l.Key].timeSignature.Value));
                }
            }
            string output = mc.PrintToString().TrimEnd('\n');

            structurePatternTextBox.Clear();

            foreach (string s in output.Split('\n'))
            {
                structurePatternTextBox.AppendText(s);
                structurePatternTextBox.AppendText(Environment.NewLine);
            }

            // 커서가 텍스트의 시작 위치에 놓이도록 함
            structurePatternTextBox.Focus();
            structurePatternTextBox.SelectionStart = 0;
            structurePatternTextBox.SelectionLength = 0;
            structurePatternTextBox.ScrollToCaret();

            structurePatternComboBox.Focus();
        }

        // https://stackoverflow.com/questions/91778/how-to-remove-all-event-handlers-from-an-event
        private void RemoveClickEvent(Button b)
        {
            FieldInfo f1 = typeof(Control).GetField("EventClick",
                BindingFlags.Static | BindingFlags.NonPublic);
            object obj = f1.GetValue(b);
            PropertyInfo pi = b.GetType().GetProperty("Events",
                BindingFlags.NonPublic | BindingFlags.Instance);
            EventHandlerList list = (EventHandlerList)pi.GetValue(b, null);
            list.RemoveHandler(obj, list[obj]);
        }
    }
}
