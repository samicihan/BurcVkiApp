using System;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Globalization;

namespace BurcVkiApp
{
    public class Form1 : Form
    {
        TextBox txtAd, txtSoyad, txtBoy, txtKilo;
        DateTimePicker dtpDogumTarihi;
        Button btnKaydet, btnGoster, btnGuncelle, btnSil;
        PictureBox pbBurcResim, pbSeciliResim;
        Label lblVki, lblBurc;
        Label lblDetayBurc, lblDetayYorum, lblDetayVki, lblDetayVkiYorum;
        ListBox lstKisiler;

        int seciliId = -1;
        string dbPath = Path.Combine(Application.StartupPath, "veritabani.db");

        public Form1()
        {
            InitForm();
            VeritabaniOlustur();
        }

        void InitForm()
        {
            this.Text = "Burç ve VKİ Hesaplayıcı";
            this.Size = new Size(600, 700);

            Label lbl1 = new Label() { Text = "Adı:", Location = new Point(30, 30) };
            txtAd = new TextBox() { Location = new Point(150, 30), Width = 150 };

            Label lbl2 = new Label() { Text = "Soyadı:", Location = new Point(30, 70) };
            txtSoyad = new TextBox() { Location = new Point(150, 70), Width = 150 };

            Label lbl3 = new Label() { Text = "Doğum Tarihi:", Location = new Point(30, 110) };
            dtpDogumTarihi = new DateTimePicker() { Location = new Point(150, 110) };

            Label lbl4 = new Label() { Text = "Boy (m):", Location = new Point(30, 150) };
            txtBoy = new TextBox() { Location = new Point(150, 150), Width = 150 };

            Label lbl5 = new Label() { Text = "Kilo (kg):", Location = new Point(30, 190) };
            txtKilo = new TextBox() { Location = new Point(150, 190), Width = 150 };

            btnSil = new Button() { Text = "Kaydı Sil", Location = new Point(150, 310), Width = 150};
            btnSil.Click += BtnSil_Click;

            this.Controls.Add(btnSil);


            btnKaydet = new Button() { Text = "Hesapla ve Kaydet", Location = new Point(150, 230), Width = 150 };
            btnKaydet.Click += BtnKaydet_Click;

            btnGoster = new Button() { Text = "Kayıtları Göster", Location = new Point(150, 270), Width = 150 };
            btnGoster.Click += BtnGoster_Click;

            btnGuncelle = new Button() { Text = "Kaydı Güncelle", Location = new Point(320, 270), Width = 150 };
            btnGuncelle.Click += BtnGuncelle_Click;

            pbBurcResim = new PictureBox() { Location = new Point(330, 30), Size = new Size(130, 130), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.StretchImage };

            lblVki = new Label() { Location = new Point(150, 310), Width = 300 };
            lblBurc = new Label() { Location = new Point(150, 340), Width = 300 };

            lstKisiler = new ListBox() { Location = new Point(30, 380), Width = 500, Height = 100 };
            lstKisiler.SelectedIndexChanged += LstKisiler_SelectedIndexChanged;

            pbSeciliResim = new PictureBox() { Location = new Point(30, 490), Size = new Size(130, 130), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.StretchImage };

            lblDetayBurc = new Label() { Location = new Point(180, 490), Width = 350 };
            lblDetayYorum = new Label() { Location = new Point(180, 520), Width = 350 };
            lblDetayVki = new Label() { Location = new Point(180, 550), Width = 350 };
            lblDetayVkiYorum = new Label() { Location = new Point(180, 580), Width = 350 };

            this.Controls.AddRange(new Control[] {
                lbl1, txtAd, lbl2, txtSoyad, lbl3, dtpDogumTarihi,
                lbl4, txtBoy, lbl5, txtKilo,
                btnKaydet, btnGoster, btnGuncelle,
                pbBurcResim, lblVki, lblBurc,
                lstKisiler, pbSeciliResim,
                lblDetayBurc, lblDetayYorum, lblDetayVki, lblDetayVkiYorum
            });
        }

        private void VeritabaniOlustur()
        {
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
                using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    conn.Open();
                    string sql = @"CREATE TABLE KisiBilgileri (
                                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    Ad TEXT,
                                    Soyad TEXT,
                                    Gun INTEGER,
                                    Ay TEXT,
                                    Yil INTEGER,
                                    Burc TEXT,
                                    BurcYorumu TEXT,
                                    BurcResimPath TEXT,
                                    Vki REAL,
                                    VkiYorum TEXT)";
                    new SQLiteCommand(sql, conn).ExecuteNonQuery();
                }
            }
        }

        private void BtnKaydet_Click(object sender, EventArgs e)
        {
            string ad = txtAd.Text;
            string soyad = txtSoyad.Text;
            DateTime dogum = dtpDogumTarihi.Value;
            double boy = double.Parse(txtBoy.Text, CultureInfo.InvariantCulture);
            double kilo = double.Parse(txtKilo.Text, CultureInfo.InvariantCulture);
            double vki = kilo / (boy * boy);

            string vkiYorum = vki < 18.5 ? "Zayıf" : vki < 25 ? "Normal" : vki < 30 ? "Kilolu" : "Obez";
            string burc = BurcHesapla(dogum.Day, dogum.Month);
            string burcYorum = BurcYorumGetir(burc);
            string burcResim = $"burclar/{burc.ToLower()}.jpg";

            if (File.Exists(burcResim))
                pbBurcResim.Image = Image.FromFile(burcResim);

            lblVki.Text = $"VKİ: {vki:F2} - {vkiYorum}";
            lblBurc.Text = $"Burç: {burc} - {burcYorum}";

            using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();
                string sql = @"INSERT INTO KisiBilgileri 
                            (Ad, Soyad, Gun, Ay, Yil, Burc, BurcYorumu, BurcResimPath, Vki, VkiYorum)
                            VALUES (@ad, @soyad, @gun, @ay, @yil, @burc, @burcYorum, @resim, @vki, @vkiYorum)";
                var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ad", ad);
                cmd.Parameters.AddWithValue("@soyad", soyad);
                cmd.Parameters.AddWithValue("@gun", dogum.Day);
                cmd.Parameters.AddWithValue("@ay", dogum.ToString("MMMM"));
                cmd.Parameters.AddWithValue("@yil", dogum.Year);
                cmd.Parameters.AddWithValue("@burc", burc);
                cmd.Parameters.AddWithValue("@burcYorum", burcYorum);
                cmd.Parameters.AddWithValue("@resim", burcResim);
                cmd.Parameters.AddWithValue("@vki", vki);
                cmd.Parameters.AddWithValue("@vkiYorum", vkiYorum);
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Kayıt başarıyla eklendi!");
        }

        private void BtnGoster_Click(object sender, EventArgs e)
        {
            lstKisiler.Items.Clear();

            using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();
                var cmd = new SQLiteCommand("SELECT * FROM KisiBilgileri", conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string id = reader["Id"].ToString();
                    string ad = reader["Ad"].ToString();
                    string soyad = reader["Soyad"].ToString();
                    string burc = reader["Burc"].ToString();
                    string yorum = reader["BurcYorumu"].ToString();
                    string vki = reader["Vki"].ToString();
                    string vkiYorum = reader["VkiYorum"].ToString();
                    string resimPath = reader["BurcResimPath"].ToString();

                    string gosterilecek = $"{id}|{ad}|{soyad}|{burc}|{yorum}|{vki}|{vkiYorum}|{resimPath}";
                    lstKisiler.Items.Add(gosterilecek);
                }
            }
        }

        private void BtnGuncelle_Click(object sender, EventArgs e)
        {
            if (seciliId == -1)
            {
                MessageBox.Show("Lütfen güncellenecek bir kayıt seçin.");
                return;
            }

            string ad = txtAd.Text;
            string soyad = txtSoyad.Text;
            DateTime dogum = dtpDogumTarihi.Value;
            double boy = double.Parse(txtBoy.Text, CultureInfo.InvariantCulture);
            double kilo = double.Parse(txtKilo.Text, CultureInfo.InvariantCulture);
            double vki = kilo / (boy * boy);

            string vkiYorum = vki < 18.5 ? "Zayıf" :
                              vki < 25 ? "Normal" :
                              vki < 30 ? "Kilolu" : "Obez";

            string burc = BurcHesapla(dogum.Day, dogum.Month);
            string burcYorum = BurcYorumGetir(burc);
            string burcResim = $"burclar/{burc.ToLower()}.jpg";

            using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();
                string sql = @"UPDATE KisiBilgileri SET
                        Ad=@ad,
                        Soyad=@soyad,
                        Gun=@gun,
                        Ay=@ay,
                        Yil=@yil,
                        Burc=@burc,
                        BurcYorumu=@burcYorum,
                        BurcResimPath=@resim,
                        Vki=@vki,
                        VkiYorum=@vkiYorum
                      WHERE Id=@id";

                var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ad", ad);
                cmd.Parameters.AddWithValue("@soyad", soyad);
                cmd.Parameters.AddWithValue("@gun", dogum.Day);
                cmd.Parameters.AddWithValue("@ay", dogum.ToString("MMMM"));
                cmd.Parameters.AddWithValue("@yil", dogum.Year);
                cmd.Parameters.AddWithValue("@burc", burc);
                cmd.Parameters.AddWithValue("@burcYorum", burcYorum);
                cmd.Parameters.AddWithValue("@resim", burcResim);
                cmd.Parameters.AddWithValue("@vki", vki);
                cmd.Parameters.AddWithValue("@vkiYorum", vkiYorum);
                cmd.Parameters.AddWithValue("@id", seciliId);
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Kayıt başarıyla güncellendi!");
            BtnGoster_Click(null, null); // Listeyi yenile
        }

        private void BtnSil_Click(object sender, EventArgs e)
        {
            if (seciliId == -1)
            {
                MessageBox.Show("Lütfen silinecek bir kayıt seçin.");
                return;
            }

            DialogResult sonuc = MessageBox.Show("Seçilen kaydı silmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (sonuc == DialogResult.Yes)
            {
                using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    conn.Open();
                    var cmd = new SQLiteCommand("DELETE FROM KisiBilgileri WHERE Id=@id", conn);
                    cmd.Parameters.AddWithValue("@id", seciliId);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Kayıt başarıyla silindi!");
                seciliId = -1;
                BtnGoster_Click(null, null); // Listeyi güncelle
                pbSeciliResim.Image = null;
                lblDetayBurc.Text = lblDetayYorum.Text = lblDetayVki.Text = lblDetayVkiYorum.Text = "";
                txtAd.Text = txtSoyad.Text = txtBoy.Text = txtKilo.Text = "";
            }
        }


        private void LstKisiler_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstKisiler.SelectedItem != null)
            {
                string[] parcalar = lstKisiler.SelectedItem.ToString().Split('|');
                if (parcalar.Length >= 8)
                {
                    seciliId = int.Parse(parcalar[0]);
                    txtAd.Text = parcalar[1];
                    txtSoyad.Text = parcalar[2];
                    txtBoy.Text = "";
                    txtKilo.Text = "";

                    lblDetayBurc.Text = $"Burç: {parcalar[3]}";
                    lblDetayYorum.Text = $"Yorum: {parcalar[4]}";
                    lblDetayVki.Text = $"VKİ: {parcalar[5]}";
                    lblDetayVkiYorum.Text = $"VKİ Yorum: {parcalar[6]}";

                    string resimYolu = parcalar[7];
                    if (File.Exists(resimYolu))
                        pbSeciliResim.Image = Image.FromFile(resimYolu);
                    else
                        pbSeciliResim.Image = null;
                }
            }
        }

        private string BurcHesapla(int gun, int ay)
        {
            if ((gun >= 21 && ay == 3) || (gun <= 20 && ay == 4)) return "Koç";
            if ((gun >= 21 && ay == 4) || (gun <= 20 && ay == 5)) return "Boğa";
            if ((gun >= 21 && ay == 5) || (gun <= 21 && ay == 6)) return "İkizler";
            if ((gun >= 22 && ay == 6) || (gun <= 22 && ay == 7)) return "Yengeç";
            if ((gun >= 23 && ay == 7) || (gun <= 23 && ay == 8)) return "Aslan";
            if ((gun >= 24 && ay == 8) || (gun <= 23 && ay == 9)) return "Başak";
            if ((gun >= 24 && ay == 9) || (gun <= 23 && ay == 10)) return "Terazi";
            if ((gun >= 24 && ay == 10) || (gun <= 22 && ay == 11)) return "Akrep";
            if ((gun >= 23 && ay == 11) || (gun <= 21 && ay == 12)) return "Yay";
            if ((gun >= 22 && ay == 12) || (gun <= 20 && ay == 1)) return "Oğlak";
            if ((gun >= 21 && ay == 1) || (gun <= 19 && ay == 2)) return "Kova";
            if ((gun >= 20 && ay == 2) || (gun <= 20 && ay == 3)) return "Balık";
            return "Bilinmiyor";
        }

        private string BurcYorumGetir(string burc)
        {
            switch (burc)
            {
                case "Koç": return "Atılgan, enerjik ve lider ruhludur.";
                case "Boğa": return "Sabırlı, güvenilir ve maddeye önem verir.";
                case "İkizler": return "Zeki, meraklı ve iletişimi kuvvetlidir.";
                case "Yengeç": return "Duygusal, sezgileri güçlü ve koruyucudur.";
                case "Aslan": return "Özgüvenli, lider ruhlu ve gururludur.";
                case "Başak": return "Detaycı, titiz ve çalışkandır.";
                case "Terazi": return "Adil, uyumlu ve estetik duygusu gelişmiştir.";
                case "Akrep": return "Tutkulu, gizemli ve kararlıdır.";
                case "Yay": return "Özgürlüğüne düşkün, iyimser ve filozofiktir.";
                case "Oğlak": return "Disiplinli, sorumluluk sahibi ve hırslıdır.";
                case "Kova": return "Yenilikçi, bağımsız ve sıradışıdır.";
                case "Balık": return "Hayalperest, sezgisel ve empati yeteneği yüksektir.";
                default: return "Genel yorum: Burç bulunamadı.";
            }
        }


        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
