using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gtk;
using Window = Gtk.Window;
using ScottPlot;

// For distinction between ScottPlot and Gtk classes
using Label = Gtk.Label;
using Box = Gtk.Box;
using Orientation = Gtk.Orientation;
using Image = Gtk.Image;

public class Transaction {
    string date;
    string description;
    double amount;
    string category;

    public string Date {
        get => date;
        set { date = value; }
    }

    public string Description {
        get => description;
        set { description = value; }
    }

    public double Amount {
        get => amount;
        set { amount = value; }
    }

    public string Category {
        get => category;
        set { category = value; }
    }
}

public class FinanceTracker : Window {
    List<Transaction> transactions = new List<Transaction>();
    string DataFile = "finance_data.txt";
    Entry dateEntry;
    Entry descriptionEntry;
    Entry amountEntry;
    Entry categoryEntry;
    TreeView treeView; // Displays transaction table
    ListStore store; // Stores transaction data to be shown in TreeView
    Label summaryLabel; // Displays monthly summary text
    bool sortNewestFirst = true;

    public bool SortNewestFirst {
        get => sortNewestFirst;
        set { sortNewestFirst = value; }
    }

    /*
    Summary of Layout:
    mainBox
        inputBox
            dateBox
                dataLabel
                dataEntry
            descBox
                descLabel
                descEntry
            amountBox
                amountLabel
                amountEntry
            categoryBox
                categoryLabel
                categoryEntry
            addButton
            showBarChartButton
        sortBox
            sortLabel
            newestFirst (RadioButton)
            oldestFirst (RadioButton)
        scroll 
            treeView
        summaryLabel
    */

    public FinanceTracker() : base("Personal Finance Tracker") { // Setting up the window
        Resize(700, 400);
        Title = "Personal Finance Tracker";

        Box mainBox = new Box(Orientation.Vertical, 8); // Main vertical container that stacks: inputBox, sortBox, treeView, summaryLabel 
        Add(mainBox);

        Box inputBox = new Box(Orientation.Horizontal, 6); // Horizontal container for the labeled entry fields: dateBox, descBox, amountBox, categoryBox, addTransaction

        // Date field
        Box dateBox = new Box(Orientation.Vertical, 2); // Vertical container for the dateBox label and entry
        Label dateLabel = new Label("Date (YYYY-MM-DD):");
        dateBox.PackStart(dateLabel, false, false, 0); // Adding the label to dateBox container
        dateEntry = new Entry();
        dateBox.PackStart(dateEntry, false, false, 0); // Adding the entry field to dateBox container

        // Description field
        Box descriptionBox = new Box(Orientation.Vertical, 2);
        Label descLabel = new Label("Description:");
        descriptionBox.PackStart(descLabel, false, false, 0);
        descriptionEntry = new Entry();
        descriptionBox.PackStart(descriptionEntry, false, false, 0);

        // Amount field
        Box amountBox = new Box(Orientation.Vertical, 2);
        Label amountLabel = new Label("Amount:");
        amountBox.PackStart(amountLabel, false, false, 0);
        amountEntry = new Entry();
        amountBox.PackStart(amountEntry, false, false, 0);

        // Category field
        Box categoryBox = new Box(Orientation.Vertical, 2);
        Label categoryLabel = new Label("Category:");
        categoryBox.PackStart(categoryLabel, false, false, 0);
        categoryEntry = new Entry();
        categoryBox.PackStart(categoryEntry, false, false, 0);

        // AddTransaction button
        Button addButton = new Button("Add Transaction");
        addButton.Clicked += AddTransaction; // AddTransaction method called when addButton clicked

        // Chart buttons
        Box chartButtonBox = new Box(Orientation.Vertical, 4);
        Button showMonthlySummaryButton = new Button("Show Monthly Summary");
        showMonthlySummaryButton.Clicked += ShowMonthlySummaryChart;
        Button showYearlyChartButton = new Button("Show Yearly Trend");
        showYearlyChartButton.Clicked += ShowYearlyTrendChart;
        chartButtonBox.PackStart(showMonthlySummaryButton, false, false, 0);
        chartButtonBox.PackStart(showYearlyChartButton, false, false, 0);

        // Adding all the labeled entry boxes to the inputBox container
        inputBox.PackStart(dateBox, true, true, 0); // Expand and fill
        inputBox.PackStart(descriptionBox, true, true, 0);
        inputBox.PackStart(amountBox, true, true, 0);
        inputBox.PackStart(categoryBox, true, true, 0);
        inputBox.PackStart(addButton, false, false, 0);
        inputBox.PackStart(chartButtonBox, false, false, 0);

        mainBox.PackStart(inputBox, false, false, 0); // inputBox added to mainBox container

        Box sortBox = new Box(Orientation.Horizontal, 6); // Container for the label and the two date-sort options
        Label sortLabel = new Label("Sort by date:");

        RadioButton newestFirst = new RadioButton("Newest first");
        RadioButton oldestFirst = new RadioButton(newestFirst, "Oldest first"); // Linked to newestFirst button

        newestFirst.Active = true; // Default selection
        newestFirst.Toggled += SortSelection; // When user's selection changes, function is called
        oldestFirst.Toggled += SortSelection;

        // Adding the label and the two radiobuttons to the sortBox container
        sortBox.PackStart(sortLabel, false, false, 0);
        sortBox.PackStart(newestFirst, false, false, 0);
        sortBox.PackStart(oldestFirst, false, false, 0);

        mainBox.PackStart(sortBox, false, false, 0);

        store = new ListStore(typeof(string), typeof(string), typeof(string), typeof(string)); // Table columns hold: date, description, amount, category
        treeView = new TreeView(store); // Displays the stored data in a row

        // Columns of transaction table
        AddColumn("Date", 0);
        AddColumn("Description", 1);
        AddColumn("Amount", 2);
        AddColumn("Category", 3);

        ScrolledWindow scroll = new ScrolledWindow(); // Scrolling the window if needed
        scroll.Add(treeView);
        mainBox.PackStart(scroll, true, true, 0);

        summaryLabel = new Label();
        mainBox.PackStart(summaryLabel, false, false, 0); // Displays summary

        // Start of application
        LoadData();
        RefreshTransactionList();
        UpdateSummary();
        ShowAll();
    }

    protected override bool OnDeleteEvent(Gdk.Event e) { // Called when user closes window
        SaveData();
        Application.Quit();
        return true;
    }

    void AddColumn(string title, int index) { // Function called when adding columns
        TreeViewColumn column = new TreeViewColumn(title, new CellRendererText(), "text", index);
        treeView.AppendColumn(column);
    }

    void AddTransaction(object sender, EventArgs e) { // Function triggered when user clicks addTransaction button
        // Gets user input
        string date = dateEntry.Text;
        string description = descriptionEntry.Text;
        string amountString = amountEntry.Text;
        string category = categoryEntry.Text;

        // Checking if any field is null or empty
        if (date == null || date.Trim() == "" || description == null || description.Trim() == "" || amountString == null || amountString.Trim() == "" || category == null || category.Trim() == "") {
            ShowError("Please enter all transaction fields.");
            return;
        }

        if (!DateTime.TryParse(date, out var parsedDate)) { // Checking if date format is correct
            ShowError("Invalid date format. Please use YYYY-MM-DD.");
            return;
        }
        
        if (!double.TryParse(amountString, out var amount)) { // Checking if amount is a valid (double) number
            ShowError("Invalid amount number.");
            return;
        }

        if (description.Contains(",")) { // To prevent SaveData and LoadData errors
            ShowError("Description cannot contain commas.");
            return;
        }

        Transaction t = new Transaction {
            Date = DateTime.Parse(date).ToString("yyyy-MM-dd"),
            Description = description,
            Amount = double.Parse(amountString),
            Category = category
        };
        transactions.Add(t);

        RefreshTransactionList();
        UpdateSummary();
        SaveData();

        // Resetting entry fields
        dateEntry.Text = "";
        descriptionEntry.Text = "";
        amountEntry.Text = "";
        categoryEntry.Text = "";
    }

    void RefreshTransactionList() { // Called from the AddTransaction function and the SortSelection function
        store.Clear(); // Removes current stored transactions

        List<Transaction> sortedTransactions = new List<Transaction>(transactions); // Temporary list
        if (SortNewestFirst) {
            sortedTransactions.Sort((a, b) => string.Compare(b.Date, a.Date)); 
        } else {
            sortedTransactions.Sort((a, b) => string.Compare(a.Date, b.Date));
        }
        
        foreach (var t in sortedTransactions) { // Restoring the transactions to be displayed according to the new sort
            string amount = t.Amount.ToString("F2"); // amount is double in transactions, but string in sort
            store.AppendValues(t.Date, t.Description, amount, t.Category);
        }
    }

    void SortSelection(object sender, EventArgs e) { // Called when the radiobuttons are selected by user
        RadioButton rb = (RadioButton)sender; // rb = the radiobutton the user clicked
        if (rb.Active) { // User pressed button
            SortNewestFirst = rb.Label == "Newest first"; // updating bool SortNewestFirst based on the label of the button clicked by the user
            RefreshTransactionList();
        }
    }

    void UpdateSummary() {
        string currentYear = DateTime.Now.Year.ToString();
        string currentMonth = DateTime.Now.Month.ToString("D2"); // Adds leftmost 0 if needed
        var summary = new Dictionary<string, double>(); // Dictionary where Key = Category, Value = Amount

        foreach (var t in transactions) { // Calculates total amount per category
            string[] parts = t.Date.Split("-");
            if (parts[0] == currentYear && parts[1]==currentMonth) {
                if (!summary.ContainsKey(t.Category)) // New category this month
                    summary[t.Category] = 0;
                summary[t.Category] += t.Amount;
            }
        }

        string summaryText = "Monthly Summary:\n";
        foreach (var s in summary) {
            summaryText += $"{s.Key} : ${s.Value:F2}\n";
        }
        summaryLabel.Text = summaryText; // Adding it to our "summaryLabel" variable so it is displayed
    }


    void ShowError(string message) { // Function for handling all errors
        MessageDialog m = new MessageDialog(this, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Ok, message);
        m.Run(); // Message appears on screen
        m.Destroy();
    }

    void SaveData() { // Saving the user's data
        try {
            using (StreamWriter sr = new StreamWriter(DataFile)) {
                foreach (var t in transactions) {
                    string line = $"{t.Date},{t.Description},{t.Amount},{t.Category}"; // Dividing the data fields by commas
                    sr.WriteLine(line);
                }
            }
        } catch (Exception ex) {
            ShowError("Error saving data: " + ex.Message);
        }
    }

    void LoadData() { // Loading the user's data
        transactions.Clear();

        if (!File.Exists(DataFile))
            return;
            
        try{    
            using (StreamReader sr = new StreamReader(DataFile)) {
                string line;
                while ((line = sr.ReadLine()) != null) {
                    var parts = line.Split(',');
                    if (parts.Length != 4 || !double.TryParse(parts[2], out double amount)) // Skipping corrupt data
                        continue;
                    
                    Transaction t = new Transaction { // Creating a new transaction from each line
                        Date = parts[0],
                        Description = parts[1],
                        Amount = double.Parse(parts[2]),
                        Category = parts[3]
                    };
                    transactions.Add(t); // Adding each new transaction to the list
                }
            }    
        } catch (Exception ex) {
            ShowError("Error loading data: " + ex.Message);
        }
    }
    
    void ShowMonthlySummaryChart(object sender, EventArgs e) { // Called when user presses showMonthlySummaryButton
        var categorySums = new Dictionary<string, double>(); // Dictionary where Key = Category, Value = Amount
        foreach (var t in transactions) {
            if (!categorySums.ContainsKey(t.Category)) // New category
                categorySums[t.Category] = 0;
            categorySums[t.Category] += t.Amount; // Summing up the amount
        }
        if (categorySums.Count == 0) {
            ShowError("No transactions for current month.");
            return;
        }

        double[] values = categorySums.Values.ToArray();
        string[] labels = categorySums.Keys.ToArray();

        ScottPlot.Plot plot = new ScottPlot.Plot(); // Creating the plot

        plot.Title("Transaction Amounts by Category");
        plot.XLabel("Categories");
        plot.YLabel("Amount");
        plot.Add.Bars(values); // Adding the bar to the plot

    // Reference for next 5 lines: https://scottplot.net/cookbook/5.0/Bar/BarTickLabels/
        ScottPlot.Tick[] ticks = new ScottPlot.Tick[categorySums.Count];
        for (int i = 0; i < categorySums.Count; i++) {
            ticks[i] = new(i, labels[i]); // X-axis tick labels
        }
        plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks);

        DisplayPlotWindow(plot, "Monthly Summary");
    }

    void ShowYearlyTrendChart(object sender, EventArgs e) { // Called when user presses showYearlyChartButton
        var monthlyData = new Dictionary<string, (double Income, double Expenses)>();  // Dictionary where Key = month, Value = Income/Expenses

        DateTime oneYearAgo = DateTime.Now.AddYears(-1); // The date a year ago
        foreach (var t in transactions) {
            if (DateTime.Parse(t.Date) < oneYearAgo) // Skipping older months
                continue;

            string monthKey = DateTime.Parse(t.Date).ToString("yyyy-MM"); // Month of the transaction
            if (!monthlyData.ContainsKey(monthKey))
                monthlyData[monthKey] = (0, 0);

            if (t.Amount >= 0) // Income
                monthlyData[monthKey] = (monthlyData[monthKey].Income + t.Amount, monthlyData[monthKey].Expenses); // Adding the amount to the income
            else // Expense
                monthlyData[monthKey] = (monthlyData[monthKey].Income, monthlyData[monthKey].Expenses + Math.Abs(t.Amount)); // Adding the amount to the expenses
        }

        if (monthlyData.Count == 0) {
            ShowError("No transactions in the past year.");
            return;
        }

        // Sorting the data by ascending month order
        var sortedMonths = monthlyData.OrderBy(x => x.Key).ToList();
        string[] months = sortedMonths.Select(x => x.Key).ToArray();
        double[] income = sortedMonths.Select(x => x.Value.Income).ToArray();
        double[] expenses = sortedMonths.Select(x => x.Value.Expenses).ToArray();

        var plot = new Plot();
        plot.Title("Yearly Income/Expenses Chart");
        plot.XLabel("Month");
        plot.YLabel("Amount");

        double[] positions = new double[months.Length];
        for (int i = 0; i < months.Length; i++)
            positions[i] = i;
        // Adding the bars charts
        var incomeBars = plot.Add.Bars(positions, income);
        var expensesBars = plot.Add.Bars(positions, expenses);

        ScottPlot.Tick[] ticks = new ScottPlot.Tick[months.Length];
        for (int i = 0; i < months.Length; i++) {
            ticks[i] = new(i, months[i]); // X-axis tick labels
        }
        plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks);

        DisplayPlotWindow(plot, "Yearly Trend");
    }

    void DisplayPlotWindow(Plot plot, string title) { // Called by both chart functions, used to display the charts
        try {
            var window = new Window($"{title} Chart"); // Setting up the window
            window.SetDefaultSize(600, 400);

            plot.SavePng($"{title}chart.png", 600, 400); // Saving the chart as an image
            var image = new Image($"{title}chart.png");
            window.Add(image);
            window.ShowAll();

        } catch (Exception ex) {
            ShowError($"Failed to display chart: {ex.Message}");
        }
    }

    public static void Main() { // Running the application
        Application.Init();
        FinanceTracker f = new FinanceTracker();
        f.ShowAll();
        Application.Run();
    }
}
