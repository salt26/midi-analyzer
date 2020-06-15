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
using System.Linq;

namespace MidiAnalyzer
{
    /// <summary>
    /// 추상적인 멜로디 형태를 표현하는 클래스입니다.
    /// 멜로디 형태를 구체화하여 악보로 만들 수 있습니다.
    /// </summary>
    public class MelodicContour
    {
        /// <summary>
        /// Distance() 함수 호출 시 콘솔에 내부 계산 과정을 출력하려면 이 값을 true로 설정합니다.
        /// </summary>
        private const bool DISTANCE_DEBUG = false;

        /// <summary>
        /// 음표 목록.
        /// 항상 음표의 시작 위치 순으로 정렬됩니다.
        /// 이전 음표의 길이만큼 지나고 다음 음표가 바로 나온다고 가정합니다.
        /// 서로 다른 클러스터 번호는 128개까지만 가질 수 있습니다.
        /// </summary>
        public LinkedList<MelodicContourNote> noteList = new LinkedList<MelodicContourNote>();

        /// <summary>
        /// 멜로디 형태의 맨 앞에 놓이는 쉼표의 길이.
        /// 0 이상의 값을 갖습니다.
        /// 음표 목록에서 처음 등장하는 음표의 시작 위치를 결정합니다.
        /// </summary>
        public int firstRestDuration = 0;

        /// <summary>
        /// 메타데이터 목록.
        /// 항상 클러스터 번호 순으로 정렬됩니다.
        /// 같은 클러스터 번호를 갖는 메타데이터를 둘 이상 포함할 수 없습니다.
        /// </summary>
        public List<MelodicContourMetadata> metadataList = new List<MelodicContourMetadata>();

        /// <summary>
        /// 이 멜로디 형태가 변경된 적이 있으면 true가 되며,
        /// 이때는 몇몇 값들을 다시 계산해야 합니다.
        /// </summary>
        public bool IsDirty { get; private set; } = false;

        /// <summary>
        /// 모든 클러스터 번호마다 절대적인 음 높이가 할당된 경우 true가 됩니다.
        /// </summary>
        public bool HasImplemented { get; private set; } = false;

        /// <summary>
        /// 새 멜로디 형태를 생성합니다.
        /// 처음에 넣을 음표들을 인자로 지정할 수 있습니다.
        /// </summary>
        /// <param name="firstRestDuration">멜로디 형태의 맨 앞에 놓이는 쉼표의 길이 (0 이상)</param>
        /// <param name="melodicContourNotes">멜로디 형태에 넣을 음표들</param>
        public MelodicContour(int firstRestDuration, params MelodicContourNote[] melodicContourNotes)
        {
            this.DelayNotes(firstRestDuration);
            foreach (MelodicContourNote n in melodicContourNotes)
            {
                this.InsertNote(noteList.Count, n);
            }
        }

        /// <summary>
        /// 악보(음표 목록)로부터 새 멜로디 형태를 생성합니다.
        /// 이 멜로디 형태에 속하는 각 MelodicContourNote의 길이는,
        /// 음표의 Rhythm와 관계 없이 한 음표와 바로 다음 음표 사이의 Position 차이로 설정됩니다.
        /// </summary>
        /// <param name="monophonicScore">monophony인 악보</param>
        /// <param name="scoreLength">악보의 길이. 64분음표가 몇 개 들어가는지를 기준으로 합니다.
        /// 이 길이보다 같거나 뒤에 놓인 음표는 고려하지 않습니다. 예) 4/4박자에서 한 마디 길이: 64</param>
        public MelodicContour(List<Note> monophonicScore, int scoreLength = 64)
        {
            List<Note> score = new List<Note>(monophonicScore);
            score.Sort((e1, e2) =>
            {
                if (e1.Measure != e2.Measure) return Math.Sign(e1.Measure - e2.Measure);
                else return e1.Position - e2.Position;
            });

            long startMeasure = 0;
            Note endNote = null;
            if (score.Count > 0)
            {
                startMeasure = score[0].Measure;
                endNote = new Note(1, 1, 1, startMeasure, scoreLength, 0, score[0].TimeSignature.Key, score[0].TimeSignature.Value);
                this.DelayNotes(score[0].Position);
            }

            for (int i = 0; i < score.Count; i++)
            {
                if (score[i].GetAbsolutePosition() >= endNote.GetAbsolutePosition())
                    break;

                if (i < score.Count - 1)
                    this.InsertNote(noteList.Count,
                        (int)(score[i + 1].GetAbsolutePosition() - score[i].GetAbsolutePosition()),
                        score[i].Pitch);
                else                                    // Last note
                    this.InsertNote(noteList.Count,
                        (int)(endNote.GetAbsolutePosition() - score[i].GetAbsolutePosition()),
                        score[i].Pitch);
            }
        }

        /// <summary>
        /// 불가능한 편집 연산을 수행한 경우 발생하는 비용입니다.
        /// 무한대라고 취급하면 됩니다.
        /// </summary>
        public const int INVALID_COST = int.MaxValue / 2;

        /// <summary>
        /// 편집 연산을 수행한 결과로 음표 또는 맨 앞 쉼표의 길이가 바뀌는 경우 발생하는 비용입니다.
        /// Delete, Insert, Replace, Delay에서만 발생할 수 있습니다.
        /// </summary>
        public const int DURATION_COST = 3;

        /// <summary>
        /// 편집 연산을 수행한 결과로 음표의 시작 위치가 바뀌는 경우 발생하는 비용입니다.
        /// Move, DelayAndReplace에서만 발생할 수 있습니다.
        /// </summary>
        public const int ONSET_COST = 3;

        /// <summary>
        /// 편집 연산을 수행한 결과로 음표의 음 높이 변화가 바뀌는 경우 발생하는 비용입니다.
        /// </summary>
        public const int PITCH_VARIANCE_COST = 4;

        /// <summary>
        /// 편집 연산을 수행한 결과로 음표의 음 높이 클러스터 순위가 바뀌는 경우 발생하는 비용입니다.
        /// </summary>
        public const int PITCH_CLUSTER_RANK_COST = 1;

        /// <summary>
        /// 편집 연산을 수행한 결과로 음표의 음 높이 클러스터 개수가 바뀌는 경우 발생하는 비용입니다.
        /// </summary>
        public const int LOCAL_PITCH_CLUSTER_COUNT_COST = 0;

        /// <summary>
        /// 두 멜로디 형태의 음 높이 클러스터 개수 차이에 곱해져서 발생하는 비용입니다.
        /// </summary>
        public const int GLOBAL_PITCH_CLUSTER_COUNT_COST = 3;

        /// <summary>
        /// 음표 목록에 있는 한 음표가 직전 음표보다 높은 음을 가지면 1,
        /// 낮은 음을 가지면 -1, 같은 음을 가지거나 첫 번째 음표이면 0을 반환합니다.
        /// 잘못된 음표 노드를 인자로 준 경우 예외를 발생시킵니다.
        /// </summary>
        /// <param name="noteNode">음표 목록에 있는 한 음표 노드</param>
        /// <returns></returns>
        public int PitchVariance(LinkedListNode<MelodicContourNote> noteNode)
        {
            if (noteNode == null)
            {
                throw new InvalidOperationException("Error in PitchVariance");
                //return -2;
            }
            else
            {
                if (noteNode.Value.pitchVariance != -2) return noteNode.Value.pitchVariance;
                else
                {
                    int variance;
                    LinkedListNode<MelodicContourNote> prev = noteNode.Previous;
                    if (prev == null) variance = 0;
                    else if (prev.Value.PitchCluster < noteNode.Value.PitchCluster) variance = 1;
                    else if (prev.Value.PitchCluster == noteNode.Value.PitchCluster) variance = 0;
                    else variance = -1;

                    noteNode.Value.pitchVariance = variance;
                    return variance;
                }
            }
        }

        /// <summary>
        /// 음표 목록에서 해당 인덱스를 가진 음표가 직전 음표보다 높은 음을 가지면 1,
        /// 낮은 음을 가지면 -1, 같은 음을 가지거나 첫 번째 음표이면 0을 반환합니다.
        /// 음표 목록에서 이 음표를 찾지 못한 경우 예외를 발생시킵니다.
        /// </summary>
        /// <param name="noteIndex">음표의 음표 목록에서의 인덱스</param>
        /// <returns></returns>
        public int PitchVariance(int noteIndex)
        {
            return PitchVariance(GetNoteNodeByIndex(noteIndex));
        }

        /// <summary>
        /// 한 클러스터에 절대적인 음 높이를 지정합니다.
        /// 음 높이는 0 이상 127 이하이어야 하고,
        /// 클러스터 번호가 클수록 항상 더 높은 음만 지정될 수 있습니다.
        /// 지정에 성공한 경우 true를 반환합니다.
        /// </summary>
        /// <param name="pitch"></param>
        public bool SetAbsolutePitch(float cluster, int pitch)
        {
            if (pitch < 0 || pitch > 127) return false;
            if (!metadataList.Exists(e => e.pitchCluster == cluster)) return false;
            int metadataIndex = metadataList.FindIndex(e => e.pitchCluster == cluster);

            int prevPitch = -1;
            int i;
            for (i = 1; metadataIndex - i >= 0; i++)
            {
                if (metadataList[metadataIndex - i].absolutePitch != -1)
                {
                    prevPitch = metadataList[metadataIndex - i].absolutePitch;
                    break;
                }
            }
            int minPitch = prevPitch + i;

            int nextPitch = 128;
            int j;
            for (j = 1; metadataIndex + j < metadataList.Count; j++)
            {
                if (metadataList[metadataIndex + j].absolutePitch != -1)
                {
                    nextPitch = metadataList[metadataIndex + j].absolutePitch;
                    break;
                }
            }
            int maxPitch = nextPitch - j;

            if (pitch >= minPitch && pitch <= maxPitch)
            {
                metadataList[metadataIndex].absolutePitch = pitch;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 한 클러스터의 절대적인 음 높이를 설정하지 않은 상태로 초기화합니다.
        /// </summary>
        /// <param name="cluster"></param>
        public void ClearAbsolutePitch(float cluster)
        {
            if (!metadataList.Exists(e => e.pitchCluster == cluster)) return;
            int metadataIndex = metadataList.FindIndex(e => e.pitchCluster == cluster);

            metadataList[metadataIndex].absolutePitch = -1;
            HasImplemented = false;
        }

        /// <summary>
        /// 음표 목록에서 주어진 음표 노드보다 앞에 위치한 음표들만 고려하여
        /// 이 음표 노드의 클러스터 순위를 반환합니다.
        /// 가장 작은 음 높이 클러스터 번호의 순위는 0입니다.
        /// 잘못된 음표 노드가 들어온 경우 -1을 반환합니다.
        /// </summary>
        /// <returns></returns>
        public int GetClusterRank(LinkedListNode<MelodicContourNote> noteNode)
        {
            if (noteNode == null) return -1;

            float pc = noteNode.Value.PitchCluster;
            List<float> pcList = new List<float>();
            LinkedListNode<MelodicContourNote> node = noteNode;
            while (node != null)
            {
                pcList.Add(node.Value.PitchCluster);
                node = node.Previous;
            }
            pcList = pcList.Distinct().ToList();
            pcList.Sort();
            return pcList.IndexOf(pc);
        }

        /// <summary>
        /// 음표 목록에서 주어진 음표 노드보다 앞에 위치한 음표들만 고려하여
        /// 멜로디 형태의 클러스터 개수를 반환합니다.
        /// 잘못된 음표 노드가 들어온 경우 0을 반환합니다.
        /// </summary>
        /// <returns></returns>
        public int GetClusterCount(LinkedListNode<MelodicContourNote> noteNode)
        {
            if (noteNode == null) return 0;

            List<float> pcList = new List<float>();
            LinkedListNode<MelodicContourNote> node = noteNode;
            while (node != null)
            {
                pcList.Add(node.Value.PitchCluster);
                node = node.Previous;
            }
            return pcList.Distinct().Count();
        }

        /// <summary>
        /// 주어진 클러스터 순위에 삽입할, 적절한 새 클러스터 번호를 반환합니다.
        /// 삽입 후에 이 인덱스 이후의 기존 클러스터들은 위상이 한 칸씩 뒤로 밀려납니다.
        /// 클러스터 번호가 작을수록 낮은 음입니다.
        /// (주의: 이 메서드는 새 클러스터를 삽입해주지 않습니다.
        /// 새 클러스터를 삽입하려면 InsertNote() 또는 ReplaceNote() 또는 MoveNotes()를 호출하고,
        /// 여기에 인자로 넣을 새 음표에 대해 이 메서드를 호출하십시오.)
        /// </summary>
        /// <param name="clusterRankToInsert">새 클러스터를 삽입할 클러스터 순위</param>
        /// <returns></returns>
        public float GetNewClusterNumber(int clusterRankToInsert)
        {
            if (metadataList.Count == 0)
            {
                // 빈 멜로디 형태에 삽입할 음표의 클러스터 번호
                return 0f;
            }
            else if (clusterRankToInsert <= 0)
            {
                // 멜로디 형태의 어떤 음보다도 더 낮은 음으로 삽입할 음표의 클러스터 번호
                return metadataList[0].pitchCluster - 8f;
            }
            else if (clusterRankToInsert >= metadataList.Count)
            {
                // 멜로디 형태의 어떤 음보다도 더 높은 음으로 삽입할 음표의 클러스터 번호
                return metadataList[metadataList.Count - 1].pitchCluster + 8f;
            }
            else
            {
                // 멜로디 형태의 연속된 두 음 사이의 음 높이를 갖도록 삽입할 음표의 클러스터 번호
                return (metadataList[clusterRankToInsert - 1].pitchCluster +
                    metadataList[clusterRankToInsert].pitchCluster) / 2f;
            }
        }

        /// <summary>
        /// 주어진 클러스터 순위를 가진 기존 클러스터 번호를 반환합니다.
        /// 이 클러스터에 새 음표를 삽입해도
        /// 다른 클러스터들의 위상에는 영향을 주지 않습니다.
        /// 클러스터 번호가 작을수록 낮은 음입니다.
        /// (주의: 이 메서드는 새 클러스터를 삽입해주지 않습니다.
        /// 새 클러스터를 삽입하려면 InsertNote() 또는 ReplaceNote() 또는 MoveNotes()를 호출하고,
        /// 여기에 인자로 넣을 새 음표에 대해 이 메서드를 호출하십시오.)
        /// </summary>
        /// <param name="clusterRank">클러스터 순위 (0 이상, 서로 다른 클러스터 개수 미만)</param>
        /// <returns></returns>
        public float GetExistingClusterNumber(int clusterRank)
        {
            if (metadataList.Count == 0)
            {
                // 빈 멜로디 형태에 삽입할 음표의 클러스터 번호
                return 0f;
            }
            else if (clusterRank < 0)
            {
                // 멜로디 형태에 있던 가장 낮은 음과 같은 음으로 삽입할 음표의 클러스터 번호
                return metadataList[0].pitchCluster;
            }
            else if (clusterRank >= metadataList.Count)
            {
                // 멜로디 형태에 있던 가장 높은 음과 같은 음으로 삽입할 음표의 클러스터 번호
                return metadataList[metadataList.Count - 1].pitchCluster;
            }
            else
            {
                // 주어진 클러스터 순위를 갖는 클러스터 번호
                return metadataList[clusterRank].pitchCluster;
            }
        }

        /// <summary>
        /// 멜로디 형태의 특정 인덱스에 음표 하나를 삽입하는 연산을 수행합니다.
        /// 반환값은 수행한 연산의 비용입니다.
        /// 삽입할 수 없는 위치에 음표를 삽입하려 하는 경우 삽입 연산이 수행되지 않고 INVALID_COST를 반환합니다.
        /// (새 음표를 정의할 때 GetNewClusterNumber() 또는
        /// GetExistingClusterNumber()를 사용하면 편리합니다.)
        /// </summary>
        /// <param name="noteIndex">새 음표가 삽입될, 음표 목록에서의 인덱스 (음수를 넣으면 맨 뒤에 삽입)</param>
        /// <param name="note">삽입할 새 음표</param>
        /// <returns></returns>
        public int InsertNote(int noteIndex, MelodicContourNote note)
        {
            if (note == null || note.Duration <= 0) return INVALID_COST;
            note = note.Copy();
            LinkedListNode<MelodicContourNote> node;

            if (noteIndex == noteList.Count)
            {
                // 맨 뒤에 삽입
                node = noteList.AddLast(note);
            }
            else if (noteIndex > noteList.Count || GetNoteNodeByIndex(noteIndex) == null)
            {
                return INVALID_COST;
            }
            else
            {
                // 해당 인덱스에 삽입
                node = noteList.AddBefore(GetNoteNodeByIndex(noteIndex), note);
            }

            // 메타데이터 편집
            int oldMetadataListCount = 0;
            if (LOCAL_PITCH_CLUSTER_COUNT_COST > 0)
            {
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                oldMetadataListCount = GetClusterCount(node.Previous);
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
            }

            MelodicContourMetadata m = metadataList.Find(e => e.pitchCluster == note.PitchCluster);
            if (m != null)
            {
                // 이미 같은 클러스터 번호의 음표가 존재하는 경우
                m.noteNodes.Add(node);
            }
            else
            {
                // 새로운 클러스터 번호를 가진 경우
                m = new MelodicContourMetadata(note.PitchCluster, node);
                metadataList.Add(m);
                metadataList.Sort();
            }

            int gamma = 0;
            if (LOCAL_PITCH_CLUSTER_COUNT_COST > 0)
            {
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                if (GetClusterCount(node) != oldMetadataListCount)
                {
                    // (음 높이 클러스터 개수 변경 비용)
                    gamma = LOCAL_PITCH_CLUSTER_COUNT_COST;
                }
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
            }

            if (node.Next != null)
            {
                // 직후 음표의 음 높이 변화 재계산
                node.Next.Value.pitchVariance = -2;
                PitchVariance(node.Next);
            }

            // 삽입된 음표의 음 높이 변화 계산
            int pv = PitchVariance(node);
            if (pv == 0)
            {
                // (음표 추가로 인한 길이 변경 비용)
                return DURATION_COST;
            }
            else
            {
                // 삽입된 음표의 음 높이 변화가 0이 아니므로 비용 추가
                // (음표 추가로 인한 길이 변경 비용 + 삽입된 음표의 음 높이 변화 변경 비용 + 삽입된 음표의 클러스터 순위 변경 비용 + gamma)
                return DURATION_COST + PITCH_VARIANCE_COST + PITCH_CLUSTER_RANK_COST + gamma;
            }
        }

        /// <summary>
        /// 멜로디 형태의 특정 인덱스에 음표 하나를 삽입하는 연산을 수행합니다.
        /// 반환값은 수행한 연산의 비용입니다.
        /// 삽입할 수 없는 위치에 음표를 삽입하려 하는 경우 삽입 연산이 수행되지 않고 INVALID_COST를 반환합니다.
        /// (새 음표를 정의할 때 GetNewClusterNumber() 또는
        /// GetExistingClusterNumber()를 사용하면 편리합니다.)
        /// </summary>
        /// <param name="noteIndex">새 음표가 삽입될, 음표 목록에서의 인덱스</param>
        /// <param name="noteDuration">삽입할 새 음표의 길이</param>
        /// <param name="notePitchCluster">삽입할 새 음표의 음 높이 클러스터 번호</param>
        /// <returns></returns>
        public int InsertNote(int noteIndex, int noteDuration, float notePitchCluster)
        {
            return InsertNote(noteIndex, new MelodicContourNote(noteDuration, notePitchCluster));
        }

        /// <summary>
        /// 멜로디 형태에서 특정 인덱스의 음표 하나를 제거하는 연산을 수행합니다.
        /// 반환값은 수행한 연산의 비용입니다.
        /// 존재하지 않는 음표를 제거하려 할 경우 제거 연산이 수행되지 않고 INVALID_COST를 반환합니다.
        /// </summary>
        /// <param name="noteIndex">제거할 기존 음표의 음표 목록에서의 인덱스</param>
        /// <returns></returns>
        public int DeleteNote(int noteIndex)
        {
            if (noteIndex >= noteList.Count) return INVALID_COST;

            LinkedListNode<MelodicContourNote> node = GetNoteNodeByIndex(noteIndex);

            if (node == null) return INVALID_COST;

            MelodicContourNote note = node.Value;

            // 메타데이터 편집
            int oldMetadataListCount = 0;
            if (LOCAL_PITCH_CLUSTER_COUNT_COST > 0)
            {
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                oldMetadataListCount = GetClusterCount(node);
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
            }

            MelodicContourMetadata m = metadataList.Find(e => e.pitchCluster == note.PitchCluster);
            if (m != null)
            {
                if (!m.noteNodes.Remove(node))
                {
                    Console.WriteLine("Error: MelodicContour DeleteNote metadata 1");
                    return INVALID_COST;
                }
                if (m.noteNodes.Count == 0)
                {
                    // 이 클러스터에 해당하는 음표가 모두 제거된 경우 메타데이터 제거
                    metadataList.Remove(m);
                }
            }
            else
            {
                Console.WriteLine("Error: MelodicContour DeleteNote metadata 2");
                return INVALID_COST;
            }

            int gamma = 0;
            if (LOCAL_PITCH_CLUSTER_COUNT_COST > 0)
            {
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                if (GetClusterCount(node.Previous) != oldMetadataListCount)
                {
                    // (음 높이 클러스터 개수 변경 비용)
                    gamma = LOCAL_PITCH_CLUSTER_COUNT_COST;
                }
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
            }

            int pv = PitchVariance(node);
            int alpha = 0;
            if (pv == -1 || pv == 1)
            {
                // 제거할 음표의 음 높이 변화가 0이 아니므로 비용 추가
                // (제거할 음표의 음 높이 변화 변경 비용 + 제거할 음표의 클러스터 순위 변경 비용)
                alpha = PITCH_VARIANCE_COST + PITCH_CLUSTER_RANK_COST; ;
            }

            LinkedListNode<MelodicContourNote> next = node.Next;

            noteList.Remove(node);

            if (next != null)
            {
                // 직후 음표의 음 높이 변화 재계산
                next.Value.pitchVariance = -2;
                PitchVariance(next);
            }

            // (음표 제거로 인한 길이 변경 비용 + alpha + gamma)
            return DURATION_COST + alpha + gamma;
        }

        /// <summary>
        /// 멜로디 형태에 있던 음표 하나의 인덱스(음표 목록에서의 상대적 위치)를 유지하면서
        /// 길이와 클러스터를 교체하는 연산을 수행합니다.
        /// 반환값은 수행한 연산의 비용입니다.
        /// 존재하지 않는 음표를 교체하려고 하는 경우
        /// 교체 연산을 수행하지 않고 INVALID_COST를 반환합니다.
        /// (새 음표를 정의할 때 GetNewClusterNumber() 또는
        /// GetExistingClusterNumber()를 사용하면 편리합니다.)
        /// </summary>
        /// <param name="oldNoteIndex">교체할 대상이 될 기존 음표의 음표 목록에서의 인덱스</param>
        /// <param name="newNote">교체될 새 음표</param>
        /// <returns></returns>
        public int ReplaceNote(int oldNoteIndex, MelodicContourNote newNote)
        {
            if (newNote == null || newNote.Duration <= 0) return INVALID_COST;
            newNote = newNote.Copy();

            if (oldNoteIndex >= noteList.Count) return INVALID_COST;

            LinkedListNode<MelodicContourNote> node = GetNoteNodeByIndex(oldNoteIndex);

            if (node == null) return INVALID_COST;

            MelodicContourNote oldNote = node.Value;

            int oldPv = PitchVariance(node);
            int oldClusterIndex = GetClusterRank(node);

            // 메타데이터 편집
            // 기존 음표와 새 음표의 클러스터 번호가 서로 같은 경우 건드릴 필요 없음
            int oldMetadataListCount = 0;
            if (LOCAL_PITCH_CLUSTER_COUNT_COST > 0)
            {
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                oldMetadataListCount = GetClusterCount(node);
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
            }

            if (oldNote.PitchCluster != newNote.PitchCluster)
            {
                // 기존 음표에 대한 메타데이터 수정
                MelodicContourMetadata m = metadataList.Find(e => e.pitchCluster == oldNote.PitchCluster);
                if (m != null)
                {
                    if (!m.noteNodes.Remove(node))
                    {
                        Console.WriteLine("Error: MelodicContour ReplaceNote metadata 1");
                        return INVALID_COST;
                    }
                    if (m.noteNodes.Count == 0)
                    {
                        // 이 클러스터에 해당하는 음표가 모두 제거된 경우 메타데이터 제거
                        metadataList.Remove(m);
                    }
                }
                else
                {
                    Console.WriteLine("Error: MelodicContour ReplaceNote metadata 2");
                    return INVALID_COST;
                }
            }

            int beta = 0;
            //Console.WriteLine("Duration: " + oldNote.Duration + " -> " + newNote.Duration);
            if (oldNote.Duration != newNote.Duration)
            {
                // (음표 길이 변경 비용)
                beta = DURATION_COST;
            }

            // 새 음표로 교체
            node.Value = newNote;
            newNote.pitchVariance = -2;

            int alpha = 0;
            int newPv = PitchVariance(node);
            //Console.WriteLine("PitchVariance: " + oldPv + " -> " + newPv);
            if (oldPv != newPv)
            {
                // (교체한 음표의 음 높이 변화 변경 비용)
                alpha += PITCH_VARIANCE_COST;
            }

            int newClusterIndex = GetClusterRank(node);
            //Console.WriteLine("ClusterRank: " + oldClusterIndex + " -> " + newClusterIndex);
            if (oldClusterIndex != newClusterIndex)
            {
                // (교체한 음표의 클러스터 순위 변경 비용)
                alpha += PITCH_CLUSTER_RANK_COST;
            }

            if (node.Next != null)
            {
                // 직후 음표의 음 높이 변화 재계산
                node.Next.Value.pitchVariance = -2;
                PitchVariance(node.Next);
            }

            if (oldNote.PitchCluster != newNote.PitchCluster)
            {
                // 새 음표에 대한 메타데이터 수정
                MelodicContourMetadata m = metadataList.Find(e => e.pitchCluster == newNote.PitchCluster);
                if (m != null)
                {
                    // 이미 같은 클러스터 번호의 음표가 존재하는 경우
                    m.noteNodes.Add(node);
                }
                else
                {
                    // 새로운 클러스터 번호를 가진 경우 메타데이터 추가 후 정렬
                    m = new MelodicContourMetadata(newNote.PitchCluster, node);
                    metadataList.Add(m);
                    metadataList.Sort();
                }
            }

            int gamma = 0;
            if (LOCAL_PITCH_CLUSTER_COUNT_COST > 0)
            {
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                if (GetClusterCount(node) != oldMetadataListCount)
                {
                    // (음 높이 클러스터 개수 변경 비용)
                    gamma = LOCAL_PITCH_CLUSTER_COUNT_COST;
                }
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
            }

            return beta + alpha + gamma;
        }

        /// <summary>
        /// 멜로디 형태에 있던 음표 하나의 인덱스(음표 목록에서의 상대적 위치)를 유지하면서
        /// 길이와 클러스터를 교체하는 연산을 수행합니다.
        /// 반환값은 수행한 연산의 비용입니다.
        /// 존재하지 않는 음표를 교체하려고 하는 경우
        /// 교체 연산을 수행하지 않고 INVALID_COST를 반환합니다.
        /// (새 음표를 정의할 때 GetNewClusterNumber() 또는
        /// GetExistingClusterNumber()를 사용하면 편리합니다.)
        /// </summary>
        /// <param name="oldNoteIndex">교체 대상이 될 기존 음표의 음표 목록에서의 인덱스</param>
        /// <param name="newNoteDuration">교체될 새 음표의 길이</param>
        /// <param name="newNotePitchCluster">교체될 새 음표의 음 높이 클러스터 번호</param>
        /// <returns></returns>
        public int ReplaceNote(int oldNoteIndex, int newNoteDuration, float newNotePitchCluster)
        {
            return ReplaceNote(oldNoteIndex, new MelodicContourNote(newNoteDuration, newNotePitchCluster));
        }

        /// <summary>
        /// 멜로디 형태의 맨 앞에 놓이는 쉼표의 길이를 변경하는 연산을 수행합니다.
        /// 음표 목록에서 처음 등장하는 음표의 시작 위치를 옮기는 효과를 가집니다.
        /// 반환값은 수행한 연산의 비용입니다.
        /// 음수를 인자로 넘기면 INVALID_COST를 반환합니다.
        /// </summary>
        /// <param name="newFirstRestDuration">맨 앞에 놓이는 쉼표의 새 길이</param>
        /// <returns></returns>
        public int DelayNotes(int newFirstRestDuration)
        {
            if (newFirstRestDuration < 0) return INVALID_COST;
            else if (firstRestDuration == newFirstRestDuration) return 0;
            else
            {
                firstRestDuration = newFirstRestDuration;
                return DURATION_COST;
            }
        }

        /// <summary>
        /// 멜로디 형태에 있던 연속된 음표 두 개의
        /// 인덱스(음표 목록에서의 상대적 위치)와 길이의 합을 유지하면서
        /// 두 음표의 길이와 클러스터를 동시에 옮기는 연산을 수행합니다.
        /// 반환값은 수행한 연산의 비용이며,
        /// 같은 결과를 내는 두 연산(Replace + Replace)을 따로 수행하는 것보다 저렴합니다.
        /// 존재하지 않는 음표를 옮기려고 하거나
        /// 연산 전후로 두 음표의 길이의 합이 달라지거나
        /// 앞 음표부터 적용한 비용이 0인 교체 연산(ReplaceNote()) 두 번으로
        /// 같은 결과를 낼 수 있는 경우
        /// 옮기기 연산을 수행하지 않고 INVALID_COST를 반환합니다.
        /// (새 음표를 정의할 때 GetNewClusterNumber() 또는
        /// GetExistingClusterNumber()를 사용하면 편리합니다.)
        /// </summary>
        /// <param name="oldFirstNoteIndex">옮길 대상이 될 기존 음표들 중 앞 음표의 음표 목록에서의 인덱스</param>
        /// <param name="newFirstNote">옮길 새 음표들 중 앞 음표</param>
        /// <param name="newSecondNote">옮길 새 음표들 중 뒤 음표</param>
        /// <returns></returns>
        public int MoveNotes(int oldFirstNoteIndex, MelodicContourNote newFirstNote, MelodicContourNote newSecondNote)
        {
            if (newFirstNote == null || newFirstNote.Duration <= 0 ||
                newSecondNote == null || newSecondNote.Duration <= 0) return INVALID_COST;
            newFirstNote = newFirstNote.Copy();
            newSecondNote = newSecondNote.Copy();

            if (oldFirstNoteIndex >= noteList.Count - 1) return INVALID_COST;

            LinkedListNode<MelodicContourNote> node1 = GetNoteNodeByIndex(oldFirstNoteIndex);
            LinkedListNode<MelodicContourNote> node2 = GetNoteNodeByIndex(oldFirstNoteIndex + 1);

            if (node1 == null || node2 == null) return INVALID_COST;

            MelodicContourNote oldFirstNote = node1.Value;
            MelodicContourNote oldSecondNote = node2.Value;

            if (oldFirstNote.Duration == newFirstNote.Duration &&
                oldSecondNote.Duration == newSecondNote.Duration)
            {
                // 이 경우 ReplaceNote 두 번을 수행하는 것으로 대체할 수 있음
                return INVALID_COST;
            }

            if (oldFirstNote.Duration + oldSecondNote.Duration != newFirstNote.Duration + newSecondNote.Duration)
            {
                // 기존의 연속된 두 음표의 길이의 합과 새 연속된 두 음표의 길이의 합이 같지 않으면 이 연산을 수행할 수 없음
                return INVALID_COST;
            }

            int oldPv1 = PitchVariance(node1);
            int oldClusterIndex1 = GetClusterRank(node1);
            int oldPv2 = PitchVariance(node2);
            int oldClusterIndex2 = GetClusterRank(node2);

            MelodicContourMetadata m1 = metadataList.Find(e => e.pitchCluster == oldFirstNote.PitchCluster);
            MelodicContourMetadata m2 = metadataList.Find(e => e.pitchCluster == oldSecondNote.PitchCluster);
            bool deleteM1 = false;
            bool deleteM2 = false;

            // 메타데이터 편집
            // 기존 음표와 새 음표의 클러스터 번호가 서로 같은 경우 건드릴 필요 없음
            if (oldFirstNote.PitchCluster != newFirstNote.PitchCluster)
            {
                // 기존 음표에 대한 메타데이터 수정
                if (m1 != null)
                {
                    // 여기서는 지울 수 있는지 확인만 하고, 두 음표의 확인이 모두 끝나면 일괄적으로 제거
                    if (!m1.noteNodes.Contains(node1))
                    {
                        Console.WriteLine("Error: MelodicContour MoveNotes metadata 1");
                        return INVALID_COST;
                    }
                    else
                    {
                        deleteM1 = true;
                    }
                }
                else
                {
                    Console.WriteLine("Error: MelodicContour MoveNotes metadata 2");
                    return INVALID_COST;
                }
            }

            // 메타데이터 편집
            // 기존 음표와 새 음표의 클러스터 번호가 서로 같은 경우 건드릴 필요 없음
            int oldMetadataListCount = 0;
            if (LOCAL_PITCH_CLUSTER_COUNT_COST > 0)
            {
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                oldMetadataListCount = GetClusterCount(node2);
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
            }

            if (oldSecondNote.PitchCluster != newSecondNote.PitchCluster)
            {
                if (m2 != null)
                {
                    // 여기서는 지울 수 있는지 확인만 하고, 두 음표의 확인이 모두 끝나면 일괄적으로 제거
                    if (!m2.noteNodes.Contains(node2))
                    {
                        Console.WriteLine("Error: MelodicContour MoveNotes metadata 3");
                        return INVALID_COST;
                    }
                    else
                    {
                        deleteM2 = true;
                    }
                }
                else
                {
                    Console.WriteLine("Error: MelodicContour MoveNotes metadata 4");
                    return INVALID_COST;
                }
            }

            if (deleteM1)
            {
                // 기존 음표에 대한 메타데이터 수정
                m1.noteNodes.Remove(node1);
                if (m1.noteNodes.Count == 0)
                {
                    // 이 클러스터에 해당하는 음표가 모두 제거된 경우 메타데이터 제거
                    metadataList.Remove(m1);
                }
            }

            if (deleteM2)
            {
                // 기존 음표에 대한 메타데이터 수정
                m2.noteNodes.Remove(node2);
                if (m2.noteNodes.Count == 0)
                {
                    // 이 클러스터에 해당하는 음표가 모두 제거된 경우 메타데이터 제거
                    metadataList.Remove(m2);
                }
            }

            // (뒤 음표 시작 위치 변경 비용)
            int beta = ONSET_COST;

            // 새 음표로 교체
            node1.Value = newFirstNote;
            newFirstNote.pitchVariance = -2;
            node2.Value = newSecondNote;
            newSecondNote.pitchVariance = -2;

            int alpha = 0;
            int newPv1 = PitchVariance(node1);
            int newPv2 = PitchVariance(node2);
            if (oldPv1 != newPv1)
            {
                // (교체한 음표의 음 높이 변화 변경 비용)
                alpha += PITCH_VARIANCE_COST;
            }
            if (oldPv2 != newPv2)
            {
                // (교체한 음표의 음 높이 변화 변경 비용)
                alpha += PITCH_VARIANCE_COST;
            }

            int newClusterIndex1 = GetClusterRank(node1);
            int newClusterIndex2 = GetClusterRank(node2);
            if (oldClusterIndex1 != newClusterIndex1)
            {
                // (교체한 음표의 클러스터 순위 변경 비용)
                alpha += PITCH_CLUSTER_RANK_COST;
            }
            if (oldClusterIndex2 != newClusterIndex2)
            {
                // (교체한 음표의 클러스터 순위 변경 비용)
                alpha += PITCH_CLUSTER_RANK_COST;
            }

            if (node2.Next != null)
            {
                // 뒤 음표의 직후 음표의 음 높이 변화 재계산
                node2.Next.Value.pitchVariance = -2;
                PitchVariance(node2.Next);
            }

            if (oldFirstNote.PitchCluster != newFirstNote.PitchCluster)
            {
                // 새 앞 음표에 대한 메타데이터 수정
                MelodicContourMetadata m = metadataList.Find(e => e.pitchCluster == newFirstNote.PitchCluster);
                if (m != null)
                {
                    // 이미 같은 클러스터 번호의 음표가 존재하는 경우
                    m.noteNodes.Add(node1);
                }
                else
                {
                    // 새로운 클러스터 번호를 가진 경우 메타데이터 추가 후 정렬
                    m = new MelodicContourMetadata(newFirstNote.PitchCluster, node1);
                    metadataList.Add(m);
                    metadataList.Sort();
                }
            }
            if (oldSecondNote.PitchCluster != newSecondNote.PitchCluster)
            {
                // 새 뒤 음표에 대한 메타데이터 수정
                MelodicContourMetadata m = metadataList.Find(e => e.pitchCluster == newSecondNote.PitchCluster);
                if (m != null)
                {
                    // 이미 같은 클러스터 번호의 음표가 존재하는 경우
                    m.noteNodes.Add(node2);
                }
                else
                {
                    // 새로운 클러스터 번호를 가진 경우 메타데이터 추가 후 정렬
                    m = new MelodicContourMetadata(newSecondNote.PitchCluster, node2);
                    metadataList.Add(m);
                    metadataList.Sort();
                }
            }

            int gamma = 0;
            if (LOCAL_PITCH_CLUSTER_COUNT_COST > 0)
            {
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                if (GetClusterCount(node2) != oldMetadataListCount)
                {
                    // (음 높이 클러스터 개수 변경 비용)
                    gamma = LOCAL_PITCH_CLUSTER_COUNT_COST;
                }
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
            }

            return beta + alpha + gamma;
        }

        /// <summary>
        /// 멜로디 형태에 있던 맨 앞 쉼표와 맨 처음 음표의 길이의 합을 유지하면서
        /// 쉼표 및 음표의 길이와 클러스터를 동시에 교체하는 연산을 수행합니다.
        /// 반환값은 수행한 연산의 비용이며,
        /// 같은 결과를 내는 두 연산(Delay + Replace)을 따로 수행하는 것보다 저렴합니다.
        /// 멜로디 형태에 아무 음표도 들어있지 않거나
        /// 연산 전후로 쉼표 및 음표의 길이의 합이 달라지거나
        /// 비용이 0인 쉼표 길이 변경 연산(DelayNotes())과 교체 연산(ReplaceNote())을
        /// 차례로 수행하여 같은 결과를 낼 수 있는 경우
        /// 옮기기 연산을 수행하지 않고 INVALID_COST를 반환합니다.
        /// (새 음표를 정의할 때 GetNewClusterNumber() 또는
        /// GetExistingClusterNumber()를 사용하면 편리합니다.)
        /// </summary>
        /// <param name="newFirstRestDuration">맨 앞에 놓이는 쉼표의 새 길이</param>
        /// <param name="newFirstNote">맨 처음 음표가 옮겨질 새 음표</param>
        /// <returns></returns>
        public int DelayAndReplaceNotes(int newFirstRestDuration, MelodicContourNote newFirstNote)
        {
            if (newFirstRestDuration < 0 ||
                newFirstNote == null || newFirstNote.Duration <= 0) return INVALID_COST;
            newFirstNote = newFirstNote.Copy();

            if (noteList.Count <= 0) return INVALID_COST;

            LinkedListNode<MelodicContourNote> node1 = GetNoteNodeByIndex(0);

            if (node1 == null) return INVALID_COST;

            MelodicContourNote oldFirstNote = node1.Value;

            if (firstRestDuration == newFirstRestDuration &&
                oldFirstNote.Duration == newFirstNote.Duration)
            {
                // 이 경우 DelayNotes와 ReplaceNote를 차례로 수행하는 것으로 대체할 수 있음
                return INVALID_COST;
            }

            if (firstRestDuration + oldFirstNote.Duration != newFirstRestDuration + newFirstNote.Duration)
            {
                // 기존의 쉼표 및 음표의 길이의 합과 새 쉼표 및 음표의 길이의 합이 같지 않으면 이 연산을 수행할 수 없음
                return INVALID_COST;
            }

            int oldPv1 = PitchVariance(node1);
            int oldClusterIndex1 = GetClusterRank(node1);

            // 메타데이터 편집
            // 기존 음표와 새 음표의 클러스터 번호가 서로 같은 경우 건드릴 필요 없음
            int oldMetadataListCount = 0;
            if (LOCAL_PITCH_CLUSTER_COUNT_COST > 0)
            {
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                oldMetadataListCount = GetClusterCount(node1);
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
            }

            if (oldFirstNote.PitchCluster != newFirstNote.PitchCluster)
            {
                MelodicContourMetadata m1 = metadataList.Find(e => e.pitchCluster == oldFirstNote.PitchCluster);

                // 기존 음표에 대한 메타데이터 수정
                if (m1 != null)
                {
                    // 기존 음표에 대한 메타데이터 수정
                    if (!m1.noteNodes.Remove(node1))
                    {
                        Console.WriteLine("Error: MelodicContour DelayAndReplaceNotes metadata 1");
                        return INVALID_COST;

                    }
                    if (m1.noteNodes.Count == 0)
                    {
                        // 이 클러스터에 해당하는 음표가 모두 제거된 경우 메타데이터 제거
                        metadataList.Remove(m1);
                    }
                }
                else
                {
                    Console.WriteLine("Error: MelodicContour DelayAndReplaceNotes metadata 2");
                    return INVALID_COST;
                }
            }

            // (음표 시작 위치 변경 비용)
            int beta = ONSET_COST;

            // 새 음표로 교체
            node1.Value = newFirstNote;
            newFirstNote.pitchVariance = -2;

            // 쉼표의 새 길이로 설정
            firstRestDuration = newFirstRestDuration;

            int newPv1 = PitchVariance(node1);

            int newClusterIndex1 = GetClusterRank(node1);

            if (node1.Next != null)
            {
                // 맨 처음 음표의 직후 음표의 음 높이 변화 재계산
                node1.Next.Value.pitchVariance = -2;
                PitchVariance(node1.Next);
            }

            if (oldFirstNote.PitchCluster != newFirstNote.PitchCluster)
            {
                // 새 음표에 대한 메타데이터 수정
                MelodicContourMetadata m = metadataList.Find(e => e.pitchCluster == newFirstNote.PitchCluster);
                if (m != null)
                {
                    // 이미 같은 클러스터 번호의 음표가 존재하는 경우
                    m.noteNodes.Add(node1);
                }
                else
                {
                    // 새로운 클러스터 번호를 가진 경우 메타데이터 추가 후 정렬
                    m = new MelodicContourMetadata(newFirstNote.PitchCluster, node1);
                    metadataList.Add(m);
                    metadataList.Sort();
                }
            }

            int gamma = 0;
            if (LOCAL_PITCH_CLUSTER_COUNT_COST > 0)
            {
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                if (GetClusterCount(node1) != oldMetadataListCount)
                {
                    // (음 높이 클러스터 개수 변경 비용)
                    gamma = LOCAL_PITCH_CLUSTER_COUNT_COST;
                }
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
            }

            return beta + gamma;
        }

        /// <summary>
        /// 주어진 편집 연산 정보에 따라 해당 연산(Delete, Insert, Move, Delay)을 수행합니다.
        /// 반환값은 수행한 연산의 비용입니다.
        /// </summary>
        /// <param name="operation">편집 연산 정보</param>
        /// <returns></returns>
        public int PerformOperation(OperationInfo operation)
        {
            if (operation == null) return INVALID_COST;
            LinkedListNode<MelodicContourNote> node;
            switch (operation.type)
            {
                case OperationInfo.Type.Delete:
                    node = GetNoteNodeByIndex(operation.noteIndex);
                    if (node == null || !operation.noteBeforeOp.Equals(node.Value))
                        return INVALID_COST;
                    else
                        return DeleteNote(operation.noteIndex);
                case OperationInfo.Type.Insert:
                    return InsertNote(operation.noteIndex, operation.noteAfterOp);
                case OperationInfo.Type.Replace:
                    node = GetNoteNodeByIndex(operation.noteIndex);
                    if (node == null || !operation.noteBeforeOp.Equals(node.Value))
                        return INVALID_COST;
                    else
                        return ReplaceNote(operation.noteIndex, operation.noteAfterOp);
                case OperationInfo.Type.Delay:
                    if (firstRestDuration != operation.firstRestDurationBeforeOp)
                        return INVALID_COST;
                    else
                        return DelayNotes(operation.firstRestDurationAfterOp);
                case OperationInfo.Type.Move:
                    node = GetNoteNodeByIndex(operation.noteIndex);
                    if (node == null || !operation.noteBeforeOp.Equals(node.Value) ||
                        node.Next == null || !operation.note2BeforeOp.Equals(node.Next.Value))
                        return INVALID_COST;
                    else
                        return MoveNotes(operation.noteIndex, operation.noteAfterOp, operation.note2AfterOp);
                case OperationInfo.Type.DelayAndReplace:
                    node = GetNoteNodeByIndex(operation.noteIndex);
                    if (firstRestDuration != operation.firstRestDurationBeforeOp ||
                        node == null || !operation.noteBeforeOp.Equals(node.Value))
                        return INVALID_COST;
                    else
                        return DelayAndReplaceNotes(operation.firstRestDurationAfterOp, operation.noteAfterOp);
                default:
                    return INVALID_COST;
            }
        }

        /// <summary>
        /// 이 멜로디 형태를 콘솔에 보기 좋게 출력합니다.
        /// 만약 'X'가 출력에 포함된다면 오류가 발생한 것입니다.
        /// </summary>
        public void Print()
        {
            if (noteList == null || noteList.Count == 0)
            {
                Console.WriteLine("Empty MelodicContour");
                return;
            }
            int onset = firstRestDuration;
            int length = onset;
            Console.Write(firstRestDuration);
            foreach (MelodicContourNote note in noteList)
            {
                Console.Write(" [" + note.Duration + "," + note.PitchCluster + "]");
                length += note.Duration;
            }
            Console.WriteLine();

            for (int j = 0; j <= length; j++)
            {
                if (j > 0 && j % 10 == 0) Console.Write((j / 10) % 10);
                else Console.Write(" ");
            }
            Console.WriteLine();

            for (int i = metadataList.Count - 1; i >= 0; i--)
            {
                LinkedListNode<MelodicContourNote> node = noteList.First;
                onset = firstRestDuration;
                for (int j = 0; j <= length; j++)
                {
                    //Console.WriteLine(node.Value.PitchCluster + " ==? " + metadataList[i].pitchCluster);
                    if (node == null)
                        Console.Write(".");
                    else if (onset == j && node.Value.PitchCluster == metadataList[i].pitchCluster &&
                        !metadataList[i].noteNodes.Contains(node))
                    {
                        // metadataList and noteList are inconsistent!
                        Console.Write("X");
                    }
                    else if (onset == j && node.Value.PitchCluster == metadataList[i].pitchCluster)
                    {
                        switch (PitchVariance(node))
                        {
                            case 0:
                                Console.Write("0");
                                break;
                            case 1:
                                Console.Write("+");
                                break;
                            case -1:
                                Console.Write("-");
                                break;
                            default:
                                // Something wrong!
                                Console.Write("X");
                                break;
                        }
                        onset += node.Value.Duration;
                        node = node.Next;
                    }
                    else if (onset == j)
                    {
                        Console.Write(".");
                        onset += node.Value.Duration;
                        node = node.Next;
                    }
                    else
                    {
                        Console.Write(".");
                    }
                }
                if (metadataList[i].pitchCluster >= 0)
                    Console.WriteLine("  " + metadataList[i].pitchCluster);
                else
                    Console.WriteLine(" " + metadataList[i].pitchCluster);
            }
        }

        /// <summary>
        /// 두 멜로디 형태 사이의 거리(비유사도)를 계산하여 반환합니다.
        /// 최소 거리를 구하기 위해 필요한 편집 연산들도 출력합니다.
        /// 거리는 이 멜로디 형태에 여러 번의 편집 연산(Delete, Insert, Replace, Delay)을 적용하여
        /// other 멜로디 형태로 만들 수 있는 최소 비용으로 정의됩니다.
        /// (이 메서드의 결과는 반대로 적용해도 똑같기 때문에 distance metric으로 사용될 수 있습니다.)
        /// </summary>
        /// <param name="other">다른 멜로디 형태</param>
        /// <returns></returns>
        public int Distance(MelodicContour other)
        {
            // Note: This method finds the local optimum, not the global optimum.
            return Math.Min(this.DistanceWithDirection(other), other.DistanceWithDirection(this));
        }

        /// <summary>
        /// 두 멜로디 형태 사이의 거리(비유사도)를 계산하여 반환합니다.
        /// 최소 거리를 구하기 위해 필요한 편집 연산들도 출력합니다.
        /// 거리는 이 멜로디 형태에 여러 번의 편집 연산(Delete, Insert, Replace, Delay)을 적용하여
        /// other 멜로디 형태로 만들 수 있는 최소 비용으로 정의됩니다.
        /// (주의: other에서 이 멜로디 형태로 가는 거리를 구하면 값이 다를 수 있습니다.
        /// 보통의 경우 이 메서드 대신 Distance()를 사용하십시오!)
        /// </summary>
        /// <param name="other">다른 멜로디 형태</param>
        /// <returns></returns>
        public int DistanceWithDirection(MelodicContour other)
        {
            // Dynamic Programming (time complexity: O(n^2))
            // Note: This method finds the local optimum, not the global optimum.
            // To find the global optimum, backtracking technique should be used. (time complexity: O(4^n))

            int lenThis = this.noteList.Count;
            int lenOther = other.noteList.Count;
            MelodicContour mc;
            LinkedListNode<MelodicContourNote> nodeB;
            LinkedListNode<MelodicContourNote> nodeB2;
            List<List<DistanceTable>> distanceTable =
                new List<List<DistanceTable>>(lenThis + 1);
            int delayCost = 0;
            int pitchClusterCountCost = 0;

            for (int i = 0; i <= lenThis; i++)
            {
                List<DistanceTable> temp =
                    new List<DistanceTable>(lenOther + 1);
                for (int j = 0; j <= lenOther; j++)
                {
                    temp.Add(new DistanceTable(INVALID_COST));
                }
                distanceTable.Add(temp);

                for (int j = 0; j <= lenOther; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        var list = new List<KeyValuePair<int, OperationInfo.Type>>();
                        distanceTable[i][j] = new DistanceTable(0, list);
                        if (DISTANCE_DEBUG)
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                            Console.Write(distanceTable[i][j].Distance + "\t");     // TODO
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
                        continue;
                    }

                    List<DistanceTable> costs = new List<DistanceTable>();

                    #region Delete 연산
                    if (i > 0)
                    {
                        foreach (List<KeyValuePair<int, OperationInfo.Type>> path in distanceTable[i - 1][j].Paths)
                        {
                            List<OperationInfo> operations = PathToOperations(path, Copy(this, i - 1), Copy(other, j - 1));
                            mc = Copy(this, i - 1);

                            MelodicContour tempMC = mc.Copy();
                            List<KeyValuePair<int, OperationInfo.Type>> tempPath = new List<KeyValuePair<int, OperationInfo.Type>>(path);
                            tempPath.Add(new KeyValuePair<int, OperationInfo.Type>(0, OperationInfo.Type.Delete));
                            costs.Add(new DistanceTable(
                                distanceTable[i - 1][j].Distance + tempMC.DeleteNote(tempMC.noteList.Count - 1),
                                tempPath));

                            for (int k = 0; k < operations.Count; k++)
                            {
                                if (mc.PerformOperation(operations[k]) == INVALID_COST)
                                {
                                    break;
                                }
                                tempMC = mc.Copy();
                                tempPath = new List<KeyValuePair<int, OperationInfo.Type>>(path);
                                tempPath.Add(new KeyValuePair<int, OperationInfo.Type>(k + 1, OperationInfo.Type.Delete));
                                costs.Add(new DistanceTable(
                                    distanceTable[i - 1][j].Distance + tempMC.DeleteNote(tempMC.noteList.Count - 1),
                                    tempPath));
                            }
                        }
                    }
                    #endregion

                    #region Insert 연산
                    if (j > 0)
                    {
                        foreach (List<KeyValuePair<int, OperationInfo.Type>> path in distanceTable[i][j - 1].Paths)
                        {
                            List<OperationInfo> operations = PathToOperations(path, Copy(this, i - 1), Copy(other, j - 1));
                            mc = Copy(this, i - 1);
                            nodeB = other.GetNoteNodeByIndex(j - 1);

                            MelodicContour tempMC = mc.Copy();
                            List<KeyValuePair<int, OperationInfo.Type>> tempPath = new List<KeyValuePair<int, OperationInfo.Type>>(path);
                            tempPath.Add(new KeyValuePair<int, OperationInfo.Type>(0, OperationInfo.Type.Insert));
                            costs.Add(new DistanceTable(
                                distanceTable[i][j - 1].Distance + tempMC.InsertNote(tempMC.noteList.Count, nodeB.Value.Copy()),
                                tempPath));

                            for (int k = 0; k < operations.Count; k++)
                            {
                                if (mc.PerformOperation(operations[k]) == INVALID_COST)
                                {
                                    break;
                                }
                                tempMC = mc.Copy();
                                tempPath = new List<KeyValuePair<int, OperationInfo.Type>>(path);
                                tempPath.Add(new KeyValuePair<int, OperationInfo.Type>(k + 1, OperationInfo.Type.Insert));
                                costs.Add(new DistanceTable(
                                    distanceTable[i][j - 1].Distance + tempMC.InsertNote(tempMC.noteList.Count, nodeB.Value.Copy()),
                                    tempPath));
                            }
                        }
                    }
                    #endregion

                    #region Replace 연산
                    if (i > 0 && j > 0)
                    {
                        foreach (List<KeyValuePair<int, OperationInfo.Type>> path in distanceTable[i - 1][j - 1].Paths)
                        {
                            List<OperationInfo> operations = PathToOperations(path, Copy(this, i - 1), Copy(other, j - 1));
                            mc = Copy(this, i - 1);
                            nodeB = other.GetNoteNodeByIndex(j - 1);

                            MelodicContour tempMC = mc.Copy();
                            List<KeyValuePair<int, OperationInfo.Type>> tempPath = new List<KeyValuePair<int, OperationInfo.Type>>(path);
                            tempPath.Add(new KeyValuePair<int, OperationInfo.Type>(0, OperationInfo.Type.Replace));
                            costs.Add(new DistanceTable(
                                distanceTable[i - 1][j - 1].Distance + tempMC.ReplaceNote(tempMC.noteList.Count - 1, nodeB.Value.Copy()),
                                tempPath));

                            for (int k = 0; k < operations.Count; k++)
                            {
                                if (mc.PerformOperation(operations[k]) == INVALID_COST)
                                {
                                    break;
                                }
                                tempMC = mc.Copy();
                                tempPath = new List<KeyValuePair<int, OperationInfo.Type>>(path);
                                tempPath.Add(new KeyValuePair<int, OperationInfo.Type>(k + 1, OperationInfo.Type.Replace));
                                costs.Add(new DistanceTable(
                                    distanceTable[i - 1][j - 1].Distance + tempMC.ReplaceNote(tempMC.noteList.Count - 1, nodeB.Value.Copy()),
                                    tempPath));
                            }
                        }
                    }
                    #endregion

                    #region Move 연산
                    if (i > 1 && j > 1)
                    {
                        foreach (List<KeyValuePair<int, OperationInfo.Type>> path in distanceTable[i - 2][j - 2].Paths)
                        {
                            List<OperationInfo> operations = PathToOperations(path, Copy(this, i - 1), Copy(other, j - 1));
                            mc = Copy(this, i - 1);
                            nodeB = other.GetNoteNodeByIndex(j - 2);
                            nodeB2 = other.GetNoteNodeByIndex(j - 1);

                            MelodicContour tempMC = mc.Copy();
                            List<KeyValuePair<int, OperationInfo.Type>> tempPath = new List<KeyValuePair<int, OperationInfo.Type>>(path);
                            tempPath.Add(new KeyValuePair<int, OperationInfo.Type>(0, OperationInfo.Type.Move));
                            costs.Add(new DistanceTable(
                                distanceTable[i - 2][j - 2].Distance + tempMC.MoveNotes(tempMC.noteList.Count - 2, nodeB.Value.Copy(), nodeB2.Value.Copy()),
                                tempPath));

                            for (int k = 0; k < operations.Count; k++)
                            {
                                if (mc.PerformOperation(operations[k]) == INVALID_COST)
                                {
                                    break;
                                }
                                tempMC = mc.Copy();
                                tempPath = new List<KeyValuePair<int, OperationInfo.Type>>(path);
                                tempPath.Add(new KeyValuePair<int, OperationInfo.Type>(k + 1, OperationInfo.Type.Move));
                                costs.Add(new DistanceTable(
                                    distanceTable[i - 2][j - 2].Distance + tempMC.MoveNotes(tempMC.noteList.Count - 2, nodeB.Value.Copy(), nodeB2.Value.Copy()),
                                    tempPath));
                            }
                        }
                    }
                    #endregion

                    #region DelayAndReplace 연산
                    if (i == 1 && j == 1)
                    {
                        foreach (List<KeyValuePair<int, OperationInfo.Type>> path in distanceTable[i - 1][j - 1].Paths)
                        {
                            List<OperationInfo> operations = PathToOperations(path, Copy(this, i - 1), Copy(other, j - 1));
                            mc = Copy(this, i - 1);
                            nodeB = other.GetNoteNodeByIndex(j - 1);

                            MelodicContour tempMC = mc.Copy();
                            List<KeyValuePair<int, OperationInfo.Type>> tempPath = new List<KeyValuePair<int, OperationInfo.Type>>(path);
                            tempPath.Add(new KeyValuePair<int, OperationInfo.Type>(0, OperationInfo.Type.DelayAndReplace));
                            costs.Add(new DistanceTable(
                                distanceTable[i - 1][j - 1].Distance + tempMC.DelayAndReplaceNotes(other.firstRestDuration, nodeB.Value.Copy()),
                                tempPath));

                            for (int k = 0; k < operations.Count; k++)
                            {
                                if (mc.PerformOperation(operations[k]) == INVALID_COST)
                                {
                                    break;
                                }
                                tempMC = mc.Copy();
                                tempPath = new List<KeyValuePair<int, OperationInfo.Type>>(path);
                                tempPath.Add(new KeyValuePair<int, OperationInfo.Type>(k + 1, OperationInfo.Type.DelayAndReplace));
                                costs.Add(new DistanceTable(
                                    distanceTable[i - 1][j - 1].Distance + tempMC.DelayAndReplaceNotes(other.firstRestDuration, nodeB.Value.Copy()),
                                    tempPath));
                            }
                        }
                    }
                    #endregion

                    #region 현재 단계의 연산 결과(d_i,j) 기록

                    distanceTable[i][j] = new DistanceTable(INVALID_COST);

                    // 최솟값을 가지는 모든 c들(argmin)을 구해서 하나의 DistanceTable로 만든다.
                    // 최솟값은 distance로 저장되고, 각 경로들은 List로 묶여서 paths로 저장된다. 
                    foreach (DistanceTable c in costs)
                    {
                        // Overflow means there is an invalid operation.

                        if (c.Distance >= 0 &&
                            c.Distance < INVALID_COST &&
                            !(i == lenThis && j == lenOther))
                        {
                            /*
                            if (c.Distance == distanceTable[i][j].Distance)
                            {
                                distanceTable[i][j].Paths.Add(c.Paths[0]);
                            }
                            else */
                            if (c.Distance < distanceTable[i][j].Distance)
                            {
                                distanceTable[i][j].Paths = new List<List<KeyValuePair<int, OperationInfo.Type>>>();
                                distanceTable[i][j].Paths.Add(c.Paths[0]);
                                distanceTable[i][j].Distance = c.Distance;
                            }
                        }

                        if (i == lenThis && j == lenOther &&
                            c.Distance < distanceTable[i][j].Distance &&
                            c.Distance >= 0 &&
                            c.Distance < INVALID_COST)
                        {
                            distanceTable[i][j].Paths = new List<List<KeyValuePair<int, OperationInfo.Type>>>();
                            distanceTable[i][j].Paths.Add(c.Paths[0]);
                            distanceTable[i][j].Distance = c.Distance;
                        }
                    }
                    if (DISTANCE_DEBUG)
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                        Console.Write(distanceTable[i][j].Distance + "\t");  // TODO
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.

                        #endregion
                }

                if (DISTANCE_DEBUG)
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                    Console.WriteLine();
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
            }

            #region Delay 연산

            mc = this.Copy();

            // 최적 경로에서 DelayAndReplace 연산을 사용하지 않은 경우 Delay 연산 적용 가능
            if (distanceTable[lenThis][lenOther].Paths[0].FindIndex(e => e.Value == OperationInfo.Type.DelayAndReplace) == -1)    
                delayCost = mc.DelayNotes(other.firstRestDuration);

            #endregion

            #region this 멜로디 형태와 other 멜로디 형태의 음 높이 클러스터 개수 차이 계산

            pitchClusterCountCost = Math.Abs(this.metadataList.Count - other.metadataList.Count) * GLOBAL_PITCH_CLUSTER_COUNT_COST;

            #endregion

            if (DISTANCE_DEBUG)
            {
                #region 위에서 계산한 연산들의 비용과 수행 가능 여부가 실제와 같은지 테스트

#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                MelodicContour mcForTest = this.Copy();
                List<OperationInfo> resultOperations = PathToOperations(distanceTable[lenThis][lenOther].Paths[0], mcForTest, other.Copy(), distanceTable);
                List<OperationInfo> inverseOperations = new List<OperationInfo>();
                foreach (OperationInfo o in resultOperations)
                {
                    o.Print();
                    Console.WriteLine(mcForTest.PerformOperation(o));
                    inverseOperations.Insert(0, o.Inverse());
                }
                mcForTest.Print();

                #endregion

                #region 반대 방향으로 역연산들을 취해주면 최적 거리가 같게 나오는지 테스트 -> 같게 나오지 않는 예제가 존재한다!

                Console.WriteLine("Inverse test");
                MelodicContour mcForTest2 = other.Copy();
                foreach (OperationInfo o in inverseOperations)
                {
                    o.Print();
                    Console.WriteLine(mcForTest2.PerformOperation(o));
                }
                mcForTest2.Print();

                #endregion

                Console.WriteLine("Pitch Cluster Count Cost: " + pitchClusterCountCost);
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
            }

            return distanceTable[lenThis][lenOther].Distance + delayCost + pitchClusterCountCost;
        }

        /// <summary>
        /// PathToOperations()에서 연산 당시 음표의 실제 인덱스를 구하기 위해 사용됩니다.
        /// </summary>
        /// <param name="adjuster">인덱스 변형 함수들</param>
        /// <param name="index">연산될 음표의 원래 멜로디 형태에서의 인덱스</param>
        /// <returns></returns>
        private int AdjustIndex(List<Func<int, bool, int>> adjuster, int index, OperationInfo.Type opType)
        {
            int ret = index;
            foreach (Func<int, bool, int> func in adjuster)
            {
                ret = func(ret, opType == OperationInfo.Type.Insert);
            }
            return ret;
        }

        /// <summary>
        /// 멜로디 형태 A를 멜로디 형태 B로 변형하는 거리 계산 과정에서 얻은,
        /// 음표의 공간적 위치 순서로 정렬된 경로(path)를
        /// 연산이 수행되는 시간적 순서로 정렬된 연산 시퀀스로 변환하여 반환합니다.
        /// 맨 앞의 음표부터 차례로, 경로에 있는 연산을 적용한다고 가정합니다.
        /// 연산 비용 계산은 하지 않습니다.
        /// </summary>
        /// <param name="path">경로</param>
        /// <param name="melodicContourA">원래 멜로디 형태 A</param>
        /// <param name="melodicContourB">목표 멜로디 형태 B</param>
        /// <returns></returns>
        private List<OperationInfo> PathToOperations(List<KeyValuePair<int, OperationInfo.Type>> path,
            MelodicContour melodicContourA, MelodicContour melodicContourB)
        {
            List<OperationInfo> operations = new List<OperationInfo>();
            List<KeyValuePair<int, int>> noteIndices = new List<KeyValuePair<int, int>>();
            int i = 0, j = 0;
            LinkedListNode<MelodicContourNote> nodeA, nodeB, nodeA2, nodeB2;
            MelodicContourNote noteA, noteB, noteA2 = new MelodicContourNote(), noteB2 = new MelodicContourNote();
            for (int k = 0; k < path.Count; k++)
            {
                OperationInfo.Type op = path[k].Value;
                nodeA = melodicContourA.GetNoteNodeByIndex(i);
                nodeB = melodicContourB.GetNoteNodeByIndex(j);
                if (nodeA != null)
                {
                    noteA = nodeA.Value.Copy();
                }
                else
                {
                    noteA = new MelodicContourNote();
                }
                if (nodeB != null)
                {
                    noteB = nodeB.Value.Copy();
                }
                else
                {
                    noteB = new MelodicContourNote();
                }
                if (op == OperationInfo.Type.Move)
                {
                    nodeA2 = melodicContourA.GetNoteNodeByIndex(i + 1);
                    nodeB2 = melodicContourB.GetNoteNodeByIndex(j + 1);

                    if (nodeA2 != null)
                    {
                        noteA2 = nodeA2.Value.Copy();
                    }
                    if (nodeB2 != null)
                    {
                        noteB2 = nodeB2.Value.Copy();
                    }
                }

                OperationInfo info = new OperationInfo(op, -1, noteA, noteB);
                switch (op)
                {
                    case OperationInfo.Type.Delete:
                        operations.Insert(path[k].Key, info);
                        noteIndices.Insert(path[k].Key, new KeyValuePair<int, int>(i, j));
                        i++;
                        break;
                    case OperationInfo.Type.Insert:
                        operations.Insert(path[k].Key, info);
                        noteIndices.Insert(path[k].Key, new KeyValuePair<int, int>(i, j));
                        j++;
                        break;
                    case OperationInfo.Type.Replace:
                        operations.Insert(path[k].Key, info);
                        noteIndices.Insert(path[k].Key, new KeyValuePair<int, int>(i, j));
                        i++;
                        j++;
                        break;
                    case OperationInfo.Type.Delay:
                        operations.Insert(path[k].Key, new OperationInfo(
                            melodicContourA.firstRestDuration, melodicContourB.firstRestDuration));
                        noteIndices.Insert(path[k].Key, new KeyValuePair<int, int>(-1, -1));
                        break;
                    case OperationInfo.Type.Move:
                        operations.Insert(path[k].Key, new OperationInfo(-1, noteA, noteA2, noteB, noteB2));
                        noteIndices.Insert(path[k].Key, new KeyValuePair<int, int>(i, j));
                        i += 2;
                        j += 2;
                        break;
                    case OperationInfo.Type.DelayAndReplace:
                        operations.Insert(path[k].Key, new OperationInfo(
                            melodicContourA.firstRestDuration, melodicContourB.firstRestDuration, noteA, noteB));
                        noteIndices.Insert(path[k].Key, new KeyValuePair<int, int>(i, j));
                        i++;
                        j++;
                        break;
                }
            }

            List<Func<int, bool, int>> indexAdjuster = new List<Func<int, bool, int>>();
            
            for (int k = 0; k < operations.Count; k++)
            {
                int index;
                switch (operations[k].type)
                {
                    case OperationInfo.Type.Delete:
                        index = AdjustIndex(indexAdjuster, noteIndices[k].Key, operations[k].type);
                        operations[k].noteIndex = index;

                        // 이 이후로 noteIndices[k].Key보다 뒤에 있었던 음표들은
                        // 인덱스가 1씩 앞으로 당겨져야 한다.
                        indexAdjuster.Add((x, isInsert) =>
                        {
                            if (x > index) return x - 1;
                            else return x;
                        });
                        break;
                    case OperationInfo.Type.Insert:
                        index = AdjustIndex(indexAdjuster, noteIndices[k].Key, operations[k].type);
                        operations[k].noteIndex = index;

                        // 이 이후로 noteIndices[k].Key보다 같거나(나중에 오는 연산이 insert가 아닌 경우)
                        // 뒤에 있었던(나중에 오는 연산 종류와 무관) 음표들은 인덱스가 1씩 뒤로 밀려야 한다.
                        indexAdjuster.Add((x, isInsert) =>
                        {
                            if (isInsert)
                            {
                                if (x > index) return x + 1;
                                else return x;
                            }
                            else
                            {
                                if (x >= index) return x + 1;
                                else return x;
                            }
                        });
                        break;
                    case OperationInfo.Type.Replace:
                        index = AdjustIndex(indexAdjuster, noteIndices[k].Key, operations[k].type);
                        operations[k].noteIndex = index;
                        break;
                    case OperationInfo.Type.Delay:
                        operations[k].noteIndex = -1;
                        break;
                    case OperationInfo.Type.Move:
                        index = AdjustIndex(indexAdjuster, noteIndices[k].Key, operations[k].type);
                        operations[k].noteIndex = index;
                        break;
                    case OperationInfo.Type.DelayAndReplace:
                        index = AdjustIndex(indexAdjuster, noteIndices[k].Key, operations[k].type);
                        operations[k].noteIndex = index;
                        break;
                }
            }
            
            return operations;
        }

        /// <summary>
        /// 멜로디 형태 A를 멜로디 형태 B로 변형하는 거리 계산 과정에서 얻은,
        /// 음표의 공간적 위치 순서로 정렬된 경로(path)를
        /// 연산이 수행되는 시간적 순서로 정렬된 연산 시퀀스로 변환하여 반환합니다.
        /// 맨 앞의 음표부터 차례로, 경로에 있는 연산을 적용한다고 가정합니다.
        /// 연산 비용도 계산하여 기록합니다.
        /// </summary>
        /// <param name="path">경로</param>
        /// <param name="melodicContourA">원래 멜로디 형태 A</param>
        /// <param name="melodicContourB">목표 멜로디 형태 B</param>
        /// <param name="distanceTable">완성된 DistanceTable</param>
        /// <returns></returns>
        private List<OperationInfo> PathToOperations(List<KeyValuePair<int, OperationInfo.Type>> path,
            MelodicContour melodicContourA, MelodicContour melodicContourB, List<List<DistanceTable>> distanceTable)
        {
            List<OperationInfo> operations = new List<OperationInfo>();
            List<KeyValuePair<int, int>> noteIndices = new List<KeyValuePair<int, int>>();
            int i = 0, j = 0;
            LinkedListNode<MelodicContourNote> nodeA, nodeB, nodeA2, nodeB2;
            MelodicContourNote noteA, noteB, noteA2 = new MelodicContourNote(), noteB2 = new MelodicContourNote();
            bool hasDelay = false;
            for (int k = 0; k < path.Count; k++)
            {
                OperationInfo.Type op = path[k].Value;
                nodeA = melodicContourA.GetNoteNodeByIndex(i);
                nodeB = melodicContourB.GetNoteNodeByIndex(j);
                if (nodeA != null)
                {
                    noteA = nodeA.Value.Copy();
                }
                else
                {
                    noteA = new MelodicContourNote();
                }
                if (nodeB != null)
                {
                    noteB = nodeB.Value.Copy();
                }
                else
                {
                    noteB = new MelodicContourNote();
                }
                if (op == OperationInfo.Type.Move)
                {
                    nodeA2 = melodicContourA.GetNoteNodeByIndex(i + 1); // TODO i - 1?
                    nodeB2 = melodicContourB.GetNoteNodeByIndex(j + 1); // TODO i - 1?

                    if (nodeA2 != null)
                    {
                        noteA2 = nodeA2.Value.Copy();
                    }
                    if (nodeB2 != null)
                    {
                        noteB2 = nodeB2.Value.Copy();
                    }
                }

                OperationInfo info = new OperationInfo(op, -1, noteA, noteB);
                switch (op)
                {
                    case OperationInfo.Type.Delete:
                        noteIndices.Insert(path[k].Key, new KeyValuePair<int, int>(i, j));
                        i++;
                        info.cost = distanceTable[i][j].Distance - distanceTable[i - 1][j].Distance;
                        operations.Insert(path[k].Key, info);
                        break;
                    case OperationInfo.Type.Insert:
                        noteIndices.Insert(path[k].Key, new KeyValuePair<int, int>(i, j));
                        j++;
                        info.cost = distanceTable[i][j].Distance - distanceTable[i][j - 1].Distance;
                        operations.Insert(path[k].Key, info);
                        break;
                    case OperationInfo.Type.Replace:
                        noteIndices.Insert(path[k].Key, new KeyValuePair<int, int>(i, j));
                        i++;
                        j++;
                        info.cost = distanceTable[i][j].Distance - distanceTable[i - 1][j - 1].Distance;
                        operations.Insert(path[k].Key, info);
                        break;
                    case OperationInfo.Type.Delay:
                        noteIndices.Insert(path[k].Key, new KeyValuePair<int, int>(-1, -1));
                        info.cost = melodicContourA.Copy().DelayNotes(melodicContourB.firstRestDuration);
                        operations.Insert(path[k].Key, new OperationInfo(
                            melodicContourA.firstRestDuration, melodicContourB.firstRestDuration));
                        hasDelay = true;
                        break;
                    case OperationInfo.Type.Move:
                        noteIndices.Insert(path[k].Key, new KeyValuePair<int, int>(i, j));
                        i += 2;
                        j += 2;
                        operations.Insert(path[k].Key, new OperationInfo(-1, noteA, noteA2, noteB, noteB2,
                            distanceTable[i][j].Distance - distanceTable[i - 2][j - 2].Distance));
                        break;
                    case OperationInfo.Type.DelayAndReplace:
                        noteIndices.Insert(path[k].Key, new KeyValuePair<int, int>(i, j));
                        i++;
                        j++;
                        operations.Insert(path[k].Key, new OperationInfo(
                            melodicContourA.firstRestDuration, melodicContourB.firstRestDuration, noteA, noteB,
                            distanceTable[i][j].Distance - distanceTable[i - 1][j - 1].Distance));
                        hasDelay = true;
                        break;
                }
            }

            List<Func<int, bool, int>> indexAdjuster = new List<Func<int, bool, int>>();

            for (int k = 0; k < operations.Count; k++)
            {
                int index;
                switch (operations[k].type)
                {
                    case OperationInfo.Type.Delete:
                        index = AdjustIndex(indexAdjuster, noteIndices[k].Key, operations[k].type);
                        operations[k].noteIndex = index;

                        // 이 이후로 noteIndices[k].Key보다 뒤에 있었던 음표들은
                        // 인덱스가 1씩 앞으로 당겨져야 한다.
                        indexAdjuster.Add((x, isInsert) =>
                        {
                            if (x > index) return x - 1;
                            else return x;
                        });
                        break;
                    case OperationInfo.Type.Insert:
                        index = AdjustIndex(indexAdjuster, noteIndices[k].Key, operations[k].type);
                        operations[k].noteIndex = index;

                        // 이 이후로 noteIndices[k].Key보다 같거나(나중에 오는 연산이 insert가 아닌 경우)
                        // 뒤에 있었던(나중에 오는 연산 종류와 무관) 음표들은 인덱스가 1씩 뒤로 밀려야 한다.
                        indexAdjuster.Add((x, isInsert) =>
                        {
                            if (isInsert)
                            {
                                if (x > index) return x + 1;
                                else return x;
                            }
                            else
                            {
                                if (x >= index) return x + 1;
                                else return x;
                            }
                        });
                        break;
                    case OperationInfo.Type.Replace:
                        index = AdjustIndex(indexAdjuster, noteIndices[k].Key, operations[k].type);
                        operations[k].noteIndex = index;
                        break;
                    case OperationInfo.Type.Delay:
                        operations[k].noteIndex = -1;
                        break;
                    case OperationInfo.Type.Move:
                        index = AdjustIndex(indexAdjuster, noteIndices[k].Key, operations[k].type);
                        operations[k].noteIndex = index;
                        break;
                    case OperationInfo.Type.DelayAndReplace:
                        index = AdjustIndex(indexAdjuster, noteIndices[k].Key, operations[k].type);
                        operations[k].noteIndex = index;
                        break;
                }
            }

            if (!hasDelay) operations.Add(new OperationInfo(
                            melodicContourA.firstRestDuration, melodicContourB.firstRestDuration, melodicContourA.Copy().DelayNotes(melodicContourB.firstRestDuration)));
            return operations;
        }

        /// <summary>
        /// 이 멜로디 형태를 새로 복제하여 반환합니다.
        /// </summary>
        /// <returns></returns>
        public MelodicContour Copy()
        {
            return Copy(this, noteList.Count - 1);
        }

        /// <summary>
        /// original 멜로디 형태에서, 앞에서부터 index번째 음표까지 포함하는
        /// 길이 index + 1의 부분 멜로디 형태를 새로 복제하여 반환합니다.
        /// </summary>
        private static MelodicContour Copy(MelodicContour original, int index)
        {
            MelodicContour mc = new MelodicContour(original.firstRestDuration);
            int i = 0;
            LinkedListNode<MelodicContourNote> node = original.noteList.First;
            while (i <= index && node != null)
            {
                mc.InsertNote(i, node.Value.Copy());
                node = node.Next;
                i++;
            }
            return mc;
        }

        /// <summary>
        /// 음표 목록에서 인덱스로 음표 노드를 찾습니다.
        /// 찾는 음표 노드가 없으면 null을 반환합니다.
        /// </summary>
        /// <param name="index">인덱스</param>
        /// <returns></returns>
        private LinkedListNode<MelodicContourNote> GetNoteNodeByIndex(int index)
        {
            if (index < 0) return null;
            int i = 0;
            LinkedListNode<MelodicContourNote> node = noteList.First;
            while (i < index && node != null)
            {
                node = node.Next;
                i++;
            }
            if (node == null) return null;
            return node;
        }

        /// <summary>
        /// 음표 노드의 인덱스를 찾습니다.
        /// 잘못된 음표 노드이면 -1을 반환합니다.
        /// 음표 개수는 최대 4096개라고 가정합니다.
        /// </summary>
        /// <param name="noteNode">인덱스</param>
        /// <returns></returns>
        private static int GetIndexOfNoteNode(LinkedListNode<MelodicContourNote> noteNode)
        {
            int i = -1;
            LinkedListNode<MelodicContourNote> node = noteNode;
            while (node != null && i < 4096)
            {
                node = node.Previous;
                i++;
            }
            if (i >= 4096) return -1;
            return i;
        }

        /// <summary>
        /// 멜로디 형태 거리 계산 시 이전의 최적 연산 비용
        /// 계산 정보를 저장하는 구조체입니다.
        /// 원래 멜로디 형태의 맨 앞에서부터 i개의 음표를
        /// 목표 멜로디 형태의 맨 앞에서부터 j개의 음표로 바꾸는
        /// 비용과 관련된 정보 d_i,j를 표현합니다.
        /// </summary>
        private class DistanceTable
        {
            // 최적 경로(int 목록)의 첫 번째 값은 항상 양수 (첫 번째 연산이라 순서를 정의할 수 없음)
            // 정방향: 이전 연산들을 먼저 모두 수행한 후에 마지막 음표의 연산을 수행하는 순서
            // 역방향: 마지막 음표의 연산을 가장 먼저 수행한 후에 이전 연산들을 모두 수행하는 순서

            /// <summary>
            /// 최종 거리.
            /// 목표 멜로디 형태에 도달하기까지 필요한 연산 비용 합의 최적.
            /// 모든 연산들(연산을 적용하는 순간에 직후 음표가 존재하지 않는 음표에 대한
            /// 연산 포함)의 비용의 합입니다.
            /// </summary>
            public int Distance { get; set; }

            /// <summary>
            /// 최종 경로.
            /// 목표 멜로디 형태에 도달하기까지 필요한 최적 경로(연산 종류와 순서).
            /// 목록 안의 값은 정방향 Delete이면 1, 정방향 Insert이면 2, 정방향 Move이면 3,
            /// 역방향 Delete이면 -1, 역방향 Insert이면 -2, 역방향 Move이면 -3을 가집니다.
            /// </summary>
            public List<List<KeyValuePair<int, OperationInfo.Type>>> Paths { get; set; }

            public DistanceTable(int distance, params List<KeyValuePair<int, OperationInfo.Type>>[] path)
            {
                this.Distance = distance;
                this.Paths = path.ToList();
            }
        }

        /// <summary>
        /// 멜로디 형태 편집 연산의 종류와
        /// 연산을 적용한 음표에 대한 정보를 담는 구조체입니다.
        /// </summary>
        public class OperationInfo
        {
            public enum Type { Invalid = 0, Delete = 1, Insert = 2, Replace = 3, Delay = 4, Move = 5, DelayAndReplace = 6 }

            /// <summary>
            /// 편집 연산 종류
            /// </summary>
            public Type type;

            /// <summary>
            /// 연산을 적용한 음표의 인덱스.
            /// Move 연산에서는 앞 음표의 인덱스가 됩니다.
            /// DelayAndReplace 연산에서는 0이 됩니다.
            /// </summary>
            public int noteIndex;

            /// <summary>
            /// 연산을 적용한 음표의 연산 직전 상태.
            /// Move 연산에서는 앞 음표의 연산 직전 상태가 됩니다.
            /// DelayAndReplace 연산에서는 처음 음표의 연산 직전 상태가 됩니다.
            /// Insert 연산에서는 없는 음표를 나타내는 new MelodicContourNote()가 됩니다.
            /// </summary>
            public MelodicContourNote noteBeforeOp;

            /// <summary>
            /// Move 연산을 적용한 뒤 음표의 연산 직전 상태.
            /// 다른 연산에서는 없는 음표를 나타내는 new MelodicContourNote()가 됩니다.
            /// </summary>
            public MelodicContourNote note2BeforeOp;

            /// <summary>
            /// 연산을 적용한 음표의 연산 직후 상태.
            /// Move 연산에서는 앞 음표의 연산 직후 상태가 됩니다.
            /// DelayAndReplace 연산에서는 처음 음표의 연산 직후 상태가 됩니다.
            /// Delete 연산에서는 없는 음표를 나타내는 new MelodicContourNote()가 됩니다.
            /// </summary>
            public MelodicContourNote noteAfterOp;

            /// <summary>
            /// Move 연산을 적용한 뒤 음표의 연산 직후 상태.
            /// 다른 연산에서는 없는 음표를 나타내는 new MelodicContourNote()가 됩니다.
            /// </summary>
            public MelodicContourNote note2AfterOp;

            /// <summary>
            /// 편집 연산을 수행한 비용.
            /// 이 구조체의 다른 필드의 값이 모두 같아도 
            /// 어떤 멜로디 형태에서 연산을 적용했는지에 따라 비용이 다를 수 있습니다.
            /// </summary>
            public int cost;

            /// <summary>
            /// 연산 직전 멜로디 형태의 맨 앞 쉼표 길이.
            /// Delay와 DelayAndReplace 연산에서만 음수가 아닌 값을 가집니다.
            /// </summary>
            public int firstRestDurationBeforeOp;

            /// <summary>
            /// 연산 직후 멜로디 형태의 맨 앞 쉼표 길이.
            /// Delay와 DelayAndReplace 연산에서만 음수가 아닌 값을 가집니다.
            /// </summary>
            public int firstRestDurationAfterOp;

            /// <summary>
            /// 연산 종류가 Delete, Insert, Replace일 때 사용하십시오.
            /// </summary>
            /// <param name="operationType"></param>
            /// <param name="noteIndex"></param>
            /// <param name="noteBeforeOp"></param>
            /// <param name="noteAfterOp"></param>
            /// <param name="cost"></param>
            public OperationInfo(Type operationType, int noteIndex,
                MelodicContourNote noteBeforeOp, MelodicContourNote noteAfterOp,
                int cost = INVALID_COST)
            {
                switch (operationType)
                {
                    case Type.Delete:
                        this.type = Type.Delete;
                        this.noteIndex = noteIndex;
                        this.noteBeforeOp = noteBeforeOp.Copy();
                        this.noteAfterOp = new MelodicContourNote();
                        this.note2BeforeOp = new MelodicContourNote();
                        this.note2AfterOp = new MelodicContourNote();
                        this.cost = cost;
                        this.firstRestDurationBeforeOp = -1;
                        this.firstRestDurationAfterOp = -1;
                        break;
                    case Type.Insert:
                        this.type = Type.Insert;
                        this.noteIndex = noteIndex;
                        this.noteBeforeOp = new MelodicContourNote();
                        this.noteAfterOp = noteAfterOp.Copy();
                        this.note2BeforeOp = new MelodicContourNote();
                        this.note2AfterOp = new MelodicContourNote();
                        this.cost = cost;
                        this.firstRestDurationBeforeOp = -1;
                        this.firstRestDurationAfterOp = -1;
                        break;
                    case Type.Replace:
                        this.type = Type.Replace;
                        this.noteIndex = noteIndex;
                        this.noteBeforeOp = noteBeforeOp.Copy();
                        this.noteAfterOp = noteAfterOp.Copy();
                        this.note2BeforeOp = new MelodicContourNote();
                        this.note2AfterOp = new MelodicContourNote();
                        this.cost = cost;
                        this.firstRestDurationBeforeOp = -1;
                        this.firstRestDurationAfterOp = -1;
                        break;
                    case Type.Move:
                    case Type.Delay:
                    case Type.DelayAndReplace:
                    default:
                        this.type = Type.Invalid;
                        this.noteIndex = -1;
                        this.noteBeforeOp = new MelodicContourNote();
                        this.noteAfterOp = new MelodicContourNote();
                        this.note2BeforeOp = new MelodicContourNote();
                        this.note2AfterOp = new MelodicContourNote();
                        this.cost = INVALID_COST;
                        this.firstRestDurationBeforeOp = -1;
                        this.firstRestDurationAfterOp = -1;
                        break;
                }
            }

            /// <summary>
            /// 연산 종류가 Delay일 때 사용하십시오.
            /// </summary>
            /// <param name="firstRestDurationBeforeOp"></param>
            /// <param name="firstRestDurationAfterOp"></param>
            /// <param name="cost"></param>
            public OperationInfo(int firstRestDurationBeforeOp,
                int firstRestDurationAfterOp,
                int cost = INVALID_COST)
            {
                this.type = Type.Delay;
                this.noteIndex = -1;
                this.noteBeforeOp = new MelodicContourNote();
                this.noteAfterOp = new MelodicContourNote();
                this.note2BeforeOp = new MelodicContourNote();
                this.note2AfterOp = new MelodicContourNote();
                this.cost = cost;
                this.firstRestDurationBeforeOp = firstRestDurationBeforeOp;
                this.firstRestDurationAfterOp = firstRestDurationAfterOp;
            }

            /// <summary>
            /// 연산 종류가 Move일 때 사용하십시오.
            /// </summary>
            /// <param name="note1Index"></param>
            /// <param name="note1BeforeOp"></param>
            /// <param name="note2BeforeOp"></param>
            /// <param name="note1AfterOp"></param>
            /// <param name="note2AfterOp"></param>
            /// <param name="cost"></param>
            public OperationInfo(int note1Index,
                MelodicContourNote note1BeforeOp, MelodicContourNote note2BeforeOp,
                MelodicContourNote note1AfterOp, MelodicContourNote note2AfterOp,
                int cost = INVALID_COST)
            {
                this.type = Type.Move;
                this.noteIndex = note1Index;
                this.noteBeforeOp = note1BeforeOp.Copy();
                this.note2BeforeOp = note2BeforeOp.Copy();
                this.noteAfterOp = note1AfterOp.Copy();
                this.note2AfterOp = note2AfterOp.Copy();
                this.cost = cost;
                this.firstRestDurationBeforeOp = -1;
                this.firstRestDurationAfterOp = -1;
            }

            /// <summary>
            /// 연산 종류가 DelayAndReplace일 때 사용하십시오.
            /// </summary>
            public OperationInfo(int firstRestDurationBeforeOp, int firstRestDurationAfterOp,
                MelodicContourNote noteBeforeOp, MelodicContourNote noteAfterOp,
                int cost = INVALID_COST)
            {
                this.type = Type.DelayAndReplace;
                this.noteIndex = 0;
                this.firstRestDurationBeforeOp = firstRestDurationBeforeOp;
                this.firstRestDurationAfterOp = firstRestDurationAfterOp;
                this.noteBeforeOp = noteBeforeOp.Copy();
                this.noteAfterOp = noteAfterOp.Copy();
                this.note2BeforeOp = new MelodicContourNote();
                this.note2AfterOp = new MelodicContourNote();
                this.cost = cost;
            }

            /// <summary>
            /// 이 연산의 역연산을 반환합니다.
            /// </summary>
            /// <returns></returns>
            public OperationInfo Inverse()
            {
                switch (this.type)
                {
                    case Type.Delete:
                        return new OperationInfo(Type.Insert, this.noteIndex, new MelodicContourNote(), this.noteBeforeOp, this.cost);
                    case Type.Insert:
                        return new OperationInfo(Type.Delete, this.noteIndex, this.noteAfterOp, new MelodicContourNote(), this.cost);
                    case Type.Replace:
                        return new OperationInfo(Type.Replace, this.noteIndex, this.noteAfterOp, this.noteBeforeOp, this.cost);
                    case Type.Delay:
                        return new OperationInfo(this.firstRestDurationAfterOp, this.firstRestDurationBeforeOp, this.cost);
                    case Type.Move:
                        return new OperationInfo(this.noteIndex, this.noteAfterOp, this.note2AfterOp, this.noteBeforeOp, this.note2BeforeOp, this.cost);
                    case Type.DelayAndReplace:
                        return new OperationInfo(this.firstRestDurationAfterOp, this.firstRestDurationBeforeOp, this.noteAfterOp, this.noteBeforeOp, this.cost);
                    default:
                        return new OperationInfo(Type.Invalid, -1, new MelodicContourNote(), new MelodicContourNote());
                }
            }

            /// <summary>
            /// 이 연산을 콘솔에 보기 좋게 출력합니다.
            /// </summary>
            public void Print(bool printCost = true)
            {
                string s = type.ToString();
                if (type == Type.Invalid)
                {
                    s += "()";
                    if (printCost) s += ": " + INVALID_COST;
                    Console.WriteLine(s);
                    return;
                }
                if (type == Type.Delay)
                {
                    s += "(" + firstRestDurationBeforeOp + " -> " + firstRestDurationAfterOp + ")";
                    if (printCost) s += ": " + cost;
                    Console.WriteLine(s);
                    return;
                }
                if (type == Type.Move)
                {
                    s += "(" + noteIndex + ", [" + noteBeforeOp.Duration + ", " + noteBeforeOp.PitchCluster + "] -> [" +
                        noteAfterOp.Duration + ", " + noteAfterOp.PitchCluster + "], " +
                        (noteIndex + 1) + ", [" + note2BeforeOp.Duration + ", " + note2BeforeOp.PitchCluster + "] -> [" +
                        note2AfterOp.Duration + ", " + note2AfterOp.PitchCluster + "])";
                    if (printCost) s += ": " + cost;
                    Console.WriteLine(s);
                    return;
                }
                if (type == Type.DelayAndReplace)
                {
                    s += "(" + firstRestDurationBeforeOp + " -> " + firstRestDurationAfterOp + ", " +
                        noteIndex + ", [" + noteBeforeOp.Duration + ", " + noteBeforeOp.PitchCluster + "] -> [" +
                        noteAfterOp.Duration + ", " + noteAfterOp.PitchCluster + "])";
                    if (printCost) s += ": " + cost;
                    Console.WriteLine(s);
                    return;
                }
                s += "(" + noteIndex + ", [";
                if (noteBeforeOp.Duration != -1)
                {
                    s += noteBeforeOp.Duration + ", " + noteBeforeOp.PitchCluster;
                    if (noteAfterOp.Duration != -1)
                    {
                        s += "] -> [";
                        s += noteAfterOp.Duration + ", " + noteAfterOp.PitchCluster;
                        s += "])";
                        if (printCost) s += ": " + cost;
                        Console.WriteLine(s);
                        return;
                    }
                    else
                    {
                        s += "])";
                        if (printCost) s += ": " + cost;
                        Console.WriteLine(s);
                        return;
                    }
                }
                else
                {
                    s += noteAfterOp.Duration + ", " + noteAfterOp.PitchCluster;
                    s += "])";
                    if (printCost) s += ": " + cost;
                    Console.WriteLine(s);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// 음표.
    /// 멜로디 형태의 음표 목록에 들어갈 구조체입니다.
    /// 음표의 길이 정보와 음 높이 변화 정보를 포함합니다.
    /// </summary>
    public class MelodicContourNote : IComparable<MelodicContourNote>
    {
        /// <summary>
        /// 음표의 길이.
        /// 한 마디를 64분음표 64개로 쪼갰을 때
        /// 음표가 얼만큼의 길이로 지속되는지 나타냅니다.
        /// 존재하는 음표의 길이는 1 이상의 값을 갖습니다.
        /// 존재하지 않는 음표의 길이는 -1입니다.
        /// </summary>
        public int Duration
        {
            get;
            private set;
        }

        /// <summary>
        /// 클러스터 번호.
        /// 같은 음 높이를 갖는 음표끼리는 같은 클러스터 번호를 가지며
        /// 클러스터 번호가 높으면 항상 절대적인 음 높이가 더 높습니다.
        /// 클러스터 번호의 절대적인 값은 의미를 갖지 않습니다.
        /// </summary>
        public float PitchCluster
        {
            get;
            private set;
        }

        /// <summary>
        /// 음 높이 변화.
        /// MelodicContour.PitchVariance()의 값을 임시로 저장하기
        /// 위한 변수이므로 이 값에 직접 접근하면 안 됩니다.
        /// 대신 MelodicContour.PitchVariance()을 호출하십시오.
        /// </summary>
        public int pitchVariance;

        /// <summary>
        /// 멜로디 형태에 들어갈 음표를 생성합니다.
        /// 길이를 0 이하로 주면 존재하지 않는 음표를 표현하는 더미 인스턴스를 생성합니다.
        /// </summary>
        /// <param name="duration">음표의 길이.
        /// 한 마디를 64분음표 64개로 쪼갰을 때
        /// 음표가 얼만큼의 길이로 지속되는지 나타냅니다.
        /// 1 이상의 값을 갖습니다.</param>
        /// <param name="pitchCluster">클러스터 번호.
        /// 같은 음 높이를 갖는 음표끼리는 같은 클러스터 번호를 가지며
        /// 클러스터 번호가 높으면 항상 절대적인 음 높이가 더 높습니다.
        /// 클러스터 번호의 절대적인 값은 의미를 갖지 않습니다.
        /// 클러스터 번호를 정할 때
        /// MelodicContour.GetNewClusterNumber()를 활용하면 도움이 됩니다.</param>
        public MelodicContourNote(int duration, float pitchCluster)
        {
            if (duration >= 1)
            {
                Duration = duration;
                PitchCluster = pitchCluster;
                pitchVariance = -2;
            }
            else
            {
                Duration = -1;
                PitchCluster = float.NaN;
                pitchVariance = 0;
            }
        }

        /// <summary>
        /// 멜로디 형태에 존재하지 않는 음표를 표현하는 더미 인스턴스를 생성합니다.
        /// 존재하지 않는 음표의 길이는 -1로, 클러스터 번호는 float.NaN으로,
        /// 음 높이 변화는 0으로 표현됩니다.
        /// </summary>
        public MelodicContourNote()
        {
            Duration = -1;
            PitchCluster = float.NaN;
            pitchVariance = 0;
        }

        /// <summary>
        /// 멜로디 형태 음표를 새로 복제하여 반환합니다.
        /// </summary>
        /// <returns></returns>
        public MelodicContourNote Copy()
        {
            return new MelodicContourNote(Duration, PitchCluster);
        }

        /// <summary>
        /// Duration을 기준으로 다른 멜로디 형태 음표와 비교합니다.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(MelodicContourNote other)
        {
            return this.Duration.CompareTo(other.Duration);
        }

        /// <summary>
        /// 두 멜로디 형태 음표가 같은지 비교합니다.
        /// Duration과 PitchCluster의 절대적인 값이 같아야 같은 음표입니다.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(MelodicContourNote))
            {
                if (this.Duration == ((MelodicContourNote)obj).Duration &&
                    this.PitchCluster == ((MelodicContourNote)obj).PitchCluster)
                    return true;
                else return false;
            }
            else return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return 31 * Duration.GetHashCode() + PitchCluster.GetHashCode();
        }

        public override string ToString()
        {
            return "[" + Duration + ", " + PitchCluster + "]";
        }
    }

    /// <summary>
    /// 메타데이터.
    /// 멜로디 형태의 클러스터 정보와
    /// 이 클러스터에 할당된 절대적인 음 높이 정보를 포함합니다.
    /// 멜로디 형태를 악보로 구체화(implement)할 때 사용됩니다.
    /// </summary>
    public class MelodicContourMetadata : IComparable<MelodicContourMetadata>
    {
        /// <summary>
        /// 클러스터 번호
        /// </summary>
        public float pitchCluster;

        /// <summary>
        /// 이 클러스터 번호에 속해 있는 음표 노드들의 목록
        /// </summary>
        public List<LinkedListNode<MelodicContourNote>> noteNodes = new List<LinkedListNode<MelodicContourNote>>();

        /// <summary>
        /// 이 클러스터에 할당된 절대적인 음 높이.
        /// MelodicContour.SetAbsolutePitch()로 값을 지정할 수 있습니다.
        /// 지정되지 않은 경우 -1을 가지고 있습니다.
        /// </summary>
        public int absolutePitch;

        public MelodicContourMetadata(float pitchCluster, LinkedListNode<MelodicContourNote> noteNodes)
        {
            this.pitchCluster = pitchCluster;
            this.noteNodes = new List<LinkedListNode<MelodicContourNote>>()
            {
                noteNodes
            };
            absolutePitch = -1;
        }

        public int CompareTo(MelodicContourMetadata other)
        {
            return this.pitchCluster.CompareTo(other.pitchCluster);
        }
    }
}
