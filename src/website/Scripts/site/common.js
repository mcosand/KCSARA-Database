function debounce(func, wait, immediate, stateFunc) {
  var timeout;
  var previousState = null;
  return function () {
    var context = this, args = arguments;
    var later = function () {
      timeout = null;
      if (stateFunc) {
        var now = stateFunc();
        if (previousState != now) { previousState = now; func.apply(context, args); }
      } else if (!immediate) {
        func.apply(context, args);
      }
    };
    var callNow = immediate && !timeout;
    clearTimeout(timeout);
    timeout = setTimeout(later, wait);
    if (callNow) {
      if (stateFunc) previousState = stateFunc();
      func.apply(context, args);
    }
  };
};
