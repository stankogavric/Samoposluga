using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace sql1
{
    class Program
    {
        public static Random random = new Random();

        static void Main(string[] args)
        {
            string connectionString = "Data Source=localhost;Initial Catalog=samoposluga;User ID=root;Password=";
            MySqlConnection conn = new MySqlConnection(connectionString);
            conn.Open();

            MySqlCommand command = conn.CreateCommand();
            #region kreiranje tabela
            napraviTabele(command);
            #endregion

            #region ubacivanje podataka u tabele
            popuniTabele(command);
            #endregion

            #region select
            SelectUpiti(command);
            #endregion
            conn.Close();
        }

        private static void SelectUpiti(MySqlCommand command)
        {

            Console.WriteLine("1.Koliki je ukupan profit od nastanka kompanije?");
            command.CommandText = "SELECT (SUM(detaljiracuna.kolicina*(detaljiracuna.cijena - detaljiracuna.cijenaDobavljaca)) - " +
                "SUM(detaljiracuna.kolicina*detaljiracuna.cijena)*0.2 - ((SELECT SUM(menadžeri.plata) FROM menadžeri) + " +
                "(SELECT SUM(zaposleni.plata)FROM zaposleni))*TIMESTAMPDIFF(MONTH, '2010-10-10', NOW()) - " +
                "(SELECT SUM(ukradeniproizvodi.kolicina * ukradeniproizvodi.cijenaDobavljaca) FROM ukradeniproizvodi)) AS profit FROM detaljiracuna";
            System.Data.DataSet ubb = executeSelect(command);

            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine(ubb.Tables[0].Rows[podaci]["profit"]);
            }

            Console.WriteLine();
            Console.WriteLine("2.Kojim proizvodima je opao profit za 5% ili više u odnosu na prethodnu godinu?");
            command.CommandText = "SELECT proizvodi.naziv, proizvodi.idProizvoda FROM proizvodi, racun, detaljiracuna, proizvodisamoposluge " +
                "WHERE detaljiracuna.idRacuna = racun.idRacuna AND detaljiracuna.idProizvodiSamoposluge = " +
                "proizvodisamoposluge.idProizvodiSamoposluge AND proizvodisamoposluge.idProizvoda = proizvodi.idProizvoda " +
                "AND (SELECT SUM((detaljiracuna.cijena - detaljiracuna.cijenaDobavljaca) * detaljiracuna.kolicina) " +
                "FROM proizvodisamoposluge, proizvodi, racun WHERE detaljiracuna.idProizvodiSamoposluge = " +
                "proizvodisamoposluge.idProizvodiSamoposluge AND proizvodisamoposluge.idProizvoda = proizvodi.idProizvoda " +
                "AND detaljiracuna.idRacuna = racun.idRacuna AND racun.datum BETWEEN DATE_SUB(NOW(), INTERVAL 2 YEAR) " +
                "AND DATE_SUB(NOW(), INTERVAL 1 YEAR) GROUP BY proizvodi.idProizvoda) > (SELECT(SUM((detaljiracuna.cijena - " +
                "detaljiracuna.cijenaDobavljaca) * detaljiracuna.kolicina)) * 1.05 " +
                "WHERE detaljiracuna.idProizvodiSamoposluge = proizvodisamoposluge.idProizvodiSamoposluge " +
                "AND proizvodisamoposluge.idProizvoda = proizvodi.idProizvoda AND detaljiracuna.idRacuna = racun.idRacuna " +
                "AND racun.datum >= DATE_SUB(NOW(), INTERVAL 1 YEAR) GROUP BY proizvodi.idProizvoda) GROUP BY proizvodi.idProizvoda";
            ubb = executeSelect(command);
            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine(ubb.Tables[0].Rows[podaci]["naziv"] + " " + ubb.Tables[0].Rows[podaci]["idProizvoda"]);
            }

            Console.WriteLine();
            Console.WriteLine("3.Koja kategorija proizvoda donosi najmanji profit?");
            command.CommandText = "SELECT kategorijeproizvoda.naziv, SUM(detaljiracuna.kolicina*(detaljiracuna.cijena - " +
                "detaljiracuna.cijenaDobavljaca)) AS profit FROM kategorijeproizvoda,detaljiracuna,proizvodi,proizvodisamoposluge " +
                "WHERE proizvodisamoposluge.idProizvodiSamoposluge=detaljiracuna.idProizvodiSamoposluge " +
                "AND proizvodi.idProizvoda=proizvodisamoposluge.idProizvoda " +
                "AND kategorijeproizvoda.idKategorijeProizvoda = proizvodi.idKategorijeProizvoda " +
                "GROUP BY kategorijeproizvoda.naziv ORDER BY profit LIMIT 1";
            ubb = executeSelect(command);
            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine(ubb.Tables[0].Rows[podaci]["naziv"] + " " + ubb.Tables[0].Rows[podaci]["profit"]);
            }

            Console.WriteLine();
            Console.WriteLine("4. Najneprofitabilniji dan u nedjelji na mjesečnom nivou?");
            command.CommandText = "SELECT DAYNAME(racun.datum) as dan, SUM(detaljiracuna.kolicina*(detaljiracuna.cijena - " +
                "detaljiracuna.cijenaDobavljaca)) - SUM(detaljiracuna.kolicina*detaljiracuna.cijena)*0.2 AS profit " +
                "FROM racun, detaljiracuna WHERE racun.datum >= DATE_SUB(NOW(),INTERVAL 1 MONTH) " +
                "AND detaljiracuna.idRacuna = racun.idRacuna GROUP BY dan ORDER BY profit LIMIT 1";
            ubb = executeSelect(command);
            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine(ubb.Tables[0].Rows[podaci]["dan"] + " " + ubb.Tables[0].Rows[podaci]["profit"]);
            }

            Console.WriteLine();
            Console.WriteLine("5. U kojoj opštini opada profit u odnosu na prethodnu godinu?");
            command.CommandText = "SELECT samoposluge.opština AS opstina FROM samoposluge " +
                "WHERE (SELECT SUM(detaljiracuna.kolicina*(detaljiracuna.cijena-detaljiracuna.cijenaDobavljaca)) - " +
                "SUM((detaljiracuna.kolicina*detaljiracuna.cijena)*0.2) - IFNULL((ukradeniproizvodi.kolicina*ukradeniproizvodi.cijenaDobavljaca),0) " +
                "FROM proizvodisamoposluge, detaljiracuna, racun, proizvodi LEFT JOIN ukradeniproizvodi " +
                "ON proizvodi.idProizvoda = ukradeniproizvodi.idProizvoda WHERE racun.idRacuna = detaljiracuna.idRacuna " +
                "AND detaljiracuna.idProizvodiSamoposluge = proizvodisamoposluge.idProizvodiSamoposluge " +
                "AND proizvodisamoposluge.idProizvoda = proizvodi.idProizvoda AND proizvodisamoposluge.idSamoposluge = samoposluge.idSamoposluge " +
                "AND racun.datum BETWEEN DATE_SUB(NOW(), INTERVAL 2 YEAR) AND DATE_SUB(NOW(), INTERVAL 1 YEAR) " +
                "GROUP BY samoposluge.opština) >= (SELECT SUM(detaljiracuna.kolicina * (detaljiracuna.cijena - detaljiracuna.cijenaDobavljaca)) - " +
                "SUM((detaljiracuna.kolicina * detaljiracuna.cijena) * 0.2) - IFNULL((ukradeniproizvodi.kolicina * ukradeniproizvodi.cijenaDobavljaca), 0) " +
                "FROM proizvodisamoposluge, detaljiracuna, racun, proizvodi LEFT JOIN ukradeniproizvodi ON proizvodi.idProizvoda = " +
                "ukradeniproizvodi.idProizvoda WHERE racun.idRacuna = detaljiracuna.idRacuna AND detaljiracuna.idProizvodiSamoposluge = " +
                "proizvodisamoposluge.idProizvodiSamoposluge AND proizvodisamoposluge.idProizvoda = proizvodi.idProizvoda " +
                "AND proizvodisamoposluge.idSamoposluge = samoposluge.idSamoposluge AND racun.datum >= DATE_SUB(NOW(), INTERVAL 1 YEAR) " +
                "GROUP BY samoposluge.opština)";
            ubb = executeSelect(command);
            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine(ubb.Tables[0].Rows[podaci]["opstina"]);
            }

            Console.WriteLine();
            Console.WriteLine("6. Koji kasiri su prodali broj proizvoda ispod prosjeka?");
            command.CommandText = "SELECT zaposleni.idZaposlenog, zaposleni.ime, zaposleni.prezime, " +
                "SUM(detaljiracuna.kolicina) AS kolicina_prodatih FROM zaposleni, detaljiracuna , racun " +
                "WHERE (SELECT SUM(detaljiracuna.kolicina) FROM racun, detaljiracuna " +
                "WHERE detaljiracuna.idRacuna = racun.idRacuna AND racun.idZaposlenog = zaposleni.idZaposlenog " +
                "GROUP BY zaposleni.idZaposlenog) < (SELECT SUM(kolicina) / (COUNT(DISTINCT(idZaposlenog))) " +
                "FROM racun, detaljiracuna WHERE racun.idRacuna = detaljiracuna.idRacuna) " +
                "AND zaposleni.idZaposlenog = racun.idZaposlenog AND racun.idRacuna = detaljiracuna.idRacuna " +
                "GROUP BY zaposleni.idZaposlenog";
            ubb = executeSelect(command);
            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine(ubb.Tables[0].Rows[podaci]["idZaposlenog"] + " " + ubb.Tables[0].Rows[podaci]["ime"]
                    + " " + ubb.Tables[0].Rows[podaci]["prezime"] + " " + ubb.Tables[0].Rows[podaci]["kolicina_prodatih"]);
            }

            Console.WriteLine();
            Console.WriteLine("7. Koliki je ponderisani prosjek profita za svaku vrstu samoposluga?");
            command.CommandText = "SELECT vrsta, profit - (broj_zaposlenih+broj_menadzera)*25000 AS profiti " +
                "FROM(SELECT vrstesamoposluga.naziv AS vrsta, ((SUM(detaljiracuna.kolicina * (detaljiracuna.cijena - detaljiracuna.cijenaDobavljaca)) - " +
                "SUM(detaljiracuna.kolicina * detaljiracuna.cijena) * 0.2) / COUNT(vrstesamoposluga.idVrsteSamoposluge)) AS profit, " +
                "COUNT(DISTINCT zaposleni.idZaposlenog) AS broj_zaposlenih, COUNT(DISTINCT menadžeri.idMenadžera) AS broj_menadzera " +
                "FROM detaljiracuna, vrstesamoposluga, samoposluge, proizvodisamoposluge, zaposleni, racun, menadžeri " +
                "WHERE vrstesamoposluga.idVrsteSamoposluge = samoposluge.idVrsteSamoposluge AND samoposluge.idSamoposluge = " +
                "proizvodisamoposluge.idSamoposluge AND proizvodisamoposluge.idProizvodiSamoposluge = detaljiracuna.idProizvodiSamoposluge " +
                "AND zaposleni.idZaposlenog = racun.idZaposlenog AND racun.idRacuna = detaljiracuna.idRacuna AND menadžeri.idMenadžera = " +
                "samoposluge.idMenadžera AND racun.datum >= DATE_SUB(NOW(), INTERVAL 1 MONTH) " +
                "GROUP BY vrstesamoposluga.naziv) AS broj ORDER BY profiti DESC";
            ubb = executeSelect(command);
            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine(ubb.Tables[0].Rows[podaci]["vrsta"] + " " + ubb.Tables[0].Rows[podaci]["profiti"]);
            }

            Console.WriteLine();
            Console.WriteLine("8. Koliko iznosi PDV za prošlu godinu?");
            command.CommandText = "SELECT SUM(detaljiracuna.kolicina*(detaljiracuna.cijena))*0.2 AS PDV FROM detaljiracuna, racun " +
                "WHERE racun.idRacuna = detaljiracuna.idRacuna AND racun.datum >= DATE_SUB(NOW(),INTERVAL 1 YEAR)";
            ubb = executeSelect(command);
            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine(ubb.Tables[0].Rows[podaci]["PDV"]);
            }

            Console.WriteLine();
            Console.WriteLine("9. Kojim proizvodima ističe rok trajanja za 14 dana?");
            command.CommandText = "SELECT proizvodi.idProizvoda, proizvodi.naziv FROM proizvodi " +
                "WHERE proizvodi.rokTrajanja <= DATE_ADD(NOW(),INTERVAL 2 WEEK)";
            ubb = executeSelect(command);
            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine(ubb.Tables[0].Rows[podaci]["idProizvoda"] + " " + ubb.Tables[0].Rows[podaci]["naziv"]);
            }

            Console.WriteLine();
            Console.WriteLine("10. Spisak svih menadžera samoposluga čiji je profit opao u odnosu na prethodnu godinu?");
            command.CommandText = "SELECT menadžeri.ime, menadžeri.idMenadžera AS idMenadzera FROM menadžeri " +
                "WHERE (SELECT SUM(detaljiracuna.kolicina*(detaljiracuna.cijena-detaljiracuna.cijenaDobavljaca)) - " +
                "SUM((detaljiracuna.kolicina*detaljiracuna.cijena)*0.2) - IFNULL((ukradeniproizvodi.kolicina*ukradeniproizvodi.cijenaDobavljaca),0) " +
                "FROM samoposluge, proizvodisamoposluge, detaljiracuna, racun, proizvodi LEFT JOIN ukradeniproizvodi ON proizvodi.idProizvoda = " +
                "ukradeniproizvodi.idProizvoda WHERE racun.idRacuna = detaljiracuna.idRacuna AND detaljiracuna.idProizvodiSamoposluge = " +
                "proizvodisamoposluge.idProizvodiSamoposluge AND proizvodisamoposluge.idProizvoda = proizvodi.idProizvoda " +
                "AND samoposluge.idMenadžera = menadžeri.idMenadžera AND proizvodisamoposluge.idSamoposluge = samoposluge.idSamoposluge " +
                "AND racun.datum BETWEEN DATE_SUB(NOW(), INTERVAL 2 YEAR) AND DATE_SUB(NOW(), INTERVAL 1 YEAR) " +
                "GROUP BY menadžeri.idMenadžera) >= (SELECT SUM(detaljiracuna.kolicina * (detaljiracuna.cijena - detaljiracuna.cijenaDobavljaca)) - " +
                "SUM((detaljiracuna.kolicina * detaljiracuna.cijena) * 0.2) - IFNULL((ukradeniproizvodi.kolicina * ukradeniproizvodi.cijenaDobavljaca), 0) " +
                "FROM samoposluge, proizvodisamoposluge, detaljiracuna, racun, proizvodi LEFT JOIN ukradeniproizvodi " +
                "ON proizvodi.idProizvoda = ukradeniproizvodi.idProizvoda WHERE racun.idRacuna = detaljiracuna.idRacuna " +
                "AND detaljiracuna.idProizvodiSamoposluge = proizvodisamoposluge.idProizvodiSamoposluge AND proizvodisamoposluge.idProizvoda = " +
                "proizvodi.idProizvoda AND samoposluge.idMenadžera = menadžeri.idMenadžera AND proizvodisamoposluge.idSamoposluge = " +
                "samoposluge.idSamoposluge AND racun.datum >= DATE_SUB(NOW(), INTERVAL 1 YEAR) GROUP BY menadžeri.idMenadžera)";
            ubb = executeSelect(command);
            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine(ubb.Tables[0].Rows[podaci]["ime"] + " " + ubb.Tables[0].Rows[podaci]["idMenadzera"]);
            }

            Console.WriteLine();
            Console.WriteLine("11. Koji proizvodi nikad nisu prodati?");
            command.CommandText = "SELECT DISTINCT proizvodi.idProizvoda, proizvodi.naziv " +
                "FROM proizvodi, proizvodisamoposluge, detaljiracuna WHERE proizvodi.idProizvoda = proizvodisamoposluge.idProizvoda " +
                "AND proizvodisamoposluge.idProizvodiSamoposluge NOT IN (SELECT detaljiracuna.idProizvodiSamoposluge FROM detaljiracuna)";
            ubb = executeSelect(command);
            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine(ubb.Tables[0].Rows[podaci]["idProizvoda"] + " " + ubb.Tables[0].Rows[podaci]["naziv"]);
            }

            Console.WriteLine();
            Console.WriteLine("12. Kojim proizvodima je prosječna nedjeljna prodaja veća od trenutne količine u zalihama?");
            command.CommandText = "SELECT proizvodi.idProizvoda, SUM(detaljiracuna.kolicina)*0.1 AS prosjek, proizvodi.kolicina " +
                "AS kolicina FROM detaljiracuna, racun, proizvodi, proizvodisamoposluge WHERE racun.idRacuna = detaljiracuna.idRacuna " +
                "AND proizvodi.idProizvoda = proizvodisamoposluge.idProizvoda AND proizvodisamoposluge.idProizvodiSamoposluge = " +
                "detaljiracuna.idProizvodiSamoposluge AND racun.datum >= DATE_SUB(NOW(),INTERVAL 10 WEEK) " +
                "GROUP BY proizvodi.idProizvoda HAVING prosjek >= kolicina";
            ubb = executeSelect(command);
            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine(ubb.Tables[0].Rows[podaci]["idProizvoda"] + " " + ubb.Tables[0].Rows[podaci]["prosjek"]
                    + " " + ubb.Tables[0].Rows[podaci]["kolicina"]);
            }

            Console.WriteLine();
            Console.WriteLine("13. Koliki profit je ostvaren od strane VIP korisnika?");
            command.CommandText = "SELECT SUM(detaljiracuna.kolicina*(detaljiracuna.cijena - detaljiracuna.cijenaDobavljaca)) - " +
                "SUM(detaljiracuna.kolicina * detaljiracuna.cijena) * 0.2 AS profit FROM detaljiracuna, racun " +
                "WHERE detaljiracuna.idRacuna = racun.idRacuna AND racun.brojKartice IS NOT NULL";
            ubb = executeSelect(command);
            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine(ubb.Tables[0].Rows[podaci]["profit"]);
            }

            Console.WriteLine();
            Console.WriteLine("14. Koji VIP korisnik nije aktivan u poslednjih 6 mjeseci?");
            command.CommandText = "SELECT vipkorisnici.ime, vipkorisnici.brojKartice FROM vipkorisnici " +
                "WHERE vipkorisnici.brojKartice NOT IN((SELECT vipkorisnici.brojKartice FROM racun " +
                "WHERE vipkorisnici.brojKartice = racun.brojKartice AND racun.datum >= DATE_SUB(NOW(),INTERVAL 6 MONTH) " +
                "GROUP BY vipkorisnici.brojKartice))";
            ubb = executeSelect(command);
            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine(ubb.Tables[0].Rows[podaci]["ime"] + " " + ubb.Tables[0].Rows[podaci]["brojKartice"]);
            }

            Console.WriteLine();
            Console.WriteLine("15. Koji proizvodi su najprodavaniji koje su kupovali VIP korisnici?");
            command.CommandText = "SELECT proizvodi.idProizvoda, proizvodi.naziv, SUM(detaljiracuna.kolicina) AS kolicina " +
                "FROM vipkorisnici, proizvodi, proizvodisamoposluge, detaljiracuna, racun WHERE detaljiracuna.idRacuna = racun.idRacuna " +
                "AND racun.brojKartice = vipkorisnici.brojKartice AND detaljiracuna.idProizvodiSamoposluge = proizvodisamoposluge.idProizvodiSamoposluge " +
                "AND proizvodisamoposluge.idProizvoda = proizvodi.idProizvoda GROUP BY proizvodi.idProizvoda ORDER BY kolicina DESC";
            ubb = executeSelect(command);
            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine(ubb.Tables[0].Rows[podaci]["idProizvoda"] + " " + ubb.Tables[0].Rows[podaci]["naziv"]
                    + " " + ubb.Tables[0].Rows[podaci]["kolicina"]);
            }

            Console.WriteLine();
            Console.WriteLine("16. Koliki je gubitak (nabavna cijena*količina) od proizvoda kojima je istekao rok trajanja u poslednjih mjesec dana?");
            command.CommandText = "SELECT SUM(proizvodi.kolicina*proizvodi.cijenaDobavljaca) AS gubitak FROM proizvodi WHERE proizvodi.rokTrajanja < NOW()";
            ubb = executeSelect(command);
            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine(ubb.Tables[0].Rows[podaci]["gubitak"]);
            }

            Console.WriteLine();
            Console.WriteLine("17. Koja samoposluga ima najveći pad profita u odnosu na prethodnu godinu?");
            command.CommandText = "SELECT samoposluge.broj, samoposluge.idSamoposluge, samoposluge.opština as opstina FROM samoposluge " +
                "WHERE (SELECT SUM(detaljiracuna.kolicina*(detaljiracuna.cijena-detaljiracuna.cijenaDobavljaca)) - " +
                "SUM((detaljiracuna.kolicina*detaljiracuna.cijena)*0.2) - IFNULL((ukradeniproizvodi.kolicina*ukradeniproizvodi.cijenaDobavljaca),0) " +
                "FROM proizvodisamoposluge, detaljiracuna, racun, proizvodi LEFT JOIN ukradeniproizvodi ON proizvodi.idProizvoda = " +
                "ukradeniproizvodi.idProizvoda WHERE racun.idRacuna = detaljiracuna.idRacuna AND detaljiracuna.idProizvodiSamoposluge = " +
                "proizvodisamoposluge.idProizvodiSamoposluge AND proizvodisamoposluge.idProizvoda = proizvodi.idProizvoda " +
                "AND proizvodisamoposluge.idSamoposluge = samoposluge.idSamoposluge AND racun.datum BETWEEN DATE_SUB(NOW(), INTERVAL 2 YEAR) " +
                "AND DATE_SUB(NOW(), INTERVAL 1 YEAR) GROUP BY samoposluge.idSamoposluge) >= (SELECT SUM(detaljiracuna.kolicina * " +
                "(detaljiracuna.cijena - detaljiracuna.cijenaDobavljaca)) - SUM((detaljiracuna.kolicina * detaljiracuna.cijena) * 0.2) - " +
                "IFNULL((ukradeniproizvodi.kolicina * ukradeniproizvodi.cijenaDobavljaca), 0) FROM proizvodisamoposluge, detaljiracuna, racun, " +
                "proizvodi LEFT JOIN ukradeniproizvodi ON proizvodi.idProizvoda = ukradeniproizvodi.idProizvoda WHERE racun.idRacuna = " +
                "detaljiracuna.idRacuna AND detaljiracuna.idProizvodiSamoposluge = proizvodisamoposluge.idProizvodiSamoposluge " +
                "AND proizvodisamoposluge.idProizvoda = proizvodi.idProizvoda AND proizvodisamoposluge.idSamoposluge = samoposluge.idSamoposluge " +
                "AND racun.datum >= DATE_SUB(NOW(), INTERVAL 1 YEAR) GROUP BY samoposluge.idSamoposluge)";
            ubb = executeSelect(command);
            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine(ubb.Tables[0].Rows[podaci]["broj"] + " " + ubb.Tables[0].Rows[podaci]["idSamoposluge"]
                    + " " + ubb.Tables[0].Rows[podaci]["opstina"]);
            }

            Console.WriteLine();
            Console.WriteLine("18. Koliki je gubitak (nabavna cijena*količina) od ukradenih proizvoda prethodnog mjeseca za svaku samoposlugu?");
            command.CommandText = "SELECT SUM(ukradeniproizvodi.kolicina* ukradeniproizvodi.cijenaDobavljaca) AS gubitak, samoposluge.broj " +
                "FROM ukradeniproizvodi, samoposluge, proizvodi, proizvodisamoposluge WHERE ukradeniproizvodi.idProizvoda = proizvodi.idProizvoda " +
                "AND proizvodi.idProizvoda = proizvodisamoposluge.idProizvoda AND proizvodisamoposluge.idSamoposluge = samoposluge.idSamoposluge " +
                "AND ukradeniproizvodi.datum >= DATE_SUB(NOW(), INTERVAL 1 MONTH) GROUP BY samoposluge.broj ORDER BY gubitak DESC";
            ubb = executeSelect(command);
            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine(ubb.Tables[0].Rows[podaci]["gubitak"] + " " + ubb.Tables[0].Rows[podaci]["broj"]);
            }

            Console.WriteLine();
            Console.WriteLine("19. Koliki je gubitak od ukradenih proizvoda grupisano po kategoriji?");
            command.CommandText = "SELECT SUM(ukradeniproizvodi.kolicina*ukradeniproizvodi.cijenaDobavljaca) AS gubitak, kategorijeproizvoda.naziv " +
                "FROM ukradeniproizvodi, proizvodi, kategorijeproizvoda WHERE ukradeniproizvodi.idProizvoda = proizvodi.idProizvoda " +
                "AND kategorijeproizvoda.idKategorijeProizvoda = proizvodi.idKategorijeProizvoda GROUP BY kategorijeproizvoda.naziv ORDER BY gubitak DESC";
            ubb = executeSelect(command);
            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine(ubb.Tables[0].Rows[podaci]["gubitak"] + " " + ubb.Tables[0].Rows[podaci]["naziv"]);
            }

            Console.WriteLine();
            Console.WriteLine("20. Koliki je gubitak od računa plaćenih karticom?");
            command.CommandText = "SELECT SUM(detaljiracuna.kolicina*detaljiracuna.cijena)*0.03 AS gubitak FROM detaljiracuna, racun, nacinplacanja " +
                "WHERE detaljiracuna.idRacuna = racun.idRacuna AND racun.idNacinaPlacanja = nacinplacanja.idNacinaPlacanja " +
                "AND nacinplacanja.naziv = 'kartica' ";
            ubb = executeSelect(command);
            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine(ubb.Tables[0].Rows[podaci]["gubitak"]);
            }

            Console.ReadKey();
        }

        private static void popuniTabele(MySqlCommand command)
        {
            Random rnd = new Random();
            #region popunjavanje tabela

            //Kategorije proizvoda
            string[] kategorijeProizvoda = { "hrana", "piće", "bijela tehnika", "kozmetika", "igračke", "party program", "škola i kancelarija", "auto program" };
            for (int i = 0; i < kategorijeProizvoda.Length; i++)
            {
                command.CommandText = "INSERT INTO `samoposluga`.`kategorijeproizvoda` (`naziv`) VALUES('" + kategorijeProizvoda[i] + "')";
                executeNonQuery(command);
            }

            //Proizvodi
            double kupovnaCijena;
            double prodajnaCijena;
            for (int i = 0; i < 50; i++)
            {
                kupovnaCijena = rnd.Next(1, 10000);
                prodajnaCijena = kupovnaCijena * 1.40;
                command.CommandText = "INSERT INTO `samoposluga`.`proizvodi` (`naziv`, `prodajnaCijena`, `cijenaDobavljača`, `rokTrajanja`, `idKategorijeProizvoda`, `količina`) VALUES('" + randStr(rnd.Next(3, 14)) + "', '" + prodajnaCijena + "', '" + kupovnaCijena + "', '" + dtfordb(DateTime.Now.AddDays(rnd.Next(1, 365))) + "', '" + rnd.Next(1, 9) + "', '" + rnd.Next(50000) + "')";
                executeNonQuery(command);
            }

            //Vrste samoposluga
            string[] vrsteSamoposluga = { "minimarket", "supermarket", "megamarket", "hipermarket" };
            for (int i = 0; i < vrsteSamoposluga.Length; i++)
            {
                command.CommandText = "INSERT INTO `samoposluga`.`vrstesamoposluga` (`naziv`) VALUES ('" + vrsteSamoposluga[i] + "')";
                executeNonQuery(command);
            }

            //Menadžeri
            string[] imena = { "Marko", "Janko", "Jovana", "Milica", "Todor", "Milovan", "Petar", "Zoran", "Goran", "Dragan", "Jelena", "Ana", "Mia", "Kristina", "Lazar", "Miroslav", "Mitar", "Ilija", "Rajko", "Nedo", "Neda" };
            string[] prezimena = { "Marković", "Janković", "Jović", "Milić", "Todorović", "Milovanović", "Petrović", "Zoranović", "Goranović", "Draganić", "Jelić", "Janić", "Mićić", "Krstić", "Lazarević", "Mikić", "Mirić", "Ilić", "Rajković", "Nedić", "Nikić" };
            for (int i = 0; i < 10; i++)
            {
                command.CommandText = "INSERT INTO `samoposluga`.`menadžeri` (`ime`, `prezime`, `jmbg`, `telefon`, `plata`) VALUES('" + imena[rnd.Next(imena.Length)] + "', '" + prezimena[rnd.Next(prezimena.Length)] + "', '" + randomLong(13) + "', '" + rnd.Next(111111111, 999999999) + "', '" + 25000 + "')";
                executeNonQuery(command);
            }

            //Načini plaćanja
            string[] nacinPlacanja = { "keš", "kartica" };
            for (int i = 0; i < nacinPlacanja.Length; i++)
            {
                command.CommandText = "INSERT INTO `samoposluga`.`načinplaćanja` (`naziv`) VALUES ('" + nacinPlacanja[i] + "')";
                executeNonQuery(command);
            }

            //Zaposleni
            for (int i = 0; i < 20; i++)
            {
                command.CommandText = "INSERT INTO `samoposluga`.`zaposleni` (`ime`, `prezime`, `jmbg`, `telefon`, `plata`) VALUES('" + imena[rnd.Next(imena.Length)] + "', '" + prezimena[rnd.Next(prezimena.Length)] + "', '" + randomLong(13) + "', '" + rnd.Next(111111111, 999999999) + "', '" + 25000 + "')";
                executeNonQuery(command);
            }

            string[] gradovi = { "Beograd","Valjevo","Vranje","Zaječar","Zrenjanin","Jagodina","Kragujevac",
                                "Kraljevo","Kruševac","Leskovac","Loznica","Niš","Novi Pazar", "Novi Sad","Pančevo","Požarevac",
                                "Priština","Smederevo","Sombor","Sremska Mitrovica","Subotica","Užice","Čačak","Šabac","Pirot" };
            //Samoposluge
            for (int i = 0; i < 5; i++)
            {
                command.CommandText = "INSERT INTO `samoposluga`.`samoposluge` (`broj`, `idVrsteSamoposluge`, `idMenadžera`, `opština`) VALUES('" + (i + 1) + "', '" + rnd.Next(1, 4) + "', '" + rnd.Next(1, 10) + "', '" + gradovi[rnd.Next(gradovi.Length)] + "')";
                executeNonQuery(command);
            }

            //VIP korisnici
            for (int i = 0; i < 5; i++)
            {
                command.CommandText = "INSERT INTO `samoposluga`.`vipkorisnici` (`ime`, `prezime`, `jmbg`, `telefon`) VALUES('" + imena[rnd.Next(imena.Length)] + "', '" + prezimena[rnd.Next(prezimena.Length)] + "', '" + randomLong(13) + "', '" + rnd.Next(111111111, 999999999) + "')";
                executeNonQuery(command);
            }

            //Račun
            int placanje;
            for (int i = 0; i < 10; i++)
            {
                placanje = rnd.Next(0, 2);
                if (placanje == 0)
                {
                    command.CommandText = "INSERT INTO `samoposluga`.`racun` (`datum`, `idZaposlenog`, `idNacinaPlacanja`, `brojKartice`) VALUES('" + dtfordb(RandomDay()) + "', '" + rnd.Next(1, 20) + "', '" + rnd.Next(1, 3) + "', '" + rnd.Next(1, 5) + "')";
                }
                else
                {
                    command.CommandText = "INSERT INTO `samoposluga`.`racun` (`datum`, `idZaposlenog`, `idNacinaPlacanja`) VALUES('" + dtfordb(RandomDay()) + "', '" + rnd.Next(1, 20) + "', '" + rnd.Next(1, 3) + "')";
                }
                executeNonQuery(command);
            }
            int[] id = new int[10];
            //ProizvodiSamoposluge
            int idProizvoda;
            for (int i = 0; i < 10; i++)
            {
                idProizvoda = rnd.Next(1, 50);
                command.CommandText = "INSERT INTO `samoposluga`.`proizvodiSamoposluge` (`kolicina`, `idSamoposluge`, `idProizvoda`) VALUES('" + rnd.Next(1, 1000) + "', '" + rnd.Next(1, 5) + "', '" + idProizvoda + "')";
                executeNonQuery(command);
                id[i] = idProizvoda;
            }

            //Ukradeni proizvodi
            for (int i = 0; i < 5; i++)
            {
                command.CommandText = "INSERT INTO `samoposluga`.`ukradeniProizvodi` (`kolicina`, `datum`, `idProizvoda`, `cijenaDobavljaca`) VALUES('" + rnd.Next(1, 10) + "', '" + dtfordb(RandomDay()) + "', '" + id[random.Next(id.Length)] + "', '" + rnd.Next(1, 10000) + "')";
                executeNonQuery(command);
            }

            //Detalji računa
            for (int i = 0; i < 50; i++)
            {
                kupovnaCijena = rnd.Next(1, 10000);
                prodajnaCijena = kupovnaCijena * 1.40;
                command.CommandText = "INSERT INTO samoposluga.`detaljiracuna` (`kolicina`, `cijena`, `idRacuna`, `cijenaDobavljaca`, `idProizvodiSamoposluge`) VALUES('" + rnd.Next(1, 20) + "', '" + prodajnaCijena + "', '" + rnd.Next(1, 10) + "', '" + kupovnaCijena + "', '" + rnd.Next(1, 10) + "')";
                executeNonQuery(command);
            }

            #endregion

        }

        private static void napraviTabele(MySqlCommand command)
        {
            #region uklanjanje tabela

            command.CommandText = "DROP TABLE IF EXISTS `samoposluga`.`ukradeniProizvodi`";
            executeNonQuery(command);

            command.CommandText = "DROP TABLE IF EXISTS `samoposluga`.`detaljiRačuna`";
            executeNonQuery(command);

            command.CommandText = "DROP TABLE IF EXISTS `samoposluga`.`proizvodiSamoposluge`";
            executeNonQuery(command);

            command.CommandText = "DROP TABLE IF EXISTS `samoposluga`.`račun`";
            executeNonQuery(command);

            command.CommandText = "DROP TABLE IF EXISTS `samoposluga`.`načinPlaćanja`";
            executeNonQuery(command);

            command.CommandText = "DROP TABLE IF EXISTS `samoposluga`.`VipKorisnici`";
            executeNonQuery(command);

            command.CommandText = "DROP TABLE IF EXISTS `samoposluga`.`samoposluge`";
            executeNonQuery(command);

            command.CommandText = "DROP TABLE IF EXISTS `samoposluga`.`menadžeri`";
            executeNonQuery(command);

            command.CommandText = "DROP TABLE IF EXISTS `samoposluga`.`vrsteSamoposluga`";
            executeNonQuery(command);

            command.CommandText = "DROP TABLE IF EXISTS `samoposluga`.`zaposleni`";
            executeNonQuery(command);

            command.CommandText = "DROP TABLE IF EXISTS `samoposluga`.`proizvodi`";
            executeNonQuery(command);

            command.CommandText = "DROP TABLE IF EXISTS `samoposluga`.`kategorijeProizvoda`";
            executeNonQuery(command);

            #endregion

            #region kreiranje tabela

            command.CommandText = "CREATE TABLE IF NOT EXISTS `samoposluga`.`kategorijeProizvoda` ("
                + "`idKategorijeProizvoda` INT NOT NULL AUTO_INCREMENT, "
                + "`naziv` VARCHAR(255) NOT NULL, "
                + "PRIMARY KEY(`idKategorijeProizvoda`),"
                + "UNIQUE INDEX `naziv` (`naziv`))";
            executeNonQuery(command);

            command.CommandText = "CREATE TABLE IF NOT EXISTS `samoposluga`.`proizvodi` ("
                + "`idProizvoda` INT NOT NULL AUTO_INCREMENT,"
                + "`naziv` VARCHAR(255) NOT NULL,"
                + "`prodajnaCijena` DECIMAL NOT NULL,"
                + "`cijenaDobavljača` DECIMAL NOT NULL,"
                + "`rokTrajanja` DATE NULL,"
                + "`idKategorijeProizvoda` INT NOT NULL,"
                + "`količina` INT NOT NULL,"
                + "PRIMARY KEY(`idProizvoda`),"
                + "INDEX `fk_proizvodi_kategorijeProizvoda1_idx` (`idKategorijeProizvoda` ASC),"
                + "CONSTRAINT `fk_proizvodi_kategorijeProizvoda1`"
                + "FOREIGN KEY(`idKategorijeProizvoda`)"
                + "REFERENCES `samoposluga`.`kategorijeProizvoda` (`idKategorijeProizvoda`))";
            executeNonQuery(command);

            command.CommandText = "CREATE TABLE IF NOT EXISTS `samoposluga`.`zaposleni` ("
                + "`idZaposlenog` INT NOT NULL AUTO_INCREMENT,"
                + "`ime` VARCHAR(255) NOT NULL,"
                + "`prezime` VARCHAR(255) NOT NULL,"
                + "`jmbg` VARCHAR(255) NOT NULL,"
                + "`telefon` INT NOT NULL,"
                + "`plata` INT NOT NULL,"
                + "PRIMARY KEY(`idZaposlenog`),"
                + "UNIQUE INDEX `jmbg` (`jmbg`))";
            executeNonQuery(command);

            command.CommandText = "CREATE TABLE IF NOT EXISTS `samoposluga`.`vrsteSamoposluga` ("
                + "`idVrsteSamoposluge` INT NOT NULL AUTO_INCREMENT,"
                + "`naziv` VARCHAR(255) NOT NULL,"
                + "PRIMARY KEY(`idVrsteSamoposluge`),"
                + "UNIQUE INDEX `naziv` (`naziv`))";
            executeNonQuery(command);

            command.CommandText = "CREATE TABLE IF NOT EXISTS `samoposluga`.`menadžeri` ("
                + "`idMenadžera` INT NOT NULL AUTO_INCREMENT,"
                + "`ime` VARCHAR(255) NOT NULL,"
                + "`prezime` VARCHAR(255) NOT NULL,"
                + "`jmbg` VARCHAR(255) NOT NULL,"
                + "`telefon` INT NOT NULL,"
                + "`plata` INT NOT NULL,"
                + "PRIMARY KEY(`idMenadžera`),"
                + "UNIQUE INDEX `jmbg` (`jmbg`))";
            executeNonQuery(command);

            command.CommandText = "CREATE TABLE IF NOT EXISTS `samoposluga`.`samoposluge` ("
                + "`idSamoposluge` INT NOT NULL AUTO_INCREMENT,"
                + "`broj` INT NOT NULL,"
                + "`idVrsteSamoposluge` INT NOT NULL,"
                + "`idMenadžera` INT NOT NULL,"
                + "`opština` VARCHAR(255) NOT NULL,"
                + "PRIMARY KEY(`idSamoposluge`),"
                + "UNIQUE INDEX `broj` (`broj`),"
                + "INDEX `fk_samoposluge_vrsteSamoposluga1_idx` (`idVrsteSamoposluge` ASC),"
                + "INDEX `fk_samoposluge_menadžeri1_idx` (`idMenadžera` ASC),"
                + "CONSTRAINT `fk_samoposluge_vrsteSamoposluga1`"
                + "FOREIGN KEY(`idVrsteSamoposluge`)"
                + "REFERENCES `samoposluga`.`vrsteSamoposluga` (`idVrsteSamoposluge`),"
                + "CONSTRAINT `fk_samoposluge_menadžeri1`"
                + "FOREIGN KEY(`idMenadžera`)"
                + "REFERENCES `samoposluga`.`menadžeri` (`idMenadžera`))";
            executeNonQuery(command);

            command.CommandText = "CREATE TABLE IF NOT EXISTS `samoposluga`.`VipKorisnici` ("
                + "`brojKartice` INT NOT NULL AUTO_INCREMENT,"
                + "`ime` VARCHAR(255) NOT NULL,"
                + "`prezime` VARCHAR(255) NOT NULL,"
                + "`jmbg` VARCHAR(255) NOT NULL,"
                + "`telefon` INT NOT NULL,"
                + "PRIMARY KEY(`brojKartice`),"
                + "UNIQUE INDEX `jmbg` (`jmbg`))";
            executeNonQuery(command);

            command.CommandText = "CREATE TABLE IF NOT EXISTS `samoposluga`.`načinPlaćanja` ("
                + "`idNačinaPlaćanja` INT NOT NULL AUTO_INCREMENT,"
                + "`naziv` VARCHAR(255) NOT NULL,"
                + "PRIMARY KEY (`idNačinaPlaćanja`),"
                + "UNIQUE INDEX `naziv` (`naziv`))";
            executeNonQuery(command);

            command.CommandText = "CREATE TABLE IF NOT EXISTS `samoposluga`.`račun` ("
                + "`idRačuna` INT NOT NULL AUTO_INCREMENT,"
                + "`datum` DATE NOT NULL,"
                + "`idZaposlenog` INT NOT NULL,"
                + "`idNačinaPlaćanja` INT NOT NULL,"
                + "`brojKartice` INT NULL,"
                + "PRIMARY KEY(`idRačuna`),"
                + "INDEX `fk_račun_zaposleni1_idx` (`idZaposlenog` ASC),"
                + "INDEX `fk_račun_VipKorisnici1_idx` (`brojKartice` ASC),"
                + "INDEX `fk_račun_načinPlaćanja1_idx` (`idNačinaPlaćanja` ASC),"
                + "CONSTRAINT `fk_račun_zaposleni1`"
                + "FOREIGN KEY(`idZaposlenog`)"
                + "REFERENCES `samoposluga`.`zaposleni` (`idZaposlenog`),"
                + "CONSTRAINT `fk_račun_VipKorisnici1`"
                + "FOREIGN KEY(`brojKartice`)"
                + "REFERENCES `samoposluga`.`VipKorisnici` (`brojKartice`),"
                + "CONSTRAINT `fk_račun_načinPlaćanja1`"
                + "FOREIGN KEY(`idNačinaPlaćanja`)"
                + "REFERENCES `samoposluga`.`načinPlaćanja` (`idNačinaPlaćanja`))";
            executeNonQuery(command);

            command.CommandText = "CREATE TABLE IF NOT EXISTS `samoposluga`.`proizvodiSamoposluge` ("
                  + "`idProizvodiSamoposluge` INT NOT NULL AUTO_INCREMENT,"
                  + "`količina` INT NOT NULL,"
                  + "`idSamoposluge` INT NOT NULL,"
                  + "`idProizvoda` INT NOT NULL,"
                  + "PRIMARY KEY(`idProizvodiSamoposluge`),"
                  + "INDEX `fk_samoposluge_has_proizvodi_proizvodi1_idx` (`idProizvoda` ASC),"
                  + "INDEX `fk_samoposluge_has_proizvodi_samoposluge1_idx` (`idSamoposluge` ASC),"
                  + "CONSTRAINT `fk_samoposluge_has_proizvodi_samoposluge1`"
                  + "FOREIGN KEY(`idSamoposluge`)"
                  + "REFERENCES `samoposluga`.`samoposluge` (`idSamoposluge`),"
                  + "CONSTRAINT `fk_samoposluge_has_proizvodi_proizvodi1`"
                  + "FOREIGN KEY(`idProizvoda`)"
                  + "REFERENCES `samoposluga`.`proizvodi` (`idProizvoda`))";
            executeNonQuery(command);

            command.CommandText = "CREATE TABLE IF NOT EXISTS `samoposluga`.`detaljiRačuna` ("
                  + "`količina` INT NOT NULL,"
                  + "`cijena` DECIMAL NOT NULL,"
                  + "`idRačuna` INT NOT NULL,"
                  + "`cijenaDobavljača` DECIMAL NOT NULL,"
                  + "`idProizvodiSamoposluge` INT NOT NULL,"
                  + "INDEX `fk_detaljiRačuna_račun1_idx` (`idRačuna` ASC),"
                  + "INDEX `fk_detaljiRačuna_proizvodiSamoposluge1_idx` (`idProizvodiSamoposluge` ASC),"
                  + "CONSTRAINT `fk_detaljiRačuna_račun1`"
                  + "FOREIGN KEY(`idRačuna`)"
                  + "REFERENCES `samoposluga`.`račun` (`idRačuna`),"
                  + "CONSTRAINT `fk_detaljiRačuna_proizvodiSamoposluge1`"
                  + "FOREIGN KEY(`idProizvodiSamoposluge`)"
                  + "REFERENCES `samoposluga`.`proizvodiSamoposluge` (`idProizvodiSamoposluge`))";
            executeNonQuery(command);

            command.CommandText = "CREATE TABLE IF NOT EXISTS `samoposluga`.`ukradeniProizvodi` ("
                + "`količina` INT NOT NULL,"
                + "`datum` DATE NOT NULL,"
                + "`idProizvoda` INT NOT NULL,"
                + "`idUkradenogProizvoda` INT NOT NULL AUTO_INCREMENT,"
                + "`cijenaDobavljača` DECIMAL NOT NULL,"
                + "INDEX `fk_ukradeniProizvodi_proizvodi1_idx` (`idProizvoda` ASC),"
                + "PRIMARY KEY(`idUkradenogProizvoda`),"
                + "CONSTRAINT `fk_ukradeniProizvodi_proizvodi1`"
                + "FOREIGN KEY(`idProizvoda`)"
                + "REFERENCES `samoposluga`.`proizvodi` (`idProizvoda`))";
            executeNonQuery(command);

            #endregion

        }

        public static string dtfordb(DateTime dt)
        {
            return dt.Year + "-" + dt.Month + "-" + dt.Day + " " + dt.TimeOfDay;
        }

        public static string randomLong(int length)
        {
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string randStr(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static DateTime RandomDay()
        {
            DateTime start = new DateTime(2010, 10, 10);
            int range = (DateTime.Today - start).Days;
            return start.AddDays(random.Next(range));
        }

        public static void executeNonQuery(MySqlCommand commandsw)
        {
            bool b = false;

            while (!b)
            {
                try
                {
                    if (!commandsw.Connection.Ping() || commandsw.Connection.State == System.Data.ConnectionState.Closed)
                    {
                        commandsw.Connection.Open();
                    }
                    commandsw.ExecuteNonQuery();
                    b = true;
                }
                catch
                {
                    Console.WriteLine(DateTime.Now + "Vrtimo se u petlji upisa u bazu");
                    Console.WriteLine(DateTime.Now + " " + commandsw.CommandText);
                    b = false;
                }
            }
        }

        public static System.Data.DataSet executeSelect(MySqlCommand command)
        {
            bool b = false;
            System.Data.DataSet cas = new System.Data.DataSet();

            while (!b)
            {
                try
                {
                    if (!command.Connection.Ping() || command.Connection.State == System.Data.ConnectionState.Closed)
                    {
                        command.Connection.Open();
                    }
                    MySqlDataAdapter adapter = new MySqlDataAdapter(command.CommandText, command.Connection);
                    adapter.Fill(cas);
                    b = true;
                }
                catch
                {
                    Console.WriteLine(DateTime.Now + "Vrtimo se u petlji SELECT u bazi");
                    Console.WriteLine(DateTime.Now + " " + command.CommandText);
                    b = false;
                }
            }
            return cas;
        }
    }
}