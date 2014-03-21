﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab01biometria
{
    class image_Gray : image_as_tab
    {
        public byte[][] Greycanal;
        public byte[][] alfa;
        public image_Gray() : base() { }

        public image_Gray(byte[] orginal_tab, int wight, int hight)
            : base(orginal_tab, wight, hight)
        {

            Greycanal = new byte[wight][];
            var k = 0;
            for (int i = 0; i < wight; i++)
            {
                Greycanal[i] = new byte[hight];
                for (int j = 0; j < hight; j++)
                {
                    k = 4 * (j * w + i);
                    Greycanal[i][j] = orginal_tab[k];
                    alfa[i][j] = orginal_tab[k+3];
                }
            }
        }
        public override void Accept(Visitor visitor)
        {
            visitor.Visit(this);
        }
        public void normalize()
        {
            
            //max
            var max = utab.Max();

            //min
            var min = utab.Min();

            var normalizeConst = max - min;



            ///pixel[x,y]=255*(pixel[x,y]-minPix)/(maxPix-minPix) 
            //[] utab1=  utab.Select((x,i )=> i%4!=0 ).ToArray().Select(x => 255 * (utab[ - min) / normalizeConst).ToArray();
            for (int i = 0; i < utab.Length; i++)
            {
                if (i % 4 != 0)
                {
                    utab[i] = (byte)(255 * (utab[i] - min) / normalizeConst);
                }

            }






        }
        public image_RGB histogram()
        {
            int[] Hist = new int[256];
            for (int k = 0; k < 256; k++)
            {
                Hist[k] = utab.Where((x, i) => i % 4 == 0 && x == k).ToArray().Length;
            }

            int max = Hist.Max();
            for (int i = 0; i < 256; i++)
            {
                Hist[i] = 180 * Hist[i] / max;
            }

            byte[][] histobraz = new byte[256][];
            for (int i = 0; i < 256; i++)
            {
                histobraz[i] = new byte[180];

                for (int j = 0; j < 180; j++)
                {
                    if (j < (180 - Hist[i]))
                        histobraz[i][j] = 0;
                    else
                        histobraz[i][j] = 100;
                }
            }
            byte[] temp = new byte[256 * 180 * 4];

            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 180; j++)
                {

                    temp[4 * (j * 256 + i)] = histobraz[i][j];
                    temp[4 * (j * 256 + i) + 1] = histobraz[i][j];
                    temp[4 * (j * 256 + i) + 2] = histobraz[i][j];
                    temp[4 * (j * 256 + i) + 3] = 255;


                }
            }

            return new image_RGB(temp, 256, 180);
        }
        public void BinaryGlobalMean()
        {
            var mean = utab.Sum(x => x) / utab.Length;
            byte one = 1;
            byte zero = 0;
            utab = utab.Select(x => x >= mean ? one : zero).ToArray();


        }
        public void BinaryLocalMean(int local)
        {
            //local jest rozmiare maski nie odleglosia od srodka
            byte zero = 0, one = 1;
            byte[][] Temp = new byte[this.w][];
            for (int x = 1; x < this.w - 1; x++)
            {
                Temp[x] = new byte[this.h];
                for (int y = 1; y < this.h - 1; y++)
                {
                    var Suma = 0;
                    for (int i = 0; i < local; i++)
                    {
                        for (int j = 0; j < local; j++)
                        {
                            Suma += this.Greycanal[x + i - 1][y + j - 1];

                        }

                    }
                    var localmean = Suma / Math.Pow(local, 2);
                    Temp[x][y] = Greycanal[x][y] >= localmean ? one : zero;
                }
            }
            Greycanal = (byte[][])Temp.Clone();
        }
        public void BinaryLocalGlobal(int local, int sigma)
        {
            byte zero = 0, one = 1;
            var globalmean = utab.Sum(x => x) / utab.Length;
            byte[][] Temp = new byte[this.w][];
            for (int x = 1; x < this.w - 1; x++)
            {
                Temp[x] = new byte[this.h];
                for (int y = 1; y < this.h - 1; y++)
                {
                    var Suma = 0;
                    for (int i = 0; i < local; i++)
                    {
                        for (int j = 0; j < local; j++)
                        {
                            Suma += this.Greycanal[x + i - 1][y + j - 1];

                        }

                    }
                    var localmean = Suma / Math.Pow(local, 2);

                    if ((globalmean - sigma < localmean) && (localmean < globalmean + sigma))
                    {
                        Temp[x][y] = Greycanal[x][y] >= localmean ? one : zero;
                    }
                    else
                        //mozna dodac +/- sigma ale konieczny dodatkowy warunek(mysle ze lepsze z sigma)
                        Temp[x][y] = Greycanal[x][y] >= globalmean? one : zero;
                }
            }
            Greycanal = (byte[][])Temp.Clone();
        }
        //binryzacja bez brzegów 
        public void Bersen(int local)
        {
            byte zero = 0, one = 1;
            var globalmean = utab.Sum(x => x) / utab.Length;
            byte epsilon = 1;
            var Suma = 0;
            byte[][] Temp = new byte[this.w][];
            for (int x = 1; x < this.w - 1; x++)
            {
                Temp[x] = new byte[this.h];
                for (int y = 1; y < this.h - 1; y++)
                {
                    Suma = 0;
                    byte[] temp = new byte[local*local];
                    for (int i = 0; i < local; i++)
                    {
                        
                        for (int j = 0; j < local; j++)
                        {
                            temp[i]= this.Greycanal[x + i - 1][y + j - 1];

                        }

                    }

                    var TempMax = temp.Max();
                    var TempMin = temp.Min();
                    var level=(TempMax+TempMin)/2;


                    if ((TempMax-TempMax)<epsilon)
                    {
                        Temp[x][y] = Greycanal[x][y] >= globalmean ? one : zero;
                    }
                    else
                        //mozna dodac +/- sigma ale konieczny dodatkowy warunek(mysle ze lepsze z sigma)
                        Temp[x][y] = Greycanal[x][y] >= level ? one : zero;
                }
            }
            Greycanal = (byte[][])Temp.Clone();

        }
        public void Otsu()
        {
            int[] Hist = new int[256];
            for (int k = 0; k < 256; k++)
            {
                Hist[k] = utab.Where((x, i) => i % 4 == 0 && x == k).ToArray().Length;
            }
            var suma = 0;
            byte one = 1;
            byte zero = 0;
            var total = this.w * this.h;
            for (int k = 0; k < 256; k++)
            {
                suma += Hist[k] * k;
            }
            var sumB = 0;
            var wB = 0;
            var wF = 0;
            var mB=0;
            var mF=0;
            var max = 0.0;
            var between = 0.0;
            var threshold1 = 0.0;
            var threshold2 = 0.0;
            for (var i = 0; i < 256; ++i) {
                wB += Hist[i];
                if (wB == 0)
                    continue;
                wF = total - wB;
                if (wF == 0)
                    break;
                sumB += i * Hist[i];
                mB = sumB / wB;
                mF = (suma - sumB) / wF;
                between = wB * wF * Math.Pow(mB - mF, 2);
                if ( between >= max ) {
                    threshold1 = i;
                    if ( between > max ) {
                        threshold2 = i;
                    }
                    max = between;            
                }
            }
            var progowanie= ( threshold1 + threshold2 ) / 2.0;
            utab = utab.Select(x => x >= progowanie ? one : zero).ToArray();
        }
        



    }
}

