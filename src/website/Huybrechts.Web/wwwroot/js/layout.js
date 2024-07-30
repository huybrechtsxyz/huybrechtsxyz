$(function () {
    document.getElementById('headerThemeToggler').addEventListener('change', (e) => {
        var value = e.target.checked ? 'on' : 'off';
        $.post("/Culture/SetTheme?theme=" + value);
        console.log(value)
    })
})