using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using libfintx;
using libfintx.Data;

namespace libfintx_test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Synchronisation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_synchronisation_Click(object sender, EventArgs e)
        {
            Segment.Reset();

            ConnectionDetails connectionDetails = new ConnectionDetails()
            {
                Account = txt_kontonummer.Text,
                Blz = Convert.ToInt32(txt_bankleitzahl.Text),
                BIC = txt_bic.Text,
                IBAN = txt_iban.Text,
                Url = txt_url.Text,
                HBCIVersion = Convert.ToInt32(txt_hbci_version.Text),
                UserId = txt_userid.Text,
                Pin = txt_pin.Text
            };

            HBCIOutput(Main.Synchronization(connectionDetails).Messages);
        }

        /// <summary>
        /// HBCI-Nachricht ausgeben
        /// </summary>
        /// <param name="hbcimsg"></param>
        private void HBCIOutput(IEnumerable<HBCIBankMessage> hbcimsg)
        {
            foreach (var msg in hbcimsg)
            {
                txt_hbci_meldung.Invoke(new MethodInvoker
                (delegate ()
                {
                    txt_hbci_meldung.Text += "Code: " + msg.Code + " | " + "Typ: " + msg.Type + " | " + "Nachricht: " + msg.Message + Environment.NewLine;
                    txt_hbci_meldung.SelectionStart = txt_hbci_meldung.TextLength;
                    txt_hbci_meldung.ScrollToCaret();
                }));
            }
        }

        /// <summary>
        /// Einfache Nachricht ausgeben
        /// </summary>
        /// <param name="msg"></param>
        private void SimpleOutput(string msg)
        {
            txt_hbci_meldung.Invoke(new MethodInvoker
                (delegate ()
                {
                    txt_hbci_meldung.Text += msg + Environment.NewLine;
                    txt_hbci_meldung.SelectionStart = txt_hbci_meldung.TextLength;
                    txt_hbci_meldung.ScrollToCaret();
                }));
        }

        /// <summary>
        /// Bankdaten laden
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_lade_bankdaten_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "CSV|*.csv";
            openFileDialog1.Title = "Datei mit Bankdaten laden";

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Lade Bankdaten falls vorhanden

                // Damit keine Zugangsdaten direkt im Code hinterlegt sind, kann optional eine Datei verwendet werden.
                // Datei liegt in C:/Users/<username>/libfintx_test_connection.csv

                var userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var connFile = Path.Combine(userDir, openFileDialog1.FileName);

                if (File.Exists(connFile))
                {
                    var lines = File.ReadAllLines(connFile).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
                    if (lines.Length != 2)
                    {
                        SimpleOutput($"Die Datei {connFile} existiert, hat aber das falsche Format.");
                        return;
                    }

                    var values = lines[1].Split(';');
                    if (values.Length != 8)
                    {
                        SimpleOutput($"Die Datei {connFile} existiert, hat aber das falsche Format.");
                        return;
                    }

                    txt_kontonummer.Text = values[0];
                    txt_bankleitzahl.Text = values[1];
                    txt_bic.Text = values[2];
                    txt_iban.Text = values[3].Replace(" ", "");
                    txt_url.Text = values[4];
                    txt_hbci_version.Text = values[5];
                    txt_userid.Text = values[6];
                    txt_pin.Text = values[7];
                }
            }
        }

        /// <summary>
        /// Überweisungsdaten laden
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_lade_überweisungsdaten_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "CSV|*.csv";
            openFileDialog1.Title = "Datei mit Überweisungsdaten laden";

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Lade Überweisungsdaten falls vorhanden

                var userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var connFile = Path.Combine(userDir, openFileDialog1.FileName);

                if (File.Exists(connFile))
                {
                    var lines = File.ReadAllLines(connFile).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
                    if (lines.Length != 2)
                    {
                        SimpleOutput($"Die Datei {connFile} existiert, hat aber das falsche Format.");
                        return;
                    }

                    var values = lines[1].Split(';');
                    if (values.Length != 5)
                    {
                        SimpleOutput($"Die Datei {connFile} existiert, hat aber das falsche Format.");
                        return;
                    }

                    txt_empfängername.Text = values[0];
                    txt_empfängeriban.Text = values[1].Replace(" ", "");
                    txt_empfängerbic.Text = values[2];
                    txt_betrag.Text = values[3];
                    txt_verwendungszweck.Text = values[4];
                }
            }
        }

        /// <summary>
        /// Kontostand abfragen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_kontostand_abfragen_Click(object sender, EventArgs e)
        {
            Segment.Reset();

            ConnectionDetails connectionDetails = new ConnectionDetails()
            {
                Account = txt_kontonummer.Text,
                Blz = Convert.ToInt32(txt_bankleitzahl.Text),
                BIC = txt_bic.Text,
                IBAN = txt_iban.Text,
                Url = txt_url.Text,
                HBCIVersion = Convert.ToInt32(txt_hbci_version.Text),
                UserId = txt_userid.Text,
                Pin = txt_pin.Text
            };

            var sync = Main.Synchronization(connectionDetails);

            HBCIOutput(sync.Messages);

            if (sync.IsSuccess)
            {
                var balance = Main.Balance(connectionDetails, false);

                HBCIOutput(balance.Messages);

                if (balance.IsSuccess)
                    SimpleOutput("Kontostand: " + Convert.ToString(balance.Data.Balance));
            }
        }

        /// <summary>
        /// Konten anzeigen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_konten_anzeigen_Click(object sender, EventArgs e)
        {
            Segment.Reset();

            ConnectionDetails connectionDetails = new ConnectionDetails()
            {
                Account = txt_kontonummer.Text,
                Blz = Convert.ToInt32(txt_bankleitzahl.Text),
                BIC = txt_bic.Text,
                IBAN = txt_iban.Text,
                Url = txt_url.Text,
                HBCIVersion = Convert.ToInt32(txt_hbci_version.Text),
                UserId = txt_userid.Text,
                Pin = txt_pin.Text
            };

            var sync = Main.Synchronization(connectionDetails);

            HBCIOutput(sync.Messages);

            if (sync.IsSuccess)
            {
                var accounts = Main.Accounts(connectionDetails, false);

                HBCIOutput(accounts.Messages);

                if (accounts.IsSuccess)
                {
                    foreach (var acc in accounts.Data)
                    {
                        SimpleOutput("Inhaber: " + acc.Accountowner + " | " + "IBAN: " + acc.Accountiban + " | "+ "Typ: " + acc.Accounttype);
                    }
                }
            }
        }

        /// <summary>
        /// Zugelassene TAN-Verfahren
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_zugelassene_tanverfahren_Click(object sender, EventArgs e)
        {
            Segment.Reset();

            ConnectionDetails connectionDetails = new ConnectionDetails()
            {
                Account = txt_kontonummer.Text,
                Blz = Convert.ToInt32(txt_bankleitzahl.Text),
                BIC = txt_bic.Text,
                IBAN = txt_iban.Text,
                Url = txt_url.Text,
                HBCIVersion = Convert.ToInt32(txt_hbci_version.Text),
                UserId = txt_userid.Text,
                Pin = txt_pin.Text
            };

            var sync = Main.Synchronization(connectionDetails);

            HBCIOutput(sync.Messages);

            if (sync.IsSuccess)
            {
                foreach (var process in TANProcesses.items)
                {
                    SimpleOutput("Name: " + process.ProcessName + " | " + "Nummer: " + process.ProcessNumber);
                }
            }
        }

        /// <summary>
        /// Umsätze abholen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_umsätze_abholen_Click(object sender, EventArgs e)
        {
            Segment.Reset();

            ConnectionDetails connectionDetails = new ConnectionDetails()
            {
                Account = txt_kontonummer.Text,
                Blz = Convert.ToInt32(txt_bankleitzahl.Text),
                BIC = txt_bic.Text,
                IBAN = txt_iban.Text,
                Url = txt_url.Text,
                HBCIVersion = Convert.ToInt32(txt_hbci_version.Text),
                UserId = txt_userid.Text,
                Pin = txt_pin.Text
            };

            var sync = Main.Synchronization(connectionDetails);

            HBCIOutput(sync.Messages);

            if (sync.IsSuccess)
            {
                var transactions = Main.Transactions(connectionDetails, false);

                HBCIOutput(transactions.Messages);

                if(transactions.IsSuccess)
                {
                    foreach (var item in transactions.Data)
                    {
                        foreach (var i in item.SWIFTTransactions)
                        {
                            SimpleOutput("Datum: " + i.inputDate + " | " + 
                                "Empfänger / Auftraggeber: " + i.partnerName + " | " + 
                                "Verwendungszweck: " + i.text + " | " 
                                + "Betrag: " + i.amount);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Umsätze im Format camt052 abholen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void camt_052_abholen_Click(object sender, EventArgs e)
        {
            Segment.Reset();

            ConnectionDetails connectionDetails = new ConnectionDetails()
            {
                Account = txt_kontonummer.Text,
                Blz = Convert.ToInt32(txt_bankleitzahl.Text),
                BIC = txt_bic.Text,
                IBAN = txt_iban.Text,
                Url = txt_url.Text,
                HBCIVersion = Convert.ToInt32(txt_hbci_version.Text),
                UserId = txt_userid.Text,
                Pin = txt_pin.Text
            };

            var sync = Main.Synchronization(connectionDetails);

            HBCIOutput(sync.Messages);

            if (sync.IsSuccess)
            {
                var transactions = Main.Transactions_camt(connectionDetails, false, camtVersion.camt052);

                HBCIOutput(transactions.Messages);

                if (transactions.IsSuccess)
                {
                    foreach (var item in transactions.Data)
                    {
                        foreach (var i in item.transactions)
                        {
                            SimpleOutput("Datum: " + i.inputDate + " | " +
                                "Empfänger / Auftraggeber: " + i.partnerName + " | " +
                                "Verwendungszweck: " + i.description + " | "
                                + "Betrag: " + String.Format("{0:0.00}", i.amount));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Umsätze im Format camt053 abholen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void camt_053_abholen_Click(object sender, EventArgs e)
        {
            Segment.Reset();

            ConnectionDetails connectionDetails = new ConnectionDetails()
            {
                Account = txt_kontonummer.Text,
                Blz = Convert.ToInt32(txt_bankleitzahl.Text),
                BIC = txt_bic.Text,
                IBAN = txt_iban.Text,
                Url = txt_url.Text,
                HBCIVersion = Convert.ToInt32(txt_hbci_version.Text),
                UserId = txt_userid.Text,
                Pin = txt_pin.Text
            };

            var sync = Main.Synchronization(connectionDetails);

            HBCIOutput(sync.Messages);

            if (sync.IsSuccess)
            {
                var transactions = Main.Transactions_camt(connectionDetails, false, camtVersion.camt053);

                HBCIOutput(transactions.Messages);

                if (transactions.IsSuccess)
                {
                    foreach (var item in transactions.Data)
                    {
                        foreach (var i in item.transactions)
                        {
                            SimpleOutput("Datum: " + i.inputDate + " | " +
                                "Empfänger / Auftraggeber: " + i.partnerName + " | " +
                                "Verwendungszweck: " + i.text + " | "
                                + "Betrag: " + i.amount);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Überweisen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_überweisen_Click(object sender, EventArgs e)
        {
            Main.Logging(true);
            Main.Tracing(true, true);

            Segment.Reset();

            ConnectionDetails connectionDetails = new ConnectionDetails()
            {
                Account = txt_kontonummer.Text,
                AccountHolder = txt_kontonummer.Text,
                Blz = Convert.ToInt32(txt_bankleitzahl.Text),
                BIC = txt_bic.Text,
                IBAN = txt_iban.Text,
                Url = txt_url.Text,
                HBCIVersion = Convert.ToInt32(txt_hbci_version.Text),
                UserId = txt_userid.Text,
                Pin = txt_pin.Text
            };

            var sync = Main.Synchronization(connectionDetails);

            HBCIOutput(sync.Messages);

            if (sync.IsSuccess)
            {
                // TAN-Verfahren
                Segment.HIRMS = txt_tanverfahren.Text;
				
				// TAN-Medium-Name
                var tanmediumname = Main.RequestTANMediumName(connectionDetails);
                Segment.HITAB = tanmediumname.Data;

                // Out image is needed e. g. for photoTAN
                var transfer = Main.Transfer(connectionDetails, txt_empfängername.Text, txt_empfängeriban.Text, txt_empfängerbic.Text,
                    decimal.Parse(txt_betrag.Text), txt_verwendungszweck.Text, Segment.HIRMS, pBox_tan, false);

                HBCIOutput(transfer.Messages);

                if (transfer.IsSuccess)
                    SimpleOutput(Main.Transaction_Output());
            }
        }

        /// <summary>
        /// TAN-Medium-Name abfragen -> Notwendig bsp. für pushTAN
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_tan_medium_name_abfragen_Click(object sender, EventArgs e)
        {
            Segment.Reset();

            ConnectionDetails connectionDetails = new ConnectionDetails()
            {
                Account = txt_kontonummer.Text,
                Blz = Convert.ToInt32(txt_bankleitzahl.Text),
                BIC = txt_bic.Text,
                IBAN = txt_iban.Text,
                Url = txt_url.Text,
                HBCIVersion = Convert.ToInt32(txt_hbci_version.Text),
                UserId = txt_userid.Text,
                Pin = txt_pin.Text
            };

            var sync = Main.Synchronization(connectionDetails);

            HBCIOutput(sync.Messages);

            if (sync.IsSuccess)
            {
                Segment.HIRMS = txt_tanverfahren.Text;

                var tanmediumname = Main.RequestTANMediumName(connectionDetails);

                SimpleOutput(tanmediumname.Data);
            }
        }

        /// <summary>
        /// Auftrag mit TAN bestätigen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_auftrag_bestätigen_tan_Click(object sender, EventArgs e)
        {
            ConnectionDetails connectionDetails = new ConnectionDetails()
            {
                Account = txt_kontonummer.Text,
                Blz = Convert.ToInt32(txt_bankleitzahl.Text),
                BIC = txt_bic.Text,
                IBAN = txt_iban.Text,
                Url = txt_url.Text,
                HBCIVersion = Convert.ToInt32(txt_hbci_version.Text),
                UserId = txt_userid.Text,
                Pin = txt_pin.Text
            };

            var tan = Main.TAN(connectionDetails, txt_tan.Text);

            HBCIOutput(tan.Messages);
        }
    }
}
