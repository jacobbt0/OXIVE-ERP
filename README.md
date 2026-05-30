# OXIVEERP - ASP.NET Web Forms Project (.NET 4.8)

## Project Structure

```
OXIVEERP/
├── OXIVEERP.csproj           ← Visual Studio project file
├── Web.config                ← App configuration
├── Site.master               ← Root master page (for Default.aspx)
├── Site.master.cs
├── Default.aspx              ← Dashboard/Home page
├── Default.aspx.cs
├── Content/
│   └── Site.css              ← Main stylesheet
└── Masters/
    ├── MastersSite.master    ← Master page for Masters subfolder
    ├── MastersSite.master.cs
    ├── UOMMaster.aspx        ← UOM Master (with grid, CRUD)
    ├── UOMMaster.aspx.cs
    ├── GroupMaster.aspx      ← Group Master (with grid, CRUD)
    ├── GroupMaster.aspx.cs
    ├── SubGroupMaster.aspx   ← Sub Group Master (with grid, CRUD)
    ├── SubGroupMaster.aspx.cs
    └── [other placeholder pages...]
```

## How to Open in Visual Studio

1. Open Visual Studio 2019/2022
2. File → Open → Project/Solution
3. Select `OXIVEERP.csproj`
4. Press F5 to run with IIS Express

## Features Implemented

| Page            | CRUD | Grid | Search | Validation |
|-----------------|------|------|--------|------------|
| UOM Master      | ✅   | ✅   | –      | ✅         |
| Group Master    | ✅   | ✅   | ✅     | ✅         |
| Sub Group Master| ✅   | ✅   | ✅     | ✅         |
| Dashboard       | –    | –    | –      | –          |

## Notes

- Data is stored in Application state (in-memory). 
- To use a real database, replace Application["..."] with SQL queries in the code-behind files.
- The connection string placeholder is in Web.config.
- The left sidebar and top navbar are fixed on all pages via the Master Page.
- Sidebar is collapsible via the hamburger ☰ button.
