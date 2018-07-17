using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using LumenWorks.Framework.IO.Csv;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;

namespace RSSActivityMonitor
{
    public class RSSActivityMonitor
    {
        /// <summary>
        /// Accepts a CSV-formatted list of companies and their RSS feeds, as well as a given number of days,
        /// and will output any companies with no RSS activity within that given number.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public string Start(string[] args)
        {
            string outputMessage;

            var inputFailures = RunSanityChecks(args);
            if (!string.IsNullOrEmpty(inputFailures))
            {
                return inputFailures;
            }

            var numDays = int.Parse(args[1]);
            var inactiveCompanies = GetInactiveCompanies(args[0], numDays);

            if (inactiveCompanies.Count == 0)
            {
                outputMessage = string.Format(Resources.Resources.NoResults, numDays);
            }
            else
            {
                inactiveCompanies.Sort();

                var sb = new StringBuilder();
                sb.AppendLine(string.Format(Resources.Resources.YesResults + Environment.NewLine, numDays));

                foreach (var company in inactiveCompanies)
                {
                    sb.AppendLine($"    {company}");
                }

                outputMessage = sb.ToString();
            }

            return outputMessage;
        }

        /// <summary>
        /// Runs some basic sanity checks on the input arguments.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private string RunSanityChecks(string[] args)
        {
            if (args.Length != 2)
            {
                return string.Format(Resources.Resources.HelpMessage, Environment.NewLine);
            }

            if (!File.Exists(args[0]))
            {
                return string.Format(Resources.Resources.FileNotFound, args[0]);
            }

            int numDays;
            if (!int.TryParse(args[1], out numDays) || numDays < 0)
            {
                return string.Format(Resources.Resources.InvalidDayCount, args[1]);
            }

            return null;
        }

        /// <summary>
        /// Reads a CSV-formatted file and builds a list of inactive companies based on the age of their RSS feeds.
        /// Feeds containing no items always count as inactive.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="numDays"></param>
        /// <returns></returns>
        private List<string> GetInactiveCompanies(string filePath, int numDays)
        {
            var companyActivity = new Dictionary<string, bool>();

            using (var reader = new CsvReader(new StreamReader(filePath), false))
            {
                while (reader.ReadNextRecord())
                {
                    var companyLower = reader[0].ToLower();
                    var feedAgeTask = CheckFeedAge(reader[1]);

                    // If the feed is missing or otherwise can't be read,
                    // throw an exception so the user can correct their data.
                    if (feedAgeTask.Status == TaskStatus.Faulted)
                    {
                        throw new Exception(string.Format(Resources.Resources.CannotLoadFeed, reader[1]));
                    }
                    
                    var feedAge = feedAgeTask.Result;

                    // "age < 0" means no items found in the feed.
                    var isFeedActive = feedAge >= 0 && feedAge < numDays;

                    // A single active feed will overwrite a company's active state,
                    // but not vice-versa.
                    if (!companyActivity.ContainsKey(companyLower))
                    {
                        companyActivity.Add(companyLower, isFeedActive);
                    }
                    else if (isFeedActive && !companyActivity[companyLower])
                    {
                        companyActivity[companyLower] = true;
                    }
                }
            }

            return companyActivity.Where(s => !s.Value).Select(kvp => kvp.Key).ToList();
        }

        /// <summary>
        /// Consumes an RSS feed to determine the number of days since its last update.
        /// Will return -1 if unable to locate any items in the feed.
        /// </summary>
        /// <param name="feedPath"></param>
        /// <returns></returns>
        private async Task<int> CheckFeedAge(string feedPath)
        {
            var client = new HttpClient();
            var feedStream = client.GetStreamAsync(new Uri(feedPath)).Result;
            
            using (var xmlReader = XmlReader.Create(feedStream, new XmlReaderSettings() { Async = true }))
            {
                var feedReader = new RssFeedReader(xmlReader);

                while (await feedReader.Read())
                {
                    if (feedReader.ElementType == SyndicationElementType.Item)
                    {
                        ISyndicationItem item = await feedReader.ReadItem();
                        var activityDate = item.LastUpdated == DateTimeOffset.MinValue ? item.Published : item.LastUpdated;

                        return (DateTimeOffset.Now - activityDate).Days;
                    }
                }
            }

            return -1;
        }
    }
}
