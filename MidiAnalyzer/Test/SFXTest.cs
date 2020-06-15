/*
MIT License

Copyright (c) 2019 salt26

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
// The original source code of this file can be found on https://github.com/salt26/chordingcoding.
using System;
using System.Collections.Generic;

namespace MidiAnalyzer.Test
{
    /// <summary>
    /// Test code for SFX.
    /// To run some tests, you can simply uncomment the line `new Test.SFXTest();` in the constructor of "Form1.cs".
    /// </summary>
    class SFXTest
    {
        /// <summary>
        /// Run some tests.
        /// </summary>
        public SFXTest()
        {
            //ChordRecognitionTest();
            MelodicContourEditTest();
            //MelodicContourDistanceTest();
        }

        private void ChordRecognitionTest()
        {
            Console.WriteLine("Chord Recognition Test");
            int Pitch(Chord.Root root, int octave)
            {
                return (int)root + (octave + 1) * 12;
            }
            Score s1 = new Score();
            s1.AddNote(Pitch(Chord.Root.C, 5), 127, 16, 0, 0, 0);
            s1.AddNote(Pitch(Chord.Root.G, 5), 127, 16, 0, 0, 0);
            s1.AddNote(Pitch(Chord.Root.G, 5), 127, 12, 0, 16, 0);
            s1.AddNote(Pitch(Chord.Root.E, 5), 127, 4, 0, 28, 0);
            s1.AddNote(Pitch(Chord.Root.G, 5), 127, 8, 0, 32, 0);
            s1.AddNote(Pitch(Chord.Root.B, 5), 127, 16, 0, 40, 0);
            s1.AddNote(Pitch(Chord.Root.E, 5), 127, 16, 0, 40, 0);
            s1.AddNote(Pitch(Chord.Root.A, 5), 127, 8, 0, 56, 0);
            s1.AddNote(Pitch(Chord.Root.Db, 5), 127, 8, 0, 56, 0);

            s1.AddNote(Pitch(Chord.Root.C, 3), 127, 8, 0, 0, 1);
            s1.AddNote(Pitch(Chord.Root.G, 3), 127, 8, 0, 8, 1);
            s1.AddNote(Pitch(Chord.Root.E, 3), 127, 8, 0, 16, 1);
            s1.AddNote(Pitch(Chord.Root.G, 3), 127, 8, 0, 24, 1);
            s1.AddNote(Pitch(Chord.Root.C, 3), 127, 8, 0, 32, 1);
            s1.AddNote(Pitch(Chord.Root.G, 3), 127, 8, 0, 40, 1);
            s1.AddNote(Pitch(Chord.Root.E, 3), 127, 8, 0, 48, 1);
            s1.AddNote(Pitch(Chord.Root.A, 3), 127, 8, 0, 56, 1);

            Chord c11 = Chord.RecognizeChordFromScore(s1.score);
            Chord c14 = Chord.RecognizeChordFromScore(s1.score, true);
            Console.WriteLine("1 harmonic: " + c11.root + "" + c11.type + ", 4 harmonics: " + c14.root + "" + c14.type);

            //

            s1 = new Score();
            //s1.AddNote(Pitch(Chord.Root.A, 5), 127, 24, 0, 0, 0);
            //s1.AddNote(Pitch(Chord.Root.Db, 5), 127, 24, 0, 0, 0);
            s1.AddNote(Pitch(Chord.Root.Bb, 5), 127, 16, 0, 24, 0);
            s1.AddNote(Pitch(Chord.Root.F, 5), 127, 16, 0, 24, 0);
            s1.AddNote(Pitch(Chord.Root.Db, 5), 127, 16, 0, 24, 0);
            s1.AddNote(Pitch(Chord.Root.A, 5), 127, 8, 0, 40, 0);
            s1.AddNote(Pitch(Chord.Root.G, 5), 127, 8, 0, 48, 0);
            s1.AddNote(Pitch(Chord.Root.E, 5), 127, 8, 0, 56, 0);

            //s1.AddNote(Pitch(Chord.Root.A, 2), 127, 8, 0, 0, 1);
            //s1.AddNote(Pitch(Chord.Root.E, 3), 127, 8, 0, 8, 1);
            //s1.AddNote(Pitch(Chord.Root.A, 2), 127, 8, 0, 16, 1);
            s1.AddNote(Pitch(Chord.Root.E, 3), 127, 8, 0, 24, 1);
            s1.AddNote(Pitch(Chord.Root.Db, 3), 127, 8, 0, 32, 1);
            s1.AddNote(Pitch(Chord.Root.E, 3), 127, 8, 0, 40, 1);
            s1.AddNote(Pitch(Chord.Root.Db, 3), 127, 8, 0, 48, 1);
            s1.AddNote(Pitch(Chord.Root.E, 3), 127, 8, 0, 56, 1);

            c11 = Chord.RecognizeChordFromScore(s1.score);
            c14 = Chord.RecognizeChordFromScore(s1.score, true);
            Console.WriteLine("1 harmonic: " + c11.root + "" + c11.type + ", 4 harmonics: " + c14.root + "" + c14.type);

            //
            Console.WriteLine();

            s1 = new Score();
            s1.AddNote(Pitch(Chord.Root.C, 5), 127, 32, 0, 0, 0);
            s1.AddNote(Pitch(Chord.Root.E, 5), 127, 16, 0, 32, 0);
            s1.AddNote(Pitch(Chord.Root.G, 5), 127, 16, 0, 48, 0);

            s1.AddNote(Pitch(Chord.Root.C, 4), 127, 8, 0, 0, 1);
            s1.AddNote(Pitch(Chord.Root.G, 4), 127, 8, 0, 8, 1);
            s1.AddNote(Pitch(Chord.Root.E, 4), 127, 8, 0, 16, 1);
            s1.AddNote(Pitch(Chord.Root.G, 4), 127, 8, 0, 24, 1);
            s1.AddNote(Pitch(Chord.Root.C, 4), 127, 8, 0, 32, 1);
            s1.AddNote(Pitch(Chord.Root.G, 4), 127, 8, 0, 40, 1);
            s1.AddNote(Pitch(Chord.Root.E, 4), 127, 8, 0, 48, 1);
            s1.AddNote(Pitch(Chord.Root.G, 4), 127, 8, 0, 56, 1);

            c11 = Chord.RecognizeChordFromScore(s1.score);
            c14 = Chord.RecognizeChordFromScore(s1.score, true);
            Console.WriteLine("1 harmonic: " + c11.root + "" + c11.type + ", 4 harmonics: " + c14.root + "" + c14.type);

            s1 = new Score();
            s1.AddNote(Pitch(Chord.Root.B, 4), 127, 24, 0, 0, 0);
            s1.AddNote(Pitch(Chord.Root.C, 5), 127, 4, 0, 24, 0);
            s1.AddNote(Pitch(Chord.Root.D, 5), 127, 4, 0, 28, 0);
            s1.AddNote(Pitch(Chord.Root.C, 5), 127, 16, 0, 32, 0);

            s1.AddNote(Pitch(Chord.Root.D, 4), 127, 8, 0, 0, 1);
            s1.AddNote(Pitch(Chord.Root.G, 4), 127, 8, 0, 8, 1);
            s1.AddNote(Pitch(Chord.Root.F, 4), 127, 8, 0, 16, 1);
            s1.AddNote(Pitch(Chord.Root.G, 4), 127, 8, 0, 24, 1);
            s1.AddNote(Pitch(Chord.Root.C, 4), 127, 8, 0, 32, 1);
            s1.AddNote(Pitch(Chord.Root.G, 4), 127, 8, 0, 40, 1);
            s1.AddNote(Pitch(Chord.Root.E, 4), 127, 8, 0, 48, 1);
            s1.AddNote(Pitch(Chord.Root.G, 4), 127, 8, 0, 56, 1);

            c11 = Chord.RecognizeChordFromScore(s1.score);
            c14 = Chord.RecognizeChordFromScore(s1.score, true);
            Console.WriteLine("1 harmonic: " + c11.root + "" + c11.type + ", 4 harmonics: " + c14.root + "" + c14.type);

            s1 = new Score();
            s1.AddNote(Pitch(Chord.Root.A, 5), 127, 32, 0, 0, 0);
            s1.AddNote(Pitch(Chord.Root.G, 5), 127, 16, 0, 32, 0);
            s1.AddNote(Pitch(Chord.Root.C, 6), 127, 16, 0, 48, 0);

            s1.AddNote(Pitch(Chord.Root.C, 4), 127, 8, 0, 0, 1);
            s1.AddNote(Pitch(Chord.Root.A, 4), 127, 8, 0, 8, 1);
            s1.AddNote(Pitch(Chord.Root.F, 4), 127, 8, 0, 16, 1);
            s1.AddNote(Pitch(Chord.Root.A, 4), 127, 8, 0, 24, 1);
            s1.AddNote(Pitch(Chord.Root.C, 4), 127, 8, 0, 32, 1);
            s1.AddNote(Pitch(Chord.Root.G, 4), 127, 8, 0, 40, 1);
            s1.AddNote(Pitch(Chord.Root.E, 4), 127, 8, 0, 48, 1);
            s1.AddNote(Pitch(Chord.Root.G, 4), 127, 8, 0, 56, 1);

            c11 = Chord.RecognizeChordFromScore(s1.score);
            c14 = Chord.RecognizeChordFromScore(s1.score, true);
            Console.WriteLine("1 harmonic: " + c11.root + "" + c11.type + ", 4 harmonics: " + c14.root + "" + c14.type);

            s1 = new Score();
            s1.AddNote(Pitch(Chord.Root.G, 5), 127, 16, 0, 0, 0);
            s1.AddNote(Pitch(Chord.Root.F, 5), 127, 4, 0, 16, 0);
            s1.AddNote(Pitch(Chord.Root.G, 6), 127, 2, 0, 20, 0);
            s1.AddNote(Pitch(Chord.Root.F, 6), 127, 2, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.E, 6), 127, 4, 0, 24, 0);
            s1.AddNote(Pitch(Chord.Root.F, 6), 127, 4, 0, 28, 0);
            s1.AddNote(Pitch(Chord.Root.E, 6), 127, 16, 0, 32, 0);

            s1.AddNote(Pitch(Chord.Root.B, 3), 127, 8, 0, 0, 1);
            s1.AddNote(Pitch(Chord.Root.G, 4), 127, 8, 0, 8, 1);
            s1.AddNote(Pitch(Chord.Root.D, 4), 127, 8, 0, 16, 1);
            s1.AddNote(Pitch(Chord.Root.G, 4), 127, 8, 0, 24, 1);
            s1.AddNote(Pitch(Chord.Root.C, 4), 127, 8, 0, 32, 1);
            s1.AddNote(Pitch(Chord.Root.G, 4), 127, 8, 0, 40, 1);
            s1.AddNote(Pitch(Chord.Root.E, 4), 127, 8, 0, 48, 1);
            s1.AddNote(Pitch(Chord.Root.G, 4), 127, 8, 0, 56, 1);

            c11 = Chord.RecognizeChordFromScore(s1.score);
            c14 = Chord.RecognizeChordFromScore(s1.score, true);
            Console.WriteLine("1 harmonic: " + c11.root + "" + c11.type + ", 4 harmonics: " + c14.root + "" + c14.type);

            //
            Console.WriteLine();


            s1 = new Score();
            s1.AddNote(Pitch(Chord.Root.F, 5), 127, 32, 0, 0, 0);
            s1.AddNote(Pitch(Chord.Root.Ab, 5), 127, 32, 0, 16, 0);
            s1.AddNote(Pitch(Chord.Root.Db, 6), 127, 24, 0, 20, 0);
            s1.AddNote(Pitch(Chord.Root.F, 6), 127, 24, 0, 22, 0);

            s1.AddNote(Pitch(Chord.Root.F, 4), 127, 64, 0, 8, 1);
            s1.AddNote(Pitch(Chord.Root.Ab, 4), 127, 64, 0, 8, 1);

            c11 = Chord.RecognizeChordFromScore(s1.score);
            c14 = Chord.RecognizeChordFromScore(s1.score, true);
            Console.WriteLine("1 harmonic: " + c11.root + "" + c11.type + ", 4 harmonics: " + c14.root + "" + c14.type);

            s1 = new Score();
            s1.AddNote(Pitch(Chord.Root.Db, 6), 127, 8, 0, 20, 0);
            s1.AddNote(Pitch(Chord.Root.F, 6), 127, 8, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.C, 6), 127, 8, 0, 20, 0);
            s1.AddNote(Pitch(Chord.Root.Eb, 6), 127, 8, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.Db, 6), 127, 8, 0, 20, 0);
            s1.AddNote(Pitch(Chord.Root.F, 6), 127, 8, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.C, 6), 127, 48, 0, 20, 0);
            s1.AddNote(Pitch(Chord.Root.Eb, 6), 127, 48, 0, 22, 0);

            s1.AddNote(Pitch(Chord.Root.Gb, 4), 127, 72, 0, 8, 1);
            s1.AddNote(Pitch(Chord.Root.A, 4), 127, 72, 0, 8, 1);

            c11 = Chord.RecognizeChordFromScore(s1.score);
            c14 = Chord.RecognizeChordFromScore(s1.score, true);
            Console.WriteLine("1 harmonic: " + c11.root + "" + c11.type + ", 4 harmonics: " + c14.root + "" + c14.type);

            s1 = new Score();
            s1.AddNote(Pitch(Chord.Root.C, 6), 127, 8, 0, 20, 0);
            s1.AddNote(Pitch(Chord.Root.Eb, 6), 127, 8, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.Bb, 5), 127, 8, 0, 20, 0);
            s1.AddNote(Pitch(Chord.Root.Db, 6), 127, 8, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.C, 6), 127, 8, 0, 20, 0);
            s1.AddNote(Pitch(Chord.Root.Eb, 6), 127, 8, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.Bb, 5), 127, 12, 0, 20, 0);
            s1.AddNote(Pitch(Chord.Root.Db, 6), 127, 12, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.F, 6), 127, 24, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.Db, 6), 127, 12, 0, 22, 0);

            s1.AddNote(Pitch(Chord.Root.F, 4), 127, 72, 0, 8, 1);
            s1.AddNote(Pitch(Chord.Root.Ab, 4), 127, 72, 0, 8, 1);

            c11 = Chord.RecognizeChordFromScore(s1.score);
            c14 = Chord.RecognizeChordFromScore(s1.score, true);
            Console.WriteLine("1 harmonic: " + c11.root + "" + c11.type + ", 4 harmonics: " + c14.root + "" + c14.type);

            s1 = new Score();
            s1.AddNote(Pitch(Chord.Root.Db, 6), 127, 8, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.C, 6), 127, 8, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.Ab, 5), 127, 8, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.Db, 6), 127, 8, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.Bb, 5), 127, 8, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.C, 6), 127, 48, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.Ab, 5), 127, 48, 0, 22, 0);

            s1.AddNote(Pitch(Chord.Root.Eb, 4), 127, 72, 0, 8, 1);
            s1.AddNote(Pitch(Chord.Root.Gb, 4), 127, 72, 0, 8, 1);

            c11 = Chord.RecognizeChordFromScore(s1.score);
            c14 = Chord.RecognizeChordFromScore(s1.score, true);
            Console.WriteLine("1 harmonic: " + c11.root + "" + c11.type + ", 4 harmonics: " + c14.root + "" + c14.type);

        }

        private void MelodicContourEditTest()
        {
            Console.WriteLine("MelodicContour Edit Test");
            MelodicContour mc = new MelodicContour(0);
            int cost = 0;
            cost += mc.InsertNote(mc.noteList.Count, new MelodicContourNote(16, 0));
            Console.WriteLine(cost);
            cost += mc.InsertNote(mc.noteList.Count, 16, -1);
            Console.WriteLine(cost);
            cost += mc.InsertNote(mc.noteList.Count, new MelodicContourNote(8, -2));
            Console.WriteLine(cost);
            cost += mc.InsertNote(mc.noteList.Count, 8, -1);
            Console.WriteLine(cost);
            cost += mc.InsertNote(mc.noteList.Count, new MelodicContourNote(8, 0));
            Console.WriteLine(cost);
            cost += mc.InsertNote(mc.noteList.Count, 8, -4);
            Console.WriteLine(cost);
            mc.Print();

            cost = 0;
            cost += mc.ReplaceNote(5, new MelodicContourNote(8, 4));
            Console.WriteLine(cost);
            cost += mc.ReplaceNote(4, 8, 3);
            Console.WriteLine(cost);
            cost += mc.ReplaceNote(3, new MelodicContourNote(8, 0));
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(1)");
            // 음표와 음표 사이에 삽입
            cost = mc.InsertNote(2, new MelodicContourNote(16, mc.GetNewClusterNumber(1)));
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(2)");
            // 잘못된 인덱스에 삽입
            cost = mc.InsertNote(10, new MelodicContourNote(24, mc.GetNewClusterNumber(3)));
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(3)");
            // 인덱스는 유지하고 길이를 바꾸는 교체 (맨 앞 음표라서 클러스터 순위 변경 비용이 없음)
            cost = mc.ReplaceNote(0, new MelodicContourNote(4, mc.GetNewClusterNumber(0)));
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(4)");
            // 맨 앞에 삽입
            cost = mc.InsertNote(0, new MelodicContourNote(4, 1));
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(5)");
            // 직전 음과 같은 음 높이를 갖는 음표 삽입
            cost = mc.InsertNote(1, new MelodicContourNote(4, 1));
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(6)");
            // 없는 음표로 교체
            cost = mc.ReplaceNote(0, new MelodicContourNote(0, 0));
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(7)");
            // 없는 음표를 교체 1
            cost = mc.ReplaceNote(-1, new MelodicContourNote(1, 3));
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(8)");
            // 없는 음표를 교체 2
            cost = mc.ReplaceNote(10, new MelodicContourNote(1, 3));
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(9)");
            // 같은 음표로 교체
            cost = mc.ReplaceNote(0, new MelodicContourNote(4, 1));
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(10)");
            // 길이와 음 높이를 모두 바꾸도록 교체 1 (맨 앞 음표라서 클러스터 순위 변경 비용이 없음)
            cost = mc.ReplaceNote(0, new MelodicContourNote(1, 3));
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(11)");
            // 길이와 음 높이를 모두 바꾸도록 교체 2
            cost = mc.ReplaceNote(3, new MelodicContourNote(15, mc.GetExistingClusterNumber(0)));
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(12)");
            // 직후 음표와 길이가 같고 클러스터가 다르도록 교체 (맨 앞 음표라서 클러스터 순위 변경 비용이 없음)
            cost = mc.ReplaceNote(2, new MelodicContourNote(15, mc.GetExistingClusterNumber(1)));
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(13)");
            // 음 높이 변화와 길이를 유지하고 클러스터 번호를 바꾸지만 클러스터 순위를 바꾸지 않도록 교체
            // (기존 클러스터가 사라지지 않음)
            cost = mc.ReplaceNote(4, 16, -1);
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(14)");
            // 음 높이 변화와 길이를 유지하면서 클러스터 번호를 바꾸지만 클러스터 순위를 바꾸지 않도록 교체
            // (기존 클러스터는 사라짐)
            cost = mc.ReplaceNote(4, 16, 1);
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(15)");
            // 음 높이 변화는 유지하면서 클러스터 순위만 다르도록 교체
            cost = mc.ReplaceNote(4, 16, 3);
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(16)");
            // 중간에 있는 기존 음표 제거
            cost = mc.DeleteNote(4);
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(17)");
            // 맨 끝의 기존 음표 제거
            cost = mc.DeleteNote(7);
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(18)");
            // 맨 앞의 기존 음표 제거
            cost = mc.DeleteNote(0);
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(19)");
            // 맨 처음 쉼표 길이 조정 1
            cost = mc.DelayNotes(8);
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(20)");
            // 제거했던 위치에 새로운 음표 삽입
            cost = mc.InsertNote(0, 8, 1);
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(21)");
            // 직전 음표와 같은 음을 갖는 음표 제거
            cost = mc.DeleteNote(1);
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(22)");
            // 없는 음표 제거
            cost = mc.DeleteNote(22);
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(23)");
            // 맨 처음 쉼표 길이 조정 2
            cost = mc.DelayNotes(4);
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(24)");
            // 맨 처음 쉼표 길이를 잘못되게 조정
            cost = mc.DelayNotes(-1);
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(25)");
            // 맨 처음 쉼표 길이를 이전 상태와 같게 조정
            cost = mc.DelayNotes(4);
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(26)");
            // 비용이 2가 되도록 연산 수행
            cost = mc.ReplaceNote(5, 8, -2);
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(27)");
            // 맨 처음 쉼표 길이 조정 3
            cost = mc.DelayNotes(0);
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(28)");
            // 연산 정보를 이용해 음표와 음표 사이에 삽입
            MelodicContour.OperationInfo op1 = new MelodicContour.OperationInfo(
                MelodicContour.OperationInfo.Type.Insert, 3, new MelodicContourNote(),
                new MelodicContourNote(2, mc.GetExistingClusterNumber(0)));
            cost = mc.PerformOperation(op1);
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(29)");
            // 연산 정보를 이용해 음표 제거
            MelodicContour.OperationInfo op2 = new MelodicContour.OperationInfo(
                MelodicContour.OperationInfo.Type.Delete, 2,
                new MelodicContourNote(15, mc.GetExistingClusterNumber(0)), new MelodicContourNote());
            cost = mc.PerformOperation(op2);
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(30)");
            // 연산 정보를 이용해 음표 교체
            MelodicContour.OperationInfo op3 = new MelodicContour.OperationInfo(
                MelodicContour.OperationInfo.Type.Replace, 1,
                new MelodicContourNote(15, -2), new MelodicContourNote(16, 2));
            cost = mc.PerformOperation(op3);
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(31)");
            // 연산 정보를 이용해 리듬 패턴의 맨 앞 쉼표 길이 조정
            MelodicContour.OperationInfo op4 = new MelodicContour.OperationInfo(0, 16);
            cost = mc.PerformOperation(op4);
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(32)");
            // 쉼표 길이를 조정하는 연산의 역연산 수행
            cost = mc.PerformOperation(op4.Inverse());
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(33)");
            // 음표를 교체하는 연산의 역연산 수행
            cost = mc.PerformOperation(op3.Inverse());
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(34)");
            // 음표를 제거하는 연산의 역연산 수행
            cost = mc.PerformOperation(op2.Inverse());
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(35)");
            // 음표를 삽입하는 연산의 역연산 수행
            cost = mc.PerformOperation(op1.Inverse());
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(36)");
            // 상황에 맞지 않는 연산 정보 1
            op1 = new MelodicContour.OperationInfo(MelodicContour.OperationInfo.Type.Replace,
                5, new MelodicContourNote(16, -2), new MelodicContourNote(16, -3));
            cost = mc.PerformOperation(op1);
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(37)");
            // 상황에 맞지 않는 연산 정보 2
            op1 = new MelodicContour.OperationInfo(MelodicContour.OperationInfo.Type.Delete,
                5, new MelodicContourNote(16, -2), new MelodicContourNote());
            cost = mc.PerformOperation(op1);
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(38)");
            // 상황에 맞지 않는 연산 정보 3
            op1 = new MelodicContour.OperationInfo(4, 0);
            cost = mc.PerformOperation(op1);
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(39)");
            // 상황에 맞지 않는 연산 정보 4
            op1 = new MelodicContour.OperationInfo(MelodicContour.OperationInfo.Type.Delete,
                5, new MelodicContourNote(8, -3), new MelodicContourNote());
            cost = mc.PerformOperation(op1);
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(40)");
            // 연속된 두 음표의 길이만 바꾸는 옮기기 연산 수행
            cost = mc.MoveNotes(3, new MelodicContourNote(4, -2), new MelodicContourNote(12, 0));
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(41)");
            // 연속된 두 음표의 길이와 음 높이를 바꾸는 옮기기 연산 수행
            cost = mc.MoveNotes(1, new MelodicContourNote(20, -2), new MelodicContourNote(10, 1));
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(42)");
            // 연산 전후로 멜로디 형태가 바뀌지 않는 옮기기 연산 수행
            cost = mc.MoveNotes(1, new MelodicContourNote(20, -2), new MelodicContourNote(10, 1));
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(43)");
            // 마지막 음표를 앞 음표로 하는 옮기기 연산 수행
            cost = mc.MoveNotes(5, new MelodicContourNote(4, -2), new MelodicContourNote(4, 0));
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(44)");
            // 없는 음표를 앞 음표로 하는 옮기기 연산 수행
            cost = mc.MoveNotes(8, new MelodicContourNote(4, -2), new MelodicContourNote(4, 0));
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(45)");
            // 연산 정보를 이용해 연속된 두 음표의 길이와 음 높이를 바꾸는 옮기기 연산 수행
            op1 = new MelodicContour.OperationInfo(1, new MelodicContourNote(20, -2), new MelodicContourNote(10, 1),
                new MelodicContourNote(15, 2), new MelodicContourNote(15, -3));
            cost = mc.PerformOperation(op1);
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(46)");
            // 45번 테스트케이스에서 수행했던 옮기기 연산의 역연산 수행
            cost = mc.PerformOperation(op1.Inverse());
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(47)");
            // 상황에 맞지 않는 연산 정보 5
            op1 = new MelodicContour.OperationInfo(1, new MelodicContourNote(20, -2), new MelodicContourNote(10, 0),
                new MelodicContourNote(15, 2), new MelodicContourNote(15, -3));
            cost = mc.PerformOperation(op1);
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(48)");
            // 연산 전후로 두 음표의 길이의 합이 달라지는 옮기기 연산 수행
            op1 = new MelodicContour.OperationInfo(1, new MelodicContourNote(20, -2), new MelodicContourNote(10, 0),
                new MelodicContourNote(16, -3), new MelodicContourNote(16, 0));
            cost = mc.PerformOperation(op1);
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(49)");
            // DelayAndReplace 연산 수행
            cost = mc.DelayAndReplaceNotes(4, new MelodicContourNote(4, 1));
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(50)");
            // 연산 전후로 멜로디 형태가 바뀌지 않는 DelayAndReplace 연산 수행
            cost = mc.DelayAndReplaceNotes(4, new MelodicContourNote(4, 1));
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(51)");
            // 연산 전후로 쉼표 및 음표의 길이의 합이 달라지는 DelayAndReplace 연산 수행
            cost = mc.DelayAndReplaceNotes(4, new MelodicContourNote(8, 1));
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(52)");
            // 연산 정보를 이용해 쉼표 및 음표의 길이와 음 높이를 바꾸는 옮기기 연산 수행
            op1 = new MelodicContour.OperationInfo(4, 0,
                new MelodicContourNote(4, 1), new MelodicContourNote(8, 0));
            cost = mc.PerformOperation(op1);
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(53)");
            // 52번 테스트케이스에서 수행했던 DelayAndReplace 연산의 역연산 수행
            cost = mc.PerformOperation(op1.Inverse());
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(54)");
            // 상황에 맞지 않는 연산 정보 6
            cost = mc.PerformOperation(op1.Inverse());
            Console.WriteLine(cost);
            mc.Print();

            Console.WriteLine("(55)");
            // 빈 멜로디 형태에 DelayAndReplace 연산 수행
            mc = new MelodicContour(16);
            cost = mc.DelayAndReplaceNotes(0, new MelodicContourNote(16, 0));
            Console.WriteLine(cost);
            mc.Print();
        }

        private void MelodicContourDistanceTest()
        {
            Console.WriteLine("MelodicContour Distance Test");

            MelodicContour mc = new MelodicContour(0);
            int cost = 0;
            mc.InsertNote(0, 16, 0);
            mc.InsertNote(1, 16, -1);
            mc.InsertNote(2, 8, -2);
            mc.InsertNote(3, 8, -1);
            mc.InsertNote(4, 8, 0);
            mc.InsertNote(5, 8, -4);
            mc.Print();

            Console.WriteLine("(1)");
            MelodicContour mc2 = mc.Copy();
            cost = mc2.ReplaceNote(5, new MelodicContourNote(8, 4));
            Console.WriteLine(cost);
            cost = mc2.ReplaceNote(4, 8, 3);
            Console.WriteLine(cost);
            cost = mc2.ReplaceNote(3, new MelodicContourNote(8, 0));
            Console.WriteLine(cost);

            mc.Print();
            Console.WriteLine();
            mc2.Print();
            Console.WriteLine("distance: " + mc.DistanceWithDirection(mc2));
            Console.WriteLine("inverse distance: " + mc2.DistanceWithDirection(mc));

            Console.WriteLine("(2)");
            mc2 = mc.Copy();
            cost = mc2.ReplaceNote(4, 8, 3);
            Console.WriteLine(cost);
            cost = mc2.ReplaceNote(3, 12, 1);
            Console.WriteLine(cost);
            cost = mc2.ReplaceNote(1, 16, 3);
            Console.WriteLine(cost);

            mc.Print();
            Console.WriteLine();
            mc2.Print();
            Console.WriteLine("distance: " + mc.DistanceWithDirection(mc2));
            Console.WriteLine("inverse distance: " + mc2.DistanceWithDirection(mc));

            Console.WriteLine("(3)");
            MelodicContour mc3 = mc2.Copy();
            mc2 = mc3.Copy();
            cost = mc2.InsertNote(5, 8, -2);
            Console.WriteLine(cost);
            cost = mc2.DeleteNote(2);
            Console.WriteLine(cost);
            cost = mc2.ReplaceNote(3, 8, 3);
            Console.WriteLine(cost);
            cost = mc2.ReplaceNote(1, 24, 1);
            Console.WriteLine(cost);

            mc3.Print();
            Console.WriteLine();
            mc2.Print();
            Console.WriteLine("distance: " + mc3.DistanceWithDirection(mc2));
            Console.WriteLine("inverse distance: " + mc2.DistanceWithDirection(mc3));

            Console.WriteLine("(4)");
            mc2 = mc.Copy();
            cost = mc2.InsertNote(5, 8, -2);
            Console.WriteLine(cost);
            cost = mc2.DeleteNote(2);
            Console.WriteLine(cost);
            cost = mc2.ReplaceNote(3, 8, 3);
            Console.WriteLine(cost);
            cost = mc2.ReplaceNote(1, 24, 1);
            Console.WriteLine(cost);

            mc.Print();
            Console.WriteLine();
            mc2.Print();
            Console.WriteLine("distance: " + mc.DistanceWithDirection(mc2));
            Console.WriteLine("inverse distance: " + mc2.DistanceWithDirection(mc));

            Console.WriteLine("(5)");
            mc = new MelodicContour(0,
                new MelodicContourNote(8, 0),
                new MelodicContourNote(6, 0),
                new MelodicContourNote(2, -2),
                new MelodicContourNote(4, 0),
                new MelodicContourNote(8, 4),
                new MelodicContourNote(8, 3),
                new MelodicContourNote(8, 1),
                new MelodicContourNote(8, 0),
                new MelodicContourNote(4, 1),
                new MelodicContourNote(4, 0),
                new MelodicContourNote(4, -1));
            mc2 = new MelodicContour(0,
                new MelodicContourNote(8, -2),
                new MelodicContourNote(6, -2),
                new MelodicContourNote(2, -2),
                new MelodicContourNote(4, -2),
                new MelodicContourNote(8, 0),
                new MelodicContourNote(36, -1));
            mc.Print();
            Console.WriteLine();
            mc2.Print();
            Console.WriteLine("distance: " + mc.DistanceWithDirection(mc2));
            mc2.Print();
            Console.WriteLine();
            mc.Print();
            Console.WriteLine("inverse distance: " + mc2.DistanceWithDirection(mc));

            Console.WriteLine("(6)");
            mc2 = new MelodicContour(0,
                new MelodicContourNote(4, -2),
                new MelodicContourNote(4, 0),
                new MelodicContourNote(4, 2),
                new MelodicContourNote(8, 5),
                new MelodicContourNote(12, 7),
                new MelodicContourNote(4, -1),
                new MelodicContourNote(4, 1),
                new MelodicContourNote(4, 3),
                new MelodicContourNote(8, 6),
                new MelodicContourNote(12, 5));
            mc.Print();
            Console.WriteLine();
            mc2.Print();
            Console.WriteLine("distance: " + mc.DistanceWithDirection(mc2));
            Console.WriteLine("inverse distance: " + mc2.DistanceWithDirection(mc));

            Console.WriteLine("(7)");
            mc2 = new MelodicContour(0,
                new MelodicContourNote(8, -1),
                new MelodicContourNote(6, -1),
                new MelodicContourNote(2, -3),
                new MelodicContourNote(4, -1),
                new MelodicContourNote(8, 3),
                new MelodicContourNote(8, 2),
                new MelodicContourNote(8, 0),
                new MelodicContourNote(8, -1),
                new MelodicContourNote(4, 0),
                new MelodicContourNote(4, -1),
                new MelodicContourNote(4, -3));
            mc.Print();
            Console.WriteLine();
            mc2.Print();
            Console.WriteLine("distance: " + mc.DistanceWithDirection(mc2));
            Console.WriteLine("inverse distance: " + mc2.DistanceWithDirection(mc));
            Console.WriteLine("min: " + mc.Distance(mc2));

            // In this example, `mc.DistanceWithDirection(mc2)` is differ from `mc2.DistanceWithDirection(mc)`.
            Console.WriteLine("(8)");
            mc = new MelodicContour(0);
            mc.InsertNote(0, 16, 0);
            mc.InsertNote(1, 16, -1);
            mc.InsertNote(2, 8, -2);
            mc.InsertNote(3, 8, -1);
            mc.InsertNote(4, 8, 0);
            mc.InsertNote(5, 8, -4);
            mc2 = new MelodicContour(0,
                new MelodicContourNote(4, -2),
                new MelodicContourNote(4, 0),
                new MelodicContourNote(4, 2),
                new MelodicContourNote(8, 5),
                new MelodicContourNote(12, 7),
                new MelodicContourNote(4, -1),
                new MelodicContourNote(4, 1),
                new MelodicContourNote(4, 3),
                new MelodicContourNote(8, 6),
                new MelodicContourNote(12, 5));
            mc.Print();
            Console.WriteLine();
            mc2.Print();
            Console.WriteLine("distance: " + mc.DistanceWithDirection(mc2));
            Console.WriteLine("inverse distance: " + mc2.DistanceWithDirection(mc));
            Console.WriteLine("min: " + mc.Distance(mc2));

            Console.WriteLine("(9)");
            mc = new MelodicContour(0,
                new MelodicContourNote(8, 0),
                new MelodicContourNote(8, 0));
            mc2 = new MelodicContour(4,
                new MelodicContourNote(4, 0),
                new MelodicContourNote(4, 1),
                new MelodicContourNote(4, 0));
            mc.Print();
            Console.WriteLine();
            mc2.Print();
            Console.WriteLine("distance: " + mc.DistanceWithDirection(mc2));
            Console.WriteLine("inverse distance: " + mc2.DistanceWithDirection(mc));
            Console.WriteLine("min: " + mc.Distance(mc2));
        }
    }

    public class Score
    {
        /// <summary>
        /// 음표들을 담을 리스트.
        /// </summary>
        public List<Note> score = new List<Note>();

        /// <summary>
        /// 악보의 길이
        /// </summary>
        private long length = 0;

        /// <summary>
        /// 악보의 길이. 읽기 전용입니다.
        /// </summary>
        public long Length
        {
            get
            {
                return length;
            }
        }

        /// <summary>
        /// 음표를 생성하여 악보에 추가합니다.
        /// </summary>
        /// <param name="pitch">음 높이(0 ~ 127). 예) 60: C4 / 64: E4 / 67: G4 / 72: C5</param>
        /// <param name="velocity">음 세기(1 ~ 127).</param>
        /// <param name="rhythm">음표의 길이(1 이상). 4/4박에서 한 마디를 64등분한 길이를 기준으로 합니다. 예) 64: 온음표 / 16: 4분음표 / 4: 16분음표 / 1: 64분음표</param>
        /// <param name="measure">음표가 위치한 마디 번호(0부터 시작).</param>
        /// <param name="position">음표의 마디 내 위치(0 ~ 63). 4/4박에서 한 마디를 64등분한 길이를 기준으로 합니다.</param>
        /// <param name="staff">음표가 놓일 Staff 번호(0 ~ 15). 9번 Staff는 타악기 전용 Staff입니다.</param>
        public void AddNote(int pitch, int velocity, int rhythm, long measure, int position, int staff = 0)
        {
            Note note = new Note(pitch, velocity, rhythm, measure, position, staff);
            score.Add(note);

            if (length < rhythm + measure * 64 + position)
            {
                length = rhythm + measure * 64 + position;
            }
        }
    }
}
