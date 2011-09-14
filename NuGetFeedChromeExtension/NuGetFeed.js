var link = document.createElement("link");
link.setAttribute("rel", "alternate");
link.setAttribute("type", "application/rss+xml");
link.setAttribute("title", "Recent releases of X");
link.setAttribute("href", document.location.href.replace('nuget.org', 'nugetfeed.apphb.com'));
document.getElementsByTagName('head')[0].appendChild(link);