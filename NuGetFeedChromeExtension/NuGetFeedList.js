$.each($('section > ul > li'), function (index, value) {
    var name = $(value).find('a:first').attr('href');
    var id = name.substring(1 + name.lastIndexOf('/'));
    var packageMeta = $(value).find('ul.packageMeta');
    packageMeta.append($('<li>')
        .append('<a href="http://nugetfeed.org' + name + '" target="_blank" title="NuGet Feed"><img src="http://nugetfeed.org/Content/Images/logo_16x16.png"/></a>')
        .append('<a href="http://nugetfeed.org/feed/addtomyfeed/' + id + '" target="_blank" title="Add to My Feed"><img src="http://nugetfeed.org/Content/Images/star.png"/></a>'));
});