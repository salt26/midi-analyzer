namespace MidiAnalyzer
{
    partial class Form1
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.loadMidiButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label12 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.analysisStartButton = new System.Windows.Forms.Button();
            this.defaultSettingButton = new System.Windows.Forms.Button();
            this.fileTextBox = new System.Windows.Forms.TextBox();
            this.numericMinimumPoints = new System.Windows.Forms.NumericUpDown();
            this.numericEpsilon = new System.Windows.Forms.NumericUpDown();
            this.numericPitchCount = new System.Windows.Forms.NumericUpDown();
            this.numericPitchRank = new System.Windows.Forms.NumericUpDown();
            this.numericPitchVariance = new System.Windows.Forms.NumericUpDown();
            this.numericOnset = new System.Windows.Forms.NumericUpDown();
            this.numericDuration = new System.Windows.Forms.NumericUpDown();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.measureTabPage = new System.Windows.Forms.TabPage();
            this.clusterTabPage = new System.Windows.Forms.TabPage();
            this.fileNameTextBox = new System.Windows.Forms.TextBox();
            this.parameterTextBox = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label13 = new System.Windows.Forms.Label();
            this.structureTabPage = new System.Windows.Forms.TabPage();
            this.measureComboBox = new System.Windows.Forms.ComboBox();
            this.measureMainLabel = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.measureOriginalScoreButton = new System.Windows.Forms.Button();
            this.measureMonophonicScoreButton = new System.Windows.Forms.Button();
            this.measureChordButton = new System.Windows.Forms.Button();
            this.measureKeyTextBox = new System.Windows.Forms.TextBox();
            this.measureTimeTextBox = new System.Windows.Forms.TextBox();
            this.measureChordTextBox = new System.Windows.Forms.TextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.measureMelodicContourTextBox = new System.Windows.Forms.TextBox();
            this.measureClusterLabel = new System.Windows.Forms.Label();
            this.measureClusterTextBox = new System.Windows.Forms.TextBox();
            this.measureClusterButton = new System.Windows.Forms.Button();
            this.measurePanel = new System.Windows.Forms.Panel();
            this.clusterComboBox = new System.Windows.Forms.ComboBox();
            this.clusterPanel = new System.Windows.Forms.Panel();
            this.clusterMainLabel = new System.Windows.Forms.Label();
            this.clusterTextBox = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericMinimumPoints)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericEpsilon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPitchCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPitchRank)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPitchVariance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericOnset)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericDuration)).BeginInit();
            this.panel2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.measureTabPage.SuspendLayout();
            this.clusterTabPage.SuspendLayout();
            this.measurePanel.SuspendLayout();
            this.clusterPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "mid";
            this.openFileDialog1.Filter = "MIDI 파일 (*.mid;*.midi)|*.mid;*midi";
            this.openFileDialog1.RestoreDirectory = true;
            this.openFileDialog1.Title = "MIDI 파일 선택";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("맑은 고딕", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.Location = new System.Drawing.Point(53, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(145, 38);
            this.label1.TabIndex = 0;
            this.label1.Text = "MIDI 파일";
            // 
            // loadMidiButton
            // 
            this.loadMidiButton.Font = new System.Drawing.Font("맑은 고딕", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.loadMidiButton.Location = new System.Drawing.Point(633, 43);
            this.loadMidiButton.Name = "loadMidiButton";
            this.loadMidiButton.Size = new System.Drawing.Size(216, 48);
            this.loadMidiButton.TabIndex = 1;
            this.loadMidiButton.Text = "MIDI 불러오기!";
            this.loadMidiButton.UseVisualStyleBackColor = true;
            this.loadMidiButton.Click += new System.EventHandler(this.loadMidiButton_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label12);
            this.panel1.Controls.Add(this.linkLabel1);
            this.panel1.Controls.Add(this.analysisStartButton);
            this.panel1.Controls.Add(this.defaultSettingButton);
            this.panel1.Controls.Add(this.fileTextBox);
            this.panel1.Controls.Add(this.numericMinimumPoints);
            this.panel1.Controls.Add(this.numericEpsilon);
            this.panel1.Controls.Add(this.numericPitchCount);
            this.panel1.Controls.Add(this.numericPitchRank);
            this.panel1.Controls.Add(this.numericPitchVariance);
            this.panel1.Controls.Add(this.numericOnset);
            this.panel1.Controls.Add(this.numericDuration);
            this.panel1.Controls.Add(this.label11);
            this.panel1.Controls.Add(this.label10);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.loadMidiButton);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(916, 641);
            this.panel1.TabIndex = 0;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label12.Font = new System.Drawing.Font("맑은 고딕", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label12.Location = new System.Drawing.Point(53, 539);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(241, 40);
            this.label12.TabIndex = 23;
            this.label12.Text = "MIDI 분석기 v.1.0";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Font = new System.Drawing.Font("맑은 고딕", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.linkLabel1.Location = new System.Drawing.Point(349, 544);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(91, 32);
            this.linkLabel1.TabIndex = 22;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "GitHub";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // analysisStartButton
            // 
            this.analysisStartButton.Enabled = false;
            this.analysisStartButton.Font = new System.Drawing.Font("맑은 고딕", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.analysisStartButton.ForeColor = System.Drawing.Color.OrangeRed;
            this.analysisStartButton.Location = new System.Drawing.Point(633, 518);
            this.analysisStartButton.Name = "analysisStartButton";
            this.analysisStartButton.Size = new System.Drawing.Size(215, 83);
            this.analysisStartButton.TabIndex = 21;
            this.analysisStartButton.Text = "분석 시작!";
            this.analysisStartButton.UseVisualStyleBackColor = true;
            this.analysisStartButton.Click += new System.EventHandler(this.analysisStartButton_Click);
            // 
            // defaultSettingButton
            // 
            this.defaultSettingButton.Font = new System.Drawing.Font("맑은 고딕", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.defaultSettingButton.Location = new System.Drawing.Point(719, 113);
            this.defaultSettingButton.Name = "defaultSettingButton";
            this.defaultSettingButton.Size = new System.Drawing.Size(129, 379);
            this.defaultSettingButton.TabIndex = 20;
            this.defaultSettingButton.Text = "기본값 설정";
            this.defaultSettingButton.UseVisualStyleBackColor = true;
            this.defaultSettingButton.Click += new System.EventHandler(this.defaultSettingButton_Click);
            // 
            // fileTextBox
            // 
            this.fileTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.fileTextBox.Enabled = false;
            this.fileTextBox.Font = new System.Drawing.Font("맑은 고딕", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.fileTextBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.fileTextBox.Location = new System.Drawing.Point(232, 48);
            this.fileTextBox.Name = "fileTextBox";
            this.fileTextBox.ReadOnly = true;
            this.fileTextBox.Size = new System.Drawing.Size(367, 38);
            this.fileTextBox.TabIndex = 19;
            this.fileTextBox.WordWrap = false;
            // 
            // numericMinimumPoints
            // 
            this.numericMinimumPoints.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.numericMinimumPoints.Location = new System.Drawing.Point(633, 458);
            this.numericMinimumPoints.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.numericMinimumPoints.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericMinimumPoints.Name = "numericMinimumPoints";
            this.numericMinimumPoints.Size = new System.Drawing.Size(60, 34);
            this.numericMinimumPoints.TabIndex = 18;
            this.numericMinimumPoints.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericMinimumPoints.ValueChanged += new System.EventHandler(this.numericMinimumPoints_ValueChanged);
            // 
            // numericEpsilon
            // 
            this.numericEpsilon.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.numericEpsilon.Location = new System.Drawing.Point(633, 408);
            this.numericEpsilon.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.numericEpsilon.Name = "numericEpsilon";
            this.numericEpsilon.Size = new System.Drawing.Size(60, 34);
            this.numericEpsilon.TabIndex = 17;
            this.numericEpsilon.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericEpsilon.ValueChanged += new System.EventHandler(this.numericEpsilon_ValueChanged);
            // 
            // numericPitchCount
            // 
            this.numericPitchCount.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.numericPitchCount.Location = new System.Drawing.Point(633, 338);
            this.numericPitchCount.Name = "numericPitchCount";
            this.numericPitchCount.Size = new System.Drawing.Size(60, 34);
            this.numericPitchCount.TabIndex = 16;
            this.numericPitchCount.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numericPitchCount.ValueChanged += new System.EventHandler(this.numericPitchCount_ValueChanged);
            // 
            // numericPitchRank
            // 
            this.numericPitchRank.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.numericPitchRank.Location = new System.Drawing.Point(633, 288);
            this.numericPitchRank.Name = "numericPitchRank";
            this.numericPitchRank.Size = new System.Drawing.Size(60, 34);
            this.numericPitchRank.TabIndex = 15;
            this.numericPitchRank.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericPitchRank.ValueChanged += new System.EventHandler(this.numericPitchRank_ValueChanged);
            // 
            // numericPitchVariance
            // 
            this.numericPitchVariance.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.numericPitchVariance.Location = new System.Drawing.Point(633, 238);
            this.numericPitchVariance.Name = "numericPitchVariance";
            this.numericPitchVariance.Size = new System.Drawing.Size(60, 34);
            this.numericPitchVariance.TabIndex = 14;
            this.numericPitchVariance.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericPitchVariance.ValueChanged += new System.EventHandler(this.numericPitchVariance_ValueChanged);
            // 
            // numericOnset
            // 
            this.numericOnset.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.numericOnset.Location = new System.Drawing.Point(633, 168);
            this.numericOnset.Name = "numericOnset";
            this.numericOnset.Size = new System.Drawing.Size(60, 34);
            this.numericOnset.TabIndex = 13;
            this.numericOnset.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numericOnset.ValueChanged += new System.EventHandler(this.numericOnset_ValueChanged);
            // 
            // numericDuration
            // 
            this.numericDuration.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.numericDuration.Location = new System.Drawing.Point(633, 118);
            this.numericDuration.Name = "numericDuration";
            this.numericDuration.Size = new System.Drawing.Size(60, 34);
            this.numericDuration.TabIndex = 12;
            this.numericDuration.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numericDuration.ValueChanged += new System.EventHandler(this.numericDuration_ValueChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("맑은 고딕", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label11.Location = new System.Drawing.Point(226, 458);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(311, 32);
            this.label11.TabIndex = 11;
            this.label11.Text = "한 클러스터의 최소 마디 수";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("맑은 고딕", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label10.Location = new System.Drawing.Point(226, 408);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(303, 32);
            this.label10.TabIndex = 10;
            this.label10.Text = "이웃으로 정의할 최대 비용";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("맑은 고딕", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label9.Location = new System.Drawing.Point(226, 338);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(375, 32);
            this.label9.TabIndex = 9;
            this.label9.Text = "서로 다른 음 높이 개수 변경 비용";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("맑은 고딕", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label8.Location = new System.Drawing.Point(226, 288);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(319, 32);
            this.label8.TabIndex = 8;
            this.label8.Text = "음표 음 높이 순위 변경 비용";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("맑은 고딕", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label7.Location = new System.Drawing.Point(226, 238);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(319, 32);
            this.label7.TabIndex = 7;
            this.label7.Text = "음표 음 높이 변화 변경 비용";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("맑은 고딕", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label6.Location = new System.Drawing.Point(226, 168);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(287, 32);
            this.label6.TabIndex = 6;
            this.label6.Text = "음표 시작 위치 변경 비용";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("맑은 고딕", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label5.Location = new System.Drawing.Point(226, 118);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(231, 32);
            this.label5.TabIndex = 5;
            this.label5.Text = "음표 길이 변경 비용";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("맑은 고딕", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label4.Location = new System.Drawing.Point(53, 403);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(157, 38);
            this.label4.TabIndex = 4;
            this.label4.Text = "클러스터링";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("맑은 고딕", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label3.Location = new System.Drawing.Point(53, 233);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(111, 38);
            this.label3.TabIndex = 3;
            this.label3.Text = "음 높이";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("맑은 고딕", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label2.Location = new System.Drawing.Point(53, 113);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 38);
            this.label2.TabIndex = 2;
            this.label2.Text = "리듬";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label13);
            this.panel2.Controls.Add(this.tableLayoutPanel1);
            this.panel2.Controls.Add(this.parameterTextBox);
            this.panel2.Controls.Add(this.fileNameTextBox);
            this.panel2.Controls.Add(this.tabControl1);
            this.panel2.Enabled = false;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(913, 641);
            this.panel2.TabIndex = 24;
            this.panel2.Visible = false;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.measureTabPage);
            this.tabControl1.Controls.Add(this.clusterTabPage);
            this.tabControl1.Controls.Add(this.structureTabPage);
            this.tabControl1.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.tabControl1.Location = new System.Drawing.Point(338, 28);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(564, 582);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.Visible = false;
            // 
            // measureTabPage
            // 
            this.measureTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.measureTabPage.Controls.Add(this.measureComboBox);
            this.measureTabPage.Controls.Add(this.measurePanel);
            this.measureTabPage.Location = new System.Drawing.Point(4, 37);
            this.measureTabPage.Name = "measureTabPage";
            this.measureTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.measureTabPage.Size = new System.Drawing.Size(556, 541);
            this.measureTabPage.TabIndex = 0;
            this.measureTabPage.Text = "마디 정보";
            // 
            // clusterTabPage
            // 
            this.clusterTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.clusterTabPage.Controls.Add(this.clusterComboBox);
            this.clusterTabPage.Controls.Add(this.clusterMainLabel);
            this.clusterTabPage.Controls.Add(this.clusterPanel);
            this.clusterTabPage.Location = new System.Drawing.Point(4, 37);
            this.clusterTabPage.Name = "clusterTabPage";
            this.clusterTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.clusterTabPage.Size = new System.Drawing.Size(556, 541);
            this.clusterTabPage.TabIndex = 1;
            this.clusterTabPage.Text = "멜로디 형태 클러스터 정보";
            // 
            // fileNameTextBox
            // 
            this.fileNameTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.fileNameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fileNameTextBox.Font = new System.Drawing.Font("맑은 고딕", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.fileNameTextBox.Location = new System.Drawing.Point(34, 43);
            this.fileNameTextBox.Name = "fileNameTextBox";
            this.fileNameTextBox.ReadOnly = true;
            this.fileNameTextBox.Size = new System.Drawing.Size(260, 31);
            this.fileNameTextBox.TabIndex = 1;
            this.fileNameTextBox.Text = "로딩 중...";
            // 
            // parameterTextBox
            // 
            this.parameterTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.parameterTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.parameterTextBox.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.parameterTextBox.Location = new System.Drawing.Point(34, 79);
            this.parameterTextBox.Multiline = true;
            this.parameterTextBox.Name = "parameterTextBox";
            this.parameterTextBox.ReadOnly = true;
            this.parameterTextBox.Size = new System.Drawing.Size(260, 61);
            this.parameterTextBox.TabIndex = 2;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoScroll = true;
            this.tableLayoutPanel1.AutoScrollMargin = new System.Drawing.Size(50, 0);
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 62F));
            this.tableLayoutPanel1.Location = new System.Drawing.Point(33, 146);
            this.tableLayoutPanel1.MaximumSize = new System.Drawing.Size(290, 363);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(290, 363);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label13.Font = new System.Drawing.Font("맑은 고딕", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label13.Location = new System.Drawing.Point(53, 539);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(241, 40);
            this.label13.TabIndex = 24;
            this.label13.Text = "MIDI 분석기 v.1.0";
            // 
            // structureTabPage
            // 
            this.structureTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.structureTabPage.Location = new System.Drawing.Point(4, 37);
            this.structureTabPage.Name = "structureTabPage";
            this.structureTabPage.Size = new System.Drawing.Size(536, 541);
            this.structureTabPage.TabIndex = 2;
            this.structureTabPage.Text = "곡의 구조 정보";
            // 
            // measureComboBox
            // 
            this.measureComboBox.DropDownHeight = 72;
            this.measureComboBox.FormattingEnabled = true;
            this.measureComboBox.IntegralHeight = false;
            this.measureComboBox.Location = new System.Drawing.Point(27, 27);
            this.measureComboBox.Name = "measureComboBox";
            this.measureComboBox.Size = new System.Drawing.Size(230, 36);
            this.measureComboBox.TabIndex = 0;
            this.measureComboBox.Text = "마디 번호 선택!";
            this.measureComboBox.SelectedIndexChanged += new System.EventHandler(this.measureComboBox_SelectedIndexChanged);
            // 
            // measureMainLabel
            // 
            this.measureMainLabel.AutoSize = true;
            this.measureMainLabel.Font = new System.Drawing.Font("맑은 고딕", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.measureMainLabel.ForeColor = System.Drawing.Color.OrangeRed;
            this.measureMainLabel.Location = new System.Drawing.Point(21, 85);
            this.measureMainLabel.Name = "measureMainLabel";
            this.measureMainLabel.Size = new System.Drawing.Size(201, 32);
            this.measureMainLabel.TabIndex = 1;
            this.measureMainLabel.Text = "곡 > 트랙 > 마디";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("맑은 고딕", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label14.Location = new System.Drawing.Point(21, 195);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(63, 32);
            this.label14.TabIndex = 2;
            this.label14.Text = "조표";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("맑은 고딕", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label15.Location = new System.Drawing.Point(21, 145);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(87, 32);
            this.label15.TabIndex = 3;
            this.label15.Text = "박자표";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("맑은 고딕", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label16.Location = new System.Drawing.Point(288, 145);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(119, 32);
            this.label16.TabIndex = 4;
            this.label16.Text = "원곡 악보";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("맑은 고딕", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label17.Location = new System.Drawing.Point(288, 195);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(143, 32);
            this.label17.TabIndex = 5;
            this.label17.Text = "멜로디 악보";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("맑은 고딕", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label18.Location = new System.Drawing.Point(21, 245);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(63, 32);
            this.label18.TabIndex = 6;
            this.label18.Text = "화음";
            // 
            // measureOriginalScoreButton
            // 
            this.measureOriginalScoreButton.Enabled = false;
            this.measureOriginalScoreButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.measureOriginalScoreButton.Location = new System.Drawing.Point(433, 143);
            this.measureOriginalScoreButton.Name = "measureOriginalScoreButton";
            this.measureOriginalScoreButton.Size = new System.Drawing.Size(91, 38);
            this.measureOriginalScoreButton.TabIndex = 10;
            this.measureOriginalScoreButton.Text = "듣기!";
            this.measureOriginalScoreButton.UseVisualStyleBackColor = true;
            // 
            // measureMonophonicScoreButton
            // 
            this.measureMonophonicScoreButton.Enabled = false;
            this.measureMonophonicScoreButton.Location = new System.Drawing.Point(433, 193);
            this.measureMonophonicScoreButton.Name = "measureMonophonicScoreButton";
            this.measureMonophonicScoreButton.Size = new System.Drawing.Size(91, 38);
            this.measureMonophonicScoreButton.TabIndex = 11;
            this.measureMonophonicScoreButton.Text = "듣기!";
            this.measureMonophonicScoreButton.UseVisualStyleBackColor = true;
            // 
            // measureChordButton
            // 
            this.measureChordButton.Enabled = false;
            this.measureChordButton.Location = new System.Drawing.Point(433, 243);
            this.measureChordButton.Name = "measureChordButton";
            this.measureChordButton.Size = new System.Drawing.Size(91, 38);
            this.measureChordButton.TabIndex = 12;
            this.measureChordButton.Text = "듣기!";
            this.measureChordButton.UseVisualStyleBackColor = true;
            // 
            // measureKeyTextBox
            // 
            this.measureKeyTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.measureKeyTextBox.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.measureKeyTextBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.measureKeyTextBox.Location = new System.Drawing.Point(110, 195);
            this.measureKeyTextBox.Name = "measureKeyTextBox";
            this.measureKeyTextBox.ReadOnly = true;
            this.measureKeyTextBox.Size = new System.Drawing.Size(167, 34);
            this.measureKeyTextBox.TabIndex = 20;
            this.measureKeyTextBox.WordWrap = false;
            // 
            // measureTimeTextBox
            // 
            this.measureTimeTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.measureTimeTextBox.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.measureTimeTextBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.measureTimeTextBox.Location = new System.Drawing.Point(110, 145);
            this.measureTimeTextBox.Name = "measureTimeTextBox";
            this.measureTimeTextBox.ReadOnly = true;
            this.measureTimeTextBox.Size = new System.Drawing.Size(167, 34);
            this.measureTimeTextBox.TabIndex = 21;
            this.measureTimeTextBox.WordWrap = false;
            // 
            // measureChordTextBox
            // 
            this.measureChordTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.measureChordTextBox.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.measureChordTextBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.measureChordTextBox.Location = new System.Drawing.Point(110, 245);
            this.measureChordTextBox.Name = "measureChordTextBox";
            this.measureChordTextBox.ReadOnly = true;
            this.measureChordTextBox.Size = new System.Drawing.Size(306, 34);
            this.measureChordTextBox.TabIndex = 22;
            this.measureChordTextBox.WordWrap = false;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("맑은 고딕", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label19.Location = new System.Drawing.Point(21, 295);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(143, 32);
            this.label19.TabIndex = 23;
            this.label19.Text = "멜로디 형태";
            // 
            // measureMelodicContourTextBox
            // 
            this.measureMelodicContourTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.measureMelodicContourTextBox.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.measureMelodicContourTextBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.measureMelodicContourTextBox.Location = new System.Drawing.Point(13, 338);
            this.measureMelodicContourTextBox.Multiline = true;
            this.measureMelodicContourTextBox.Name = "measureMelodicContourTextBox";
            this.measureMelodicContourTextBox.ReadOnly = true;
            this.measureMelodicContourTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.measureMelodicContourTextBox.Size = new System.Drawing.Size(528, 138);
            this.measureMelodicContourTextBox.TabIndex = 24;
            this.measureMelodicContourTextBox.WordWrap = false;
            // 
            // measureClusterLabel
            // 
            this.measureClusterLabel.AutoSize = true;
            this.measureClusterLabel.Font = new System.Drawing.Font("맑은 고딕", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.measureClusterLabel.Location = new System.Drawing.Point(23, 490);
            this.measureClusterLabel.Name = "measureClusterLabel";
            this.measureClusterLabel.Size = new System.Drawing.Size(247, 32);
            this.measureClusterLabel.TabIndex = 25;
            this.measureClusterLabel.Text = "멜로디 형태 클러스터";
            // 
            // measureClusterTextBox
            // 
            this.measureClusterTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.measureClusterTextBox.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.measureClusterTextBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.measureClusterTextBox.Location = new System.Drawing.Point(276, 490);
            this.measureClusterTextBox.Name = "measureClusterTextBox";
            this.measureClusterTextBox.ReadOnly = true;
            this.measureClusterTextBox.Size = new System.Drawing.Size(63, 34);
            this.measureClusterTextBox.TabIndex = 26;
            this.measureClusterTextBox.Text = "...";
            this.measureClusterTextBox.WordWrap = false;
            // 
            // measureClusterButton
            // 
            this.measureClusterButton.Location = new System.Drawing.Point(357, 488);
            this.measureClusterButton.Name = "measureClusterButton";
            this.measureClusterButton.Size = new System.Drawing.Size(167, 38);
            this.measureClusterButton.TabIndex = 27;
            this.measureClusterButton.Text = "자세히 보기!";
            this.measureClusterButton.UseVisualStyleBackColor = true;
            // 
            // measurePanel
            // 
            this.measurePanel.Controls.Add(this.measureMainLabel);
            this.measurePanel.Controls.Add(this.label14);
            this.measurePanel.Controls.Add(this.label15);
            this.measurePanel.Controls.Add(this.label16);
            this.measurePanel.Controls.Add(this.label17);
            this.measurePanel.Controls.Add(this.label18);
            this.measurePanel.Controls.Add(this.measureOriginalScoreButton);
            this.measurePanel.Controls.Add(this.measureMonophonicScoreButton);
            this.measurePanel.Controls.Add(this.measureChordButton);
            this.measurePanel.Controls.Add(this.measureKeyTextBox);
            this.measurePanel.Controls.Add(this.measureTimeTextBox);
            this.measurePanel.Controls.Add(this.measureChordTextBox);
            this.measurePanel.Controls.Add(this.label19);
            this.measurePanel.Controls.Add(this.measureMelodicContourTextBox);
            this.measurePanel.Controls.Add(this.measureClusterLabel);
            this.measurePanel.Controls.Add(this.measureClusterTextBox);
            this.measurePanel.Controls.Add(this.measureClusterButton);
            this.measurePanel.Location = new System.Drawing.Point(0, 0);
            this.measurePanel.Name = "measurePanel";
            this.measurePanel.Size = new System.Drawing.Size(556, 541);
            this.measurePanel.TabIndex = 28;
            // 
            // clusterComboBox
            // 
            this.clusterComboBox.DropDownHeight = 72;
            this.clusterComboBox.FormattingEnabled = true;
            this.clusterComboBox.IntegralHeight = false;
            this.clusterComboBox.Location = new System.Drawing.Point(27, 27);
            this.clusterComboBox.Name = "clusterComboBox";
            this.clusterComboBox.Size = new System.Drawing.Size(230, 36);
            this.clusterComboBox.TabIndex = 1;
            this.clusterComboBox.Text = "클러스터 번호 선택!";
            this.clusterComboBox.SelectedIndexChanged += new System.EventHandler(this.clusterComboBox_SelectedIndexChanged);
            // 
            // clusterPanel
            // 
            this.clusterPanel.Controls.Add(this.clusterTextBox);
            this.clusterPanel.Location = new System.Drawing.Point(0, 0);
            this.clusterPanel.Name = "clusterPanel";
            this.clusterPanel.Size = new System.Drawing.Size(556, 545);
            this.clusterPanel.TabIndex = 2;
            // 
            // clusterMainLabel
            // 
            this.clusterMainLabel.AutoSize = true;
            this.clusterMainLabel.Font = new System.Drawing.Font("맑은 고딕", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.clusterMainLabel.ForeColor = System.Drawing.Color.OrangeRed;
            this.clusterMainLabel.Location = new System.Drawing.Point(21, 85);
            this.clusterMainLabel.Name = "clusterMainLabel";
            this.clusterMainLabel.Size = new System.Drawing.Size(452, 32);
            this.clusterMainLabel.TabIndex = 2;
            this.clusterMainLabel.Text = "몇 분 정도의 시간이 소요될 수 있습니다.";
            // 
            // clusterTextBox
            // 
            this.clusterTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.clusterTextBox.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.clusterTextBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.clusterTextBox.Location = new System.Drawing.Point(14, 138);
            this.clusterTextBox.Multiline = true;
            this.clusterTextBox.Name = "clusterTextBox";
            this.clusterTextBox.ReadOnly = true;
            this.clusterTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.clusterTextBox.Size = new System.Drawing.Size(528, 388);
            this.clusterTextBox.TabIndex = 25;
            this.clusterTextBox.WordWrap = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(914, 641);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "MIDI 분석기";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericMinimumPoints)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericEpsilon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPitchCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPitchRank)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPitchVariance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericOnset)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericDuration)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.measureTabPage.ResumeLayout(false);
            this.clusterTabPage.ResumeLayout(false);
            this.clusterTabPage.PerformLayout();
            this.measurePanel.ResumeLayout(false);
            this.measurePanel.PerformLayout();
            this.clusterPanel.ResumeLayout(false);
            this.clusterPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button loadMidiButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.NumericUpDown numericMinimumPoints;
        private System.Windows.Forms.NumericUpDown numericEpsilon;
        private System.Windows.Forms.NumericUpDown numericPitchCount;
        private System.Windows.Forms.NumericUpDown numericPitchRank;
        private System.Windows.Forms.NumericUpDown numericPitchVariance;
        private System.Windows.Forms.NumericUpDown numericOnset;
        private System.Windows.Forms.NumericUpDown numericDuration;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button analysisStartButton;
        private System.Windows.Forms.Button defaultSettingButton;
        private System.Windows.Forms.TextBox fileTextBox;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TextBox parameterTextBox;
        private System.Windows.Forms.TextBox fileNameTextBox;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage measureTabPage;
        private System.Windows.Forms.TabPage clusterTabPage;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label measureMainLabel;
        private System.Windows.Forms.ComboBox measureComboBox;
        private System.Windows.Forms.TabPage structureTabPage;
        private System.Windows.Forms.TextBox measureChordTextBox;
        private System.Windows.Forms.TextBox measureTimeTextBox;
        private System.Windows.Forms.TextBox measureKeyTextBox;
        private System.Windows.Forms.Button measureChordButton;
        private System.Windows.Forms.Button measureMonophonicScoreButton;
        private System.Windows.Forms.Button measureOriginalScoreButton;
        private System.Windows.Forms.Button measureClusterButton;
        private System.Windows.Forms.TextBox measureClusterTextBox;
        private System.Windows.Forms.Label measureClusterLabel;
        private System.Windows.Forms.TextBox measureMelodicContourTextBox;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Panel measurePanel;
        private System.Windows.Forms.Panel clusterPanel;
        private System.Windows.Forms.ComboBox clusterComboBox;
        private System.Windows.Forms.Label clusterMainLabel;
        private System.Windows.Forms.TextBox clusterTextBox;
    }
}

