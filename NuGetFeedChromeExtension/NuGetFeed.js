var link = document.createElement("link");
link.setAttribute("rel", "alternate");
link.setAttribute("type", "application/rss+xml");
link.setAttribute("title", "Recent releases of " + document.location.href);
link.setAttribute("href", document.location.href.replace('nuget.org', 'nugetfeed.org'));
document.getElementsByTagName('head')[0].appendChild(link);

$('ul.links').append('<li><img src="http://nugetfeed.org/Content/Images/logo_10x10.png"> <a href="' + document.location.href.replace('nuget.org', 'nugetfeed.org') + '">NuGet Feed</a></li>');
