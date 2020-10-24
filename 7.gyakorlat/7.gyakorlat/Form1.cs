using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using _7.gyakorlat.Entities;

namespace _7.gyakorlat
{
    
    public partial class Form1 : Form
    {

        #region Variables
        List<Person> Population = new List<Person>();
        List<BirthProbability> BirthProbabilities = new List<Entities.BirthProbability>();
        List<DeathProbability> DeathProbabilities = new List<DeathProbability>();
        List<int> NumberOfMales = new List<int>();
        List<int> NumberOfFemales = new List<int>();

        int startyear = 2005;
        int endyear;
        string popfile = "";


        Random rng = new Random(1234);
        #endregion
        public Form1()
        {
            InitializeComponent();
            Population = GetPopulation(@"C:\Temp\nép.csv");
            BirthProbabilities = GetBirthProbabilities(@"C:\Temp\születés.csv");
            DeathProbabilities = GetDeathProbabilities(@"C:\Temp\halál.csv");
            numericUpDown1.Text = Resource1.Zaroev;
            label2.Text = Resource1.NepFajl;
            textBox1.Text = popfile;
            button1.Text = Resource1.Browse;
            button2.Text = Resource1.Start;



        }
        private void Simulation()
        {
            endyear = int.Parse(numericUpDown1.Value.ToString());
            NumberOfMales.Clear();
            NumberOfFemales.Clear();

            for (int year = startyear; year <= endyear; year++)
            {

                for (int i = 0; i < Population.Count; i++)
                {
                    Person person = Population[i];
                    SimStep(year, person);


                }

                int nbrOfMales = (from x in Population
                                  where x.Gender == Gender.Male && x.IsAlive
                                  select x).Count();
                int nbrOfFemales = (from x in Population
                                    where x.Gender == Gender.Female && x.IsAlive
                                    select x).Count();
                NumberOfMales.Add(nbrOfMales);
                NumberOfFemales.Add(nbrOfFemales);

            }
        }
        private void DisplayResults(int year, List<int> NumberOfMales, List<int> NumberOfFemales)
        {
            string newline = Environment.NewLine;
            for (int i = 0; i < NumberOfFemales.Count(); i++)
            {
                richTextBox1.AppendText(String.Format("{1}Szimulációs év: {0}{1}\tFérfiak: {2}{1}\tNők: {3}{1}", year + i, newline, NumberOfMales[i], NumberOfFemales[i]));
            }

        }


        public List<Person> GetPopulation(string csvpath)
        {
            List<Person> population = new List<Person>();

            using (StreamReader sr = new StreamReader(csvpath, Encoding.Default))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().Split(';');
                    population.Add(new Person()
                    {
                        BirthYear = int.Parse(line[0]),
                        Gender = (Gender)Enum.Parse(typeof(Gender), line[1]),
                        NbrOfChildren = int.Parse(line[2])
                    });
                }
            }

            return population;
        }
        public List<BirthProbability> GetBirthProbabilities(string csvpath)
        {
            List<BirthProbability> birthProbability = new List<BirthProbability>();

            using (StreamReader sr = new StreamReader(csvpath, Encoding.Default))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().Split(';');
                    birthProbability.Add(new BirthProbability()
                    {
                        Age = int.Parse(line[0]),
                        NbrOfChildren = int.Parse(line[1]),
                        Probability = double.Parse(line[2])
                        

                });
                }
            }

            return birthProbability;
        }

        public List<DeathProbability> GetDeathProbabilities(string csvpath)
        {
            List<DeathProbability> deathProbability = new List<DeathProbability>();

            using (StreamReader sr = new StreamReader(csvpath, Encoding.Default))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().Split(';');
                    deathProbability.Add(new DeathProbability()
                    {
                        Gender = (Gender)Enum.Parse(typeof(Gender), line[0]),
                        Age = int.Parse(line[1]),                        
                        DthProbability = double.Parse(line[2])

                    });
                }
            }

            return deathProbability;
        }

        public void SimStep(int year, Person person)
        {

            if (!person.IsAlive) return;

            byte age = (byte)(year - person.BirthYear);


            double pDeath = (from x in DeathProbabilities
                             where x.Gender == person.Gender && x.Age == age
                             select x.DthProbability).FirstOrDefault();

            if (rng.NextDouble() <= pDeath)
                person.IsAlive = false;


            if (person.IsAlive && person.Gender == Gender.Female)
            {

                double pBirth = (from x in BirthProbabilities
                                 where x.Age == age
                                 select x.Probability).FirstOrDefault();

                if (rng.NextDouble() <= pBirth)
                {
                    Person újszülött = new Person();
                    újszülött.BirthYear = year;
                    újszülött.NbrOfChildren = 0;
                    újszülött.Gender = (Gender)(rng.Next(1, 3));
                    Population.Add(újszülött);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Simulation();
            DisplayResults(startyear, NumberOfMales, NumberOfFemales);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    popfile = ofd.FileName;
                    textBox1.Text = popfile;
                    Population = GetPopulation(popfile);
                    BirthProbabilities = GetBirthProbabilities(@"C:\Temp\születés.csv");
                    DeathProbabilities = GetDeathProbabilities(@"C:\Temp\halál.csv");
                }
            }

        }
    }
}
