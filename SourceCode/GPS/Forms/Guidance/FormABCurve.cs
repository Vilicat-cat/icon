﻿using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace AgOpenGPS
{
    public partial class FormABCurve : Form
    {
        //access to the main GPS form and all its variables
        private readonly FormGPS mf;
        private int originalSelected = 0;
        public bool formLoading = false;

        public FormABCurve(Form _mf)
        {
            Owner = mf = _mf as FormGPS;
            InitializeComponent();

            btnPausePlay.Text = gStr.gsPause;
            this.Text = gStr.gsABCurve;
        }

        private void FormABCurve_Load(object sender, EventArgs e)
        {
            btnPausePlay.Enabled = false;
            btnAPoint.Enabled = false;
            btnBPoint.Enabled = false;
            btnAddToFile.Enabled = false;
            btnAddAndGo.Enabled = false;
            textBox1.Enabled = false;

            originalSelected = mf.ABLine.numABLineSelected;

            mf.curve.isOkToAddPoints = false;

            formLoading = true;
            if (mf.curve.SpiralMode)
            {
                //comboBox1.Text = "Spiral Mode";
                //button2.Enabled = true;
            }
            else if (mf.curve.CircleMode)
            {
                //comboBox1.Text = "Circle Mode";
                //button2.Enabled = true;
            }
            else
            {
                //comboBox1.Text = "AB Curve";
                //button2.Enabled = false;
            }
            formLoading = false;


            if ((mf.curve.SpiralMode || mf.curve.CircleMode) && (mf.curve.refList.Count == 1))
            {
                lblCurveExists.Text = gStr.gsCurveSet;
            }
            else if (mf.curve.refList.Count > 3)
            {
                lblCurveExists.Text = gStr.gsCurveSet;
            }
            else
            {
                mf.curve.ResetCurveLine();
                lblCurveExists.Text = " > Off <";
            }

            lvLines.Clear();
            ListViewItem itm;

            foreach (var item in mf.curve.curveArr)
            {
                itm = new ListViewItem(item.Name);
                lvLines.Items.Add(itm);
            }

            // go to bottom of list - if there is a bottom
            if (lvLines.Items.Count > 0) lvLines.Items[lvLines.Items.Count - 1].EnsureVisible();

            ShowSavedPanel(true);
            //this.Size = new System.Drawing.Size(280, 440);
            //btnMulti.Image = Properties.Resources.ArrowLeft;
        }

        //for calculating for display the averaged new line
        public void SmoothAB(int smPts)
        {
            //count the reference list of original curve
            int cnt = mf.curve.refList.Count;

            //the temp array
            Vec3[] arr = new Vec3[cnt];

            //read the points before and after the setpoint
            for (int s = 0; s < smPts / 2; s++)
            {
                arr[s].easting = mf.curve.refList[s].easting;
                arr[s].northing = mf.curve.refList[s].northing;
                arr[s].heading = mf.curve.refList[s].heading;
            }

            for (int s = cnt - (smPts / 2); s < cnt; s++)
            {
                arr[s].easting = mf.curve.refList[s].easting;
                arr[s].northing = mf.curve.refList[s].northing;
                arr[s].heading = mf.curve.refList[s].heading;
            }

            //average them - center weighted average
            for (int i = smPts / 2; i < cnt - (smPts / 2); i++)
            {
                for (int j = -smPts / 2; j < smPts / 2; j++)
                {
                    arr[i].easting += mf.curve.refList[j + i].easting;
                    arr[i].northing += mf.curve.refList[j + i].northing;
                }
                arr[i].easting /= smPts;
                arr[i].northing /= smPts;
                arr[i].heading = mf.curve.refList[i].heading;
            }

            //make a list to draw
            mf.curve.refList?.Clear();
            for (int i = 0; i < cnt; i++)
            {
                mf.curve.refList.Add(arr[i]);
            }
        }

        private void BtnAddToFile_Click(object sender, EventArgs e)
        {
            if (mf.curve.refList.Count > 0)
            {
                if (textBox1.Text.Length > 0)
                {
                    mf.curve.curveArr.Add(new CCurveLines());

                    mf.curve.numCurveLines = mf.curve.curveArr.Count;
                    if (mf.curve.numCurveLineSelected > mf.curve.numCurveLines) mf.curve.numCurveLineSelected = mf.curve.numCurveLines;

                    //array number is 1 less since it starts at zero
                    int cnt = mf.curve.curveArr.Count - 1;

                    mf.curve.curveArr[cnt].Name = textBox1.Text.Trim();

                    mf.curve.curveArr[cnt].spiralmode = mf.curve.SpiralMode;
                    mf.curve.curveArr[cnt].circlemode = mf.curve.CircleMode;

                    mf.curve.curveArr[cnt].aveHeading = mf.curve.aveLineHeading;

                    //write out the Curve Points
                    foreach (var item in mf.curve.refList)
                    {
                        mf.curve.curveArr[cnt].curvePts.Add(item);
                    }

                    //update the listbox with new curve name
                    ListViewItem itm = new ListViewItem(mf.curve.curveArr[cnt].Name);
                    lvLines.Items.Add(itm);
                    lvLines.Enabled = true;
                    textBox1.BackColor = SystemColors.ControlLight;
                    textBox1.Text = "";
                    textBox1.Enabled = false;
                    btnAddAndGo.Enabled = false;
                    btnAddToFile.Enabled = false;
                    btnAPoint.Enabled = false;
                    btnCancel.Enabled = true;
                    btnNewCurve.Enabled = true;
                    lvLines.SelectedItems.Clear();
                    btnNewCurve.Enabled = true;

                    mf.FileSaveCurveLines();
                }
            }
            else
            {
                mf.TimedMessageBox(2000, gStr.gsNoABCurveCreated, gStr.gsCompleteAnABCurveLineFirst);
                textBox1.BackColor = SystemColors.Window;
            }
        }
        private void BtnAddAndGo_Click(object sender, EventArgs e)
        {
            if (mf.curve.refList.Count > 0)
            {
                if (textBox1.Text.Length > 0)
                {
                    mf.curve.curveArr.Add(new CCurveLines());

                    mf.curve.numCurveLines = mf.curve.curveArr.Count;
                    mf.curve.numCurveLineSelected = mf.curve.numCurveLines;

                    //array number is 1 less since it starts at zero
                    int idx = mf.curve.curveArr.Count - 1;

                    mf.curve.curveArr[idx].Name = textBox1.Text.Trim();
                    mf.curve.curveArr[idx].aveHeading = mf.curve.aveLineHeading;

                    mf.curve.curveArr[idx].spiralmode = mf.curve.SpiralMode;
                    mf.curve.curveArr[idx].circlemode = mf.curve.CircleMode;

                    //write out the Curve Points
                    foreach (var item in mf.curve.refList)
                    {
                        mf.curve.curveArr[idx].curvePts.Add(item);
                    }

                    mf.curve.isCurveSet = true;

                    mf.FileSaveCurveLines();

                    Close();
                }
            }
            else
            {
                mf.TimedMessageBox(2000, gStr.gsNoABCurveCreated, gStr.gsCompleteAnABCurveLineFirst);
                textBox1.BackColor = SystemColors.Window;
            }

        }

        private void BtnNewCurve_Click(object sender, EventArgs e)
        {
            ShowSavedPanel(false);
            btnNewCurve.Enabled = false;
            btnCancel.Enabled = false;
            btnAPoint.Enabled = true;
        }

        private void BtnAPoint_Click(object sender, System.EventArgs e)
        {
            mf.curve.moveDistance = 0;
            //clear out the reference list
            lblCurveExists.Text = gStr.gsDriving;
            mf.curve.ResetCurveLine();

            btnAPoint.Enabled = false;
            mf.curve.isOkToAddPoints = true;
            btnPausePlay.Enabled = true;
            btnPausePlay.Visible = true;
            btnBPoint.Enabled = true;
        }

        private void BtnBPoint_Click(object sender, System.EventArgs e)
        {
            mf.curve.aveLineHeading = 0;
            mf.curve.isOkToAddPoints = false;
            btnBPoint.Enabled = false;
            btnAPoint.Enabled = false;
            btnPausePlay.Enabled = false;
            btnCancel.Enabled = false;
            lvLines.Enabled = false;

            int cnt = mf.curve.refList.Count;

            if (mf.curve.SpiralMode || mf.curve.CircleMode)
            {
                if (mf.curve.refList.Count > 1)
                {
                    double easting = 0;
                    double northing = 0;

                    if (mf.curve.refList.Count > 1)
                    {
                        for (int i = 0; i < (mf.curve.refList.Count); i++)
                        {
                            easting += mf.curve.refList[i].easting;
                            northing += mf.curve.refList[i].northing;
                        }
                    }
                    easting /= mf.curve.refList.Count;
                    northing /= mf.curve.refList.Count;

                    mf.curve.refList?.Clear();
                    mf.curve.refList.Add(new Vec3(easting, northing, 0));

                }
                else if (mf.curve.refList.Count < 1)
                {
                    mf.curve.refList.Add(new Vec3(mf.pivotAxlePos.easting, mf.pivotAxlePos.northing, 0));
                }


                mf.curve.OldhowManyPathsAway = -1;//reset
                mf.curve.isCurveSet = true;
                mf.EnableYouTurnButtons();
                //mf.FileSaveCurveLine();
                lblCurveExists.Text = gStr.gsCurveSet;

                ShowSavedPanel(true);

                btnAddAndGo.Enabled = true;
                btnAddToFile.Enabled = true;
                btnAPoint.Enabled = false;
                btnBPoint.Enabled = false;
                btnPausePlay.Enabled = false;

                textBox1.BackColor = Color.LightGreen;
                textBox1.Enabled = true;

                if (mf.curve.SpiralMode) textBox1.Text = "spiral " + DateTime.Now.ToString("hh:mm:ss", CultureInfo.InvariantCulture);

                if (mf.curve.CircleMode) textBox1.Text = "circle " + DateTime.Now.ToString("hh:mm:ss", CultureInfo.InvariantCulture);
            }
            else if (cnt > 3)
            {
                //make sure distance isn't too big between points on Turn
                for (int i = 0; i < cnt - 1; i++)
                {
                    int j = i + 1;
                    //if (j == cnt) j = 0;
                    double distance = Glm.Distance(mf.curve.refList[i], mf.curve.refList[j]);
                    if (distance > 1.2)
                    {
                        Vec3 pointB = new Vec3((mf.curve.refList[i].easting + mf.curve.refList[j].easting) / 2.0,
                            (mf.curve.refList[i].northing + mf.curve.refList[j].northing) / 2.0,
                            mf.curve.refList[i].heading);

                        mf.curve.refList.Insert(j, pointB);
                        cnt = mf.curve.refList.Count;
                        i = -1;
                    }
                }

                //calculate average heading of line
                double x = 0, y = 0;
                mf.curve.isCurveSet = true;
                foreach (var pt in mf.curve.refList)
                {
                    x += Math.Cos(pt.heading);
                    y += Math.Sin(pt.heading);
                }
                x /= mf.curve.refList.Count;
                y /= mf.curve.refList.Count;
                mf.curve.aveLineHeading = Math.Atan2(y, x);
                if (mf.curve.aveLineHeading < 0) mf.curve.aveLineHeading += Glm.twoPI;

                //build the tail extensions
                mf.curve.AddFirstLastPoints();
                SmoothAB(4);
                mf.curve.CalculateTurnHeadings();

                mf.curve.isCurveSet = true;
                mf.EnableYouTurnButtons();
                //mf.FileSaveCurveLine();
                lblCurveExists.Text = gStr.gsCurveSet;

                ShowSavedPanel(true);

                btnAddAndGo.Enabled = true;
                btnAddToFile.Enabled = true;
                btnAPoint.Enabled = false;
                btnBPoint.Enabled = false;
                btnPausePlay.Enabled = false;

                textBox1.BackColor = Color.LightGreen;
                textBox1.Enabled = true;
                textBox1.Text = (Math.Round(Glm.ToDegrees(mf.curve.aveLineHeading), 1)).ToString(CultureInfo.InvariantCulture)
                    + "\u00B0" + mf.FindDirection(mf.curve.aveLineHeading)
                    + DateTime.Now.ToString("hh:mm:ss", CultureInfo.InvariantCulture);
            }
            else
            {
                mf.curve.isCurveSet = false;
                mf.curve.refList?.Clear();

                lblCurveExists.Text = " > Off <";
                ShowSavedPanel(true);
                btnNewCurve.Enabled = true;
                btnCancel.Enabled = true;
                lvLines.Enabled = true;
            }

            lvLines.SelectedItems.Clear();
        }
        private void TextBox1_Enter(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

        private void BtnCancel_Click(object sender, System.EventArgs e)
        {
            mf.curve.OldhowManyPathsAway = -99999;
            mf.curve.moveDistance = 0;
            mf.curve.isOkToAddPoints = false;
            mf.curve.isCurveSet = false;
            mf.curve.refList?.Clear();
            mf.curve.isCurveSet = false;
            mf.DisableYouTurnButtons();
            //mf.btnContourPriority.Enabled = false;
            //mf.curve.ResetCurveLine();
            mf.curve.isBtnCurveOn = false;
            mf.btnCurve.Image = Properties.Resources.CurveOff;
            if (mf.isAutoSteerBtnOn) mf.btnAutoSteer.PerformClick();
            if (mf.yt.isYouTurnBtnOn) mf.btnAutoYouTurn.PerformClick();

            mf.curve.numCurveLineSelected = 0;
            Close();
        }

        private void BtnListDelete_Click(object sender, EventArgs e)
        {
            mf.curve.moveDistance = 0;

            if (lvLines.SelectedItems.Count > 0)
            {
                int num = lvLines.SelectedIndices[0];
                mf.curve.curveArr.RemoveAt(num);
                lvLines.SelectedItems[0].Remove();

                //everything changed, so make sure its right
                mf.curve.numCurveLines = mf.curve.curveArr.Count;
                if (mf.curve.numCurveLineSelected > mf.curve.numCurveLines) mf.curve.numCurveLineSelected = mf.curve.numCurveLines;

                //if there are no saved oned, empty out current curve line and turn off
                if (mf.curve.numCurveLines == 0)
                {
                    mf.curve.ResetCurveLine();
                    if (mf.isAutoSteerBtnOn) mf.btnAutoSteer.PerformClick();
                    if (mf.yt.isYouTurnBtnOn) mf.btnAutoYouTurn.PerformClick();
                }

                mf.FileSaveCurveLines();
            }
        }

        private void BtnListUse_Click(object sender, EventArgs e)
        {
            mf.curve.OldhowManyPathsAway = -99999;
            mf.curve.moveDistance = 0;

            int count = lvLines.SelectedItems.Count;

            if (count > 0)
            {
                int idx = lvLines.SelectedIndices[0];
                mf.curve.numCurveLineSelected = idx + 1;

                mf.curve.SpiralMode = mf.curve.curveArr[idx].spiralmode;
                mf.curve.CircleMode = mf.curve.curveArr[idx].circlemode;



                if (mf.curve.curveArr[idx].spiralmode || mf.curve.curveArr[idx].circlemode)
                {
                    //if (mf.curve.curveArr[idx].SpiralMode) comboBox1.Text = "Spiral Mode";
                    //else comboBox1.Text = "Circle Mode";
                    if (mf.curve.curveArr[idx].curvePts.Count == 1)
                    {
                        mf.curve.refList.Clear();
                        mf.curve.refList.Add(new Vec3(mf.curve.curveArr[idx].curvePts[0].easting, mf.curve.curveArr[idx].curvePts[0].northing, 0));
                    }
                    else if (mf.curve.curveArr[idx].curvePts.Count > 1)
                    {
                        double easting = 0;
                        double northing = 0;
                        for (int i = 0; i < (mf.curve.curveArr[idx].curvePts.Count); i++)
                        {
                            easting += mf.curve.curveArr[idx].curvePts[i].easting;
                            northing += mf.curve.curveArr[idx].curvePts[i].northing;
                        }
                        easting /= mf.curve.curveArr[idx].curvePts.Count;
                        northing /= mf.curve.curveArr[idx].curvePts.Count;
                        mf.curve.refList.Clear();
                        mf.curve.refList.Add(new Vec3(easting, northing, 0));
                    }
                    else
                    {
                        mf.curve.refList.Clear();
                        mf.curve.refList.Add(new Vec3(mf.pivotAxlePos.easting, mf.pivotAxlePos.northing, 0));
                    }
                    mf.curve.OldhowManyPathsAway = -1;//reset
                    mf.curve.isCurveSet = true;
                    mf.EnableYouTurnButtons();
                }
                else if (mf.curve.refList.Count < 3)
                {
                    mf.btnCurve.PerformClick();
                    mf.curve.ResetCurveLine();
                    mf.DisableYouTurnButtons();
                }
                else
                {
                    mf.curve.aveLineHeading = mf.curve.curveArr[idx].aveHeading;
                    mf.curve.refList?.Clear();
                    for (int i = 0; i < mf.curve.curveArr[idx].curvePts.Count; i++)
                    {
                        mf.curve.refList.Add(mf.curve.curveArr[idx].curvePts[i]);
                    }
                    mf.curve.isCurveSet = true;
                    mf.EnableYouTurnButtons();
                }
                //can go back to Mainform without seeing form.
                Close();
            }

            //no item selected
            else
            {
                return;
            }
        }

        private void BtnPausePlay_Click(object sender, EventArgs e)
        {
            if (mf.curve.isOkToAddPoints)
            {
                mf.curve.isOkToAddPoints = false;
                btnPausePlay.Image = Properties.Resources.BoundaryRecord;
                btnPausePlay.Text = gStr.gsRecord;
                btnBPoint.Enabled = false;
            }
            else
            {
                mf.curve.isOkToAddPoints = true;
                btnPausePlay.Image = Properties.Resources.boundaryPause;
                btnPausePlay.Text = gStr.gsPause;
                btnBPoint.Enabled = true;
            }
        }

        private void ShowSavedPanel(bool showPanel)
        {
            //show the list
            if (showPanel)
            {
                this.Size = new System.Drawing.Size(436, 415);
                btnAddToFile.Visible = true;
                btnAddAndGo.Visible = true;
                btnListDelete.Visible = true;
                btnListUse.Visible = true;
                textBox1.Visible = true;
                lvLines.Visible = true;
                btnCancel.Visible = true;
                btnNewCurve.Visible = true;
                btnPausePlay.Visible = false;

                btnAPoint.Visible = false;
                btnBPoint.Visible = false;
                btnPausePlay.Visible = false;
                label2.Visible = false;
                lblCurveExists.Visible = false;
                btnCancelMain.Visible = true;
                btnCancel2.Visible = false;
            }
            else //show the A B Pause
            {
                this.Size = new System.Drawing.Size(239, 350);
                btnAddToFile.Visible = false;
                btnAddAndGo.Visible = false;
                btnListDelete.Visible = false;
                btnListUse.Visible = false;
                textBox1.Visible = false;
                lvLines.Visible = false;
                btnCancel.Visible = false;
                btnNewCurve.Visible = false;
                btnPausePlay.Visible = false;

                btnAPoint.Visible = true;
                btnBPoint.Visible = true;
                btnPausePlay.Visible = true;
                label2.Visible = true;
                lblCurveExists.Visible = true;

                btnCancelMain.Visible = false;
                btnCancel2.Visible = true;
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            int count = lvLines.SelectedItems.Count;
            if (count > 0)
            {
                btnListDelete.Enabled = true;
                btnListUse.Enabled = true;
            }
            else
            {
                btnListDelete.Enabled = false;
                btnListUse.Enabled = false;
            }
        }

        private void FormABCurve_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (this.Width < 300) e.Cancel = true;
        }

        private void LvLines_SelectedIndexChanged(object sender, EventArgs e)
        {
            mf.curve.moveDistance = 0;

            int count = lvLines.SelectedItems.Count;

            if (count > 0)
            {
                int idx = lvLines.SelectedIndices[0];
                mf.curve.numCurveLineSelected = idx + 1;

                mf.curve.SpiralMode = mf.curve.curveArr[idx].spiralmode;
                mf.curve.CircleMode = mf.curve.curveArr[idx].circlemode;


                if (mf.curve.curveArr[idx].spiralmode || mf.curve.curveArr[idx].circlemode)
                {
                    //if (mf.curve.curveArr[idx].SpiralMode) comboBox1.Text = "Spiral Mode";
                    //else comboBox1.Text = "Circle Mode";

                    if (mf.curve.curveArr[idx].curvePts.Count == 1)
                    {
                        mf.curve.refList.Clear();
                        mf.curve.refList.Add(new Vec3(mf.curve.curveArr[idx].curvePts[0].easting, mf.curve.curveArr[idx].curvePts[0].northing, 0));
                    }
                    else if (mf.curve.curveArr[idx].curvePts.Count > 1)
                    {
                        double easting = 0;
                        double northing = 0;
                        for (int i = 0; i < (mf.curve.curveArr[idx].curvePts.Count); i++)
                        {
                            easting += mf.curve.curveArr[idx].curvePts[i].easting;
                            northing += mf.curve.curveArr[idx].curvePts[i].northing;
                        }
                        easting /= mf.curve.curveArr[idx].curvePts.Count;
                        northing /= mf.curve.curveArr[idx].curvePts.Count;
                        mf.curve.refList.Clear();
                        mf.curve.refList.Add(new Vec3(easting, northing, 0));
                    }
                    else
                    {
                        mf.curve.refList.Clear();
                        mf.curve.refList.Add(new Vec3(mf.pivotAxlePos.easting, mf.pivotAxlePos.northing, 0));
                    }
                    mf.curve.OldhowManyPathsAway = -1;//reset
                    mf.curve.isCurveSet = true;
                    //mf.EnableYouTurnButtons();
                }
                else if (mf.curve.curveArr[idx].curvePts.Count < 3)
                {
                    mf.btnCurve.PerformClick();
                    mf.curve.ResetCurveLine();
                    //mf.DisableYouTurnButtons();

                    mf.curve.curveArr.RemoveAt(idx);
                    lvLines.SelectedItems[0].Remove();

                    //everything changed, so make sure its right
                    mf.curve.numCurveLines = mf.curve.curveArr.Count;
                    if (mf.curve.numCurveLineSelected > mf.curve.numCurveLines) mf.curve.numCurveLineSelected = mf.curve.numCurveLines;

                    //if there are no saved oned, empty out current curve line and turn off
                    if (mf.curve.numCurveLines == 0)
                    {
                        mf.curve.ResetCurveLine();
                        if (mf.isAutoSteerBtnOn) mf.btnAutoSteer.PerformClick();
                        if (mf.yt.isYouTurnBtnOn) mf.btnAutoYouTurn.PerformClick();
                    }

                    mf.FileSaveCurveLines();

                    //delete?
                }
                else
                {
                    mf.curve.aveLineHeading = mf.curve.curveArr[idx].aveHeading;
                    mf.curve.refList?.Clear();
                    for (int i = 0; i < mf.curve.curveArr[idx].curvePts.Count; i++)
                    {
                        mf.curve.refList.Add(mf.curve.curveArr[idx].curvePts[i]);
                    }
                    mf.curve.isCurveSet = true;
                    //mf.EnableYouTurnButtons();
                }
                //can go back to Mainform without seeing form.
            }

            //no item selected
            else
            {
                return;
            }
        }
        private void BtnCancelMain_Click(object sender, EventArgs e)
        {
            mf.curve.numCurveLines = mf.curve.curveArr.Count;
            if (mf.curve.numCurveLineSelected > mf.curve.numCurveLines) mf.curve.numCurveLineSelected = mf.curve.numCurveLines;

            if (mf.curve.numCurveLineSelected < originalSelected)
            {
                mf.curve.numCurveLineSelected = 0;
            }
            else mf.curve.numCurveLineSelected = originalSelected;

            if (mf.curve.numCurveLineSelected > 0)
            {
                int idx = mf.curve.numCurveLineSelected - 1;
                mf.curve.aveLineHeading = mf.curve.curveArr[idx].aveHeading;

                mf.curve.refList?.Clear();
                for (int i = 0; i < mf.curve.curveArr[idx].curvePts.Count; i++)
                {
                    mf.curve.refList.Add(mf.curve.curveArr[idx].curvePts[i]);
                }

                if (mf.curve.refList.Count < 3)
                {
                    mf.btnCurve.PerformClick();
                    mf.curve.ResetCurveLine();
                    mf.DisableYouTurnButtons();
                }
                else
                {
                    mf.curve.isCurveSet = true;
                }
                Close();
            }
            else
            {
                mf.curve.moveDistance = 0;
                mf.curve.isOkToAddPoints = false;
                mf.curve.isCurveSet = false;
                mf.curve.refList?.Clear();
                mf.curve.isCurveSet = false;
                mf.DisableYouTurnButtons();
                //mf.btnContourPriority.Enabled = false;
                //mf.curve.ResetCurveLine();
                mf.curve.isBtnCurveOn = false;
                mf.btnCurve.Image = Properties.Resources.CurveOff;
                if (mf.isAutoSteerBtnOn) mf.btnAutoSteer.PerformClick();
                if (mf.yt.isYouTurnBtnOn) mf.btnAutoYouTurn.PerformClick();

                mf.curve.numCurveLineSelected = 0;
                Close();
            }
        }
    }
}