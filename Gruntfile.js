module.exports = function (grunt) {
  grunt.initConfig({
    copy: {
      files: {
        cwd: 'src/website/scripts/',  // set working folder / root to copy
        src: ['**/*', '!**/*.TMP'],           // copy all files and subfolders
        dest: 'tests/website.qunit/prod/',    // destination folder
        expand: true,           // required when using cwd
        flatten: false
      }
    },
    qunit: {
      all: {
        options: {
          urls: ['tests/Website.QUnit/test.html?suite=app,utils,components/login-form,components/searchbox'],
          timeout: 5000
        }
      }
    },
    blanket_qunit: {
      all: {
        options: {
          urls: [
            'tests/website.qunit/test.html?coverage=true&gruntReport&suite=' +
            encodeURIComponent(['app', 'utils', 'components/login-form', 'components/searchbox'].join())
          ],
          threshold: 60
        }
      }
    },
    watch: {
      files: ['src/website/scripts/**/*.js', 'tests/website.qunit/tests/**', 'Gruntfile.js'],
      tasks: ['copy','blanket_qunit']
    }
  });

  grunt.loadNpmTasks('grunt-contrib-watch');
  grunt.loadNpmTasks('grunt-contrib-qunit');
  grunt.loadNpmTasks('grunt-blanket-qunit');
  grunt.loadNpmTasks('grunt-contrib-copy');
};