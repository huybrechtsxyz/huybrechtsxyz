grecaptcha.ready(function () {
	grecaptcha.execute('6LfVo7wUAAAAAGXveXDAXduxgOBlRGYbHeEGTrQ-', { action: 'contact' }).then(function (token) {
		document.getElementById("__captchaVerification").value = token;
	});
});