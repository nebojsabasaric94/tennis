using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Drawing;

namespace Assets
{
    public class BackPropagation
    {
        public static int MAXN = 225;      //Maximalan broj neurona po slojevima
        public static int SLOJEVI = 3;   //Ukupan broj slojeva
        public static int uzorciN = 6;   //Ukupan broj uzoraka
        public static int MAXITERACIJA = 10000;  //Maximalan broj iteracija za obucavanje
        public static double MAXGRESKA = 0.005; //Zadata vrednost maximalne greske
        public static int BROJPOKUSAJA = 100;

        public TSloj[] slojevi = new TSloj[SLOJEVI]; //Svi slojevi u mrezi
        public double[, ,] tezine = new double[SLOJEVI - 1, MAXN, MAXN];  //Sve matrice tezina
        public double[, ,] tezineP = new double[SLOJEVI - 1, MAXN, MAXN]; //Matrice promena odgovarajucih tezina
        public double[, ,] obucavajuciSkup;// = new double[uzorciN, 2, MAXN];//
        public double ni;  //brzina obucavanja
        public double beta;//uticaj prethodnih promena

        public BackPropagation(int brojUzoraka, double[, ,] obucavajuciSkup)
        {
            uzorciN = brojUzoraka;
            this.obucavajuciSkup = obucavajuciSkup;
            inicijalizacija();
        }

        protected void inicijalizacija()
        {
            for (int i = 0; i < SLOJEVI; i++)
                slojevi[i] = new TSloj();
            ni = 0.05;
            beta = 0.5;
            slojevi[0].n = 225;  //broj neurona ulaznog sloja
            slojevi[1].n = 10;  //broj neurona skrivenog sloja
            slojevi[2].n = 1;  //broj neurona izlaznog sloja
        }

        protected void inicijalizacijaTezina()
        {
            Random rnd = new Random();
            for (int s = 0; s < SLOJEVI - 1; s++)
                for (int u = 0; u < slojevi[s].n; u++)
                    for (int v = 0; v < slojevi[s + 1].n; v++)
                    {//Inicijalizacija matrice tezina po preporuci W je element [-0.1, 0.1]
                        tezine[s, u, v] = (float)(rnd.NextDouble() - 0.5) / 5;
                        tezineP[s, u, v] = 0;
                    }//NextDouble()  - returns a random number between 0.0 and 1.0

            for (int s = 0; s < SLOJEVI; s++)
                for (int u = 0; u < MAXN; u++)
                {
                    slojevi[s].bias[u] = 0;
                    slojevi[s].biasp[u] = 0;
                }

            for (int s = 1; s < SLOJEVI - 1; s++)
                for (int u = 0; u < slojevi[s].n; u++)
                    slojevi[s].bias[u] = (float)rnd.NextDouble();
            //Inicijalizacija pragova aktivacije. [0,1]
        }

        double sigmoid(double net)
        {
            return 1 / (1 + Math.Exp(-net));
        }

        double[] izracunajIzlaz()
        {
            double net = 0.0;
            for (int s = 1; s < SLOJEVI; s++)
                for (int v = 0; v < slojevi[s].n; v++)
                {
                    net = slojevi[s].bias[v];
                    for (int u = 0; u < slojevi[s - 1].n; u++)
                    {
                        net += slojevi[s - 1].izlaz[u] * tezine[s - 1, u, v];
                    }
                    slojevi[s].izlaz[v] = sigmoid(net);
                }

            return slojevi[SLOJEVI - 1].izlaz;
        }

        void postaviUlaz(int uzorak)
        {
            for (int u = 0; u < slojevi[0].n; u++)
                slojevi[0].izlaz[u] = obucavajuciSkup[uzorak, 0, u];
        }

        double izracunajGreske(int uzorak)
        {
            double greska = 0.0;
            // greska u izlaznom sloju
            for (int v = 0; v < slojevi[SLOJEVI - 1].n; v++)
            {
                slojevi[SLOJEVI - 1].delta[v] = obucavajuciSkup[uzorak, 1, v] - slojevi[SLOJEVI - 1].izlaz[v];
                greska += (slojevi[SLOJEVI - 1].delta[v]) * (slojevi[SLOJEVI - 1].delta[v]);
            }
            // greska u ostalim slojevima
            for (int s = SLOJEVI - 2; s >= 0; s--)
                for (int u = 0; u < slojevi[s].n; u++)
                {
                    double sigmaa = 0.0;
                    for (int v = 0; v < slojevi[s + 1].n; v++)
                        sigmaa += slojevi[s + 1].delta[v] * tezine[s, u, v];
                    double f = slojevi[s].izlaz[u];
                    slojevi[s].delta[u] = f * (1 - f) * sigmaa;
                }
            return greska;
        }

        void korigujTezine()
        {
            for (int s = 0; s < SLOJEVI - 1; s++)
                for (int v = 0; v < slojevi[s + 1].n; v++)
                {
                    for (int u = 0; u < slojevi[s].n; u++)
                    {
                        tezineP[s, u, v] = ni * slojevi[s + 1].delta[v] * slojevi[s].izlaz[u] + beta * tezineP[s, u, v];
                        tezine[s, u, v] += tezineP[s, u, v];
                    }
                    slojevi[s + 1].biasp[v] = ni * slojevi[s + 1].delta[v] +
                                            beta * slojevi[s + 1].biasp[v];
                    slojevi[s + 1].bias[v] += slojevi[s + 1].biasp[v];
                    // izracunavanje korigovane vrednosti izlaza u sloju
                    double net = slojevi[s + 1].bias[v];
                    for (int u = 0; u < slojevi[s].n; u++)
                        net += slojevi[s].izlaz[u] * tezine[s, u, v];
                    slojevi[s + 1].izlaz[v] = sigmoid(net);
                }
        }

        double obucavanje()
        {
          //  greske = new List<PointF>();
            double greska = 0;
            for (int iteracija = 0; iteracija < MAXITERACIJA; iteracija++)
            {
                greska = 0;
                for (int uzorak = 0; uzorak < uzorciN; uzorak++)
                {
                    postaviUlaz(uzorak);
                    izracunajIzlaz();
                    greska += 0.5 * izracunajGreske(uzorak);
                    korigujTezine();
                }
                //Console.WriteLine("" + iteracija + " " + greska);
               // greske.Add(new PointF(iteracija, (float)greska));
                if (greska < MAXGRESKA)
                    break;
            }
            return greska;
        }

       // public List<PointF> greske = null;

        public void obuci()
        {
            for (int pokusaj = 0; pokusaj < BROJPOKUSAJA; pokusaj++)
            {
                inicijalizacijaTezina();
                double greska = obucavanje();
                if (greska < MAXGRESKA)
                    break;
            }
        }

        public double[] izracunaj(double[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                slojevi[0].izlaz[i] = data[i];
            }

            return izracunajIzlaz();
        }

        public int izracunajCifru(double[] data)
        {
            double[] d = izracunaj(data);
            // TODO 3
            // izracunati koja je cifra prepoznata na osnovu
            // podataka na izlazima neuronske mreze

            return -1;
        }


        public void ispisObucavjucegSkupa()//samo izlaza
        {
            for (int i = 0; i < uzorciN; i++)
            {
                Console.Write("\n uzorak " + i + " : ");
                for (int j = 0; j < 1; j++)
                {
                    Console.Write(obucavajuciSkup[i, 1, 0] + ", ");
                }
            }

        }


    }

    public class TSloj  //Tip u kojem se opisuje jedan sloj
    {
        public int n;  //Stvaran broj neurona u sloju
        public double[] izlaz = new double[BackPropagation.MAXN];
        public double[] delta = new double[BackPropagation.MAXN]; //Greska u posmatranom cvoru
        public double[] bias = new double[BackPropagation.MAXN];  //Prag aktivacije
        public double[] biasp = new double[BackPropagation.MAXN]; //Promena praga aktivacije
    }
}
