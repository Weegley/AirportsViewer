[![Build & Release AirportsViewer ](https://github.com/Weegley/AirportsViewer/actions/workflows/BuildAndReleaseAirportsViewer.yml/badge.svg)](https://github.com/Weegley/AirportsViewer/actions/workflows/BuildAndReleaseAirportsViewer.yml)
# AirportsViewer

**AirportsViewer** is a Windows application for viewing, searching, and filtering airport data from a CSV file.  
It is designed for fast lookup and convenient filtering of airports by code, name, country, and other parameters, with an option to update the airports database from an online source.

![AirportsViewer screenshot](https://github.com/user-attachments/assets/2ae99f3e-2d02-43a7-80fd-7a3e9ac8e51b)


## Features

- **Load and display** airports from a CSV file (`airports.csv`)
- **Filter/search** by airport code, name, country, city, and other fields
- **Clickable links**:
  - The airport name opens Google Maps with the location (if coordinates are available)
  - The `url` field opens the airport's official site (if available)
- **Country tooltips:** Shows full country name when hovering over the country code
- **Update CSV:** Download and update the airport list from the official [Airports GitHub database](https://github.com/lxndrblz/Airports)
- **Resizable UI:** Table and columns automatically adjust to the window size
- **Automatic column width adjustment** for main fields

## Getting Started

1. **Download or build the application.**
2. Place the `airports.csv` file in the same directory as the executable, or use the "Update CSV" button in the app to download the latest version.
3. Run `AirportsViewer.exe`.

## Usage

- Use the text fields above the table to filter by airport code, name, or country.
- Click on the airport name to open Google Maps.
- Click the website link (if present) to open the official airport page.
- Hover over the country code to see the full country name.
- To update the database, click the "Update CSV" button. Progress will be shown in a popup window.

## File Format

The CSV file should have columns:

```code,icao,name,latitude,longitude,elevation,url,time_zone,city_code,country,city,state,county,type```

Sample:

```AAA,NTGA,Anaa,-17.3506654,-145.51111994065877,36,,Pacific/Tahiti,AAA,PF,,,,AP```


## Online Database

The official CSV source used by the program is:  
https://raw.githubusercontent.com/lxndrblz/Airports/refs/heads/main/airports.csv

## Build Instructions

1. Open the solution in Visual Studio (recommended: 2019 or later).
2. Build in Release mode for .NET Framework 4.8.
3. The compiled files will be located in the `bin/Release/net48` directory.

## License

This project is distributed under the MIT License.

## Credits

Airport database: [lxndrblz/Airports](https://github.com/lxndrblz/Airports)




## Changelog

## v1.0.0
### Initial Release

## v1.0.1
### Online Database check
- Added online database update check on startup

## v1.1.0
### Table speedup
- Updated table handling for table to update and scroll faster.
