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
using NAudio.Midi;
using System.Collections.Generic;

namespace MidiAnalyzer
{
    public class Measure
    {
        public enum Key
        {
            C, Db, D, Eb, E, F, Gb, G, Ab, A, Bb, B,
            Cm, Dbm, Dm, Ebm, Em, Fm, Gbm, Gm, Abm, Am, Bbm, Bm
        }

        public string songName;
        public int trackNum;
        public int measureNum;
        public long startTime;
        public long endTime;
        public Key key;
        public KeyValuePair<int, int> timeSignature;
        public List<Note> originalScore;
        public List<Note> monophonicScore;
        public Chord chord;
        public MelodicContour melodicContour;
        public int melodicContourID;
        // Time signature is always fixed to 4/4.

        public Measure()
        {
            originalScore = new List<Note>();
            monophonicScore = new List<Note>();
        }

        public override string ToString()
        {
            return "Measure: measure = " + measureNum + ", keySignature = " + key.ToString() + //", startTime = " + startTime + ", endTime = " + endTime +
                ", timeSignature = " + timeSignature.Key + "/" + timeSignature.Value + ", noteCount = " + originalScore.Count;
        }

        public static Key GetKeyFromKeySignature(KeySignatureEvent keySignature)
        {
            // major = 0 ~ 11 / minor = 21 ~ 23, 12 ~ 20
            int k = 0;

            switch (keySignature.SharpsFlats)
            {
                case 0:     // C
                    break;
                case -5:    // Db = C#
                case 7:
                    k += 1;
                    break;  
                case 2:     // D
                    k += 2;
                    break;
                case -3:    // Eb
                    k += 3;
                    break;
                case 4:     // E
                    k += 4;
                    break;
                case -1:    // F
                    k += 5;
                    break;
                case -6:    // Gb = F#
                case 6:
                    k += 6;
                    break;
                case 1:     // G
                    k += 7;
                    break;
                case -4:    // Ab
                    k += 8;
                    break;
                case 3:     // A
                    k += 9;
                    break;
                case -2:    // Bb
                    k += 10;
                    break;
                case 5:     // B = Cb
                case -7:
                    k += 11;
                    break;
            }
            if (keySignature.MajorMinor == 0)
            {
                // major
                return (Key)k;
            }
            else
            {
                // minor
                return (Key)(((k + 9) % 12) + 12);
            }
        }
    }
}
