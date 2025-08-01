# Finance Tracker (C# + GTK#)

Welcome! This is a Personal Finance Tracker application created using C# and GTK. The program helps you keep track of your financial transactions by allowing you to add, view, sort, and summarize your spending.

---

## Features
-	Add transactions: Date, Description, Amount, and Category.
-	View all transactions in a table.
-	View monthly summary in the form of a Bar Chart based on category
-	View yearly trend chart displaying total income/expenses each month
-	Sort all transactions by date: Newest frist or Oldest first.
-	See a monthly summary of the total amounts per category.
-	Saves your transaction data automatically and loads it when restarting the app.

---

## How to Use the Application
### 1.	Adding a Transaction
-	Fill in the Date (YYYY-MM-DD), Description, Amount, Category
-	Click the Add Transaction button
-	If any field is missing, the application will notify the user

###2.	Sorting Transactions
-	Choose how to sort the transaction list by selecting either the Newest first or Oldest first button
-	The transaction table updates automatically according to the user’s choice

###3.	Viewing Transactions
-	The user’s transactions are displayed in a table with columns for Date, Description, Amount, Category
-	There is a scroll bar for long lists
-	There are two button that display images of a bar chart. “Show Monthly Summary” displays the user’s amounts per category spent/gained in the current month. “Show Yearly Trend” displays the user’s total income/expenses per month during the past year.

###4.	Monthly Summary
-	At the bottom of the application, a summary shows the user’s total amount per category for the current month

###5.	Saving and Loading Data
-	All the user’s transactions are saved automatically to a file when the application is closed
-	When you restart the application, the user’s previous data is loaded automatically


