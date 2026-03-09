var dictLanguage = {};
//初始化多语言
function initLanguage() {
  //console.log("initLanguage lang is " + $.cookie("lang"))
  registerWords();
}

function clearLangData(){
	sessionStorage.removeItem('enData')
}
	
function setLanguage() {
  // setCookie("lang=" + lang + "; path=/;");
  var lang = $.cookie("lang")
 //  if($.cookie("lang")=="en"){
	// setLanguage("en");
 //  }else if($.cookie("lang")=="zh"){
	// setLanguage("zh");
 //  }else{
	// setLanguage("zh");
 //  }
  translate(lang);
}

// function getCookieVal(name) {
//   var items = document.cookie.split(";");
//   for (var i in items) {
// 	var cookie = $.trim(items[i]);
// 	var eqIdx = cookie.indexOf("=");
// 	var key = cookie.substring(0, eqIdx);
// 	if (name == $.trim(key)) {
// 	  return $.trim(cookie.substring(eqIdx + 1));
// 	}
//   }
//   return null;
// }

// function setCookie(cookie) {
//   var Days = 30; //此 cookie 将被保存 30 天
//   var exp = new Date(); //new Date("December 31, 9998");
//   exp.setTime(exp.getTime() + Days * 24 * 60 * 60 * 1000);
//   document.cookie = cookie+ ";expires=" + exp.toGMTString();
// }

function translate(lang) {
	//console.log("translate lang is "+lang)
  if(lang == "zh"){
	  setZhLangVal()
	  return
  }
  
  if(sessionStorage.getItem(lang + "Data") != null){
	dictLanguage = JSON.parse(sessionStorage.getItem(lang + "Data"));
	//console.log("111111111111111 dictLanguage "+JSON.stringify(dictLanguage))
  }else{
	loadDict();
  }
  setLangVal()
}

function setLangVal(){
  $("[lang]").each(function () {
	switch (this.tagName.toLowerCase()) {
	  case "input":
		//console.log("33333333333333 "+$(this).attr("lang"))
		//console.log("placeholder "+$(this).attr("langholder"))
		$(this).val(__tr($(this).attr("lang")));
		$(this).attr("placeholder", __tr($(this).attr("langHolder")))
		break;
	  default:
	   //console.log("setLangVal "+$(this).attr("lang"))
		$(this).text(__tr($(this).attr("lang")));
	}
  });
}

function setZhLangVal(){
  $("[lang]").each(function () {
	switch (this.tagName.toLowerCase()) {
	  case "input":
		//console.log("33333333333333 "+$(this).attr("lang"))
		//console.log("placeholder "+$(this).attr("langholder"))
		$(this).val($(this).attr("lang"));
		$(this).attr("placeholder", $(this).attr("langholder"))
		break;
	  default:
	   //console.log("setLangVal "+$(this).attr("lang"))
		$(this).text($(this).attr("lang"));
	}
  });
}

function __tr(src) {
  //console.log("src is "+src)
  return (dictLanguage[src] || src);
}

function loadDict() {
  var lang = $.cookie('lang')
  $.ajax({
	async: false,
	type: "GET",
	url: "lang/"+lang + ".json",
	success: function (msg) {
	  dictLanguage = msg;
	  sessionStorage.setItem(lang + 'Data', JSON.stringify(dictLanguage));
	  //console.log("dictLanguage is "+JSON.stringify(dictLanguage))
	  initLanguage()
	}
  });

}

// 遍历所有lang属性的标签赋值
function registerWords() {
	//console.log("2222222222222222222222222222")
  $("[lang]").each(function () {
	switch (this.tagName.toLowerCase()) {
	  case "input":
		if($(this).attr("lang")==""){
		  $(this).attr("lang", $(this).val());
		  //console.log("text is "+$(this).val())
		}
		if($(this).attr("langholder") == ""){
			//console.log("2323 "+$(this).attr('placeholder'))
			$(this).attr("langholder", $(this).attr('placeholder'))
		}
		break;
	  default:
		if($(this).attr("lang")==""){
		  $(this).attr("lang", $(this).text());
		}
		//console.log("text22 is "+$(this).text())
	}
  });
}
