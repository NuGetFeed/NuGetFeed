var link = document.createElement("link");
link.setAttribute("rel", "alternate");
link.setAttribute("type", "application/rss+xml");
link.setAttribute("title", "Recent releases of " + document.location.href);
link.setAttribute("href", document.location.href.replace('nuget.org', 'nugetfeed.org'));
document.getElementsByTagName('head')[0].appendChild(link);