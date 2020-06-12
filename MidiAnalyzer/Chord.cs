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
using System.Drawing;
using System.Collections.Generic;

namespace MidiAnalyzer
{
    public class Chord
    {
        public enum Root { C, Db, D, Eb, E, F, Gb, G, Ab, A, Bb, B }
        public enum Type { Major, minor, sus2, sus4, dim, aug, dom7, m7, NULL = -1 }

        public Root root;
        public Type type;
        public int octave;

        /// <summary>
        /// 1배음만을 고려한 72가지 화음 템플릿.
        /// RecognizeChordLabelFromScore()를 use4Harmonics = false로 처음 호출할 때 초기화됩니다.
        /// 예: C Major는 { 0.334, 0, 0, 0, 0.333, 0, 0, 0.333, 0, 0, 0, 0 }
        /// </summary>
        private static Dictionary<Type, Dictionary<Root, double[]>> chordTemplates1 = null;

        /// <summary>
        /// 4배음까지 고려한 72가지 화음 템플릿.
        /// RecognizeChordLabelFromScore()를 use4Harmonics = true로 처음 호출할 때 초기화됩니다.
        /// 예: C Major는 { 0.278, 0, 0.055, 0, 0.278, 0, 0, 0.334, 0, 0, 0, 0.055 }
        /// </summary>
        private static Dictionary<Type, Dictionary<Root, double[]>> chordTemplates4 = null;

        /// <summary>
        /// 주어진 인자로 화음을 초기화합니다.
        /// </summary>
        /// <param name="root">근음의 음이름</param>
        /// <param name="type">화음의 종류</param>
        public Chord(Root root, Type type, int octave)
        {
            this.root = root;
            this.type = type;
            this.octave = octave;
        }

        /// <summary>
        /// 화음에 맞는 음 하나를 반환합니다.
        /// 반환값은 MIDI의 음 높이입니다.
        /// </summary>
        /// <returns></returns>
        public int NextNote()
        {
            int v = 0;
            int p = (int)root + octave * 12;
            Random r = new Random();
            if (type != Type.dom7 && type != Type.m7)
            {
                switch (r.Next(18))
                {
                    case 0:
                    case 1:
                    case 2:
                    case 15:
                        v = 0;
                        break;
                    case 3:
                    case 4:
                    case 5:
                    case 16:
                        v = 1;
                        break;
                    case 6:
                    case 7:
                    case 8:
                    case 17:
                        v = 2;
                        break;
                    case 9:
                    case 10:
                        v = 0;
                        p += 12;
                        break;
                    case 11:
                    case 12:
                        v = 2;
                        p -= 12;
                        break;
                    case 13:
                        v = 1;
                        p += 12;
                        break;
                    case 14:
                        v = 1;
                        p -= 12;
                        break;
                }
            }
            else
            {
                switch (r.Next(24))
                {
                    case 0:
                    case 1:
                    case 2:
                    case 15:
                        v = 0;
                        break;
                    case 3:
                    case 4:
                    case 5:
                    case 16:
                        v = 1;
                        break;
                    case 6:
                    case 7:
                    case 8:
                    case 17:
                        v = 2;
                        break;
                    case 18:
                    case 19:
                    case 20:
                    case 21:
                        v = 3;
                        break;
                    case 9:
                    case 10:
                        v = 0;
                        p += 12;
                        break;
                    case 22:
                    case 23:
                        v = 3;
                        p -= 12;
                        break;
                    case 11:
                    case 12:
                        v = 2;
                        p -= 12;
                        break;
                    case 13:
                        v = 1;
                        p += 12;
                        break;
                    case 14:
                        v = 1;
                        p -= 12;
                        break;
                }
            }
            
            return (p + TypeToNote(v)) % 128;
        }

        /// <summary>
        /// 3화음 또는 7화음의 음들을 반환합니다.
        /// 반환값은 MIDI의 음 높이 배열입니다.
        /// </summary>
        /// <returns></returns>
        public int[] NotesInChord()
        {
            int[] r;
            if (type != Type.dom7 && type != Type.m7)
            {
                r = new int[3] {  (octave) * 12 + (int)root + TypeToNote(0),
                                  (octave) * 12 + (int)root + TypeToNote(1),
                                  (octave) * 12 + (int)root + TypeToNote(2) };
            }
            else
            {
                r = new int[4] {  (octave) * 12 + (int)root + TypeToNote(0),
                                  (octave) * 12 + (int)root + TypeToNote(1),
                                  (octave) * 12 + (int)root + TypeToNote(2),
                                  (octave) * 12 + (int)root + TypeToNote(3) };
            }
            return r;
        }

        private int TypeToNote(int order)
        {
            if (order < 0 || ((type != Type.dom7 && type != Type.m7) && order >= 3 ||
                (type == Type.dom7 || type == Type.m7) && order >= 4)) return -1;
            int[] typeToNote;
            switch (type)
            {
                case Type.Major:
                    typeToNote = new int[3] { 0, 4, 7 };
                    break;
                case Type.dom7:
                    typeToNote = new int[4] { 0, 4, 7, 10 };
                    break;
                case Type.minor:
                    typeToNote = new int[3] { 0, 3, 7 };
                    break;
                case Type.m7:
                    typeToNote = new int[4] { 0, 3, 7, 10 };
                    break;
                case Type.sus2:
                    typeToNote = new int[3] { 0, 2, 7 };
                    break;
                case Type.sus4:
                    typeToNote = new int[3] { 0, 5, 7 };
                    break;
                case Type.aug:
                    typeToNote = new int[3] { 0, 4, 8 };
                    break;
                case Type.dim:
                    typeToNote = new int[3] { 0, 3, 6 };
                    break;
                default: // case Type.NULL:
                    return 0;
            }
            return typeToNote[order];
        }

        /// <summary>
        /// 악보의 음들로부터 화음을 인식합니다.
        /// 반환되는 화음의 종류는 Major, minor, sus4, dim, aug, dom7, m7입니다.
        /// 따라서 12개의 Root 음에 대해 총 84가지 화음이 나올 수 있습니다.
        /// 반환되는 화음의 옥타브는 5로 고정됩니다.
        /// 빈 악보가 들어온 경우 화음의 종류가 NULL인 화음이 반환됩니다.
        /// </summary>
        /// <param name="score">악보</param>
        /// <param name="use4Harmonics">false이면 1배음 템플릿 사용(추천), true이면 4배음 템플릿 사용</param>
        /// <returns>인식한 화음</returns>
        public static Chord RecognizeChordFromScore(List<Note> score, bool use4Harmonics = false)
        {
            // L. Oudre, Y. Grenier, and C. Févotte. Chord Recognition by Fitting Rescaled Chroma Vectors to Chord Templates. IEEE Transactions on Audio, Speech, and Language Processing, vol. 19, no. 7, pp. 2222-2233, 2011.
            
            const double e = 0.0000000000000001;  // To avoid numerical instability

            #region Set an appropriate chord template.

            Dictionary<Type, Dictionary<Root, double[]>> template;
            if (!use4Harmonics)
            {
                // Initialize chord template(1 harmonic) at the first time.
                if (chordTemplates1 == null)
                {
                    chordTemplates1 = new Dictionary<Type, Dictionary<Root, double[]>>
                    {
                        { Type.Major, new Dictionary<Root, double[]>() },
                        { Type.minor, new Dictionary<Root, double[]>() },
                        { Type.sus4, new Dictionary<Root, double[]>() },
                        { Type.dim, new Dictionary<Root, double[]>() },
                        { Type.aug, new Dictionary<Root, double[]>() },
                        { Type.dom7, new Dictionary<Root, double[]>() },
                        { Type.m7, new Dictionary<Root, double[]>() }
                    };
                    chordTemplates1[Type.Major].Add(Root.C, new double[] { 0.334 - 9 * e, e, e, e, 0.333, e, e, 0.333, e, e, e, e });
                    chordTemplates1[Type.minor].Add(Root.C, new double[] { 0.334 - 9 * e, e, e, 0.333, e, e, e, 0.333, e, e, e, e });
                    chordTemplates1[Type.sus4].Add(Root.C, new double[] { 0.334 - 9 * e, e, e, e, e, 0.333, e, 0.333, e, e, e, e });
                    chordTemplates1[Type.dim].Add(Root.C, new double[] { 0.334 - 9 * e, e, e, 0.333, e, e, 0.333, e, e, e, e, e });
                    chordTemplates1[Type.aug].Add(Root.C, new double[] { 0.334 - 9 * e, e, e, e, 0.333, e, e, e, 0.333, e, e, e });
                    chordTemplates1[Type.dom7].Add(Root.C, new double[] { 0.25 - 2 * e, e, e, e, 0.25 - 2 * e, e, e, 0.25 - 2 * e, e, e, 0.25 - 2 * e, e });
                    chordTemplates1[Type.m7].Add(Root.C, new double[] { 0.25 - 2 * e, e, e, 0.25 - 2 * e, e, e, e, 0.25 - 2 * e, e, e, 0.25 - 2 * e, e });

                    foreach (Type t in new Type[] { Type.Major, Type.minor, Type.sus4, Type.dim, Type.aug, Type.dom7, Type.m7 })
                    {
                        for (int r = 1; r < 12; r++)
                        {
                            double[] temp = new double[12];
                            for (int i = 0; i < 12; i++)
                            {
                                temp[i] = chordTemplates1[t][Root.C][(i - r + 12) % 12];
                            }
                            chordTemplates1[t].Add((Root)r, temp);
                        }
                    }
                }
                template = chordTemplates1;
            }
            else
            {
                // Initialize chord template(4 harmonics) at the first time.
                if (chordTemplates4 == null)
                {
                    chordTemplates4 = new Dictionary<Type, Dictionary<Root, double[]>>
                    {
                        { Type.Major, new Dictionary<Root, double[]>() },
                        { Type.minor, new Dictionary<Root, double[]>() },
                        { Type.sus4, new Dictionary<Root, double[]>() },
                        { Type.dim, new Dictionary<Root, double[]>() },
                        { Type.aug, new Dictionary<Root, double[]>() },
                        { Type.dom7, new Dictionary<Root, double[]>() },
                        { Type.m7, new Dictionary<Root, double[]>() }
                    };
                    chordTemplates4[Type.Major].Add(Root.C, new double[] { 0.278, e, 0.055, e, 0.278, e, e, 0.334 - 7 * e, e, e, e, 0.055 });
                    chordTemplates4[Type.minor].Add(Root.C, new double[] { 0.278, e, 0.055, 0.278, e, e, e, 0.334 - 7 * e, e, e, 0.055, e });
                    chordTemplates4[Type.sus4].Add(Root.C, new double[] { 0.334 - 8 * e, e, 0.055, e, e, 0.278, e, 0.333, e, e, e, e });
                    chordTemplates4[Type.dim].Add(Root.C, new double[] { 0.279 - 6 * e, 0.055, e, 0.278, e, e, 0.278, 0.055, e, e, 0.055, e });
                    chordTemplates4[Type.aug].Add(Root.C, new double[] { 0.279 - 6 * e, e, e, 0.055, 0.278, e, e, 0.055, 0.278, e, e, 0.055 });
                    chordTemplates4[Type.dom7].Add(Root.C, new double[] { 0.209 - e, e, 0.041, e, 0.209 - e, 0.041, e, 0.25 - 2 * e, e, e, 0.209 - e, 0.041 });
                    chordTemplates4[Type.m7].Add(Root.C, new double[] { 0.209 - e, e, 0.041, 0.209 - e, e, 0.041, e, 0.25 - 2 * e, e, e, 0.25 - 2 * e, e });

                    foreach (Type t in new Type[] { Type.Major, Type.minor, Type.sus4, Type.dim, Type.aug, Type.dom7, Type.m7 })
                    {
                        for (int r = 1; r < 12; r++)
                        {
                            double[] temp = new double[12];
                            for (int i = 0; i < 12; i++)
                            {
                                temp[i] = chordTemplates4[t][Root.C][(i - r + 12) % 12];
                            }
                            chordTemplates4[t].Add((Root)r, temp);
                        }
                    }
                }
                template = chordTemplates4;
            }
            #endregion

            // Calculate the sum of duration of each pitch class in score.
            double[] pitchClassProfile = new double[12] { e, e, e, e, e, e, e, e, e, e, e, e };
            bool hasNoNote = true;
            foreach (Note n in score)
            {
                if (n.Velocity > 0)
                {
                    pitchClassProfile[n.Pitch % 12] += n.Rhythm;
                    hasNoNote = false;
                }
            }

            if (hasNoNote)
            {
                // There is no chord in the score.
                Random r = new Random();
                return new Chord((Root)r.Next(12), Type.NULL, 5);
            }

            // Normalize the pitch class profile.
            double sum = 0;
            for (int i = 0; i < 12; i++)
            {
                sum += pitchClassProfile[i];
            }
            for (int i = 0; i < 12; i++)
            {
                pitchClassProfile[i] /= sum;
            }

            // Calculate the measure of fit using KL2. (Kullback-Leibler divergence)
            Root minRoot = Root.G;
            Type minType = Type.minor;
            double minDistance = double.MaxValue;
            foreach (Type t in new Type[] { Type.Major, Type.minor, Type.sus4, Type.dim, Type.aug, Type.dom7, Type.m7 })
            {
                for (int r = 0; r < 12; r++)
                {
                    double distance = 0;
                    for (int m = 0; m < 12; m++)
                    {
                        double p = template[t][(Root)r][m];
                        double c = pitchClassProfile[m];
                        distance += p * Math.Log(p / c) - p + c;
                    }
                    //Console.WriteLine(r + "" + t + ": " + distance);
                    if (distance < minDistance)
                    {
                        // choose argmin
                        minDistance = distance;
                        minRoot = (Root)r;
                        minType = t;
                    }
                }
            }

            return new Chord(minRoot, minType, 5);
        }

        /// <summary>
        /// 화음 chord에 어울리는 색을 찾아서 반환합니다.
        /// </summary>
        /// <param name="chord"></param>
        /// <returns></returns>
        public static Color ChordColor(Chord chord)
        {
            int h = 0;
            float s = 1f, v = 1f;

            h = HueOfPitch(chord.root);

            switch (chord.type)
            {
                case Chord.Type.Major:
                    s = 0.7f;
                    v = 1f;
                    break;
                case Chord.Type.dom7:
                    s = 0.7f;
                    v = 0.8f;
                    break;
                case Chord.Type.minor:
                    s = 0.5f;
                    v = 0.5f;
                    h = (h + 60) % 360;
                    break;
                case Chord.Type.m7:
                    s = 0.5f;
                    v = 0.4f;
                    h = (h + 60) % 360;
                    break;
                case Chord.Type.sus2:
                    s = 0.5f;
                    v = 0.9f;
                    h = (h - 15) % 360;
                    break;
                case Chord.Type.sus4:
                    s = 0.5f;
                    v = 0.9f;
                    h = (h + 15) % 360;
                    break;
                case Chord.Type.dim:
                    s = 0.2f;
                    v = 0.5f;
                    break;
                case Chord.Type.aug:
                    s = 0.3f;
                    v = 0.8f;
                    break;
            }

            return HSVToColor(h, s, v);
        }

        /// <summary>
        /// 현재 화음에 어울리는 색을 찾아서 반환합니다.
        /// </summary>
        /// <returns></returns>
        public Color ChordColor()
        {
            return ChordColor(this);
        }

        /// <summary>
        /// pitch의 음이름에 어울리는 색을 찾아서 반환합니다.
        /// </summary>
        /// <param name="pitch"></param>
        /// <returns></returns>
        public static Color PitchColor(Root pitch)
        {
            int h = HueOfPitch(pitch);
            return HSVToColor(h, 0.7f, 1f);
        }

        /// <summary>
        /// 주어진 색의 alpha(투명도)를 바꿔서 반환합니다.
        /// alpha는 0 이상 255 이하입니다.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public static Color SetAlpha(Color c, int alpha)
        {
            if (alpha < 0) alpha = 0;
            if (alpha > 255) alpha = 255;
            return Color.FromArgb(alpha, c);
        }

        /// <summary>
        /// pitch의 음이름에 따른 색의 Hue 값을 반환합니다.
        /// 반환하는 값은 0 이상 359 이하입니다.
        /// </summary>
        /// <param name="pitch"></param>
        /// <returns></returns>
        private static int HueOfPitch(Root pitch)
        {
            int h = 0;
            switch (pitch)
            {
                case Chord.Root.C:
                    h = 120;
                    break;
                case Chord.Root.G:
                    h = 90;
                    break;
                case Chord.Root.D:
                    h = 60;
                    break;
                case Chord.Root.A:
                    h = 30;
                    break;
                case Chord.Root.E:
                    h = 0;
                    break;
                case Chord.Root.B:
                    h = 330;
                    break;
                case Chord.Root.Gb:
                    h = 300;
                    break;
                case Chord.Root.Db:
                    h = 270;
                    break;
                case Chord.Root.Ab:
                    h = 240;
                    break;
                case Chord.Root.Eb:
                    h = 210;
                    break;
                case Chord.Root.Bb:
                    h = 180;
                    break;
                case Chord.Root.F:
                    h = 150;
                    break;
            }
            return h;
        }

        /// <summary>
        /// HSV를 RGB로 변환한 색을 반환합니다.
        /// </summary>
        /// <param name="h">Hue, 0 이상 359 이하</param>
        /// <param name="s">Saturation, 0.0f 이상 1.0f 이하</param>
        /// <param name="v">Value(Brightness), 0.0f 이상 1.0f 이하</param>
        /// <returns></returns>
        private static Color HSVToColor(int h, float s, float v)
        {
            h = h % 360;

            if (v < 0) v = 0;
            else if (v > 1) v = 1;

            if (s <= 0)
            {
                return Color.FromArgb((int)(v * 255), (int)(v * 255), (int)(v * 255));
            }
            else if (s > 1) s = 1;

            /* HSV to RGB */
            int ff = h % 60;
            float p = v * (1f - s);
            float q = v * (1f - (s * (ff / 60f)));
            float t = v * (1f - (s * (1f - (ff / 60f))));
            float r, g, b;
            if (h >= 0 && h < 60)
            {
                r = v;
                g = t;
                b = p;
            }
            else if (h >= 60 && h < 120)
            {
                r = q;
                g = v;
                b = p;
            }
            else if (h >= 120 && h < 180)
            {
                r = p;
                g = v;
                b = t;
            }
            else if (h >= 180 && h < 240)
            {
                r = p;
                g = q;
                b = v;
            }
            else if (h >= 240 && h < 300)
            {
                r = t;
                g = p;
                b = v;
            }
            else if (h >= 300 && h < 360)
            {
                r = v;
                g = p;
                b = q;
            }
            else
            {
                r = 0;
                g = 0;
                b = 0;
            }
            r *= 255;
            g *= 255;
            b *= 255;
            if (r < 0) r = 0;
            if (r > 255) r = 255;
            if (g < 0) g = 0;
            if (g > 255) g = 255;
            if (b < 0) b = 0;
            if (b > 255) b = 255;

            return Color.FromArgb((int)(r + 0.5f), (int)(g + 0.5f), (int)(b + 0.5f));
        }
    }

}
