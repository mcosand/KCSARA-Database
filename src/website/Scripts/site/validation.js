/*
 * Copyright 2015 Matthew Cosand
 */
(function ($) {
  var defaultOptions = {
    errorClass: 'has-error',
    highlight: function (element, errorClass) { $(element).closest(".form-group").addClass(errorClass); },
    unhighlight: function (element, errorClass, validClass) { $(element).closest(".form-group").removeClass(errorClass); }
  };
  $.validator.setDefaults(defaultOptions);
  $.validator.unobtrusive.options = { errorClass: defaultOptions.errorClass };
})(jQuery);
