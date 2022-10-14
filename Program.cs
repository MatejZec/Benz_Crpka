/*
 * -------------------------------------------------------------------------------------------------------
 * 
 * Autor: Matej Zec
 * Projekt: Benzinska crpka
 * Predmet: Osnove programiranja
 * Ustanova: VSMTI
 * Godina: 2020/21.
 * 
 * -------------------------------------------------------------------------------------------------------
 */
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Xml;
using System.IO;
using ConsoleTables;

namespace benz_crpka2
{
    class Program
    {
        static void Loger(string tekst)
        {
            string putanja = "Logovi.txt";
         
            if (File.Exists(putanja))
            {
                using (StreamWriter sw = File.AppendText(putanja))
                {
                    sw.WriteLine(DateTime.Now.ToString("dd.MM.yyyy.HH:mm:ss") + " - " + tekst);
                    sw.Close();
                }
            }
            else
            {
                using (StreamWriter sw = File.CreateText(putanja))
                {
                    sw.WriteLine(DateTime.Now.ToString("dd.MM.yyyy.HH:mm:ss") + " - " + tekst);
                    sw.Close();
                }
            }
        }

        static string Prijava(string string_xml) 
        {
            Console.WriteLine("********** PRIJAVA **********");
            Loger("Pokusaj prijave");
            Console.Write("\nUnesite korisnicko ime: ");
            string pokusajK = Console.ReadLine();
            Console.Write("Unesite lozinku: ");
            string pokusajL = Console.ReadLine();

            XmlDocument open_xml = new XmlDocument();
            open_xml.LoadXml(string_xml); 
            //LISTA ZAPOSLENIKA
            XmlNodeList xml_nodes = open_xml.SelectNodes("//data/zaposlenik"); 

            bool zastavica = false;
            for (int i = 0; i < xml_nodes.Count; i++) //petlja za provjeru ispravnosti korisnickog imena i lozinke
            {
                if (pokusajK == xml_nodes[i].Attributes["korisnicko_ime"].Value & pokusajL == xml_nodes[i].Attributes["lozinka"].Value) //ako u u istom nodu (zaposleniku) nadje upisanu sifru i korisnicko ime
                {
                    Console.WriteLine("Prijava uspijesna!");
                    Loger("Uspijesna prijava");
                    zastavica = true;
                    return xml_nodes[i].Attributes["id"].Value; // metoda vraca id prijavljenog zaposlenika
                }
            }
            if (zastavica == false)
            {
                Loger("Neuspijesna prijava");
                Console.WriteLine("Netocna lozinka ili korisnicko ime! Pokusajte ponovno\n");
                return Prijava(string_xml); //metoda ce vratiti string (id) ponovnim (rekurzivnim) pozivom metode
            }
            return "nista"; //nikad se nece izvrsit jer zastavica mora biti ili true ili false
        }

        public struct Racun
        {
            public string vrijeme;
            public string datum;
            public string gorivo_id;
            public double kolicina;
            public double cijena_L;
            public double cijena;
            public string iznos_PDV;
            public double cijena_PDV;
            public string vrsta_placanja;
            public string zaposlenik_id;


            public Racun(string vrijeme, string datum, string gorivo_id, double kolicina, double cijena_L, double cijena, string iznos_PDV, double cijena_PDV, string vrsta_placanja, string zaposlenik_id)
            {
                this.vrijeme = vrijeme;
                this.datum = datum;
                this.gorivo_id = gorivo_id;
                this.kolicina = kolicina;
                this.cijena_L = cijena_L;
                this.cijena = cijena;
                this.iznos_PDV = iznos_PDV;
                this.cijena_PDV = cijena_PDV;
                this.vrsta_placanja = vrsta_placanja;
                this.zaposlenik_id = zaposlenik_id;
            }
        }

        public struct Spremnik 
        {
            public string spremnik_id;
            public string vrsta_spremnika;
            public double stanje;
            public double cijena_goriva; 

            public Spremnik(string spremnik_id, string vrsta_spremnika, double stanje, double cijena_goriva)
            {
                this.spremnik_id = spremnik_id;
                this.vrsta_spremnika = vrsta_spremnika;
                this.stanje = stanje;
                this.cijena_goriva = cijena_goriva;
            }
        }

        static void Ispis_racuna(string string_json_racun, string string_json_spremnik, string string_xml, string string_xml_vrste_placanja) //-----------------------1----------------------------
        {
            Loger("Pregled Racuna");
            Console.WriteLine("********** PREGLED RACUNA **********");

            XmlDocument OpenXml = new XmlDocument();                             //ZBOG SVIH ZAPOSLENIKA ID
            OpenXml.LoadXml(string_xml);                                         //
            XmlNodeList openNodes = OpenXml.SelectNodes("//data/zaposlenik");    //radim listu zaposlenika (svaki clan u listi jedan zaposlenik/node)

            List<Spremnik> spremnici = new List<Spremnik>();                                 //ZBOG GORIVO ID radim listu spremnici
            spremnici = JsonConvert.DeserializeObject<List<Spremnik>>(string_json_spremnik); //punim listu spremici (svaki clan u listi jedan node/spremnik)
            double cijena_diesel = 0;
            double cijena_benzin = 0;

            XmlDocument OpenXml2 = new XmlDocument();                                 //ZBOG VRSTA PLACANJA ID
            OpenXml2.LoadXml(string_xml_vrste_placanja);                                            //
            XmlNodeList ListXmlVP = OpenXml2.SelectNodes("//data/nacin_placanja");    //radim listu vrsta placanja (svaki clan u listi jedan node/jedna vrsta placanja)

            List<Racun> racuni = new List<Racun>();
            racuni = JsonConvert.DeserializeObject<List<Racun>>(string_json_racun); //punim listu racuna

            short brojac = 1;
            Console.WriteLine("Pregled svih racuna:\n");
            //ISPIS RACUNA
            foreach (Racun racun in racuni)
            {
                var table = new ConsoleTable("Racun br.", Convert.ToString(brojac));
                //Console.Write("\nRacun " + brojac + ":");
                table.AddRow("Vrijeme", racun.vrijeme);
                table.AddRow("Datum", racun.datum);                
                foreach (Spremnik spremnik in spremnici)
                {
                    if (spremnik.spremnik_id == "111")
                        cijena_diesel = spremnik.cijena_goriva;
                    else
                        cijena_benzin = spremnik.cijena_goriva;
                    if (racun.gorivo_id == spremnik.spremnik_id)
                        table.AddRow("Vrsta goriva", spremnik.vrsta_spremnika);
                }
                table.AddRow("Kolicina", racun.kolicina + " litara");
                if (racun.gorivo_id == "111") //ako je racun za dizel
                {
                    table.AddRow("Cijena po litri", cijena_diesel + " HRK"); 
                    table.AddRow("Cijena", racun.kolicina * cijena_diesel + " HRK"); 
                    table.AddRow("Cijena uz PDV", racun.cijena_PDV + " HRK"); 
                }
                else
                {
                    table.AddRow("Cijena po litri", cijena_benzin + " HRK");
                    table.AddRow("Cijena", racun.kolicina * cijena_benzin + " HRK");
                    table.AddRow("Cijena uz PDV", racun.cijena_PDV + " HRK");
                }
                table.AddRow("Iznos PDV-a", racun.iznos_PDV);

                foreach (XmlNode node in ListXmlVP)
                {
                    if (racun.vrsta_placanja == node.Attributes["id"].Value) //ako je id vrste placanja iz racuna isti kao i id nekog nodea u listi vp
                        table.AddRow("Vrsta placanja", node.Attributes["naziv"].Value); //onda ispisi naziv te vrste placanja
                }
        
                foreach (XmlNode node in openNodes)
                {
                    if (racun.zaposlenik_id == node.Attributes["id"].Value)
                        table.AddRow("Zaposlenik", node.Attributes["ime"].Value + " " + node.Attributes["prezime"].Value);
                }
                brojac++;
                table.Write(Format.MarkDown);
                Console.WriteLine();
            }
            Izvjestaj(racuni); //na kraju ispisa svih racuna zovem izvjestaj (prosledjujem listu racuna)
        }

        static void Izvjestaj(List<Racun> racuni) //-----------------------1----------------------------
        {
            double ukupno_kolicina = 0;
            double ukupno_diesel = 0;
            double ukupno_benzin = 0;
            foreach (Racun racun in racuni)
            {
                ukupno_kolicina += racun.kolicina;
                if (racun.gorivo_id == "111")
                {
                    ukupno_diesel += racun.kolicina;                    
                }
                else
                {
                    ukupno_benzin += racun.kolicina;                   
                }
            }

            var table = new ConsoleTable("Izvjestaj o prodaji", " ");

            if (ukupno_diesel > ukupno_benzin)
            {
                table.AddRow("Gorivo s vecom prodanom kolicinom", "Diesel");
                table.AddRow("Prodano litara", ukupno_diesel);
                table.AddRow("Postotak od ukupne prodane kolicine", Math.Round((ukupno_diesel / ukupno_kolicina) * 100, 2) + "%");
            }
            if (ukupno_diesel < ukupno_benzin)
            {
                table.AddRow("Gorivo s vecom prodanom kolicinom", "Benzin");
                table.AddRow("Prodano litara", ukupno_benzin);
                table.AddRow("Postotak od ukupne prodane kolicine", Math.Round((ukupno_benzin / ukupno_kolicina) * 100, 2) + "%");
            }
            if (ukupno_diesel == ukupno_benzin)
                table.AddRow("Prodana je identicna kolicina obje vrste goriva - Prodano litara", ukupno_diesel);

            table.Write(Format.Alternative);
        }

        static void StanjeSpremniciGorivo(string string_json) //-------------------2-----------------------
        {
            Loger("Pregled stanja u spremnicima");
            Console.WriteLine("********** STANJE U SPREMNICIMA **********");
            List<Spremnik> spremnici = new List<Spremnik>();
            spremnici = JsonConvert.DeserializeObject<List<Spremnik>>(string_json);

            Console.WriteLine();
            foreach (Spremnik spremnik in spremnici)
            {
                var table = new ConsoleTable("Vrsta spremnika", spremnik.vrsta_spremnika);
                table.AddRow("Stanje spremnika", spremnik.stanje + " litara");
                /*if (spremnik.vrsta_spremnika == "diesel")*/
                {
                    table.AddRow("Cijena goriva", spremnik.cijena_goriva + " HRK/litra");
                }/*
                else
                {
                    table.AddRow("Cijena benzina", spremnik.cijena_goriva + " HRK/litra");                    
                }*/
                table.Write(Format.Alternative);
            }
        }

        static void Ispis_spremnici(string string_json) //-----------------------3----------------------------
        {
            Loger("Azuriranje");
            Console.WriteLine("***** AZURIRANJE STANJA SPREMNIKA ILI CIJENE GORIVA *****");
            Console.WriteLine("\n\t1 - Azuriraj stanje spremnika\n");
            Console.WriteLine("\t2 - Azuriraj cijenu goriva\n");
            Console.Write("Odaberite opciju: ");
            int odluka = Convert.ToInt32(Console.ReadLine());

            List<Spremnik> spremnici = new List<Spremnik>();
            spremnici = JsonConvert.DeserializeObject<List<Spremnik>>(string_json);
            double trenutno_stanje_benzin = 0;
            double trenutno_stanje_diesel = 0;
            double trenutna_cijena_dizel = 0;
            double trenutna_cijena_benzin = 0;

            switch (odluka)
            {
                case 1:
                    Loger("Odabrano - Azuriranje spremnika");
                    foreach (Spremnik spremnik in spremnici) //ide po listi spreminka
                    {
                        Console.Write("\nVrsta spremnika: " + spremnik.vrsta_spremnika);
                        Console.Write(", Stanje: " + spremnik.stanje + " litara");

                        if (spremnik.vrsta_spremnika == "diesel")
                        {
                            trenutno_stanje_diesel = spremnik.stanje;  
                            trenutna_cijena_dizel = spremnik.cijena_goriva;
                        }
                        else
                        {
                            trenutno_stanje_benzin = spremnik.stanje;
                            trenutna_cijena_benzin = spremnik.cijena_goriva;
                        }
                    }
                    Odabir_spremnika(trenutno_stanje_benzin, trenutno_stanje_diesel, trenutna_cijena_benzin, trenutna_cijena_dizel);
                    break;
                case 2:
                    Loger("Odabrano - Azuriranje goriva");
                    foreach (Spremnik spremnik in spremnici)
                    {
                        if(spremnik.spremnik_id == "111")
                        {
                            trenutno_stanje_diesel = spremnik.stanje;
                            trenutna_cijena_dizel = spremnik.cijena_goriva;
                            Console.Write("\nCijena diesela: {0} HRK/litra\n", spremnik.cijena_goriva);
                        }
                        else
                        {
                            trenutno_stanje_benzin = spremnik.stanje;
                            trenutna_cijena_benzin = spremnik.cijena_goriva;
                            Console.Write("Cijena benzina: {0} HRK/litra\n", spremnik.cijena_goriva);
                        }
                    }
                    Odabir_goriva(trenutno_stanje_benzin, trenutno_stanje_diesel, trenutna_cijena_benzin, trenutna_cijena_dizel);
                    break;
                default:
                    Console.WriteLine("Pogresan unos!");
                    Ispis_spremnici(string_json);
                    break;
            }
        }

        static void Odabir_goriva(double trenutno_stanje_benzin, double trenutno_stanje_diesel, double trenutna_cijena_benzin, double trenutna_cijena_dizel) //-----------------------3----------------------------
        {
            Console.WriteLine("\n\t1 - Diesel");
            Console.WriteLine("\n\t2 - Benzin");
            Console.Write("\nOdaberite vrstu goriva za azuriranje: ");
            int odabir = Convert.ToInt32(Console.ReadLine());

            switch (odabir)
            {
                case 1:
                    Loger("Odabrano - Diesel gorivo");
                    Azuriranje_cijene_goriva(odabir, trenutno_stanje_benzin, trenutno_stanje_diesel, trenutna_cijena_benzin, trenutna_cijena_dizel);
                    break;
                case 2:
                    Loger("Odabrano - Benzin gorivo");
                    Azuriranje_cijene_goriva(odabir, trenutno_stanje_benzin, trenutno_stanje_diesel, trenutna_cijena_benzin, trenutna_cijena_dizel);
                    break;
                default:
                    Console.WriteLine("Pogresan unos!");
                    Odabir_goriva(trenutno_stanje_benzin, trenutno_stanje_diesel, trenutna_cijena_benzin, trenutna_cijena_dizel);
                    break;
            }
        }

        static void Azuriranje_cijene_goriva(int odabir, double trenutno_stanje_benzin, double trenutno_stanje_diesel, double trenutna_cijena_benzin, double trenunta_cijena_dizel) //-----------------------3----------------------------
        {
            Console.Write("Unesite novu cijenu u kunama: ");
            double nova_cijena = Convert.ToDouble(Console.ReadLine());

            List<Spremnik> novo_cijena_gorivo = new List<Spremnik>(); //radim novu listu tipa spremnik (svaki elem. u njoj bice tipa spremink sto znaci da ce svaki element liste biti jedan objekt strukture spremnik)
            if (odabir == 1) //dizel
            {
                novo_cijena_gorivo.Add(new Spremnik()       //dodajem prvi element (novi objekt spermink) u listu i postavljam mu atribute
                {
                    spremnik_id = "111",
                    vrsta_spremnika = "diesel",
                    stanje = trenutno_stanje_diesel,
                    cijena_goriva = nova_cijena
                });
                novo_cijena_gorivo.Add(new Spremnik()          //ovaj ostaje isti
                {
                    spremnik_id = "112",
                    vrsta_spremnika = "benzin",
                    stanje = trenutno_stanje_benzin,
                    cijena_goriva = trenutna_cijena_benzin
                });
            }
            else
            {
                novo_cijena_gorivo.Add(new Spremnik()            //ovaj ostaje isti
                {
                    spremnik_id = "111",
                    vrsta_spremnika = "diesel",
                    stanje = trenutno_stanje_diesel,
                    cijena_goriva = trenunta_cijena_dizel
                });
                novo_cijena_gorivo.Add(new Spremnik()
                {
                    spremnik_id = "112",
                    vrsta_spremnika = "benzin",
                    stanje = trenutno_stanje_benzin,
                    cijena_goriva = nova_cijena
                });
            }
            Console.WriteLine("Cijena goriva azurirana!");
            string novo_json = JsonConvert.SerializeObject(novo_cijena_gorivo.ToArray(), Newtonsoft.Json.Formatting.Indented); //serializiram novu listu u novi string
            File.WriteAllText(@"C:\Users\Gratisfaction\source\repos\benz_crpka2\spremnici.json", novo_json); //i onda prebrisem postojecu datoteku spremnici.json sa novim stringom
            Loger("Gorivo azurirano");
        }
       
        static void Odabir_spremnika(double trenutno_stanje_benzin, double trenutno_stanje_diesel, double trenutna_cijena_benzin, double trenutna_cijena_dizel) //-----------------------3----------------------------
        {
            Console.WriteLine("\n\n\t1 - Diesel spremnik");
            Console.WriteLine("\n\t2 - Benzin spremnik");
            Console.Write("\nOdaberite spremnik za unos goriva: ");
            int odabir = Convert.ToInt32(Console.ReadLine());

            switch (odabir)
            {
                case 1:
                    Loger("Odabran - Diesel spremnik");
                    Azuriranje_spremnika(odabir, trenutno_stanje_benzin, trenutno_stanje_diesel, trenutna_cijena_benzin, trenutna_cijena_dizel);
                    break;
                case 2:
                    Loger("Odabran - Benzin spremnik");
                    Azuriranje_spremnika(odabir, trenutno_stanje_benzin, trenutno_stanje_diesel, trenutna_cijena_benzin, trenutna_cijena_dizel);
                    break;
                default:
                    Console.WriteLine("Pogresan unos!");
                    Odabir_spremnika(trenutno_stanje_benzin, trenutno_stanje_diesel, trenutna_cijena_benzin, trenutna_cijena_dizel);
                    break;
            }
        }

        
        static void Azuriranje_spremnika(int odabir, double trenutno_stanje_benzin, double trenutno_stanje_diesel, double trenutna_cijena_benzin, double trenutna_cijena_dizel) //-----------------------3----------------------------
        {
            double max_unos;
            if (odabir == 1)
                max_unos = 20000 - trenutno_stanje_diesel;
            else
                max_unos = 20000 - trenutno_stanje_benzin;

            Console.Write("Iznos novog goriva (u litrama): ");
            double gorivo = Convert.ToDouble(Console.ReadLine());
            if (gorivo <= max_unos & gorivo >= 1000)
            {
                List<Spremnik> novo_spremnici = new List<Spremnik>();
                double novo_stanje;
                if (odabir == 1)
                {
                    novo_stanje = gorivo + trenutno_stanje_diesel;

                    novo_spremnici.Add(new Spremnik()
                    {
                        spremnik_id = "111",
                        vrsta_spremnika = "diesel",
                        stanje = novo_stanje,
                        cijena_goriva = trenutna_cijena_dizel
                    });
                    novo_spremnici.Add(new Spremnik()
                    {
                        spremnik_id = "112",
                        vrsta_spremnika = "benzin",
                        stanje = trenutno_stanje_benzin,
                        cijena_goriva = trenutna_cijena_benzin
                    });
                }
                else
                {
                    novo_stanje = gorivo + trenutno_stanje_benzin;

                    novo_spremnici.Add(new Spremnik()
                    {
                        spremnik_id = "111",
                        vrsta_spremnika = "diesel",
                        stanje = trenutno_stanje_diesel,
                        cijena_goriva = trenutna_cijena_dizel
                    });
                    novo_spremnici.Add(new Spremnik()
                    {
                        spremnik_id = "112",
                        vrsta_spremnika = "benzin",
                        stanje = novo_stanje,
                        cijena_goriva = trenutna_cijena_benzin
                    });
                }
                Console.WriteLine("Stanje spremnika azurirano!");
                string novo_json = JsonConvert.SerializeObject(novo_spremnici.ToArray(), Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(@"C:\Users\Gratisfaction\source\repos\benz_crpka2\spremnici.json", novo_json);
                Loger("Spremnik azuriran");
            }
            else
            {
                Console.WriteLine("Pogresan unos!");
                Azuriranje_spremnika(odabir, trenutno_stanje_benzin, trenutno_stanje_diesel, trenutna_cijena_benzin, trenutna_cijena_dizel);
            }
            
        }

        static void Izrada_racuna(string string_json, string string_json2, string string_xml, string zaposlenik_id) //-----------------------4----------------------------
        {
            Loger("Izrada racuna");
            Console.WriteLine("********** IZRADA RACUNA **********");
            List<Spremnik> spremnici = new List<Spremnik>();
            spremnici = JsonConvert.DeserializeObject<List<Spremnik>>(string_json2);
            double gorivo = 0;
            double max_unos = 0;
            double cijena = 0;
            string gorivo_id = "";
            string odabir_placanje = "";
            Console.WriteLine("\nOdabir goriva:");
            Console.WriteLine("\n\t1 - diesel");
            Console.WriteLine("\t2 - benzin\n");
            Console.Write("Odaberite opciju: ");
            int odabir = Convert.ToInt32(Console.ReadLine());
            switch (odabir) 
            {
                case 1:   //DIZEL                 
                    foreach (Spremnik spremnik in spremnici)     //idem po listi spremink ispisujem dostupnu kolicinu i cijenu i pamtim maksimalni moguci unos
                    {
                        if (spremnik.spremnik_id == "111")
                        {
                            Console.WriteLine("\nDostupna kolicina diesela: {0} litara", spremnik.stanje);
                            max_unos = spremnik.stanje;
                            cijena = spremnik.cijena_goriva;
                            gorivo_id = spremnik.spremnik_id;
                            Console.WriteLine("Cijena: {0} HRK/litra", spremnik.cijena_goriva);
                        }
                    }
                    while (true) //petlja sluzi ako unos bude nepravilan da se ponavlja sve dok ne bude pravilan 
                    {
                        Console.Write("Unesite kolicinu goriva za prodaju: ");
                        gorivo = Convert.ToDouble(Console.ReadLine());
                        if (gorivo >= 5 & gorivo <= max_unos)
                        {
                            odabir_placanje = Odabir_placanje(string_xml);  //prije nego zovem funkciju kreacija_racuna zovem funkciju odabir placanja koja ce vratit zeljenu vrstu placanja u varijablu odabir_placanje
                            Kreacija_racuna(string_json, zaposlenik_id, gorivo, cijena, cijena*gorivo, gorivo_id, odabir_placanje); //prosljedujem sve potrebno za kreaciju racuna
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Pogresan Unos");
                        }
                    }                        
                    break;
                case 2:    //BENZIN
                    foreach (Spremnik spremnik in spremnici)
                    {
                        if (spremnik.spremnik_id == "112")
                        {
                            Console.WriteLine("\nDostupna kolicina benzina: {0} litara", spremnik.stanje);
                            max_unos = spremnik.stanje;
                            cijena = spremnik.cijena_goriva;
                            gorivo_id = spremnik.spremnik_id;
                            Console.WriteLine("Cijena: {0} HRK/litra", spremnik.cijena_goriva);
                        }
                    }
                    while (true)
                    {
                        Console.Write("Unesite kolicinu goriva za prodaju: ");
                        gorivo = Convert.ToDouble(Console.ReadLine());
                        if (gorivo >= 5 & gorivo <= max_unos)
                        {
                            odabir_placanje = Odabir_placanje(string_xml);
                            Kreacija_racuna(string_json, zaposlenik_id, gorivo, cijena, cijena * gorivo, gorivo_id, odabir_placanje);
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Pogresan Unos");
                        }
                    }
                    break;
                default:
                    Console.WriteLine("Pogresan unos!");
                    Izrada_racuna(string_json, string_json2, string_xml, zaposlenik_id); //rekurzivni poziv ako korisnik pogrjesi unos na pocetku
                    break;
            }

            Azuriranje_spremnika2(string_json2, gorivo, odabir); // kada se izvrsi kracija novog racuna vratit ce se u switch, breakat te doci ovdje, pa se tu zove azuriranje spremnika2
        }

        static string Odabir_placanje(string string_xml) //-----------------------4----------------------------
        {
            Console.WriteLine("\nOdabir nacina placanja:\n");
            Console.WriteLine("\t1 - Gotovina");
            Console.WriteLine("\t2 - Karticno placanje");
            Console.WriteLine("\t3 - R1\n");
            Console.Write("Odaberite opciju: ");
            int odabir = Convert.ToInt32(Console.ReadLine());
            if (odabir < 1 || odabir > 3)
                Odabir_placanje(string_xml); //rekurzivni poziv ako unos nevalja

            XmlDocument open_xml = new XmlDocument();
            open_xml.LoadXml(string_xml);
            XmlNodeList xml_nodes = open_xml.SelectNodes("//data/nacin_placanja"); //lista xml_nodes za vrsteplacanja

            for (int i = 0; i < xml_nodes.Count; i++) //ide po listi xml_nodes
            {
                if (odabir == 1 & xml_nodes[i].Attributes["naziv"].Value == "Gotovina")
                {
                    return xml_nodes[i].Attributes["id"].Value; //funcija returna/vraca string (id od zeljenog nacina placanja)
                }
                if (odabir == 2 & xml_nodes[i].Attributes["naziv"].Value == "Karticno")
                {
                    return xml_nodes[i].Attributes["id"].Value;
                }
                if (odabir == 3 & xml_nodes[i].Attributes["naziv"].Value == "R1")
                {
                    return xml_nodes[i].Attributes["id"].Value;
                }
            }
            return "";
        }

        static void Kreacija_racuna(string string_json, string zaposlenik_id, double gorivo, double cijena_L, double cijena_goriva, string gorivo_id, string odabir_placanje) //-----------------------4----------------------------
        {
            /*citam iz xml-a i radim listu xml nodova*/                          //ZBOG imena i prezimena zaposlenika u racunu
            StreamReader sr = new StreamReader("config.xml");                    //
            string string_xml = sr.ReadToEnd();                                  //
            sr.Close();                                                          //
            XmlDocument OpenXml = new XmlDocument();                             //
            OpenXml.LoadXml(string_xml);                                         //
            XmlNodeList openNodes = OpenXml.SelectNodes("//data/zaposlenik");    //

            /*radim listu za vrstu placanja*/                                         //ZBOG naziva vrste placanja
            StreamReader sr3 = new StreamReader("vrste_placanja.xml");                //
            string string_xml2 = sr3.ReadToEnd();                                     //
            sr3.Close();                                                              //
            XmlDocument OpenXml2 = new XmlDocument();                                 //
            OpenXml2.LoadXml(string_xml2);                                            //
            XmlNodeList ListXmlVP = OpenXml2.SelectNodes("//data/nacin_placanja");    //

            List<Racun> lista_racuna = JsonConvert.DeserializeObject<List<Racun>>(string_json); //radim listu svih racuna

            lista_racuna.Add(new Racun() // i njoj dodam novi racun kojem postavljamo atribute
            {
                vrijeme = DateTime.Now.ToShortTimeString(), //vrijeme i datum popravljeni
                datum = DateTime.Now.ToShortDateString(),
                gorivo_id = gorivo_id,
                kolicina = gorivo,
                cijena_L = cijena_L,
                cijena = Math.Round(cijena_goriva, 2),
                iznos_PDV = "25%",
                cijena_PDV = Math.Round(cijena_goriva*1.25, 2),
                vrsta_placanja = odabir_placanje,
                zaposlenik_id = zaposlenik_id
            });

            Console.WriteLine("Izdani racun:\n");
            //*---------------------------------------------------------------------------ispis trenutnog racuna

            var table = new ConsoleTable("Racun br.", Convert.ToString(lista_racuna.Count));
            table.AddRow("Vrijeme", lista_racuna[lista_racuna.Count - 1].vrijeme); //zelim ispisati zadnji racun, znaci samo lista_racuna[lista.count - 1] sto znaci zadnji element u listi
            table.AddRow("Datum", lista_racuna[lista_racuna.Count - 1].datum);
            if (lista_racuna[lista_racuna.Count - 1].gorivo_id == "111") //da zna koje gorivo ispisat
                table.AddRow("Vrsta goriva", "diesel");
            else
                table.AddRow("Vrsta goriva", "benzin");
            table.AddRow("Kolicina", lista_racuna[lista_racuna.Count - 1].kolicina + " litara");
            table.AddRow("Cijena po litri", lista_racuna[lista_racuna.Count - 1].cijena_L + " HRK");
            table.AddRow("Cijena", lista_racuna[lista_racuna.Count - 1].cijena + " HRK");
            table.AddRow("PDV", lista_racuna[lista_racuna.Count - 1].iznos_PDV);
            table.AddRow("Cijena uz PDV", lista_racuna[lista_racuna.Count - 1].cijena_PDV + " HRK");
            foreach (XmlNode node in ListXmlVP) //idemo po listi za vrstu_placanja
            {
                if (lista_racuna[lista_racuna.Count - 1].vrsta_placanja == node.Attributes["id"].Value) //ako je vrsta placanja id od zadnjeg racuna == nekom u listi vrsta placanja
                    table.AddRow("Vrsta placanja", node.Attributes["naziv"].Value); // onda ispisi naziv te vrste placanja iz liste vrsta placanja
            }
            foreach (XmlNode node in openNodes) // i na kraju ide po listi zaposelnika i gleda kojem id iz zadnjeg racuna odgovara u listi i ispisi ime i prezime
            {
                if (lista_racuna[lista_racuna.Count - 1].zaposlenik_id == node.Attributes["id"].Value)
                    table.AddRow("Zaposlenik", node.Attributes["ime"].Value + " " + node.Attributes["prezime"].Value);
            }
            table.Write(Format.MarkDown);


            string novo_json = JsonConvert.SerializeObject(lista_racuna.ToArray(), Newtonsoft.Json.Formatting.Indented); //isto ko i prije tu novu listu sa novo svim starim racunima + novo dodanim racunom serijaliziram u string 
            File.WriteAllText(@"C:\Users\Gratisfaction\source\repos\benz_crpka2\racuni.json", novo_json); // i prebrisat datoteku racuni.json sa novim stringom
            Loger("Izdan racun");

        }

        static void Azuriranje_spremnika2(string string_json2, double gorivo, int odabir) //to se desava nakon izrade novog racuna //-----------------------4----------------------------
        {
            List<Spremnik> spremnici = new List<Spremnik>();
            spremnici = JsonConvert.DeserializeObject<List<Spremnik>>(string_json2);
            double trenutno_stanje_diesel = 0;
            double trenutno_stanje_benzin = 0;
            double trenutna_cijena_diesel = 0;
            double trenutna_cijena_benzin = 0;
            foreach (Spremnik spremnik in spremnici)
            {
                if (spremnik.spremnik_id == "111")
                {
                    trenutno_stanje_diesel = spremnik.stanje;
                    trenutna_cijena_diesel = spremnik.cijena_goriva;
                }
                else
                {
                    trenutno_stanje_benzin = spremnik.stanje;
                    trenutna_cijena_benzin = spremnik.cijena_goriva;
                }
            }

            List<Spremnik> azurirano = new List<Spremnik>();
            double novo_stanje;
            if (odabir == 1)
            {
                novo_stanje = trenutno_stanje_diesel - gorivo;

                azurirano.Add(new Spremnik()
                {
                    spremnik_id = "111",
                    vrsta_spremnika = "diesel",
                    stanje = novo_stanje,
                    cijena_goriva = trenutna_cijena_diesel
                });
                azurirano.Add(new Spremnik()
                {
                    spremnik_id = "112",
                    vrsta_spremnika = "benzin",
                    stanje = trenutno_stanje_benzin,
                    cijena_goriva = trenutna_cijena_benzin
                });
            }
            else
            {
                novo_stanje = trenutno_stanje_benzin - gorivo;

                azurirano.Add(new Spremnik()
                {
                    spremnik_id = "111",
                    vrsta_spremnika = "diesel",
                    stanje = trenutno_stanje_diesel,
                    cijena_goriva = trenutna_cijena_diesel
                });
                azurirano.Add(new Spremnik()
                {
                    spremnik_id = "112",
                    vrsta_spremnika = "benzin",
                    stanje = novo_stanje,
                    cijena_goriva = trenutna_cijena_benzin
                });
            }

            string novo_json = JsonConvert.SerializeObject(azurirano.ToArray(), Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(@"C:\Users\Gratisfaction\source\repos\benz_crpka2\spremnici.json", novo_json);
            Loger("Spremnik azuriran nakon izdaje racuna");
        }

        static void Statistika(string string_json, string string_xml) //-----------------------5----------------------------
        {
            Loger("Pregled Statistike");
            Console.WriteLine("********** STATISTIKA PRODANOG GORIVA **********");
            Console.WriteLine("\nOdabir:");
            Console.WriteLine("\t1 - Ukupna kolicina prodanog diesela i ukupni prihod od prodanog diesela");
            Console.WriteLine("\t2 - Ukupna kolicina prodanog benzina i ukupni prihod od prodanog benzina");
            Console.WriteLine("\t3 - Zaposelnici sortirani prema kolicini prodanog goriva");           
            int odluka;
            while (true)
            {
                Console.Write("\nOdaberite opciju: ");
                odluka = Convert.ToInt32(Console.ReadLine());
                if (odluka < 1 || odluka > 3)
                {
                    Console.WriteLine("Pogresan unos!");
                }
                else
                    break;
            }
   
            List<Racun> racuni = new List<Racun>();
            racuni = JsonConvert.DeserializeObject<List<Racun>>(string_json);

            XmlDocument OpenXml = new XmlDocument();
            OpenXml.LoadXml(string_xml);
            XmlNodeList openNodes = OpenXml.SelectNodes("//data/zaposlenik");

            double uk_prihod_diesela = 0;
            double uk_prihod_benzina = 0;
            double uk_kolicina_diesela = 0;
            double uk_kolicina_benzina = 0;
            Console.WriteLine();
            
            if (odluka == 1)
            {
                var table = new ConsoleTable("Prodajna statistika", "Diesel");
                foreach (Racun racun in racuni)
                {
                    if (racun.gorivo_id == "111")
                    {
                        uk_kolicina_diesela += racun.kolicina;
                        uk_prihod_diesela += racun.cijena;
                    }
                }
                table.AddRow("Ukupna prodana kolicina", uk_kolicina_diesela + " litara");
                table.AddRow("Ukupni prihod prodaje", uk_prihod_diesela + " HRK");
                table.Write(Format.Alternative);
            }
            else if (odluka == 2)
            {
                var table = new ConsoleTable("Prodajna statistika", "Benzin");
                foreach (Racun racun in racuni)
                {
                    if (racun.gorivo_id == "112")
                    {
                        uk_kolicina_benzina += racun.kolicina;
                        uk_prihod_benzina += racun.cijena;
                    }
                }
                table.AddRow("Ukupna prodana kolicina", uk_kolicina_benzina + " litara");
                table.AddRow("Ukupni prihod prodaje", uk_prihod_benzina + " HRK");
                table.Write(Format.Alternative);
            }
            else
            {
                // sortiranje zaposlenika:
                List<string> zaposlenici_ids = new List<string>(); //tu cu spremit sve id-eove zaposlenika
                List<double> prodane_kolicine = new List<double>(); //tu ce bit sume prodanog goriva. logika je ta da ce 1. suma odgovarat 1. id-u iz zaposlenici_ids a druga drugom id itd... 
                foreach(XmlNode node in openNodes)
                {
                    zaposlenici_ids.Add(node.Attributes["id"].Value); //dodajem id u listu_ids za svaki node/zasposlenik --> [id1, id2, id3]
                    prodane_kolicine.Add(0); //dodajem nule. U listu prodane_kolicine ce doci onolko nula koliko ima zaposelnika --> [0, 0, 0]
                }

                for (int i = 0; i < zaposlenici_ids.Count; i++) //vanjska petlja: ide po svim id-evima/zaposlenicima
                {
                    foreach (Racun racun in racuni) //untarnja: ide po racunima  
                    {
                        if (racun.zaposlenik_id == zaposlenici_ids[i]) // i kad nadje da id od racuna odgovara id-u iz liste[i]
                        {
                            prodane_kolicine[i] += racun.kolicina; // nadoda sumi[i] kolicinu iz racuna
                        }
                    }
                }
                //Console.WriteLine("prije sortiranja:");                                                          //
                //Console.WriteLine(zaposlenici_ids[0] + "   " + zaposlenici_ids[1] + "   " + zaposlenici_ids[2]); //  provjera da l isortiranje radi. racunaj kolicinu za svaki id
                //Console.WriteLine(prodane_kolicine[0] + " " + prodane_kolicine[1] + " " + prodane_kolicine[2]);  //

                for (int i = 0; i < zaposlenici_ids.Count; i++) //BUBBLE SORT - sortiram kolicine ali na isti nacin prebacujem zaposlenici_id da ne izgubim povezanost
                {
                    for (int j = 0; j < prodane_kolicine.Count - i - 1; j++)
                    {
                        if (prodane_kolicine[j] < prodane_kolicine[j + 1]) //sortiram silazno, ako je trenutni elem. manji od sljedeceg --> zamjeni im mjesta
                        {
                            //zamjena temp i lista[i]
                            double temp1 = prodane_kolicine[j];
                            string temp2 = zaposlenici_ids[j];
                            prodane_kolicine[j] = prodane_kolicine[j + 1];
                            prodane_kolicine[j + 1] = temp1;
                            zaposlenici_ids[j] = zaposlenici_ids[j + 1];
                            zaposlenici_ids[j + 1] = temp2;
                        }
                    }
                }
                //Console.WriteLine("poslje sortiranja:");                                                          //
                //Console.WriteLine(zaposlenici_ids[0] + "   " + zaposlenici_ids[1] + "   " + zaposlenici_ids[2]);  // 
                //Console.WriteLine(prodane_kolicine[0] + " " + prodane_kolicine[1] + " " + prodane_kolicine[2]);   //

                Console.Write("\n");
                var table = new ConsoleTable("Zaposlenik", "Prodana kolicina");
                for (int i = 0; i < zaposlenici_ids.Count; i++) //ispis na konzolu
                {
                    foreach (XmlNode node in openNodes)
                    {
                        if (node.Attributes["id"].Value == zaposlenici_ids[i])
                        {
                            table.AddRow(node.Attributes["ime"].Value.Substring(0,1) + ". " + node.Attributes["prezime"].Value, prodane_kolicine[i] + " litara");
                        }
                    }
                }
                table.Write(Format.Alternative);
            }

        }

        static void Pritisni_tipku(string zaposlenik_id)
        {
            Console.WriteLine("\n\nPritisni tipku 'Enter' za povratak u glavni izbornik ili tipku 'Esc' za prekid programa.");
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKey tipka = Console.ReadKey().Key;
                    if(tipka == ConsoleKey.Enter)
                    {
                        Loger("Povratak u glavni izbornik");
                        Izbornik(zaposlenik_id);
                        break;
                    }
                    if (tipka == ConsoleKey.Escape)
                    {
                        Loger("Odjava");
                        break;
                    }
                }
            }
        }

        static void Izbornik(string zaposlenik_id) 
        {            
            Console.WriteLine("********** GLAVNI IZBORNIK **********");
            Console.WriteLine("\n\t1 - Pregled racuna\n");
            Console.WriteLine("\t2 - Stanje u spremnicima i trenutna cijena\n");
            Console.WriteLine("\t3 - Azuziraj stanje spremnika ili cijene goriva\n");
            Console.WriteLine("\t4 - Izradi racun\n");
            Console.WriteLine("\t5 - Statistika prodanog goriva\n");
            Console.WriteLine("\t6 - Odjava");
            Console.Write("\nOdaberite opciju: ");
            int odabir = Convert.ToInt32(Console.ReadLine());

            StreamReader sr = new StreamReader("racuni.json");
            string string_json = sr.ReadToEnd();
            sr.Close();
            StreamReader sr2 = new StreamReader("spremnici.json");
            string string_json2 = sr2.ReadToEnd();
            sr2.Close();
            StreamReader sr3 = new StreamReader("vrste_placanja.xml");
            string string_xml = sr3.ReadToEnd();
            sr3.Close();

            StreamReader sr4 = new StreamReader("config.xml");
            string string_xml2 = sr4.ReadToEnd();
            sr4.Close();

            switch (odabir)
            {
                case 1:
                    Ispis_racuna(string_json, string_json2, string_xml2, string_xml);
                    Pritisni_tipku(zaposlenik_id);
                    break;
                case 2:                   
                    StanjeSpremniciGorivo(string_json2);
                    Pritisni_tipku(zaposlenik_id);
                    break;
                case 3:
                    Ispis_spremnici(string_json2);
                    Pritisni_tipku(zaposlenik_id);
                    break;
                case 4:
                    Izrada_racuna(string_json, string_json2, string_xml, zaposlenik_id);
                    Pritisni_tipku(zaposlenik_id);
                    break;
                case 5:
                    Statistika(string_json, string_xml2);
                    Pritisni_tipku(zaposlenik_id);
                    break;
                case 6:
                    break;
                default:
                    Console.WriteLine("Pogresan unos!");
                    Izbornik(zaposlenik_id);
                    break;
            }

        }

        static void Main(string[] args) 
        {
            Loger("Pocetak programa");
            StreamReader sr = new StreamReader("config.xml");
            string string_xml = sr.ReadToEnd();
            sr.Close();
            string zaposlenik_id = Prijava(string_xml);
            Loger("Glavni izbornik");
            Izbornik(zaposlenik_id);
            
            Console.WriteLine("\nOdjavljivanje...");
            Loger("Kraj programa\n");
            Thread.Sleep(750);
        }
    }
}
