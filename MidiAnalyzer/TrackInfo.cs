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
using DbscanImplementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiAnalyzer
{
    public class TrackInfo
    {
        public enum AnalysisStatus { Wait, Analyzing, Complete }

        public string songName;
        public int trackNum;
        public int measureCount;    // max(# of measure in all tracks with the same songName)
        public List<Measure> measures;
        public List<Note> score;
        public DbscanResult<KeyValuePair<int, MelodicContour>> dbscanResult;
        public List<KeyValuePair<int, List<int>>> repeatedMelodicContourSequences;

        // representatives[melodicContourID] = KeyValuePair(measureNum, melodicContour)
        public Dictionary<int, KeyValuePair<int, MelodicContour>> representatives;
        public Dictionary<int, string> clusterOutputs;
        public List<int> clusterIDs;

        public AnalysisStatus status;

        public TrackInfo(string fileName, int trackNum)
        {
            songName = fileName.Substring(0, fileName.LastIndexOf('.'));
            this.trackNum = trackNum;
            measures = new List<Measure>();
            score = new List<Note>();
            representatives = new Dictionary<int, KeyValuePair<int, MelodicContour>>();
            clusterOutputs = new Dictionary<int, string>();
            clusterIDs = new List<int>();
            repeatedMelodicContourSequences = new List<KeyValuePair<int, List<int>>>();
            status = AnalysisStatus.Wait;
        }
    }
}
