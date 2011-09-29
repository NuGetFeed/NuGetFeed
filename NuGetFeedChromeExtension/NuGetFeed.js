var link = document.createElement("link");
link.setAttribute("rel", "alternate");
link.setAttribute("type", "application/rss+xml");
link.setAttribute("title", "Recent releases of " + document.location.href);
link.setAttribute("href", document.location.href.replace('nuget.org', 'nugetfeed.org'));
document.getElementsByTagName('head')[0].appendChild(link);

var href = $('ul.links').find('a').filter(':contains("Contact Owners")').attr('href');
var id = href.substring(1 + href.lastIndexOf('/'));

$('ul.links').append($('<li>')
        .append('<a href="' + document.location.href.replace('nuget.org', 'nugetfeed.org') + '" target="_blank" title="NuGet Feed"><img src="http://nugetfeed.org/Content/Images/logo_16x16.png"/></a>')
        .append('<a href="http://nugetfeed.org/feed/addtomyfeed/' + id + '" target="_blank" title="Add to My Feed"><img src="http://nugetfeed.org/Content/Images/star.png"/></a>'));
