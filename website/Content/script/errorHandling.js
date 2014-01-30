window.onerror = function (err, msg, loc) {
  $.ajax(errorUrl,
      {
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ Error: err, Message: msg, Location: loc }),
        dataType: 'json'
      })
};

if(typeof console === "undefined") {
  console = {
    log: function() { },
    debug: function() { }
    };
}