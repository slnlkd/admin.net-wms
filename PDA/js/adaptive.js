(function () {
    var html = document.documentElement;
    function onWindowResize() {
        html.style.fontSize = html.getBoundingClientRect().width / 15 + 'px';
    }
    // 750/15=50   1rem=25px
    // 640/16=40   1rem=20px
    window.addEventListener('resize', onWindowResize);
    onWindowResize();
})();