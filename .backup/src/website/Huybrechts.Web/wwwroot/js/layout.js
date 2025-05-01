$(function () {
    document.getElementById('headerThemeToggler').addEventListener('change', (e) => {
        var value = e.target.checked ? 'on' : 'off';
        $.post("/Application/SetTheme?theme=" + value);
        //console.log(value)
    })
})