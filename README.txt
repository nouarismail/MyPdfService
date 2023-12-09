# MyPdfService is ASP.NET Core Web API with File Upload and download functionality.
## Overview
This project demonstrates the implementation of an ASP.NET Core web service 
that accepts HTML files from clients, stores them in a database using Entity Framework Core, 
converts them to PDF using Puppeteer Sharp, 
and returns download links to the clients. 
Additionally, it integrates Polly to handle transient faults and improve API stability.

## Framework
It was created using .NET Core 7 

## Dependencies
- Entity Framework Core.
- Puppeteer Sharp.
- Polly: Used for implementing retry policies to handle transient faults.


## File Upload
- Clients can send a POST request to `/api/files/upload` with an HTML file in the request body.
- The server stores the file in the database and converts it to a PDF using Puppeteer Sharp.
- Clients receive a download link in the response.

## Polly Resilience
- Polly is integrated to handle transient faults, such as temporary network issues or IIS stops.


## Note
- Tested with Pdf file around 2500 pages, the conversion process took some time, but finally retairned the response.
