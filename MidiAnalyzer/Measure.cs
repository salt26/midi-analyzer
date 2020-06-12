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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public override string ToString()
        {
            return "Measure: measure = " + measureNum + ", startTime = " + startTime + ", endTime = " + endTime + ", timeSignature = " + timeSignature.Key + "/" + timeSignature.Value;
        }
    }
}
