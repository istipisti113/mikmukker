using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AutoKoltsegNyilvantarto
{
    // ===================== MODELLEK =====================

    public class Auto
    {
        public int Id { get; set; }
        public string Rendszam { get; set; }
        public string Marka { get; set; }
        public string Modell { get; set; }
        public int Evjarat { get; set; }
        public string Uzemanyag { get; set; }
        public int Km { get; set; }

        public Auto() { }

        public Auto(string sor)
        {
            var m = sor.Split(';');
            Id = int.Parse(m[0]);
            Rendszam = m[1];
            Marka = m[2];
            Modell = m[3];
            Evjarat = int.Parse(m[4]);
            Uzemanyag = m[5];
            Km = int.Parse(m[6]);
        }

        public string ToCsv() => $"{Id};{Rendszam};{Marka};{Modell};{Evjarat};{Uzemanyag};{Km}";
    }

    public class Koltseg
    {
        public int Id { get; set; }
        public int AutoId { get; set; }
        public DateTime Datum { get; set; }
        public string Kategoria { get; set; }
        public string Megnevezes { get; set; }
        public decimal Osszeg { get; set; }
        public int Km { get; set; }

        public Koltseg() { }

        public Koltseg(string sor)
        {
            var m = sor.Split(';');
            Id = int.Parse(m[0]);
            AutoId = int.Parse(m[1]);
            Datum = DateTime.ParseExact(m[2], "yyyy.MM.dd.", CultureInfo.InvariantCulture);
            Kategoria = m[3];
            Megnevezes = m[4];
            Osszeg = decimal.Parse(m[5]);
            Km = int.Parse(m[6]);
        }

        public string ToCsv() => $"{Id};{AutoId};{Datum:yyyy.MM.dd.};{Kategoria};{Megnevezes};{Osszeg};{Km}";
    }

    // ===================== ADATKEZELŐ =====================

    public static class Adatkezelo
    {
        public static List<Auto> Autok { get; private set; } = new List<Auto>();
        public static List<Koltseg> Koltsegek { get; private set; } = new List<Koltseg>();

        private const string AutoFile = "autok.csv";
        private const string KoltsegFile = "koltsegek.csv";

        public static void Betolt()
        {
            Autok.Clear();
            Koltsegek.Clear();

            if (File.Exists(AutoFile))
            {
                var sorok = File.ReadAllLines(AutoFile);
                for (int i = 1; i < sorok.Length; i++)
                    if (!string.IsNullOrWhiteSpace(sorok[i]))
                        Autok.Add(new Auto(sorok[i]));
            }

            if (File.Exists(KoltsegFile))
            {
                var sorok = File.ReadAllLines(KoltsegFile);
                for (int i = 1; i < sorok.Length; i++)
                    if (!string.IsNullOrWhiteSpace(sorok[i]))
                        Koltsegek.Add(new Koltseg(sorok[i]));
            }
        }

        public static void Ment()
        {
            var autoLines = new List<string> { "Id;Rendszám;Márka;Modell;Évjárat;Üzemanyag;Km" };
            autoLines.AddRange(Autok.Select(a => a.ToCsv()));
            File.WriteAllLines(AutoFile, autoLines);

            var koltsegLines = new List<string> { "Id;AutoId;Dátum;Kategória;Megnevezés;Összeg;Km" };
            koltsegLines.AddRange(Koltsegek.Select(k => k.ToCsv()));
            File.WriteAllLines(KoltsegFile, koltsegLines);
        }

        public static int KovetkezoAutoId() => Autok.Count == 0 ? 1 : Autok.Max(a => a.Id) + 1;
        public static int KovetkezoKoltsegId() => Koltsegek.Count == 0 ? 1 : Koltsegek.Max(k => k.Id) + 1;

        public static decimal AutoOsszKoltseg(int autoId) =>
            Koltsegek.Where(k => k.AutoId == autoId).Sum(k => k.Osszeg);

        public static List<Koltseg> AutoKoltsegei(int autoId) =>
            Koltsegek.Where(k => k.AutoId == autoId).OrderBy(k => k.Datum).ToList();

        public static int AutoLegnagyobbKm(int autoId)
        {
            var auto = Autok.FirstOrDefault(a => a.Id == autoId);
            if (auto == null) return 0;
            int maxKoltsegKm = Koltsegek.Where(k => k.AutoId == autoId)
                                        .Select(k => k.Km)
                                        .DefaultIfEmpty(0)
                                        .Max();
            return Math.Max(auto.Km, maxKoltsegKm);
        }
    }

    // ===================== AUTÓ FELVÉTEL ŰRLAP =====================

    public class AutoForm : Form
    {
        private TextBox txtRendszam, txtMarka, txtModell, txtEvjarat, txtKm;
        private ComboBox cmbUzemanyag;

        public AutoForm()
        {
            Text = "Új autó felvétele";
            Width = 400; Height = 380;
            StartPosition = FormStartPosition.CenterParent;

            var lbl1 = new Label { Text = "Rendszám:", Left = 20, Top = 20, Width = 100 };
            txtRendszam = new TextBox { Left = 130, Top = 20, Width = 220 };

            var lbl2 = new Label { Text = "Márka:", Left = 20, Top = 60, Width = 100 };
            txtMarka = new TextBox { Left = 130, Top = 60, Width = 220 };

            var lbl3 = new Label { Text = "Modell:", Left = 20, Top = 100, Width = 100 };
            txtModell = new TextBox { Left = 130, Top = 100, Width = 220 };

            var lbl4 = new Label { Text = "Évjárat:", Left = 20, Top = 140, Width = 100 };
            txtEvjarat = new TextBox { Left = 130, Top = 140, Width = 220 };

            var lbl5 = new Label { Text = "Üzemanyag:", Left = 20, Top = 180, Width = 100 };
            cmbUzemanyag = new ComboBox { Left = 130, Top = 180, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbUzemanyag.Items.AddRange(new object[] { "Benzin", "Dízel", "Elektromos", "Hibrid", "Gáz" });
            cmbUzemanyag.SelectedIndex = 0;

            var lbl6 = new Label { Text = "Kilométer:", Left = 20, Top = 220, Width = 100 };
            txtKm = new TextBox { Left = 130, Top = 220, Width = 220 };

            var btnMent = new Button { Text = "Mentés", Left = 130, Top = 270, Width = 100 };
            btnMent.Click += BtnMent_Click;

            var btnMegse = new Button { Text = "Mégse", Left = 250, Top = 270, Width = 100 };
            btnMegse.Click += (s, e) => Close();

            Controls.AddRange(new Control[] {
                lbl1, txtRendszam, lbl2, txtMarka, lbl3, txtModell,
                lbl4, txtEvjarat, lbl5, cmbUzemanyag, lbl6, txtKm,
                btnMent, btnMegse
            });
        }

        private void BtnMent_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRendszam.Text) ||
                string.IsNullOrWhiteSpace(txtMarka.Text) ||
                string.IsNullOrWhiteSpace(txtModell.Text) ||
                string.IsNullOrWhiteSpace(txtEvjarat.Text) ||
                string.IsNullOrWhiteSpace(txtKm.Text))
            {
                MessageBox.Show("Minden mező kitöltése kötelező!");
                return;
            }

            if (!int.TryParse(txtEvjarat.Text, out int evjarat) || evjarat < 1900 || evjarat > DateTime.Now.Year + 1)
            {
                MessageBox.Show("Az évjárat nem megfelelő (1900 és " + (DateTime.Now.Year + 1) + " között)!");
                return;
            }

            if (!int.TryParse(txtKm.Text, out int km) || km < 0)
            {
                MessageBox.Show("A kilométer nem lehet negatív, és számnak kell lennie!");
                return;
            }

            string rendszam = txtRendszam.Text.Trim().ToUpper();
            if (Adatkezelo.Autok.Any(a => a.Rendszam.Equals(rendszam, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Ez a rendszám már szerepel a nyilvántartásban!");
                return;
            }

            var uj = new Auto
            {
                Id = Adatkezelo.KovetkezoAutoId(),
                Rendszam = rendszam,
                Marka = txtMarka.Text.Trim(),
                Modell = txtModell.Text.Trim(),
                Evjarat = evjarat,
                Uzemanyag = cmbUzemanyag.SelectedItem.ToString(),
                Km = km
            };

            Adatkezelo.Autok.Add(uj);
            Adatkezelo.Ment();
            MessageBox.Show("Autó sikeresen mentve!");
            Close();
        }
    }

    // ===================== KÖLTSÉG FELVÉTEL ŰRLAP =====================

    public class KoltsegForm : Form
    {
        private Auto _auto;
        private DateTimePicker dtpDatum;
        private ComboBox cmbKategoria;
        private TextBox txtMegnevezes, txtOsszeg, txtKm;

        public KoltsegForm(Auto auto)
        {
            _auto = auto;
            Text = $"Új költség - {auto.Rendszam}";
            Width = 400; Height = 380;
            StartPosition = FormStartPosition.CenterParent;

            var lbl0 = new Label { Text = $"Autó: {auto.Marka} {auto.Modell} ({auto.Rendszam})", Left = 20, Top = 10, Width = 340, Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold) };

            var lbl1 = new Label { Text = "Dátum:", Left = 20, Top = 50, Width = 100 };
            dtpDatum = new DateTimePicker { Left = 130, Top = 50, Width = 220, Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy.MM.dd." };

            var lbl2 = new Label { Text = "Kategória:", Left = 20, Top = 90, Width = 100 };
            cmbKategoria = new ComboBox { Left = 130, Top = 90, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbKategoria.Items.AddRange(new object[] {
                "Üzemanyag", "Szerviz", "Gumiabroncs", "Biztosítás",
                "Adó", "Műszaki vizsga", "Javítás", "Egyéb"
            });
            cmbKategoria.SelectedIndex = 0;

            var lbl3 = new Label { Text = "Megnevezés:", Left = 20, Top = 130, Width = 100 };
            txtMegnevezes = new TextBox { Left = 130, Top = 130, Width = 220 };

            var lbl4 = new Label { Text = "Összeg (Ft):", Left = 20, Top = 170, Width = 100 };
            txtOsszeg = new TextBox { Left = 130, Top = 170, Width = 220 };

            var lbl5 = new Label { Text = "Km óra állás:", Left = 20, Top = 210, Width = 100 };
            txtKm = new TextBox { Left = 130, Top = 210, Width = 220 };
            txtKm.Text = Adatkezelo.AutoLegnagyobbKm(auto.Id).ToString();

            var btnMent = new Button { Text = "Mentés", Left = 130, Top = 260, Width = 100 };
            btnMent.Click += BtnMent_Click;

            var btnMegse = new Button { Text = "Mégse", Left = 250, Top = 260, Width = 100 };
            btnMegse.Click += (s, e) => Close();

            Controls.AddRange(new Control[] {
                lbl0, lbl1, dtpDatum, lbl2, cmbKategoria, lbl3, txtMegnevezes,
                lbl4, txtOsszeg, lbl5, txtKm, btnMent, btnMegse
            });
        }

        private void BtnMent_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMegnevezes.Text) ||
                string.IsNullOrWhiteSpace(txtOsszeg.Text) ||
                string.IsNullOrWhiteSpace(txtKm.Text))
            {
                MessageBox.Show("Minden mező kitöltése kötelező!");
                return;
            }

            if (!decimal.TryParse(txtOsszeg.Text, out decimal osszeg) || osszeg <= 0)
            {
                MessageBox.Show("Az összeg csak pozitív szám lehet!");
                return;
            }

            if (!int.TryParse(txtKm.Text, out int km) || km < 0)
            {
                MessageBox.Show("A kilométeróra-állás nem negatív szám kell legyen!");
                return;
            }

            int minKm = Adatkezelo.AutoLegnagyobbKm(_auto.Id);
            if (km < minKm)
            {
                MessageBox.Show($"A kilométeróra-állás nem lehet kisebb, mint a korábban rögzített érték ({minKm} km)!");
                return;
            }

            var uj = new Koltseg
            {
                Id = Adatkezelo.KovetkezoKoltsegId(),
                AutoId = _auto.Id,
                Datum = dtpDatum.Value.Date,
                Kategoria = cmbKategoria.SelectedItem.ToString(),
                Megnevezes = txtMegnevezes.Text.Trim(),
                Osszeg = osszeg,
                Km = km
            };

            Adatkezelo.Koltsegek.Add(uj);

            // Frissítjük az autó km-ét, ha nagyobb
            if (km > _auto.Km) _auto.Km = km;

            Adatkezelo.Ment();
            MessageBox.Show("Költség sikeresen mentve!");
            Close();
        }
    }

    // ===================== FŐABLAK =====================

    public class MainForm : Form
    {
        private DataGridView dgvAutok, dgvKoltsegek;
        private TextBox txtKeres;
        private ComboBox cmbSzuro;
        private Label lblOsszesito;

        public MainForm()
        {
            Text = "Autófenntartási Nyilvántartó";
            Width = 1150; Height = 700;
            StartPosition = FormStartPosition.CenterScreen;

            dgvAutok = new DataGridView
            {
                Left = 10, Top = 50, Width = 540, Height = 280,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false
            };
            dgvAutok.SelectionChanged += (s, e) => FrissitKoltsegek();

            dgvKoltsegek = new DataGridView
            {
                Left = 560, Top = 50, Width = 560, Height = 280,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false
            };

            var btnAutoHozzaad = new Button { Text = "Új autó", Left = 10, Top = 10, Width = 110 };
            btnAutoHozzaad.Click += (s, e) => { new AutoForm().ShowDialog(); FrissitAutok(); };

            var btnAutoTorol = new Button { Text = "Autó törlése", Left = 130, Top = 10, Width = 110 };
            btnAutoTorol.Click += BtnAutoTorol_Click;

            var btnKoltsegHozzaad = new Button { Text = "Új költség", Left = 560, Top = 10, Width = 110 };
            btnKoltsegHozzaad.Click += (s, e) =>
            {
                var auto = KivalasztottAuto();
                if (auto == null) { MessageBox.Show("Válasszon autót!"); return; }
                new KoltsegForm(auto).ShowDialog();
                FrissitAutok();
                FrissitKoltsegek();
            };

            var btnKoltsegTorol = new Button { Text = "Költség törlése", Left = 680, Top = 10, Width = 120 };
            btnKoltsegTorol.Click += BtnKoltsegTorol_Click;

            var btnMent = new Button { Text = "Mentés", Left = 990, Top = 340, Width = 130 };
            btnMent.Click += (s, e) => { Adatkezelo.Ment(); MessageBox.Show("Mentés kész."); };

            var btnBetolt = new Button { Text = "Betöltés", Left = 850, Top = 340, Width = 130 };
            btnBetolt.Click += (s, e) => { Adatkezelo.Betolt(); FrissitAutok(); };

            var lblKeres = new Label { Text = "Keresés:", Left = 10, Top = 345, Width = 60 };
            txtKeres = new TextBox { Left = 75, Top = 342, Width = 180 };
            txtKeres.TextChanged += (s, e) => FrissitAutok();

            var lblSzuro = new Label { Text = "Szűrő:", Left = 270, Top = 345, Width = 45 };
            cmbSzuro = new ComboBox { Left = 320, Top = 342, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSzuro.Items.AddRange(new object[] { "Összes", "Benzin", "Dízel", "Elektromos", "Hibrid", "Gáz" });
            cmbSzuro.SelectedIndex = 0;
            cmbSzuro.SelectedIndexChanged += (s, e) => FrissitAutok();

            lblOsszesito = new Label
            {
                Left = 10, Top = 390, Width = 1100, Height = 30,
                Font = new System.Drawing.Font("Arial", 11, System.Drawing.FontStyle.Bold)
            };

            Controls.AddRange(new Control[] {
                dgvAutok, dgvKoltsegek, btnAutoHozzaad, btnAutoTorol,
                btnKoltsegHozzaad, btnKoltsegTorol, btnMent, btnBetolt,
                lblKeres, txtKeres, lblSzuro, cmbSzuro, lblOsszesito
            });

            Adatkezelo.Betolt();
            FrissitAutok();
        }

        private Auto KivalasztottAuto()
        {
            if (dgvAutok.SelectedRows.Count == 0) return null;
            int id = (int)dgvAutok.SelectedRows[0].Cells["Id"].Value;
            return Adatkezelo.Autok.FirstOrDefault(a => a.Id == id);
        }

        private void FrissitAutok()
        {
            var lista = Adatkezelo.Autok.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(txtKeres.Text))
            {
                string q = txtKeres.Text.ToLower();
                lista = lista.Where(a => a.Rendszam.ToLower().Contains(q)
                                      || a.Marka.ToLower().Contains(q)
                                      || a.Modell.ToLower().Contains(q));
            }

            if (cmbSzuro.SelectedIndex > 0)
                lista = lista.Where(a => a.Uzemanyag == cmbSzuro.SelectedItem.ToString());

            var megjelenit = lista.Select(a => new
            {
                a.Id,
                a.Rendszam,
                a.Marka,
                a.Modell,
                a.Evjarat,
                a.Uzemanyag,
                Km = a.Km.ToString("N0"),
                OsszesKoltseg = Adatkezelo.AutoOsszKoltseg(a.Id).ToString("N0") + " Ft"
            }).ToList();

            dgvAutok.DataSource = null;
            dgvAutok.DataSource = megjelenit;
            if (dgvAutok.Columns["Id"] != null) dgvAutok.Columns["Id"].Visible = false;

            lblOsszesito.Text = $"Autók száma: {Adatkezelo.Autok.Count}  |  " +
                                $"Összes költség: {Adatkezelo.Koltsegek.Sum(k => k.Osszeg):N0} Ft  |  " +
                                $"Költség rekordok: {Adatkezelo.Koltsegek.Count} db";

            FrissitKoltsegek();
        }

        private void FrissitKoltsegek()
        {
            var auto = KivalasztottAuto();
            if (auto == null) { dgvKoltsegek.DataSource = null; return; }

            var lista = Adatkezelo.AutoKoltsegei(auto.Id).Select(k => new
            {
                k.Id,
                Datum = k.Datum.ToString("yyyy.MM.dd."),
                k.Kategoria,
                k.Megnevezes,
                Osszeg = k.Osszeg.ToString("N0") + " Ft",
                Km = k.Km.ToString("N0")
            }).ToList();

            dgvKoltsegek.DataSource = null;
            dgvKoltsegek.DataSource = lista;
            if (dgvKoltsegek.Columns["Id"] != null) dgvKoltsegek.Columns["Id"].Visible = false;
        }

        private void BtnAutoTorol_Click(object sender, EventArgs e)
        {
            var auto = KivalasztottAuto();
            if (auto == null) { MessageBox.Show("Válasszon autót!"); return; }

            if (MessageBox.Show($"Biztosan törli a(z) {auto.Rendszam} autót és összes költségét?",
                "Megerősítés", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Adatkezelo.Koltsegek.RemoveAll(k => k.AutoId == auto.Id);
                Adatkezelo.Autok.Remove(auto);
                Adatkezelo.Ment();
                FrissitAutok();
            }
        }

        private void BtnKoltsegTorol_Click(object sender, EventArgs e)
        {
            if (dgvKoltsegek.SelectedRows.Count == 0)
            {
                MessageBox.Show("Válasszon költséget!");
                return;
            }

            int id = (int)dgvKoltsegek.SelectedRows[0].Cells["Id"].Value;
            var k = Adatkezelo.Koltsegek.FirstOrDefault(x => x.Id == id);

            if (k != null && MessageBox.Show("Biztosan törli a kiválasztott költséget?",
                "Megerősítés", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Adatkezelo.Koltsegek.Remove(k);
                Adatkezelo.Ment();
                FrissitAutok();
                FrissitKoltsegek();
            }
        }
    }

    // ===================== BELÉPÉSI PONT =====================

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
