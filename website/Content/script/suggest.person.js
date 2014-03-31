
/*
*	jquery.suggest 1.2 - 2007-08-21
*	
*  Original Script by Peter Vulgaris (www.vulgarisoip.com)
*  Updates by Chris Schuld (http://chrisschuld.com/)
*
*/

(function ($) {

  $.suggest = function (input, options) {

    var $input = $(input).attr("autocomplete", "off");
    var results;

    var timeout = false; 	// hold timeout ID for suggestion results to appear	
    var prevLength = 0; 		// last recorded length of $input.val()
    var cache = []; 			// cache MRU list
    var cacheSize = 0; 		// size of cache in chars (bytes?)

    var lookup = [];
    var lastCall = "";

    results = $('<ul class="ac_results"></ul>').appendTo('body');


    //results.addClass(options.resultsClass);

    resetPosition();
    $(window)
    .load(resetPosition)		// just in case user is changing size of page while loading
    .resize(resetPosition);

    $input.blur(function () {
      setTimeout(function () { results.hide() }, 200);
    });

    // help IE users if possible
    try {
      results.bgiframe();
    } catch (e) { }


    // I really hate browser detection, but I don't see any other way
    //if ($.browser.mozilla)
    //    $input.keypress(processKey); // onkeypress repeats arrow keys in Mozilla/Opera
    //else
    $input.keydown(processKey); 	// onkeydown repeats arrow keys in IE/Safari

    $input.keyup(function () {
      if ($input.val().length != prevLength) {
        $input.css("backgroundColor", "#ffffbb");

        //                var dem = $("#dem_" + options.dataContainer);
        //                dem.attr('disabled', false);
        //                dem.css("backgroundColor", "#ffffbb");
        //                if (options.onClear) {
        //                    options.onClear.apply(options.dataContainer);
        //                }

      }
    });


    function resetPosition() {
      // requires jquery.dimension plugin
      var offset = $input.offset();
      results.css({
        top: (offset.top + input.offsetHeight) + 'px',
        left: offset.left + 'px'
      });
    }


    function processKey(e) {

      // handling up/down/escape requires results to be visible
      // handling enter/tab requires that AND a result to be selected
      if ((/27$|38$|40$/.test(e.keyCode) && results.is(':visible')) ||
    (/^13$|^9$/.test(e.keyCode) && getCurrentResult())) {

        if (e.preventDefault)
          e.preventDefault();
        if (e.stopPropagation)
          e.stopPropagation();

        e.cancelBubble = true;
        e.returnValue = false;

        switch (e.keyCode) {

          case 38: // up
            prevResult();
            break;

          case 40: // down
            nextResult();
            break;

          case 9:  // tab
          case 13: // return
            selectCurrentResult();
            break;

          case 27: //	escape
            results.hide();
            break;

        }

      } else if ($input.val().length != prevLength) {
        if (timeout)
          clearTimeout(timeout);
        timeout = setTimeout(suggest, options.delay);
        prevLength = $input.val().length;

      }


    }


    function suggest() {

      var q = $.trim($input.val());

      if (q.length >= options.minchars) {

        cached = checkCache(q);

        if (cached) {

          displayItems(eval(cached['items']));

        } else {

          var progress = $("#progress");
          if (progress != null) {
            var offset = $input.offset();
            progress.css({
              top: (offset.top + ($input.outerHeight() - progress.outerHeight()) / 2) + 'px',
              left: (offset.left - progress.outerWidth()) + 'px'
            });
            progress.show();
          }

          var oldBg = $input.css("backgroundColor");
          $input.css("backgroundColor", "Orange");

          lastCall = q;

          $.ajax({
            type: 'POST', url: options.source, data: { q: q }, dataType: 'json',
            success: function (txt) {
              addToCache(q, txt, txt.length);
              if (q != lastCall) {
                return;
              }

              results.hide();

              var items = eval(txt);  //parseTxt(txt, q);
              lookup = [];

              displayItems(items);

              if (progress != null) {
                progress.hide();
              }
              $input.css("backgroundColor", oldBg);
            }
          });

        }

      } else {

        results.hide();

      }

    }


    function checkCache(q) {

      for (var i = 0; i < cache.length; i++)
        if (cache[i]['q'] == q) {
          cache.unshift(cache.splice(i, 1)[0]);
          return cache[0];
        }

      return false;

    }

    function addToCache(q, items, size) {

      while (cache.length && (cacheSize + size > options.maxCacheSize)) {
        var cached = cache.pop();
        cacheSize -= cached['size'];
      }

      cache.push({
        q: q,
        size: size,
        items: items
      });

      cacheSize += size;

    }

    function displayItems(items) {

      if (!items)
        return;

      if (!items.length) {
        results.hide();
        return;
      }

      var html = '';
      for (var i = 0; i < items.length; i++) {
        html += '<li id="s_' + items[i]['Id'] + '">' + items[i]['Name'] + ' [' + items[i]['DEM'] + ']' + '</li>';
        lookup[items[i]['Id']] = items[i];
      }
      resetPosition();
      results.html(html).show();

      results
    .children('li')
    .mouseover(function () {
      results.children('li').removeClass(options.selectClass);
      $(this).addClass(options.selectClass);
    })
    .click(function (e) {
      e.preventDefault();
      e.stopPropagation();
      selectCurrentResult();
    });

    }

    function parseTxt(txt, q) {

      var items = [];
      var tokens = txt.split(options.delimiter);

      // parse returned data for non-empty items
      for (var i = 0; i < tokens.length; i++) {
        var data = $.trim(tokens[i]).split(options.dataDelimiter);
        if (data.length > 1) {
          token = data[0];
          key = data[1];
        }
        else {
          token = data[0]
          key = '';
        }

        if (token) {
          token = token.replace(
            new RegExp(q, 'ig'),
            function (q) { return '<span class="' + options.matchClass + '">' + q + '</span>' }
            );
          items[items.length] = { 'value': token, 'key': key };
        }
      }

      return items;
    }

    function getCurrentResult() {

      if (!results.is(':visible'))
        return false;

      var $currentResult = results.children('li.' + options.selectClass);

      if (!$currentResult.length)
        $currentResult = false;

      return $currentResult;

    }

    function selectCurrentResult() {

      $currentResult = getCurrentResult();

      if ($currentResult) {
        var $id = $currentResult.attr('id').replace('s_', '');

        $input.val(lookup[$id].Name);
        results.hide();

        if (options.dataContainer) {
          $("#pid_" + options.dataContainer).val($id);
          var dem = $("#dem_" + options.dataContainer);
          dem.val(lookup[$id].DEM);
          dem.attr('disabled', true);
          dem.css('backgroundColor', "#ffffff");
        }

        if (options.onSelect) {
          options.onSelect.apply($input[0], new Array(options.dataContainer, lookup[$id]));
        }

        $input.css("backgroundColor", "#ffffff");
        prevLength = $input.val().length;
      }

    }

    function nextResult() {

      $currentResult = getCurrentResult();

      if ($currentResult)
        $currentResult
    .removeClass(options.selectClass)
    .next()
      .addClass(options.selectClass);
      else
        results.children('li:first-child').addClass(options.selectClass);

    }

    function prevResult() {

      $currentResult = getCurrentResult();

      if ($currentResult)
        $currentResult
    .removeClass(options.selectClass)
    .prev()
      .addClass(options.selectClass);
      else
        results.children('li:last-child').addClass(options.selectClass);

    }

  }

  $.fn.suggest = function (source, options) {

    if (!source)
      return;

    options = options || {};
    options.source = source;
    options.delay = options.delay || 150;
    options.resultsClass = options.resultsClass || 'ac_results';
    options.selectClass = options.selectClass || 'ac_over';
    options.matchClass = options.matchClass || 'ac_match';
    options.minchars = options.minchars || 2;
    options.delimiter = options.delimiter || '\n';
    options.onSelect = options.onSelect || false;
    options.maxCacheSize = options.maxCacheSize || 65536;
    options.dataDelimiter = options.dataDelimiter || '\t';
    options.dataContainer = options.dataContainer || '#SuggestResult';
    options.attachObject = options.attachObject || null;

    this.each(function () {
      new $.suggest(this, options);
    });

    return this;

  };

})(jQuery);














// Requires: jquery-json, jquery
(function ($) {

  $.suggest2 = function (input, options) {
    var $input = $(input).attr("autocomplete", "off");
    var results;

    var timeout = false; 	// hold timeout ID for suggestion results to appear	
    var prevText = $input.val(); 		// last recorded length of $input.val()
    var cache = []; 			// cache MRU list
    var cacheSize = 0; 		// size of cache in chars (bytes?)

    var lookup = [];
    var lastCall = "";

    if (!options.attachObject)
      options.attachObject = $(document.createElement("ul")).appendTo('body');

    results = $(options.attachObject);
    results.addClass(options.resultsClass);

    resetPosition();
    $(window)
    .load(resetPosition)		// just in case user is changing size of page while loading
    .resize(resetPosition);

    $input.blur(function () {
      setTimeout(function () { results.hide() }, 200);
    });

    // help IE users if possible
    try {
      results.bgiframe();
    } catch (e) { }


    // I really hate browser detection, but I don't see any other way
    //if ($.browser.mozilla)
    //  $input.keypress(processKey); // onkeypress repeats arrow keys in Mozilla/Opera
    //else
      $input.keydown(processKey); 	// onkeydown repeats arrow keys in IE/Safari

    $input.keyup(function () {
      if ($input.val().length != prevText.length) {
        $input.css("backgroundColor", "#ffffbb");
        $input.removeAttr("result");

        var dem = $("#dem_" + options.dataContainer);
        dem.attr('disabled', false);
        dem.css("backgroundColor", "#ffffbb");
        if (options.onClear) {
          options.onClear.apply(options.dataContainer);
        }

      }
    });


    function resetPosition() {
      // requires jquery.dimension plugin
      var offset = $input.offset();
      results.css({
        top: (offset.top + input.offsetHeight) + 'px',
        left: offset.left + 'px'
      });
    }


    function processKey(e) {

      // handling up/down/escape requires results to be visible
      // handling enter/tab requires that AND a result to be selected
      if ((/27$|38$|40$/.test(e.keyCode) && results.is(':visible')) ||
    (/^13$|^9$/.test(e.keyCode) && getCurrentResult())) {

        if (e.preventDefault)
          e.preventDefault();
        if (e.stopPropagation)
          e.stopPropagation();

        e.cancelBubble = true;
        e.returnValue = false;

        switch (e.keyCode) {

          case 38: // up
            prevResult();
            break;

          case 40: // down
            nextResult();
            break;

          case 9:  // tab
          case 13: // return
            selectCurrentResult();
            break;

          case 27: //	escape
            results.hide();
            break;

        }

      } else if ($input.val().length != prevText.length) {
        if (timeout)
          clearTimeout(timeout);
        timeout = setTimeout(suggest, options.delay);
        //                prevLength = $input.val().length;

      }


    }


    function suggest() {

      var q = $.trim($input.val());

      if (q.length >= options.minchars) {

        cached = checkCache(q);

        if (cached) {

          displayItems(eval(cached['items']));

        } else {

          var progress = $("#progress");
          if (progress != null) {
            var offset = $input.offset();
            progress.css({
              top: (offset.top + ($input.outerHeight() - progress.outerHeight()) / 2) + 'px',
              left: (offset.left - progress.outerWidth()) + 'px'
            });
            progress.show();
          }

          var oldBg = $input.css("backgroundColor");
          $input.css("backgroundColor", "Orange");

          lastCall = q;

          $.ajax({
            type: 'POST', url: options.source, data: { q: q, when: ($("#StartDay").val()) }, dataType: 'json',
            success: function (txt) {
              addToCache(q, txt, txt.length);
              if (q != lastCall) {
                return;
              }

              results.hide();

              var items = eval(txt);  //parseTxt(txt, q);
              lookup = [];

              displayItems(items);

              if (progress != null) {
                progress.hide();
              }
              $input.css("backgroundColor", oldBg);
            }
          });
        }

      } else {

        results.hide();

      }

    }


    function checkCache(q) {

      for (var i = 0; i < cache.length; i++)
        if (cache[i]['q'] == q) {
          cache.unshift(cache.splice(i, 1)[0]);
          return cache[0];
        }

      return false;

    }

    function addToCache(q, items, size) {

      while (cache.length && (cacheSize + size > options.maxCacheSize)) {
        var cached = cache.pop();
        cacheSize -= cached['size'];
      }

      cache.push({
        q: q,
        size: size,
        items: items
      });

      cacheSize += size;

    }

    function displayItems(items) {

      if (!items)
        return;

      if (!items.length) {
        results.hide();
        return;
      }

      var html = '';
      for (var i = 0; i < items.length; i++) {
        html += '<li id="s_' + items[i]['Id'] + '">' + items[i]['Name'] + ' [' + items[i]['DEM'] + ']' + '</li>';
        lookup[items[i]['Id']] = items[i];
      }
      resetPosition();
      results.html(html).show();

      results
    .children('li')
    .mouseover(function () {
      results.children('li').removeClass(options.selectClass);
      $(this).addClass(options.selectClass);
    })
    .click(function (e) {
      e.preventDefault();
      e.stopPropagation();
      selectCurrentResult();
    });

    }

    function parseTxt(txt, q) {

      var items = [];
      var tokens = txt.split(options.delimiter);

      // parse returned data for non-empty items
      for (var i = 0; i < tokens.length; i++) {
        var data = $.trim(tokens[i]).split(options.dataDelimiter);
        if (data.length > 1) {
          token = data[0];
          key = data[1];
        }
        else {
          token = data[0]
          key = '';
        }

        if (token) {
          token = token.replace(
    new RegExp(q, 'ig'),
    function (q) { return '<span class="' + options.matchClass + '">' + q + '</span>' }
    );
          items[items.length] = { 'value': token, 'key': key };
        }
      }

      return items;
    }

    function getCurrentResult() {

      if (!results.is(':visible'))
        return false;

      var $currentResult = results.children('li.' + options.selectClass);

      if (!$currentResult.length)
        $currentResult = false;

      return $currentResult;

    }

    function selectCurrentResult() {
      $currentResult = getCurrentResult();

      if ($currentResult) {
        var $id = $currentResult.attr('id').replace('s_', '');
        $input.val(lookup[$id].Name);
        $input.attr("result", JSON.stringify(lookup[$id]));

        results.hide();

        if (options.dataContainer) {
          $("#pid_" + options.dataContainer).val($id);
          var dem = $("#dem_" + options.dataContainer);
          dem.val(lookup[$id].DEM);
          dem.attr('disabled', true);
          dem.css('backgroundColor', "#ffffff");
        }

        if (options.onSelect) {
          options.onSelect.apply($input[0], new Array(options.dataContainer, lookup[$id]));
        }

        $input.css("backgroundColor", "#ffffff");
        //            prevLength = $input.val().length;
      }

    }

    function nextResult() {

      $currentResult = getCurrentResult();

      if ($currentResult)
        $currentResult
    .removeClass(options.selectClass)
    .next()
      .addClass(options.selectClass);
      else
        results.children('li:first-child').addClass(options.selectClass);

    }

    function prevResult() {

      $currentResult = getCurrentResult();

      if ($currentResult)
        $currentResult
    .removeClass(options.selectClass)
    .prev()
      .addClass(options.selectClass);
      else
        results.children('li:last-child').addClass(options.selectClass);

    }

  }

  $.fn.suggest2 = function (source, options) {
    if (!source)
      return;

    options = options || {};
    options.source = source;
    options.delay = options.delay || 150;
    options.resultsClass = options.resultsClass || 'ac_results';
    options.selectClass = options.selectClass || 'ac_over';
    options.matchClass = options.matchClass || 'ac_match';
    options.minchars = options.minchars || 2;
    options.delimiter = options.delimiter || '\n';
    options.onSelect = options.onSelect || false;
    options.maxCacheSize = options.maxCacheSize || 65536;
    options.dataDelimiter = options.dataDelimiter || '\t';
    //       options.dataContainer = options.dataContainer || '#SuggestResult';
    //       options.attachObject = options.attachObject || null;

    this.each(function () {
      new $.suggest2(this, options);
    });

    return this;

  };

})(jQuery);

(function ($) {

  $.suggest3 = function (input, options) {

    var $input = $(input).attr("autocomplete", "off");
    var results = $('<ul class="ac_results"></ul>').appendTo('body');
    $input[0].setPerson = function (person) { setItem(person); }

    var timeout = false; 	// hold timeout ID for suggest3ion results to appear	
    var prevText = $input.val(); 		// last recorded length of $input.val()
    var cache = []; 			// cache MRU list
    var cacheSize = 0; 		// size of cache in chars (bytes?)

    //    var lookup = [];
    //    var lastCall = "";


    //results.addClass(options.resultsClass);

    resetPosition();
    $(window)
  .load(resetPosition)		// just in case user is changing size of page while loading
  .resize(resetPosition);

    $input.blur(function () {
      setTimeout(function () { results.hide() }, 200);
    });

    // help IE users if possible
    try {
      results.bgiframe();
    } catch (e) { }


    // I really hate browser detection, but I don't see any other way
    if ($.browser.mozilla)
      $input.keypress(processKey); // onkeypress repeats arrow keys in Mozilla/Opera
    else
      $input.keydown(processKey); 	// onkeydown repeats arrow keys in IE/Safari

    $input.keyup(function () {
      if ($input.val() != prevText) {
        $input.css("backgroundColor", "#ffffbb");
      }
    });


    function resetPosition() {
      // requires jquery.dimension plugin
      var offset = $input.offset();
      results.css({
        top: (offset.top + input.offsetHeight) + 'px',
        left: offset.left + 'px'
      });
    }


    function processKey(e) {

      // handling up/down/escape requires results to be visible
      // handling enter/tab requires that AND a result to be selected
      if ((/27$|38$|40$/.test(e.keyCode) && results.is(':visible')) ||
  (/^13$|^9$/.test(e.keyCode) && getCurrentResult())) {

        if (e.preventDefault)
          e.preventDefault();
        if (e.stopPropagation)
          e.stopPropagation();

        e.cancelBubble = true;
        e.returnValue = false;

        switch (e.keyCode) {

          case 38: // up
            prevResult();
            break;

          case 40: // down
            nextResult();
            break;

          case 9:  // tab
          case 13: // return
            selectCurrentResult();
            break;

          case 27: //	escape
            results.hide();
            break;

        }

      } else if ($input.val() != prevText) {
        if (timeout)
          clearTimeout(timeout);
        timeout = setTimeout(suggest3, options.delay);
        prevText = $input.val();
        $input[0].person = null;
      }
    }


    function suggest3() {
      var q = $.trim($input.val());
      if (q.length >= options.minchars) {
        cached = checkCache(q);
        if (cached) {
          displayItems(eval(cached['items']));
        } else {
          var progress = $("#progress");
          if (progress != null) {
            var offset = $input.offset();
            progress.css({
              top: (offset.top + ($input.outerHeight() - progress.outerHeight()) / 2) + 'px',
              left: (offset.left - progress.outerWidth()) + 'px'
            });
            progress.show();
          }

          var oldBg = $input.css("backgroundColor");
          $input.css("backgroundColor", "Orange");

          lastCall = q;

          $.ajax({
            type: 'POST', url: options.source, data: { q: q }, dataType: 'json',
            success: function (txt) {
              addToCache(q, txt, txt.length);
              if (q != lastCall) {
                return;
              }

              results.hide();

              var items = eval(txt);  //parseTxt(txt, q);
              //    lookup = [];

              displayItems(items);

              if (progress != null) {
                progress.hide();
              }
              $input.css("backgroundColor", oldBg);
            }
          });

        }
      } else {
        results.hide();
      }
    }


    function checkCache(q) {

      for (var i = 0; i < cache.length; i++)
        if (cache[i]['q'] == q) {
          cache.unshift(cache.splice(i, 1)[0]);
          return cache[0];
        }

      return false;

    }

    function addToCache(q, items, size) {

      while (cache.length && (cacheSize + size > options.maxCacheSize)) {
        var cached = cache.pop();
        cacheSize -= cached['size'];
      }

      cache.push({
        q: q,
        size: size,
        items: items
      });

      cacheSize += size;

    }

    function displayItems(items) {

      if (!items)
        return;

      if (!items.length) {
        results.hide();
        return;
      }

      results.html('');
      for (var i = 0; i < items.length; i++) {
        var li = $('<li>' + items[i]['Name'] + ' [' + items[i]['DEM'] + ']</li>').appendTo(results);
        li[0].item = items[i];
        //        html += '<li id="s_' + items[i]['Id'] + '">' + items[i]['Name'] + ' [' + items[i]['DEM'] + ']' + '</li>';
        //        lookup[items[i]['Id']] = items[i];
      }
      resetPosition();
      results/*.html(html)*/.show();

      results
  .children('li')
  .mouseover(function () {
    results.children('li').removeClass(options.selectClass);
    $(this).addClass(options.selectClass);
  })
  .click(function (e) {
    e.preventDefault();
    e.stopPropagation();
    selectCurrentResult();
  });

    }

    function parseTxt(txt, q) {

      var items = [];
      var tokens = txt.split(options.delimiter);

      // parse returned data for non-empty items
      for (var i = 0; i < tokens.length; i++) {
        var data = $.trim(tokens[i]).split(options.dataDelimiter);
        if (data.length > 1) {
          token = data[0];
          key = data[1];
        }
        else {
          token = data[0]
          key = '';
        }

        if (token) {
          token = token.replace(
  new RegExp(q, 'ig'),
  function (q) { return '<span class="' + options.matchClass + '">' + q + '</span>' }
  );
          items[items.length] = { 'value': token, 'key': key };
        }
      }

      return items;
    }

    function getCurrentResult() {

      if (!results.is(':visible'))
        return false;

      var $currentResult = results.children('li.' + options.selectClass);

      if (!$currentResult.length)
        $currentResult = false;

      return $currentResult;

    }

    function setItem(item) {
      $input[0].person = item;
      $input.val(item.Name);
    }

    function selectCurrentResult() {
      $currentResult = getCurrentResult();

      if ($currentResult) {
        //var $id = $currentResult.attr('id').replace('s_', '');
        setItem($currentResult[0].item);

        //$input.val(lookup[$id].Name);
        results.hide();

        //        if (options.dataContainer) {
        //          $("#pid_" + options.dataContainer).val($id);
        //          var dem = $("#dem_" + options.dataContainer);
        //          dem.val(lookup[$id].DEM);
        //          dem.attr('disabled', true);
        //          dem.css('backgroundColor', "#ffffff");
        //        }

        if (options.onSelect) {
          options.onSelect($currentResult[0].item);
        }

        $input.css("backgroundColor", "#ffffff");
        prevText = $input.val();
      }

    }

    function nextResult() {

      $currentResult = getCurrentResult();

      if ($currentResult)
        $currentResult
  .removeClass(options.selectClass)
  .next()
    .addClass(options.selectClass);
      else
        results.children('li:first-child').addClass(options.selectClass);

    }

    function prevResult() {

      $currentResult = getCurrentResult();

      if ($currentResult)
        $currentResult
  .removeClass(options.selectClass)
  .prev()
    .addClass(options.selectClass);
      else
        results.children('li:last-child').addClass(options.selectClass);

    }

  }

  $.fn.suggest3 = function (source, options) {

    if (!source)
      return;

    options = options || {};
    options.source = source;
    options.delay = options.delay || 150;
    options.resultsClass = options.resultsClass || 'ac_results';
    options.selectClass = options.selectClass || 'ac_over';
    options.matchClass = options.matchClass || 'ac_match';
    options.minchars = options.minchars || 2;
    //options.delimiter = options.delimiter || '\n';
    options.onSelect = options.onSelect || false;
    //options.maxCacheSize = options.maxCacheSize || 65536;
    //options.dataDelimiter = options.dataDelimiter || '\t';
    //options.dataContainer = options.dataContainer || '#suggest3Result';
    //options.attachObject = options.attachObject || null;

    this.each(function () {
      new $.suggest3(this, options);
    });

    return this;

  };

})(jQuery);