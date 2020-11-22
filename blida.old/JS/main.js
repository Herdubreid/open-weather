import 'bootstrap';

document.addEventListener('click', function () {
    var collapse = document.getElementById('topNavbar');
    if (collapse != null) {
        var exp = collapse.getAttribute('class').match('show');
        if (exp !== null) {
            collapse.setAttribute('class', 'collapse navbar-collapse');
        }
    }
});
