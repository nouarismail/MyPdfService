using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyPdfService.Data;
using MyPdfService.Models;
using PuppeteerSharp;
using Polly;
using Microsoft.Extensions.Configuration;

namespace MyPdfService.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FileController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        public FileController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
            
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Invalid file.");

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                var fileModel = new FileModel
                {
                    FileName = file.FileName,
                    Content = memoryStream.ToArray()
                };


                _dbContext.Files.Add(fileModel);
                try
                {
                    await _dbContext.SaveChangesAsync();
                }
                catch (System.Exception e)
                {

                    Console.WriteLine(e.Message);
                }

                var retryPolicy = Policy
                .Handle<Exception>() // Retry on any exception
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));


                var pdfFileName = $"{fileModel.Id}_output.pdf";


                await retryPolicy.ExecuteAsync(async () => await ConvertToPdf(fileModel.Content, pdfFileName));

                fileModel.PdfFileName = pdfFileName;
                await _dbContext.SaveChangesAsync();

                return Ok(new { DownloadLink = Url.Action("DownloadFile", new { id = fileModel.Id }) });
            }
        }

        [HttpGet("download/{id}")]
        public IActionResult DownloadFile(int id)
        {
            var fileModel = _dbContext.Files.FirstOrDefault(f => f.Id == id);

            if (fileModel == null)
                return NotFound();
            string? directoryPath = string.IsNullOrEmpty(configuration["DirectoryPath"])?"./Outputs/":configuration["DirectoryPath"];
            var fileBytes = System.IO.File.ReadAllBytes(directoryPath+fileModel.PdfFileName);
            return File(fileBytes, "application/pdf", fileModel.PdfFileName);
        }

        private async Task ConvertToPdf(byte[] htmlContent, string pdfFileName)
        {
            using var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();

            var launchOptions = new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            };

            using var browser = await Puppeteer.LaunchAsync(launchOptions);
            using var page = await browser.NewPageAsync();
            string? directoryPath = string.IsNullOrEmpty(configuration["DirectoryPath"])?"./Outputs/":configuration["DirectoryPath"];
            if (Directory.Exists(directoryPath))
            {
                await page.SetContentAsync(Encoding.UTF8.GetString(htmlContent));
                await page.PdfAsync(directoryPath + pdfFileName);
            }
            else
            {
                // Create the directory
                Directory.CreateDirectory(directoryPath);
                Console.WriteLine("Directory created successfully.");
                await page.SetContentAsync(Encoding.UTF8.GetString(htmlContent));
                await page.PdfAsync(directoryPath + pdfFileName);
            }
            

        }
    }
}