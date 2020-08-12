﻿using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace AgOpenGPS
{
    public class CHeadLines
    {
        //list of coordinates of boundary line
        public List<Vec3> HeadLine = new List<Vec3>();
        public List<Vec3> HeadArea = new List<Vec3>();

        //the list of constants and multiples of the boundary
        public List<Vec2> calcList = new List<Vec2>();
        public List<bool> isDrawList = new List<bool>();

        public double Northingmin, Northingmax, Eastingmin, Eastingmax;


        public void DrawHeadLine(int linewidth)
        {
        

            ////draw the turn line oject
            //if (hdLine.Count < 1) return;
            //int ptCount = hdLine.Count;
            //GL.LineWidth(linewidth);
            //GL.Color3(0.732f, 0.7932f, 0.30f);
            //GL.Begin(PrimitiveType.LineStrip);
            //for (int h = 0; h < ptCount; h++) GL.Vertex3(hdLine[h].easting, hdLine[h].northing, 0);
            //GL.Vertex3(hdLine[0].easting, hdLine[0].northing, 0);
            //GL.End();
            
            if (HeadLine.Count < 2) return;
            int ptCount = HeadLine.Count;
            int cntr = 0;
            if (ptCount > 1)
            {
                GL.LineWidth(linewidth);
                GL.Color3(0.960f, 0.96232f, 0.30f);
                //GL.PointSize(2);

                while (cntr < ptCount)
                {
                    if (isDrawList.Count > cntr)
                    {

                        if (isDrawList[cntr])
                        {
                            GL.Begin(PrimitiveType.LineStrip);

                            if (cntr > 0) GL.Vertex3(HeadLine[cntr - 1].easting, HeadLine[cntr - 1].northing, 0);
                            else GL.Vertex3(HeadLine[HeadLine.Count - 1].easting, HeadLine[HeadLine.Count - 1].northing, 0);


                            for (int i = cntr; i < ptCount; i++)
                            {
                                cntr++;
                                if (!isDrawList[i]) break;
                                GL.Vertex3(HeadLine[i].easting, HeadLine[i].northing, 0);
                            }
                            if (cntr < ptCount - 1)
                                GL.Vertex3(HeadLine[cntr + 1].easting, HeadLine[cntr + 1].northing, 0);

                            GL.End();
                        }
                        else
                        {
                            cntr++;
                        }
                    }
                    else
                    {
                        cntr++;
                    }
                }
            }
        }

        public void PreCalcHeadLines()
        {
            int j = HeadLine.Count - 1;
            //clear the list, constant is easting, multiple is northing
            calcList.Clear();
            Vec2 constantMultiple = new Vec2(0, 0);

            Northingmin = Northingmax = HeadLine[0].northing;
            Eastingmin = Eastingmax = HeadLine[0].easting;

            for (int i = 0; i < HeadLine.Count; j = i++)
            {
                if (Northingmin > HeadLine[i].northing) Northingmin = HeadLine[i].northing;
                if (Northingmax < HeadLine[i].northing) Northingmax = HeadLine[i].northing;
                if (Eastingmin > HeadLine[i].easting) Eastingmin = HeadLine[i].easting;
                if (Eastingmax < HeadLine[i].easting) Eastingmax = HeadLine[i].easting;

                //check for divide by zero
                if (Math.Abs(HeadLine[i].northing - HeadLine[j].northing) < double.Epsilon)
                {
                    constantMultiple.easting = HeadLine[i].easting;
                    constantMultiple.northing = 0;
                    calcList.Add(constantMultiple);
                }
                else
                {
                    //determine constant and multiple and add to list
                    constantMultiple.easting = HeadLine[i].easting - ((HeadLine[i].northing * HeadLine[j].easting)
                                    / (HeadLine[j].northing - HeadLine[i].northing)) + ((HeadLine[i].northing * HeadLine[i].easting)
                                        / (HeadLine[j].northing - HeadLine[i].northing));
                    constantMultiple.northing = (HeadLine[j].easting - HeadLine[i].easting) / (HeadLine[j].northing - HeadLine[i].northing);
                    calcList.Add(constantMultiple);
                }
            }
        }

        public void DrawHeadBackBuffer()
        {
            int ptCount = HeadArea.Count;
            if (ptCount < 3) return;
            GL.Begin(PrimitiveType.Triangles);
            for (int h = 0; h < ptCount - 2; h++)
            {
                GL.Vertex3(HeadArea[h].easting, HeadArea[h].northing, 0);
            }
            GL.End();
        }

        public void PreCalcHeadArea()
        {
            Tess tess = new Tess(HeadLine);
            HeadArea.Clear();
            for (int i = 0; i < tess.ElementCount; i++)
            {
                for (int k = 0; k < 3; k++)
                {
                    int index = tess.Elements[i * 3 + k];
                    if (index == -1) continue;
                    HeadArea.Add(new Vec3(tess.Vertices[index].easting, tess.Vertices[index].northing, 0));
                }
            }
        }
    }
}