module.exports = function(grunt) {
  'use strict';
  grunt.initConfig({
    jasmine : {
coverage: {
      src : [
        'src/website/scripts/site/common.js',
        'src/website/scripts/site/searchbox.js',
        'src/website/scripts/site/app.js',
        'src/website/scripts/site/account/loginmodel.js'
      ],
      options : {
        vendor: [
          'src/website/scripts/jquery-2.1.3.min.js',
          'src/website/scripts/bootstrap.min.js',
          'src/website/scripts/jquery.toaster.js',
          'src/website/scripts/knockout-3.2.0.js'
        ],
        styles: [
          'src/website/content/bootstrap.min.css',
          'src/website/content/bootstrap-theme.css',
          'src/website/content/font-awesome.min.css',
          'src/website/content/size-next.css'
        ],
        specs : 'tests/jasmine/website/**/*.js',
        keepRunner:true,
template: require('grunt-template-jasmine-istanbul'),
templateOptions: {
  coverage: 'cover.json',
  report: 'coverage/jasmine'
}
      },
}
    },
    watch: {
      files: ['src/website/scripts/**/*.js', 'tests/jasmine/**/*/js', 'Gruntfile.js'],
      tasks: ['jasmine']
    }
  });
  grunt.loadNpmTasks('grunt-contrib-jasmine');
  grunt.loadNpmTasks('grunt-contrib-watch');
};