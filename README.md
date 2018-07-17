# RSSActivityMonitor
A small demo that accepts a CSV-formatted list of companies and their RSS feeds and outputs a list of all companies with no RSS activity for the past X days.

Usage:
```
dotnet RSSActivityMonitor.dll <list_file> <inactive_day_count>
```

Example:
```
dotnet RSSActivityMonitor.dll rssFeeds.txt 7
```

List files should be CSV-formatted, with the company name first and then the URL of the RSS feed.

Example:
```
"NASA","https://www.nasa.gov/rss/dyn/image_of_the_day.rss"
"Reuters","http://feeds.reuters.com/Reuters/worldNews"
```
