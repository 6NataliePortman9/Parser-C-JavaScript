###Parser 
Simple cross-platform HTML parser using C# and JavaScript.

### Overview
Parser is a easy tool for extracting content from HTML documents. This parser conteins of two parts, 
C# backend as simple WPF desktop app with user friendly interface, and JavaScript as a main tool which
analize web page, catch necessary parameters and write them in DataBase.

### Features
1. Load local or remote HTML files;
2. Extract specific tags and attributes;
3. Easy for modyfication
4. Simple user friendly interface

### Requirements
- [.NET Framework 4.8+]
- [Node.js]
- [Puppeteer]

### Installation

1. Clone the repository:

git clone https://github.com/6NataliePortman9/Parser-C-JavaScript.git  

cd Parser

2. Build and run the C# project:

cd ../CS_part

dotnet build

dotnet run

3. (Optional) Run the JavaScript part:

cd ../JS_part

npm install

node app.js

  ### Usage
1. Make a local DB with appropriate tables and fields
2. In C#  part:
  * Set a direction to you DB in MainWindow, EditQuestionWindow and AddQuestionWindow in 25 line.
  * Change Queries in MainWindow line 45 / 177 / 193.
  * Change Queries in EditQuestionWindow line 80 / 225.
  * Change Queries in AddQuestionWindow line 78 / 126.
3. In the JS part:
  * Set the database path on line 5
  * Change Queries in line 134 / 135
  * Set the browser executable path on line 13
  * Set the URL of the webpage to be parsed on line 20
  * Set your email account on line 60
  * Set your email password on line 64
4. Customize the button IDs if needed to match your target webpage structure.
  
