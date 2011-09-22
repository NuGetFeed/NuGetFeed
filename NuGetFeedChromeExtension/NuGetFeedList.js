$.each($('section > ul > li'), function(index, value) {
    var name = $(value).find('a:first').attr('href');
    $(value).find('ul.packageMeta').append('<li><img src="http://nugetfeed.org/Content/Images/logo_10x10.png"> <a href="http://nugetfeed.org' + name + '">NuGet Feed</a></li>');
});

