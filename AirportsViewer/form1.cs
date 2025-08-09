using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace AirportsViewer
{
    public partial class Form1 : Form
    {
        // ============================
        // Data storage for VirtualMode
        // _all  = all rows loaded from CSV
        // _view = filtered view shown in the grid
        // ============================
        private List<Airport> _all = new List<Airport>();
        private List<Airport> _view = new List<Airport>();

        // ============================
        // State/flags
        // ============================
        private bool isDownloadInProgress = false;
        private bool needFormResize = true;     // whether to auto-resize form width to fit columns
        private bool _columnsInited = false;    // columns are created once
        private bool _autoSizedOnce = false;    // one-shot column autosize (like header double-click)

        public Form1()
        {
            InitializeComponent();

            // ---- DataGridView performance settings ----
            dataGridView1.VirtualMode = true;                   // render on-demand
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            // Enable DoubleBuffering (reduce flicker / smoother scrolling)
            typeof(DataGridView)
                .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(dataGridView1, true, null);

            // ---- Wire up events (VirtualMode + UX) ----
            dataGridView1.CellValueNeeded += dataGridView1_CellValueNeeded;         // core for VirtualMode
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;       // open maps/links
            dataGridView1.CellToolTipTextNeeded += dataGridView1_CellToolTipTextNeeded; // country tooltips
            dataGridView1.ColumnWidthChanged += dataGridView1_ColumnWidthChanged;   // auto-fit form width

            textBoxCode.TextChanged += FilterChanged;
            textBoxName.TextChanged += FilterChanged;
            textBoxCountry.TextChanged += FilterChanged;

            buttonUpdateCsv.Click += buttonUpdateCsv_Click;

  
        }

        // ---------------------------------------------
        // Load CSV and build in-memory lists
        // ---------------------------------------------
        private void LoadCsv()
        {
            try
            {
                if (!File.Exists("airports.csv"))
                {
                    MessageBox.Show(
                        "airports.csv not found.\nPlease download using \"Update Database\" button.",
                        "File not found",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    _all.Clear();
                    _view.Clear();

                    //if (dataGridView1.RowCount > 0) { dataGridView1.RowCount = 0; };
                    return;
                }
            }
            catch {
                
                    }

            try
            {
                using (var reader = new StreamReader("airports.csv"))
                using (var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    _all = csv.GetRecords<Airport>().ToList();
                }

                // Create columns once (explicitly) and apply filter to show rows
                EnsureColumns();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading CSV file: " + ex.Message);
                _all.Clear();
                _view.Clear();
                dataGridView1.RowCount = 0;
            }
        }

        // ---------------------------------------------
        // Create grid columns once (explicit types/order)
        // We do NOT add: latitude, longitude, elevation, city_code, type
        // ---------------------------------------------
        private void EnsureColumns()
        {
            if (_columnsInited) return;

            dataGridView1.Columns.Clear();

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { Name = "code", HeaderText = "code", SortMode = DataGridViewColumnSortMode.NotSortable });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { Name = "icao", HeaderText = "icao", SortMode = DataGridViewColumnSortMode.NotSortable });

            var nameCol = new DataGridViewLinkColumn
            {
                Name = "name",
                HeaderText = "name",
                TrackVisitedState = false,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            dataGridView1.Columns.Add(nameCol);

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { Name = "time_zone", HeaderText = "time_zone", SortMode = DataGridViewColumnSortMode.NotSortable });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { Name = "country", HeaderText = "country", SortMode = DataGridViewColumnSortMode.NotSortable });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { Name = "city", HeaderText = "city", SortMode = DataGridViewColumnSortMode.NotSortable });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { Name = "state", HeaderText = "state", SortMode = DataGridViewColumnSortMode.NotSortable });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { Name = "county", HeaderText = "county", SortMode = DataGridViewColumnSortMode.NotSortable });

            var urlCol = new DataGridViewLinkColumn
            {
                Name = "url",
                HeaderText = "url",
                TrackVisitedState = false,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            dataGridView1.Columns.Add(urlCol);

            _columnsInited = true;
        }

        // ---------------------------------------------
        // Filtering logic
        // - code matches code
        // - name filter also matches city/state/country (as requested)
        // - country filter matches ISO code
        // ---------------------------------------------
        private void FilterChanged(object sender, EventArgs e) => ApplyFilter();

        private void ApplyFilter()
        {
            string codeFilter = (textBoxCode.Text ?? "").Trim().ToLowerInvariant();
            string nameFilter = (textBoxName.Text ?? "").Trim().ToLowerInvariant();
            string countryFilter = (textBoxCountry.Text ?? "").Trim().ToLowerInvariant();

            _view = _all.Where(a =>
                (string.IsNullOrEmpty(codeFilter) || (!string.IsNullOrEmpty(a.code) && a.code.ToLowerInvariant().Contains(codeFilter))) &&
                (string.IsNullOrEmpty(nameFilter) ||
                    (!string.IsNullOrEmpty(a.name) && a.name.ToLowerInvariant().Contains(nameFilter)) ||
                    (!string.IsNullOrEmpty(a.city) && a.city.ToLowerInvariant().Contains(nameFilter)) ||
                    (!string.IsNullOrEmpty(a.state) && a.state.ToLowerInvariant().Contains(nameFilter)) ||
                    (!string.IsNullOrEmpty(a.country) && a.country.ToLowerInvariant().Contains(nameFilter))
                ) &&
                (string.IsNullOrEmpty(countryFilter) || (!string.IsNullOrEmpty(a.country) && a.country.ToLowerInvariant().Contains(countryFilter)))
            ).ToList();

            // VirtualMode: tell the grid how many rows we have now
            dataGridView1.RowCount = _view.Count;

            // One-shot manual autosize (like double-click on headers) when we first have data
            if (!_autoSizedOnce && _view.Count > 0)
            {
                _autoSizedOnce = true;

                Action doAuto = () =>
                {
                    AutoResizeOnce(new[] { "code", "icao", "name", "country" });
                    dataGridView1_ColumnWidthChanged(null, null); // also resize the form width
                };

                // Safety: BeginInvoke only if handle exists, otherwise wait for creation
                if (IsHandleCreated) BeginInvoke(doAuto);
                else this.HandleCreated += (s, ev) => BeginInvoke(doAuto);
            }

            dataGridView1.Invalidate(); // redraw visible cells
        }

        // Manual one-time autosizing for specific columns
        private void AutoResizeOnce(string[] cols)
        {
            foreach (var name in cols)
            {
                if (dataGridView1.Columns.Contains(name))
                    dataGridView1.AutoResizeColumn(dataGridView1.Columns[name].Index, DataGridViewAutoSizeColumnMode.AllCells);
            }
        }

        // ---------------------------------------------
        // VirtualMode: provide cell values on demand
        // ---------------------------------------------
        private void dataGridView1_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _view.Count) return;
            var a = _view[e.RowIndex];
            var col = dataGridView1.Columns[e.ColumnIndex].Name;

            switch (col)
            {
                case "code": e.Value = a.code; break;
                case "icao": e.Value = a.icao; break;
                case "name": e.Value = a.name; break; // link column
                case "time_zone": e.Value = a.time_zone; break;
                case "country": e.Value = a.country; break;
                case "city": e.Value = a.city; break;
                case "state": e.Value = a.state; break;
                case "county": e.Value = a.county; break;
                case "url": e.Value = string.IsNullOrWhiteSpace(a.url) ? "" : a.url; break; // empty -> looks inactive
            }
        }

        // ---------------------------------------------
        // Link clicks:
        // - name -> Google Maps (uses your current URL pattern)
        // - url  -> open website if not empty
        // ---------------------------------------------
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _view.Count) return;

            var column = dataGridView1.Columns[e.ColumnIndex].Name;
            var airport = _view[e.RowIndex];

            if (column == "name" && airport != null)
            {
                if (airport.latitude != 0 && airport.longitude != 0)
                {
                    string lat = airport.latitude.ToString(CultureInfo.InvariantCulture);
                    string lon = airport.longitude.ToString(CultureInfo.InvariantCulture);

                    // Keeping your existing link format:
                    // query by name + center by ll + zoom
                    string url = $"https://www.google.com/maps/?q={airport.name} airport&ll={lat},{lon}&z=13";
                    System.Diagnostics.Process.Start(url);
                }
            }
            else if (column == "url" && airport != null && !string.IsNullOrWhiteSpace(airport.url))
            {
                System.Diagnostics.Process.Start(airport.url);
            }
        }

        // ---------------------------------------------
        // Country tooltip (ISO code -> full country name)
        // ---------------------------------------------
        private void dataGridView1_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _view.Count || e.ColumnIndex < 0) return;
            var col = dataGridView1.Columns[e.ColumnIndex];
            if (col.Name != "country") return;

            var code = _view[e.RowIndex].country?.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(code)) return;

            if (CountryCodes.All.TryGetValue(code, out var full))
                e.ToolTipText = $"{code} — {full}";
            else
                e.ToolTipText = code;
        }

        // ---------------------------------------------
        // Resize form width to fit visible columns (no horizontal scrollbar)
        // Called on ColumnWidthChanged and once after initial autosize
        // ---------------------------------------------
        private void dataGridView1_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            if (!needFormResize) return;

            int columnsWidth = 0;
            foreach (DataGridViewColumn col in dataGridView1.Columns)
                if (col.Visible)
                    columnsWidth += col.Width;

            int vScrollBarWidth = (dataGridView1.DisplayedRowCount(false) < dataGridView1.RowCount)
                ? SystemInformation.VerticalScrollBarWidth : 0;

            int padding = dataGridView1.Location.X * 2 + 20;
            int targetWidth = columnsWidth + vScrollBarWidth + padding;
            int minWidth = 900;
            this.Width = Math.Max(targetWidth, minWidth);
        }

        // ---------------------------------------------
        // Update CSV button (download with a progress popup)
        // ---------------------------------------------
        private async void buttonUpdateCsv_Click(object sender, EventArgs e)
        {
            if (isDownloadInProgress) return;  // ignore double-clicks
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

                        // Reload CSV and rebuild view
                        needFormResize = false;
                        LoadCsv();
                        AutoResizeOnce(new[] { "code", "icao", "name", "country" });
                        needFormResize = true;
                        dataGridView1_ColumnWidthChanged(this, null);
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

        // ---------------------------------------------
        // Streaming download with progress bar updates
        // ---------------------------------------------
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
                        Application.DoEvents(); // keep UI responsive in the progress form
                    }
                }
            }
        }

        // ---------------------------------------------
        // Keep the update button aligned when resizing the form
        // ---------------------------------------------
        private void Form1_Resize(object sender, EventArgs e)
        {
            buttonUpdateCsv.Left = this.Width - 34 - buttonUpdateCsv.Width;
        }

        // ---------------------------------------------
        // Form Load: safe place to load CSV and do one-shot autosize,
        // then run async remote update check in background.
        // ---------------------------------------------
        private async void Form1_Load(object sender, EventArgs e)
        {
            LoadCsv();
            AutoResizeOnce(new[] { "code", "icao", "name", "country" });
            dataGridView1_ColumnWidthChanged(null, null);
            await CheckCsvFileVersionAsync();
        }

        // ---------------------------------------------
        // Background check: compare local CSV last write time vs latest GitHub commit
        // Disable the update button while checking to avoid confusion.
        // ---------------------------------------------
        private async Task CheckCsvFileVersionAsync()
        {
            buttonUpdateCsv.Enabled = false;
            buttonUpdateCsv.Text = "Checking Update";

            string csvPath = "airports.csv";
            DateTime? localDate = null;
            if (File.Exists(csvPath))
                localDate = File.GetLastWriteTime(csvPath);

            DateTime? remoteDate = await GetRemoteCsvUpdateDateAsync();

            buttonUpdateCsv.Enabled = true;
            buttonUpdateCsv.Text = "Update Database";

            if (localDate != null && remoteDate != null && localDate < remoteDate.Value.AddMinutes(-1))
            {
                MessageBox.Show(
                    "A new version of airports.csv is available online.\nYou can update it by pressing the 'Update CSV' button.",
                    "CSV is outdated",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }

        // ---------------------------------------------
        // Query GitHub API for the latest commit date that touched airports.csv
        // Returns local time (or null on failure).
        // ---------------------------------------------
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
                // Silently ignore errors to not disturb the user on startup
                return null;
            }
        }
    }
}
