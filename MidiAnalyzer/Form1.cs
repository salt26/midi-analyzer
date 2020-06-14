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

        public List<TrackInfo> tracks = new List<TrackInfo>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs eventArgs)
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
                        bool strictMode = true;
                        MidiFile mf = new MidiFile(file.FullName, strictMode);

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
                            TrackInfo track = new TrackInfo();
                            track.songName = file.Name.Substring(0, file.Name.LastIndexOf('.'));
                            track.trackNum = n;
                            track.measures = new List<Measure>();
                            track.score = new List<Note>();

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

                            List<KeyValuePair<int, MelodicContour>> melodicContourData = new List<KeyValuePair<int, MelodicContour>>();

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

                                #endregion

                                melodicContourData.Add(new KeyValuePair<int, MelodicContour>(measure.measureNum, measure.melodicContour));

                                //Console.WriteLine("-------------");
                            }

                            /*
                            List<KeyValuePair<int, MelodicContour>> melodicContourData = new List<KeyValuePair<int, MelodicContour>>();

                            for (int i = 0; i <= track.measures.Count; i++)
                            {
                                MelodicContour mc;
                                if (i == 0)
                                {
                                    mc = new MelodicContour(track.measures[i].monophonicScore,
                                        64 * track.measures[i].timeSignature.Key / track.measures[i].timeSignature.Value);
                                    mc.DelayNotes(mc.firstRestDuration + 64 * track.measures[i].timeSignature.Key / track.measures[i].timeSignature.Value);
                                }
                                else if (i == track.measures.Count)
                                {
                                    mc = new MelodicContour(track.measures[i - 1].monophonicScore,
                                        64 * 2 * track.measures[i - 1].timeSignature.Key / track.measures[i - 1].timeSignature.Value);
                                }
                                else
                                {
                                    List<Note> twoMeasures = new List<Note>();
                                    foreach (Note note in track.measures[i - 1].monophonicScore)
                                    {
                                        twoMeasures.Add(note);
                                    }
                                    foreach (Note note in track.measures[i].monophonicScore)
                                    {
                                        twoMeasures.Add(note);
                                    }
                                    mc = new MelodicContour(twoMeasures,
                                        64 * 2 * track.measures[i].timeSignature.Key / track.measures[i].timeSignature.Value);
                                }
                                melodicContourData.Add(new KeyValuePair<int, MelodicContour>(i, mc));
                            }
                            */


                            #region DBSCAN clustering for melodic contour

                            DbscanAlgorithm<KeyValuePair<int, MelodicContour>> dbscan =
                                new DbscanAlgorithm<KeyValuePair<int, MelodicContour>>((e1, e2) => e1.Value.Distance(e2.Value));

                            DbscanResult<KeyValuePair<int, MelodicContour>> result = dbscan.ComputeClusterDbscan(
                                melodicContourData.ToArray(), 4, 2);

                            track.dbscanResult = result;

                            foreach (var p in result.Clusters)
                            {
                                Console.WriteLine("Cluster " + p.Key);
                                foreach (var point in p.Value)
                                {
                                    track.measures.Find(e => e.measureNum == point.Feature.Key).melodicContourID = p.Key;
                                    Console.WriteLine("Measure " + point.Feature.Key + ": " + point.PointType);
                                    point.Feature.Value.Print();
                                }
                                Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~");
                            }

                            Console.WriteLine("Noise");
                            foreach (var point in result.Noise)
                            {
                                // No cluster
                                track.measures.Find(e => e.measureNum == point.Feature.Key).melodicContourID = 0;
                                Console.WriteLine("Measure " + point.Feature.Key);
                                point.Feature.Value.Print();
                            }

                            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~");

                            foreach (Measure m in track.measures)
                            {
                                Console.Write(m.melodicContourID + " ");
                            }

                            Console.WriteLine();

                            #endregion

                            tracks.Add(track);
                        }

                        foreach (TrackInfo track in tracks.FindAll(e => e.songName == file.Name.Substring(0, file.Name.LastIndexOf('.'))))
                        {
                            track.measureCount = measureCount;
                        }


                        #region Recognize chord

                        for (int m = 0; m < measureCount; m++)
                        {
                            List<Note> scoreFromAllTracks = new List<Note>();
                            foreach (TrackInfo track in tracks.FindAll(e => e.songName == file.Name.Substring(0, file.Name.LastIndexOf('.'))))
                            {
                                if (m >= track.measures.Count) break;
                                scoreFromAllTracks.AddRange(track.measures[m].originalScore);
                            }

                            Chord chord = Chord.RecognizeChordFromScore(scoreFromAllTracks);

                            foreach (TrackInfo track in tracks.FindAll(e => e.songName == file.Name.Substring(0, file.Name.LastIndexOf('.'))))
                            {
                                if (m >= track.measures.Count) break;
                                track.measures[m].chord = chord;
                            }

                            if (chord.type != Chord.Type.NULL)
                                Console.WriteLine("chord of measure " + (m + 1) + ": " + chord.root + chord.type);
                        }

                        #endregion
                    }
                }
            }
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
    }
}
