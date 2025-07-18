using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace AirportsViewer
{
    public partial class Form1 : Form
    {
        private BindingList<Airport> airportsList = new BindingList<Airport>();
        private BindingSource bindingSource = new BindingSource();
        private bool isDownloadInProgress = false;
        private bool needFormResize = true;  

        public Form1()
        {
            InitializeComponent();

            dataGridView1.RowHeadersVisible = false;
            dataGridView1.DataSource = bindingSource;

            LoadCsv();
            dataGridView1_ColumnWidthChanged(null, null);


            textBoxCode.TextChanged += FilterChanged;
            textBoxName.TextChanged += FilterChanged;
            textBoxCountry.TextChanged += FilterChanged;
            buttonUpdateCsv.Click += buttonUpdateCsv_Click;

            dataGridView1.CellContentClick += dataGridView1_CellContentClick;
            dataGridView1.CellFormatting += dataGridView1_CellFormatting;
            dataGridView1.CellToolTipTextNeeded += dataGridView1_CellToolTipTextNeeded;
            dataGridView1.ColumnWidthChanged += dataGridView1_ColumnWidthChanged;
        }

        private void LoadCsv()
        {
            if (!File.Exists("airports.csv"))
            {
                MessageBox.Show("airports.csv not found.\nPlease download using \"Update Database\" button.",
                                "File not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                bindingSource.DataSource = null; // очищаем таблицу
                return;
            }
            try
            {
                using (var reader = new StreamReader("airports.csv"))
                using (var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<Airport>().ToList();
                    airportsList = new BindingList<Airport>(records);
                    bindingSource.DataSource = airportsList;
                    SetupDataGridView();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading CSV file: " + ex.Message);
            }
        }

        private void SetupDataGridView()
        {
            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = bindingSource;

            string[] hidden = { "latitude", "longitude", "elevation", "city_code", "type" };
            foreach (var col in hidden)
                if (dataGridView1.Columns.Contains(col))
                    dataGridView1.Columns[col].Visible = false;

            if (dataGridView1.Columns.Contains("name"))
            {
                var idx = dataGridView1.Columns["name"].Index;
                var link = new DataGridViewLinkColumn
                {
                    Name = "name",
                    HeaderText = "name",
                    DataPropertyName = "name",
                    LinkBehavior = LinkBehavior.SystemDefault,
                    UseColumnTextForLinkValue = false,
                    TrackVisitedState = false
                };
                dataGridView1.Columns.RemoveAt(idx);
                dataGridView1.Columns.Insert(idx, link);
            }
            if (dataGridView1.Columns.Contains("url"))
            {
                var idx = dataGridView1.Columns["url"].Index;
                var link = new DataGridViewLinkColumn
                {
                    Name = "url",
                    HeaderText = "url",
                    DataPropertyName = "url",
                    LinkBehavior = LinkBehavior.SystemDefault,
                    UseColumnTextForLinkValue = false,
                    TrackVisitedState = false
                };
                dataGridView1.Columns.RemoveAt(idx);
                dataGridView1.Columns.Insert(idx, link);
            }


        }

        private void FilterChanged(object sender, EventArgs e)
        {
            string codeFilter = textBoxCode.Text.ToLower();
            string nameFilter = textBoxName.Text.ToLower();
            string countryFilter = textBoxCountry.Text.ToLower();

            var filtered = airportsList.Where(a =>
                (string.IsNullOrEmpty(codeFilter) || (a.code != null && a.code.ToLower().Contains(codeFilter)))
                &&
                (string.IsNullOrEmpty(nameFilter) ||
                    (a.name != null && a.name.ToLower().Contains(nameFilter)) ||
                    (a.city != null && a.city.ToLower().Contains(nameFilter)) ||
                    (a.state != null && a.state.ToLower().Contains(nameFilter)) ||
                    (a.country != null && a.country.ToLower().Contains(nameFilter))
                )
                &&
                (string.IsNullOrEmpty(countryFilter) || (a.country != null && a.country.ToLower().Contains(countryFilter)))
            ).ToList();

            bindingSource.DataSource = new BindingList<Airport>(filtered);
        }


        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var grid = (DataGridView)sender;
            var column = grid.Columns[e.ColumnIndex];
            var airport = (Airport)grid.Rows[e.RowIndex].DataBoundItem;

            if (column.Name == "name" && airport != null)
            {
                if (airport.latitude != 0 && airport.longitude != 0)
                {
                    string lat = airport.latitude.ToString(CultureInfo.InvariantCulture);
                    string lon = airport.longitude.ToString(CultureInfo.InvariantCulture);
                    string url = $"https://www.google.com/maps/?q={airport.name} airport&ll={lat},{lon}&z=13";
                    System.Diagnostics.Process.Start(url);
                }
            }
            else if (column.Name == "url" && airport != null && !string.IsNullOrWhiteSpace(airport.url))
            {
                System.Diagnostics.Process.Start(airport.url);
            }
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var columnName = dataGridView1.Columns[e.ColumnIndex].Name;
            if (columnName == "url")
            {
                var airport = dataGridView1.Rows[e.RowIndex].DataBoundItem as Airport;
                if (airport == null || string.IsNullOrWhiteSpace(airport.url))
                {
                    e.Value = "";
                    dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly = true;
                    if (dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex] is DataGridViewLinkCell linkCell)
                    {
                        linkCell.LinkColor = System.Drawing.Color.Black;
                    }
                }
            }
        }

        private void dataGridView1_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            var grid = (DataGridView)sender;
            var col = grid.Columns[e.ColumnIndex];
            if (col.Name == "country")
            {
                var airport = grid.Rows[e.RowIndex].DataBoundItem as Airport;
                if (airport != null && !string.IsNullOrWhiteSpace(airport.country))
                {
                    string code = airport.country.Trim().ToUpper();
                    if (CountryCodes.All.TryGetValue(code, out var name))
                        e.ToolTipText = $"{code} — {name}";
                    else
                        e.ToolTipText = code;
                }
            }
        }

        private void dataGridView1_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            if (needFormResize)
            {
                int columnsWidth = 0;
                foreach (DataGridViewColumn col in dataGridView1.Columns)
                {
                    if (col.Visible)
                        columnsWidth += col.Width;
                }
                int vScrollBarWidth = (dataGridView1.DisplayedRowCount(false) < dataGridView1.RowCount) ? SystemInformation.VerticalScrollBarWidth : 0;
                int padding = dataGridView1.Location.X * 2 + 20;
                int targetWidth = columnsWidth + vScrollBarWidth + padding;
                int minWidth = 900;
                this.Width = Math.Max(targetWidth, minWidth);
            }
        }

        private async void buttonUpdateCsv_Click(object sender, EventArgs e)
        {
            if (isDownloadInProgress) return; // Уже идёт загрузка — просто игнорируем повторный клик!
            isDownloadInProgress = true;

            string url = "https://raw.githubusercontent.com/lxndrblz/Airports/refs/heads/main/airports.csv";
            string fileName = "airports.csv";

            buttonUpdateCsv.Enabled = false;
            buttonUpdateCsv.Text = "Downloading...";

            using (var progressForm = new ProgressForm())
            using (var cts = new CancellationTokenSource())
            {
                progressForm.ButtonCancel.Click += (s, ev) => cts.Cancel();

                var downloadTask = DownloadFileWithProgressAsync(url, fileName, progressForm, cts.Token);

                progressForm.Show();

                try
                {
                    await downloadTask;
                    if (!cts.IsCancellationRequested)
                    {
                        progressForm.LabelStatus.Text = "Database updated successfully!";
                        await Task.Delay(300);

                        needFormResize = false;
                        LoadCsv();
                        dataGridView1_DataBindingComplete(this,null);
                        needFormResize = true;
                        dataGridView1_ColumnWidthChanged(this,null);

                    }
                }
                catch (OperationCanceledException)
                {
                    progressForm.LabelStatus.Text = "Download cancelled!";
                    await Task.Delay(700);
                }
                catch (Exception ex)
                {
                    progressForm.LabelStatus.Text = "Error: " + ex.Message;
                    await Task.Delay(1500);
                    MessageBox.Show("Error downloading file: " + ex.Message, "Error");
                }
                finally
                {
                    progressForm.Close();
                    buttonUpdateCsv.Enabled = true;
                    buttonUpdateCsv.Text = "Update Database";
                    isDownloadInProgress = false;
                }
            }
        }


        private async Task DownloadFileWithProgressAsync(string url, string fileName, ProgressForm progressForm, CancellationToken token)
        {
            using (HttpClient client = new HttpClient())
            using (var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token))
            {
                response.EnsureSuccessStatusCode();
                var contentLength = response.Content.Headers.ContentLength;

                using (var inputStream = await response.Content.ReadAsStreamAsync())
                using (var outputStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    byte[] buffer = new byte[8192];
                    long totalRead = 0;
                    int read;
                    progressForm.ProgressBar.Minimum = 0;
                    progressForm.ProgressBar.Value = 0;
                    progressForm.ProgressBar.Maximum = contentLength.HasValue ? (int)(contentLength.Value / 1024) : 100;
                    progressForm.LabelStatus.Text = "File downloading...";

                    while ((read = await inputStream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                    {
                        await outputStream.WriteAsync(buffer, 0, read, token);
                        totalRead += read;

                        if (contentLength.HasValue)
                        {
                            int value = (int)(totalRead / 1024);
                            int max = (int)(contentLength.Value / 1024);
                            if (value > max) value = max;
                            progressForm.ProgressBar.Value = value;
                            progressForm.LabelStatus.Text = $"downloaded {totalRead / 1024} / {contentLength.Value / 1024} KB";
                        }
                        else
                        {
                            progressForm.LabelStatus.Text = $"Downloaded {totalRead / 1024} KB";
                        }
                        Application.DoEvents();
                    }
                }
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            buttonUpdateCsv.Left = this.Width - 34 - buttonUpdateCsv.Width;
        }
        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {

            string[] autoColumns = { "code", "icao", "name", "country" };
            foreach (string col in autoColumns)
            {
                if (dataGridView1.Columns.Contains(col))
                {
                    dataGridView1.AutoResizeColumn(dataGridView1.Columns[col].Index, DataGridViewAutoSizeColumnMode.AllCells);
                }
            }

        }

        private async void Form1_Load(object sender, EventArgs e)
        {

            dataGridView1_DataBindingComplete(null, null);
            await CheckCsvFileVersionAsync();

        }
        private async Task CheckCsvFileVersionAsync()
        {
            buttonUpdateCsv.Enabled = false;
            buttonUpdateCsv.Text = "Checking Update";

            string csvPath = "airports.csv";
            DateTime? localDate = null;
            if (File.Exists(csvPath))
            {
                localDate = File.GetLastWriteTime(csvPath);
            }

            DateTime? remoteDate = await GetRemoteCsvUpdateDateAsync();

            buttonUpdateCsv.Enabled = true;
            buttonUpdateCsv.Text = "Update Database";

            if (localDate != null && remoteDate != null && localDate < remoteDate.Value.AddMinutes(-1))
            {
                MessageBox.Show(
                    "A new version of airports.csv is available online.\nYou can update it by pressing the 'Update CSV' button.",
                    "CSV is outdated", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async Task<DateTime?> GetRemoteCsvUpdateDateAsync()
        {
            try
            {
                string apiUrl = "https://api.github.com/repos/lxndrblz/Airports/commits?path=airports.csv&page=1&per_page=1";
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AirportsViewer", "1.0"));
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    if (!response.IsSuccessStatusCode) return null;

                    string json = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(json) || json == "[]") return null;

                    var serializer = new JavaScriptSerializer();
                    dynamic commits = serializer.Deserialize<dynamic>(json);

                    string dateStr = commits[0]["commit"]["committer"]["date"];
                    DateTime remoteDate = DateTime.Parse(dateStr).ToLocalTime();
                    return remoteDate;
                }
            }
            catch
            {
                // Не выводим ошибку, чтобы не мешать работе приложения
                return null;
            }
        }
    }
}
